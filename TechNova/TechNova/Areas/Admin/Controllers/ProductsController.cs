
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ClosedXML.Excel;
using System.Globalization;
using System.Text;
using TechNova.Data;
using TechNova.Models;
using TechNova.Services;

namespace TechNova.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Policy = "AdminOnly")]
public class ProductsController : Controller
{
    private const int PageSize = 20;

    private readonly ApplicationDbContext _context;
    private readonly ILogger<ProductsController> _logger;
    private readonly FileService _fileService;

    public ProductsController(
        ApplicationDbContext context,
        ILogger<ProductsController> logger,
        FileService fileService)
    {
        _context = context;
        _logger = logger;
        _fileService = fileService;
    }

    // GET: Admin/Products
    [HttpGet]
    public async Task<IActionResult> Index(
        string? search,
        int? categoryId,
        int? brandId,
        int page = 1)
    {
        page = Math.Max(page, 1);

        var query = _context.Products
            .AsNoTracking()
            .Include(p => p.Category)
            .Include(p => p.Brand)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            search = search.Trim();

            query = query.Where(p =>
                p.Name.Contains(search) ||
                (p.Model != null && p.Model.Contains(search)));
        }

        if (categoryId.HasValue)
            query = query.Where(p => p.CategoryId == categoryId.Value);

        if (brandId.HasValue)
            query = query.Where(p => p.BrandId == brandId.Value);

        var totalItems = await query.CountAsync();

        var totalPages = (int)Math.Ceiling(
            totalItems / (double)PageSize);

        page = totalPages > 0
            ? Math.Min(page, totalPages)
            : 1;

        var products = await query
            .OrderByDescending(p => p.CreatedAt)
            .ThenBy(p => p.Name)
            .Skip((page - 1) * PageSize)
            .Take(PageSize)
            .ToListAsync();

        // Index needs actual lists, not SelectList.
        ViewBag.Categories = await _context.Categories
            .AsNoTracking()
            .OrderBy(c => c.Name)
            .ToListAsync();

        ViewBag.Brands = await _context.Brands
            .AsNoTracking()
            .OrderBy(b => b.Name)
            .ToListAsync();

        ViewBag.CurrentPage = page;
        ViewBag.TotalPages = totalPages;
        ViewBag.Search = search;
        ViewBag.CategoryId = categoryId;
        ViewBag.BrandId = brandId;

        return View(products);
    }

    // GET: Admin/Products/Create
    [HttpGet]
    public async Task<IActionResult> Create()
    {
        await LoadLookupsAsync();

        return View(new Product
        {
            IsActive = true,
            IsFeatured = false,
            IsNewArrival = false,
            IsBestseller = false,
            StockQuantity = 0
        });
    }

    // POST: Admin/Products/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Product product, IFormFile? image)
    {
        // Fix navigation property validation.
        ModelState.Remove("Category");
        ModelState.Remove("Brand");

        if (!await ValidLookupsAsync(
            product.CategoryId,
            product.BrandId))
        {
            ModelState.AddModelError(
                string.Empty,
                "Please select a valid category and brand.");
        }

        if (!ModelState.IsValid)
        {
            await LoadLookupsAsync(
                product.CategoryId,
                product.BrandId);

            return View(product);
        }

        try
        {
            // Handle image upload
            if (image != null && image.Length > 0)
            {
                string imageUrl = await _fileService.SaveFileAsync(
                    image,
                    "images/products");

                product.MainImageUrl = imageUrl;
            }

            product.CreatedAt = DateTime.Now;

            _context.Products.Add(product);

            await _context.SaveChangesAsync();

            TempData["Success"] = "Product added successfully!";

            return RedirectToAction(nameof(Index));
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Product creation failed.");

            ModelState.AddModelError(
                string.Empty,
                "Database error. Product could not be added. Check the database fields and category/brand relationships.");

            await LoadLookupsAsync(
                product.CategoryId,
                product.BrandId);

            return View(product);
        }
    }

    // GET: Admin/Products/Edit/17
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        // Eagerly load relational images and specifications lists to populate the editing dashboard view tabs
        var product = await _context.Products
            .Include(p => p.ProductImages)
            .Include(p => p.Specifications)
            .FirstOrDefaultAsync(p => p.Id == id);

        // Return a 404 status error if the target tracking identifier is invalid or missing
        if (product == null)
            return NotFound();

        await LoadLookupsAsync(
            product.CategoryId,
            product.BrandId);

        return View(product);
    }

    // GET: Admin/Products/Details/17
    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        // Eagerly load all related data for display
        var product = await _context.Products
            .Include(p => p.Category)
            .Include(p => p.Brand)
            .Include(p => p.ProductImages)
            .Include(p => p.Specifications)
            .Include(p => p.Reviews)
                .ThenInclude(r => r.User)
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id);

        if (product == null)
            return NotFound();

        return View(product);
    }

    // POST: Admin/Products/Edit/17
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Product product, IFormFile? image)
    {
        if (id != product.Id)
            return BadRequest();

        // Fix Category and Brand required validation.
        ModelState.Remove("Category");
        ModelState.Remove("Brand");

        if (!await ValidLookupsAsync(
            product.CategoryId,
            product.BrandId))
        {
            ModelState.AddModelError(
                string.Empty,
                "Please select a valid category and brand.");
        }

        if (!ModelState.IsValid)
        {
            await LoadLookupsAsync(
                product.CategoryId,
                product.BrandId);

            return View(product);
        }

        var existingProduct = await _context.Products
            .FirstOrDefaultAsync(p => p.Id == id);

        if (existingProduct == null)
            return NotFound();

        try
        {
            // Update product information.
            existingProduct.Name = product.Name;
            existingProduct.Model = product.Model;
            existingProduct.ShortDescription = product.ShortDescription;
            existingProduct.Description = product.Description;

            // Update pricing.
            existingProduct.OriginalPrice = product.OriginalPrice;
            existingProduct.SalePrice = product.SalePrice;

            // Update stock.
            existingProduct.StockQuantity = product.StockQuantity;

            // Handle image upload
            if (image != null && image.Length > 0)
            {
                // Delete old image if it exists
                if (!string.IsNullOrWhiteSpace(existingProduct.MainImageUrl))
                {
                    _fileService.DeleteFile(existingProduct.MainImageUrl);
                }

                // Save new image
                string imageUrl = await _fileService.SaveFileAsync(
                    image,
                    "images/products");

                existingProduct.MainImageUrl = imageUrl;
            }
            // If no new image provided, keep the existing image (do nothing)

            // Update category and brand.
            existingProduct.CategoryId = product.CategoryId;
            existingProduct.BrandId = product.BrandId;

            // Update visibility.
            existingProduct.IsActive = product.IsActive;
            existingProduct.IsFeatured = product.IsFeatured;
            existingProduct.IsNewArrival = product.IsNewArrival;
            existingProduct.IsBestseller = product.IsBestseller;

            existingProduct.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();

            TempData["Success"] = "Product updated successfully!";

            return RedirectToAction(nameof(Index));
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(
                ex,
                "Product update failed for ID {ProductId}.",
                id);

            ModelState.AddModelError(
                string.Empty,
                "Database error. Product changes could not be saved.");

            await LoadLookupsAsync(
                product.CategoryId,
                product.BrandId);

            return View(product);
        }
    }

    // POST: Admin/Products/Delete/17
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var product = await _context.Products.FindAsync(id);

        if (product == null)
            return NotFound();

        try
        {
            // Soft delete: retain the record for order history and relationships.
            product.IsActive = false;

            await _context.SaveChangesAsync();

            TempData["Success"] = "Product deleted successfully.";
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Product soft deletion failed for ID {ProductId}.", id);

            TempData["Error"] =
                "Product could not be deleted.";
        }

        return RedirectToAction(nameof(Index));
    }

    // GET: Admin/Products/Export
    [HttpGet]
    public async Task<IActionResult> Export(
        string? search,
        int? categoryId,
        int? brandId)
    {
        var query = _context.Products
            .AsNoTracking()
            .Include(p => p.Category)
            .Include(p => p.Brand)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            search = search.Trim();

            query = query.Where(p =>
                p.Name.Contains(search) ||
                (p.Model != null && p.Model.Contains(search)));
        }

        if (categoryId.HasValue)
            query = query.Where(p => p.CategoryId == categoryId.Value);

        if (brandId.HasValue)
            query = query.Where(p => p.BrandId == brandId.Value);

        var products = await query
            .OrderBy(p => p.Name)
            .ToListAsync();

        var csv = new StringBuilder();

        csv.AppendLine(
            "Name,Model,Brand,Category,Original Price,Sale Price,Stock,Active,Featured");

        foreach (var p in products)
        {
            csv.AppendLine(string.Join(",",
                Csv(p.Name),
                Csv(p.Model),
                Csv(p.Brand?.Name),
                Csv(p.Category?.Name),
                p.OriginalPrice.ToString("0.00"),
                p.SalePrice.ToString("0.00"),
                p.StockQuantity,
                p.IsActive,
                p.IsFeatured));
        }

        return File(
            Encoding.UTF8.GetPreamble()
                .Concat(Encoding.UTF8.GetBytes(csv.ToString()))
                .ToArray(),
            "text/csv",
            $"technova-products-{DateTime.Now:yyyyMMdd}.csv");
    }

    // GET: Admin/Products/ExportToExcel
    [HttpGet]
    public async Task<IActionResult> ExportToExcel(
        string? search,
        int? categoryId,
        int? brandId)
    {
        var query = _context.Products
            .AsNoTracking()
            .Include(p => p.Category)
            .Include(p => p.Brand)
            .Where(p => p.IsActive)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            search = search.Trim();
            query = query.Where(p =>
                p.Name.Contains(search) ||
                (p.Model != null && p.Model.Contains(search)));
        }

        if (categoryId.HasValue)
            query = query.Where(p => p.CategoryId == categoryId.Value);

        if (brandId.HasValue)
            query = query.Where(p => p.BrandId == brandId.Value);

        var products = await query
            .OrderBy(p => p.Name)
            .ToListAsync();

        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Products");
        var headers = new[]
        {
            "ID", "Name", "Model", "Category", "Brand", "Original Price",
            "Sale Price", "Stock", "Featured", "Active"
        };

        for (var column = 0; column < headers.Length; column++)
            worksheet.Cell(1, column + 1).Value = headers[column];

        worksheet.Range(1, 1, 1, headers.Length).Style.Font.Bold = true;

        for (var index = 0; index < products.Count; index++)
        {
            var product = products[index];
            var row = index + 2;

            worksheet.Cell(row, 1).Value = product.Id;
            worksheet.Cell(row, 2).Value = product.Name;
            worksheet.Cell(row, 3).Value = product.Model ?? string.Empty;
            worksheet.Cell(row, 4).Value = product.Category?.Name ?? string.Empty;
            worksheet.Cell(row, 5).Value = product.Brand?.Name ?? string.Empty;
            worksheet.Cell(row, 6).Value = product.OriginalPrice;
            worksheet.Cell(row, 7).Value = product.SalePrice;
            worksheet.Cell(row, 8).Value = product.StockQuantity;
            worksheet.Cell(row, 9).Value = product.IsFeatured ? "Yes" : "No";
            worksheet.Cell(row, 10).Value = product.IsActive ? "Yes" : "No";
        }

        worksheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);

        return File(
            stream.ToArray(),
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"technova-products-{DateTime.Now:yyyyMMdd}.xlsx");
    }

    // POST: Admin/Products/ImportFromExcel
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ImportFromExcel(IFormFile? file)
    {
        if (file == null || file.Length == 0)
        {
            TempData["Error"] = "Please select an Excel file.";
            return RedirectToAction(nameof(Index));
        }

        if (!string.Equals(Path.GetExtension(file.FileName), ".xlsx", StringComparison.OrdinalIgnoreCase))
        {
            TempData["Error"] = "Please upload an .xlsx Excel file.";
            return RedirectToAction(nameof(Index));
        }

        try
        {
            using var stream = new MemoryStream();
            await file.CopyToAsync(stream);
            stream.Position = 0;

            using var workbook = new XLWorkbook(stream);
            var worksheet = workbook.Worksheet(1);
            var categories = await _context.Categories
                .AsNoTracking()
                .ToDictionaryAsync(c => c.Name, StringComparer.OrdinalIgnoreCase);
            var brands = await _context.Brands
                .AsNoTracking()
                .ToDictionaryAsync(b => b.Name, StringComparer.OrdinalIgnoreCase);
            var existingKeys = (await _context.Products
                .AsNoTracking()
                .Select(p => new { p.CategoryId, p.BrandId, p.Name, p.Model })
                .ToListAsync())
                .Select(p => ProductImportKey.Create(p.CategoryId, p.BrandId, p.Name, p.Model))
                .ToHashSet();

            var imported = 0;
            var skipped = 0;

            foreach (var row in worksheet.RowsUsed().Skip(1))
            {
                var name = row.Cell(2).GetString().Trim();
                var model = row.Cell(3).GetString().Trim();
                var categoryName = row.Cell(4).GetString().Trim();
                var brandName = row.Cell(5).GetString().Trim();

                if (string.IsNullOrWhiteSpace(name) ||
                    !categories.TryGetValue(categoryName, out var category) ||
                    !brands.TryGetValue(brandName, out var brand) ||
                    !decimal.TryParse(row.Cell(6).GetString(), NumberStyles.Number, CultureInfo.InvariantCulture, out var originalPrice) ||
                    !decimal.TryParse(row.Cell(7).GetString(), NumberStyles.Number, CultureInfo.InvariantCulture, out var salePrice) ||
                    !int.TryParse(row.Cell(8).GetString(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var stockQuantity))
                {
                    skipped++;
                    continue;
                }

                var key = ProductImportKey.Create(category.Id, brand.Id, name, model);
                if (!existingKeys.Add(key))
                {
                    skipped++;
                    continue;
                }

                _context.Products.Add(new Product
                {
                    Name = name,
                    Model = string.IsNullOrWhiteSpace(model) ? null : model,
                    CategoryId = category.Id,
                    BrandId = brand.Id,
                    OriginalPrice = originalPrice,
                    SalePrice = salePrice,
                    StockQuantity = stockQuantity,
                    IsFeatured = IsYes(row.Cell(9).GetString()),
                    IsActive = true
                });
                imported++;
            }

            await _context.SaveChangesAsync();
            TempData["Success"] = $"{imported} product(s) imported successfully." +
                (skipped > 0 ? $" {skipped} row(s) skipped." : string.Empty);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Product Excel import failed.");
            TempData["Error"] = "Error importing products. Check that the workbook uses the exported format.";
        }

        return RedirectToAction(nameof(Index));
    }

    private static bool IsYes(string value) =>
        value.Equals("yes", StringComparison.OrdinalIgnoreCase) ||
        value.Equals("true", StringComparison.OrdinalIgnoreCase);

    private readonly record struct ProductImportKey(int CategoryId, int BrandId, string Name, string Model)
    {
        public static ProductImportKey Create(int categoryId, int brandId, string? name, string? model) =>
            new(categoryId, brandId, name?.Trim() ?? string.Empty, model?.Trim() ?? string.Empty);
    }

    // Load category and brand dropdowns for forms.
    private async Task LoadLookupsAsync(
        int? selectedCategoryId = null,
        int? selectedBrandId = null)
    {
        var categories = await _context.Categories
            .AsNoTracking()
            .OrderBy(c => c.Name)
            .ToListAsync();

        var brands = await _context.Brands
            .AsNoTracking()
            .OrderBy(b => b.Name)
            .ToListAsync();

        ViewBag.Categories = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(
            categories,
            "Id",
            "Name",
            selectedCategoryId);

        ViewBag.Brands = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(
            brands,
            "Id",
            "Name",
            selectedBrandId);
    }

    // Validate database foreign keys.
    private async Task<bool> ValidLookupsAsync(
        int categoryId,
        int brandId)
    {
        var categoryExists = await _context.Categories
            .AnyAsync(c => c.Id == categoryId);

        var brandExists = await _context.Brands
            .AnyAsync(b => b.Id == brandId);

        return categoryExists && brandExists;
    }

    // CSV formatting.
    private static string Csv(string? value)
    {
        var escaped = (value ?? string.Empty)
            .Replace("\"", "\"\"");

        return $"\"{escaped}\"";
    }

    // GET: Admin/Products/GetLatestProductId
    [HttpGet]
    public async Task<IActionResult> GetLatestProductId()
    {
        // Fetch only the ID of the most recently inserted product to minimize database transmission payload
        var latest = await _context.Products
            .OrderByDescending(p => p.Id)
            .Select(p => new { id = p.Id })
            .FirstOrDefaultAsync();

        // Return the result as a JSON object, defaulting to an ID of 0 if the table is currently empty
        return Json(latest ?? new { id = 0 });
    }

    // POST: Admin/Products/AddSpecification
    [HttpPost]
    public async Task<IActionResult> AddSpecification(int productId, string name, string value, string? groupName)
    {
        // Initialize a new specification entity mapping technical attributes
        var spec = new ProductSpecification
        {
            ProductId = productId,
            Name = name,
            Value = value,
            GroupName = groupName // Optional parameter enabling dynamic categorization groupings
        };

        _context.ProductSpecifications.Add(spec);
        await _context.SaveChangesAsync(); // Persists and generates the database-level spec.Id

        // Return an anonymous JSON payload indicating successful execution state alongside the record identity
        return Json(new { success = true, id = spec.Id });
    }

    // POST: Admin/Products/RemoveSpecification/5
    [HttpPost]
    public async Task<IActionResult> RemoveSpecification(int id)
    {
        // Locate the matching technical requirement entry inside the tracking context asynchronously
        var spec = await _context.ProductSpecifications.FindAsync(id);

        // Safely verify existence to prevent targeting exceptions during state removal phases
        if (spec != null)
        {
            _context.ProductSpecifications.Remove(spec);
            await _context.SaveChangesAsync(); // Commit structural data modifications directly to the database
        }

        // Return a lightweight status verification result payload to confirm data pipeline clearance
        return Json(new { success = true });
    }

    // Helper method used to verify product existence within the master context
    private bool ProductExists(int id)
    {
        return _context.Products.Any(e => e.Id == id);
    }
}
