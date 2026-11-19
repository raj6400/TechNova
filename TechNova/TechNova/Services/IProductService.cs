using Microsoft.EntityFrameworkCore;
using TechNova.Data;
using TechNova.Models;
using TechNova.ViewModels;

namespace TechNova.Services
{
    public interface IProductService
    {
        // Fetch a single product by its primary key
        Task<Product?> GetProductByIdAsync(int id);

        Task<bool> AddReviewAsync(int productId, string userId, ReviewViewModel review);

        // Fetch similar products based on Category or Brand
        // Default count is 4
        Task<List<Product>> GetRelatedProductsAsync(int productId, int count = 4);

        // Fetch filtered and paginated products
        Task<ProductListViewModel> GetProductsAsync(ProductListViewModel filter);

        // Fetch featured products
        Task<List<Product>> GetFeaturedProductsAsync(int count = 8);

        // Fetch new arrival products
        Task<List<Product>> GetNewArrivalsAsync(int count = 8);

        // Fetch bestseller products
        Task<List<Product>> GetBestsellersAsync(int count = 8);

        // Fetch brands
        Task<List<Brand>> GetBrandsAsync(int count = 6);
    }

    public class ProductService : IProductService
    {
        private readonly ApplicationDbContext _context;

        public ProductService(ApplicationDbContext context)
        {
            _context = context;
        }

        // ============================================================
        // GET BESTSELLERS
        // ============================================================

        public async Task<List<Product>> GetBestsellersAsync(int count = 8)
        {
            return await _context.Products
                .Include(p => p.Brand)
                .Include(p => p.Reviews)
                .Where(p => p.IsBestseller && p.IsActive)
                .Take(count)
                .ToListAsync();
        }

        // ============================================================
        // GET FEATURED PRODUCTS
        // ============================================================

        public async Task<List<Product>> GetFeaturedProductsAsync(int count = 8)
        {
            return await _context.Products
                .Include(p => p.Brand)
                .Include(p => p.Reviews)
                .Where(p => p.IsFeatured && p.IsActive)
                .Take(count)
                .ToListAsync();
        }

        // ============================================================
        // GET NEW ARRIVALS
        // ============================================================

        public async Task<List<Product>> GetNewArrivalsAsync(int count = 8)
        {
            return await _context.Products
                .Include(p => p.Brand)
                .Include(p => p.Reviews)
                .Where(p => p.IsNewArrival && p.IsActive)
                .OrderByDescending(p => p.CreatedAt)
                .Take(count)
                .ToListAsync();
        }

        // ============================================================
        // GET PRODUCTS WITH FILTERING, SEARCHING, SORTING & PAGINATION
        // ============================================================

        public async Task<ProductListViewModel> GetProductsAsync(
            ProductListViewModel filter)
        {
            // 1. Start query with related data
            var query = _context.Products
                .Include(p => p.Brand)
                .Include(p => p.Category)
                .Include(p => p.Reviews)
                .Where(p => p.IsActive)
                .AsQueryable();

            // ========================================================
            // 2. CATEGORY FILTER
            // ========================================================

            if (filter.SelectedCategoryId.HasValue)
            {
                query = query.Where(p =>
                    p.CategoryId == filter.SelectedCategoryId.Value);
            }

            // ========================================================
            // 3. BRAND FILTER
            // ========================================================

            if (filter.SelectedBrandId.HasValue)
            {
                query = query.Where(p =>
                    p.BrandId == filter.SelectedBrandId.Value);
            }

            // ========================================================
            // 4. SEARCH FILTER
            // ========================================================

            if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
            {
                string searchTerm = filter.SearchTerm.Trim();

                query = query.Where(p =>
                    p.Name.Contains(searchTerm) ||
                    (p.Description != null &&
                     p.Description.Contains(searchTerm)) ||
                    p.Brand.Name.Contains(searchTerm));
            }

            // ========================================================
            // 5. MINIMUM PRICE FILTER
            // ========================================================

            if (filter.MinPrice.HasValue)
            {
                query = query.Where(p =>
                    p.SalePrice >= filter.MinPrice.Value);
            }

            // ========================================================
            // 6. MAXIMUM PRICE FILTER
            // ========================================================

            if (filter.MaxPrice.HasValue)
            {
                query = query.Where(p =>
                    p.SalePrice <= filter.MaxPrice.Value);
            }

            // ========================================================
            // 7. SORTING
            // ========================================================

            query = filter.SortOrder?.ToLower() switch
            {
                "price_asc" =>
                    query.OrderBy(p => p.SalePrice),

                "price_desc" =>
                    query.OrderByDescending(p => p.SalePrice),

                "name_asc" =>
                    query.OrderBy(p => p.Name),

                "name_desc" =>
                    query.OrderByDescending(p => p.Name),

                "newest" =>
                    query.OrderByDescending(p => p.CreatedAt),

                _ =>
                    query.OrderByDescending(p => p.CreatedAt)
            };

            // ========================================================
            // 8. PAGINATION CALCULATION
            // ========================================================

            filter.TotalCount = await query.CountAsync();

            filter.TotalPages = (int)Math.Ceiling(
                filter.TotalCount /
                (double)filter.PageSize);

            // Make sure page number is at least 1
            if (filter.PageNumber < 1)
            {
                filter.PageNumber = 1;
            }

            // ========================================================
            // 9. GET PRODUCTS FOR CURRENT PAGE
            // ========================================================

            filter.Products = await query
                .Skip(
                    (filter.PageNumber - 1) *
                    filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync();

            // ========================================================
            // 10. LOAD ACTIVE CATEGORIES
            // ========================================================

            filter.Categories = await _context.Categories
                .Where(c => c.IsActive)
                .OrderBy(c => c.Name)
                .ToListAsync();

            // ========================================================
            // 11. LOAD ACTIVE BRANDS
            // ========================================================

            filter.Brands = await _context.Brands
                .Where(b => b.IsActive)
                .OrderBy(b => b.Name)
                .ToListAsync();

            return filter;
        }

        // ============================================================
        // GET PRODUCT BY ID
        // ============================================================

        public async Task<Product?> GetProductByIdAsync(int id)
        {
            return await _context.Products
                .Include(p => p.Brand)
                .Include(p => p.Category)
                .Include(p => p.ProductImages)
                .Include(p => p.Specifications)
                .Include(p => p.Reviews.Where(r => r.IsApproved))
                    .ThenInclude(r => r.User)
                .FirstOrDefaultAsync(p =>
                    p.Id == id &&
                    p.IsActive);

        }

        public async Task<bool> AddReviewAsync(int productId, string userId, ReviewViewModel review)
        {
            var productExists = await _context.Products
                .AnyAsync(p => p.Id == productId && p.IsActive);

            if (!productExists)
            {
                return false;
            }

            var isVerifiedPurchase = await _context.OrderItems
                .AnyAsync(item =>
                    item.ProductId == productId &&
                    item.Order.UserId == userId &&
                    item.Order.Status == OrderStatus.Delivered &&
                    item.Order.PaymentStatus == PaymentStatus.Paid);

            var newReview = new Review
            {
                ProductId = productId,
                UserId = userId,
                Rating = review.Rating,
                Title = review.Title,
                Comment = review.Comment,
                IsVerifiedPurchase = isVerifiedPurchase,
                IsApproved = true
            };

            _context.Reviews.Add(newReview);
            await _context.SaveChangesAsync();
            return true;
        }

        // ============================================================
        // GET RELATED PRODUCTS
        // ============================================================

        public async Task<List<Product>> GetRelatedProductsAsync(
            int productId,
            int count = 4)
        {
            // Find the current product
            var product = await _context.Products
                .FirstOrDefaultAsync(p => p.Id == productId);

            // Product doesn't exist
            if (product == null)
            {
                return new List<Product>();
            }

            // Find products with same brand OR same category
            return await _context.Products
                .Include(p => p.Brand)
                .Include(p => p.Category)
                .Include(p => p.Reviews)
                .Where(p =>
                    p.Id != productId &&
                    (
                        p.BrandId == product.BrandId ||
                        p.CategoryId == product.CategoryId
                    ) &&
                    p.IsActive)
                .OrderByDescending(p => p.CreatedAt)
                .Take(count)
                .ToListAsync();
        }

        // ============================================================
        // GET BRANDS
        // ============================================================

        public async Task<List<Brand>> GetBrandsAsync(
            int count = 6)
        {
            return await _context.Brands
                .Where(b => b.IsActive)
                .OrderBy(b => b.Name)
                .Take(count)
                .ToListAsync();
        }
    }
}