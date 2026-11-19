using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechNova.Data;
using TechNova.Models;

namespace TechNova.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Policy = "AdminOnly")]
public class BrandsController : Controller
{
    private readonly ApplicationDbContext _context;

    public BrandsController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var brands = await _context.Brands
            .AsNoTracking()
            .Include(brand => brand.Products)
            .OrderBy(brand => brand.Name)
            .ToListAsync();

        return View(brands);
    }

    public IActionResult Create()
    {
        return View(new Brand { IsActive = true });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        [Bind("Name,Description,LogoUrl,Country,WebsiteUrl,IsActive")] Brand brand)
    {
        if (!ModelState.IsValid)
        {
            return View(brand);
        }

        brand.CreatedAt = DateTime.Now;
        _context.Brands.Add(brand);
        await _context.SaveChangesAsync();

        TempData["Success"] = "Brand created successfully.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var brand = await _context.Brands.FindAsync(id);
        return brand is null ? NotFound() : View(brand);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        int id,
        [Bind("Id,Name,Description,LogoUrl,Country,WebsiteUrl,IsActive")] Brand submittedBrand)
    {
        if (id != submittedBrand.Id)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            return View(submittedBrand);
        }

        var brand = await _context.Brands.FindAsync(id);
        if (brand is null)
        {
            return NotFound();
        }

        brand.Name = submittedBrand.Name;
        brand.Description = submittedBrand.Description;
        brand.LogoUrl = submittedBrand.LogoUrl;
        brand.Country = submittedBrand.Country;
        brand.WebsiteUrl = submittedBrand.WebsiteUrl;
        brand.IsActive = submittedBrand.IsActive;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await BrandExistsAsync(id))
            {
                return NotFound();
            }

            throw;
        }

        TempData["Success"] = "Brand updated successfully.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var brand = await _context.Brands.FindAsync(id);
        if (brand is null)
        {
            return NotFound();
        }

        brand.IsActive = false;
        await _context.SaveChangesAsync();

        TempData["Success"] = "Brand deactivated successfully.";
        return RedirectToAction(nameof(Index));
    }

    private Task<bool> BrandExistsAsync(int id)
    {
        return _context.Brands.AnyAsync(brand => brand.Id == id);
    }
}
