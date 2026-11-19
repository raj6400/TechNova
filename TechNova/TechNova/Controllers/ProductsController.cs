
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using TechNova.Data;
using TechNova.Models;
using TechNova.Services;
using TechNova.ViewModels;
using System.Threading.Tasks;

namespace TechNova.Controllers
{
    public class ProductsController : Controller
    {
        private readonly IProductService _productService;
        private readonly IShoppingCartService _cartService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;

        // Constructor Injection
        public ProductsController(
            IProductService productService,
            IShoppingCartService cartService,
            UserManager<ApplicationUser> userManager,
            ApplicationDbContext context)
        {
            _productService = productService;
            _cartService = cartService;
            _userManager = userManager;
            _context = context;
        }

        // Display Products
        public async Task<IActionResult> Index(
            int? categoryId,
            int? brandId,
            string? search,
            string? sort,
            decimal? minPrice,
            decimal? maxPrice,
            int page = 1)
        {
            var filter = new ProductListViewModel
            {
                SelectedCategoryId = categoryId,
                SelectedBrandId = brandId,
                SearchTerm = search,
                SortOrder = sort,
                MinPrice = minPrice,
                MaxPrice = maxPrice,
                PageNumber = page
            };

            // Get filtered products
            var result = await _productService.GetProductsAsync(filter);

            // Get cart item count
            ViewBag.CartItemCount =
                await _cartService.GetCartItemCountAsync();

            return View(result);
        }

        public async Task<IActionResult> Search(string term)
        {
            if (string.IsNullOrWhiteSpace(term) || term.Length < 2)
                return Json(new List<object>());

            var filter = new ProductListViewModel
            {
                SearchTerm = term,
                PageSize = 5
            };

            var result = await _productService.GetProductsAsync(filter);
            var suggestions = result.Products.Select(p => new
            {
                id = p.Id,
                name = p.Name,
                price = p.SalePrice,
                image = p.MainImageUrl,
                brand = p.Brand?.Name
            });

            return Json(suggestions);
        }

        // Display Product Details
        public async Task<IActionResult> Details(int id)
        {
            // Get product
            var product =
                await _productService.GetProductByIdAsync(id);

            if (product == null)
            {
                return NotFound();
            }

            // Get related products
            var relatedProducts =
                await _productService.GetRelatedProductsAsync(id);

            var user = await _userManager.GetUserAsync(User);
            var isInWishlist = user != null && await _context.WishlistItems
                .AnyAsync(item => item.UserId == user.Id && item.ProductId == id);

            // Create ViewModel
            var viewModel = new ProductDetailViewModel
            {
                Product = product,
                RelatedProducts = relatedProducts,
                IsInWishlist = isInWishlist
            };

            // Get cart item count
            ViewBag.CartItemCount =
                await _cartService.GetCartItemCountAsync();

            return View(viewModel);
        }

        public async Task<IActionResult> Compare(int[]? ids)
        {
            if (ids == null || ids.Distinct().Count() < 2)
            {
                TempData["Error"] = "Please select at least 2 products to compare.";
                return RedirectToAction(nameof(Index));
            }

            var products = new List<Product>();

            foreach (var id in ids.Distinct().Take(4))
            {
                var product = await _productService.GetProductByIdAsync(id);
                if (product != null)
                {
                    products.Add(product);
                }
            }

            if (products.Count < 2)
            {
                TempData["Error"] = "Compare requires minimum 2 valid products.";
                return RedirectToAction(nameof(Index));
            }

            var viewModel = new CompareProductsViewModel
            {
                Products = products,
                SpecificationNames = products
                    .SelectMany(product => product.Specifications)
                    .Select(specification => specification.Name)
                    .Distinct()
                    .ToList()
            };

            ViewBag.CartItemCount = await _cartService.GetCartItemCountAsync();
            return View(viewModel);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddReview(int productId, ReviewViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Please fill in all required fields.";
                return RedirectToAction(nameof(Details), new { id = productId });
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }

            var result = await _productService.AddReviewAsync(productId, user.Id, model);
            if (result)
            {
                TempData["Success"] = "Thank you for your review!";
            }
            else
            {
                TempData["Error"] = "Failed to add review. Please try again.";
            }

            return RedirectToAction(nameof(Details), new { id = productId });
        }
    }
}
