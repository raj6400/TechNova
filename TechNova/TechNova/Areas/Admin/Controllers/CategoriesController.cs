using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechNova.Data;
using TechNova.Models;

namespace TechNova.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Policy = "AdminOnly")]
public class CategoriesController : Controller
{
    private readonly ApplicationDbContext _context;

    public CategoriesController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: Admin/Categories
    public async Task<IActionResult> Index()
    {
        var categories = await _context.Categories
            .AsNoTracking()
            .Include(category => category.Products)
            .OrderBy(category => category.DisplayOrder)
            .ThenBy(category => category.Name)
            .ToListAsync();

        return View(categories);
    }

    // GET: Admin/Categories/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: Admin/Categories/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        [Bind("Name,Description,ImageUrl,DisplayOrder,IsActive")] Category category)
    {
        if (!ModelState.IsValid)
        {
            return View(category);
        }

        category.CreatedAt = DateTime.Now;
        _context.Categories.Add(category);
        await _context.SaveChangesAsync();

        TempData["Success"] = "Category created successfully.";
        return RedirectToAction(nameof(Index));
    }

    // GET: Admin/Categories/Edit/5
    public async Task<IActionResult> Edit(int id)
    {
        var category = await _context.Categories.FindAsync(id);
        return category is null ? NotFound() : View(category);
    }

    // POST: Admin/Categories/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        int id,
        [Bind("Id,Name,Description,ImageUrl,DisplayOrder,IsActive")] Category submittedCategory)
    {
        if (id != submittedCategory.Id)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            return View(submittedCategory);
        }

        var category = await _context.Categories.FindAsync(id);
        if (category is null)
        {
            return NotFound();
        }

        category.Name = submittedCategory.Name;
        category.Description = submittedCategory.Description;
        category.ImageUrl = submittedCategory.ImageUrl;
        category.DisplayOrder = submittedCategory.DisplayOrder;
        category.IsActive = submittedCategory.IsActive;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await CategoryExistsAsync(id))
            {
                return NotFound();
            }

            throw;
        }

        TempData["Success"] = "Category updated successfully.";
        return RedirectToAction(nameof(Index));
    }

    // POST: Admin/Categories/Delete/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var category = await _context.Categories.FindAsync(id);
        if (category is null)
        {
            return NotFound();
        }

        // Soft delete: preserve product relationships and historical data.
        category.IsActive = false;
        await _context.SaveChangesAsync();

        TempData["Success"] = "Category deactivated successfully.";
        return RedirectToAction(nameof(Index));
    }

    private Task<bool> CategoryExistsAsync(int id)
    {
        return _context.Categories.AnyAsync(category => category.Id == id);
    }
}
