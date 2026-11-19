using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechNova.Data;
using TechNova.Models;
using TechNova.Services;

namespace TechNova.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Policy = "AdminOnly")]
public class SlidersController : Controller
{
    private const int MaxSliders = 5;
    private const long MaxFileSize = 5 * 1024 * 1024; // 5 MB
    private const string UploadFolder = "images/sliders";
    private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".webp", ".gif" };

    private readonly ApplicationDbContext _context;
    private readonly FileService _fileService;

    public SlidersController(ApplicationDbContext context, FileService fileService)
    {
        _context = context;
        _fileService = fileService;
    }

    // LIST
    public async Task<IActionResult> Index()
    {
        var sliders = await _context.Sliders
            .AsNoTracking()
            .OrderBy(s => s.DisplayOrder)
            .ThenBy(s => s.Id)
            .ToListAsync();

        ViewBag.MaxSliders = MaxSliders;
        return View(sliders);
    }

    // CREATE
    public async Task<IActionResult> Create()
    {
        if (await _context.Sliders.CountAsync() >= MaxSliders)
        {
            TempData["Error"] = $"Maximum {MaxSliders} sliders allowed. Delete one to add a new slider.";
            return RedirectToAction(nameof(Index));
        }

        var nextOrder = (await _context.Sliders.MaxAsync(s => (int?)s.DisplayOrder) ?? 0) + 1;
        return View(new Slider { IsActive = true, DisplayOrder = nextOrder });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        [Bind("Title,SubTitle,ButtonText,ButtonLink,DisplayOrder,IsActive,ImageFile")] Slider slider)
    {
        ModelState.Remove(nameof(Slider.ImagePath));

        if (await _context.Sliders.CountAsync() >= MaxSliders)
        {
            TempData["Error"] = $"Maximum {MaxSliders} sliders allowed.";
            return RedirectToAction(nameof(Index));
        }

        if (slider.ImageFile == null || slider.ImageFile.Length == 0)
        {
            ModelState.AddModelError(nameof(Slider.ImageFile), "Please select a slider image.");
        }
        else
        {
            ValidateImage(slider.ImageFile);
        }

        if (!ModelState.IsValid)
        {
            return View(slider);
        }

        slider.ImagePath = await _fileService.SaveFileAsync(slider.ImageFile!, UploadFolder);
        slider.CreatedAt = DateTime.Now;

        _context.Sliders.Add(slider);
        await _context.SaveChangesAsync();

        TempData["Success"] = "Slider added successfully.";
        return RedirectToAction(nameof(Index));
    }

    // EDIT
    public async Task<IActionResult> Edit(int id)
    {
        var slider = await _context.Sliders.FindAsync(id);
        return slider is null ? NotFound() : View(slider);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        int id,
        [Bind("Id,Title,SubTitle,ButtonText,ButtonLink,DisplayOrder,IsActive,ImageFile")] Slider submitted)
    {
        if (id != submitted.Id)
        {
            return NotFound();
        }

        ModelState.Remove(nameof(Slider.ImagePath));

        var slider = await _context.Sliders.FindAsync(id);
        if (slider is null)
        {
            return NotFound();
        }

        // Image is optional on edit; validate only if a new one is uploaded
        if (submitted.ImageFile != null && submitted.ImageFile.Length > 0)
        {
            ValidateImage(submitted.ImageFile);
        }

        if (!ModelState.IsValid)
        {
            submitted.ImagePath = slider.ImagePath; // so preview still shows
            return View(submitted);
        }

        if (submitted.ImageFile != null && submitted.ImageFile.Length > 0)
        {
            var newPath = await _fileService.SaveFileAsync(submitted.ImageFile, UploadFolder);
            _fileService.DeleteFile(slider.ImagePath); // remove old image
            slider.ImagePath = newPath;
        }

        slider.Title = submitted.Title;
        slider.SubTitle = submitted.SubTitle;
        slider.ButtonText = submitted.ButtonText;
        slider.ButtonLink = submitted.ButtonLink;
        slider.DisplayOrder = submitted.DisplayOrder;
        slider.IsActive = submitted.IsActive;

        await _context.SaveChangesAsync();

        TempData["Success"] = "Slider updated successfully.";
        return RedirectToAction(nameof(Index));
    }

    // SHOW / HIDE
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleActive(int id)
    {
        var slider = await _context.Sliders.FindAsync(id);
        if (slider is null)
        {
            return NotFound();
        }

        slider.IsActive = !slider.IsActive;
        await _context.SaveChangesAsync();

        TempData["Success"] = slider.IsActive ? "Slider is now visible on Home page." : "Slider hidden from Home page.";
        return RedirectToAction(nameof(Index));
    }

    // DELETE
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var slider = await _context.Sliders.FindAsync(id);
        if (slider is null)
        {
            return NotFound();
        }

        _fileService.DeleteFile(slider.ImagePath);
        _context.Sliders.Remove(slider);
        await _context.SaveChangesAsync();

        TempData["Success"] = "Slider deleted successfully.";
        return RedirectToAction(nameof(Index));
    }

    private void ValidateImage(IFormFile file)
    {
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

        if (!AllowedExtensions.Contains(extension))
        {
            ModelState.AddModelError(nameof(Slider.ImageFile), "Only JPG, PNG, WEBP or GIF images are allowed.");
        }

        if (file.Length > MaxFileSize)
        {
            ModelState.AddModelError(nameof(Slider.ImageFile), "Image size must be less than 5 MB.");
        }
    }
}
