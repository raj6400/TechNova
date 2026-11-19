using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechNova.Data;
using TechNova.Services;
using TechNova.ViewModels;

namespace TechNova.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Policy = "AdminOnly")]
public class ManagementController : Controller
{
    private const int PageLimit = 100;
    private readonly ApplicationDbContext _context;
    private readonly IReportService _reportService;

    public ManagementController(ApplicationDbContext context, IReportService reportService)
    {
        _context = context;
        _reportService = reportService;
    }

    public async Task<IActionResult> Products()
    {
        return RedirectToAction("Index", "Products", new { area = "Admin" });
    }

    public async Task<IActionResult> Categories()
    {
        var totalCount = await _context.Categories.CountAsync();
        var categories = await _context.Categories
            .AsNoTracking()
            .OrderBy(category => category.DisplayOrder)
            .Take(PageLimit)
            .Select(category => new
            {
                category.Name,
                category.Description,
                ProductCount = category.Products.Count(),
                category.IsActive
            })
            .ToListAsync();

        return View("Index", CreateList(
            "Categories", "Product categories and the items assigned to them", totalCount,
            ["Category", "Description", "Products", "Status"],
            categories.Select(category => new AdminTableRowViewModel
            {
                Cells = [category.Name, category.Description ?? "—", category.ProductCount.ToString(), GetStatus(category.IsActive)]
            })));
    }

    public async Task<IActionResult> Brands()
    {
        var totalCount = await _context.Brands.CountAsync();
        var brands = await _context.Brands
            .AsNoTracking()
            .OrderBy(brand => brand.Name)
            .Take(PageLimit)
            .Select(brand => new
            {
                brand.Name,
                Country = brand.Country ?? "—",
                ProductCount = brand.Products.Count(),
                brand.IsActive
            })
            .ToListAsync();

        return View("Index", CreateList(
            "Brands", "Brands represented in the store catalog", totalCount,
            ["Brand", "Country", "Products", "Status"],
            brands.Select(brand => new AdminTableRowViewModel
            {
                Cells = [brand.Name, brand.Country, brand.ProductCount.ToString(), GetStatus(brand.IsActive)]
            })));
    }

    public async Task<IActionResult> Orders()
    {
        var totalCount = await _context.Orders.CountAsync();
        var orders = await _context.Orders
            .AsNoTracking()
            .OrderByDescending(order => order.OrderDate)
            .Take(PageLimit)
            .ToListAsync();

        return View("Index", CreateList(
            "Orders", "Recent customer orders and payment status", totalCount,
            ["Order", "Customer", "Email", "Order status", "Payment", "Amount", "Date"],
            orders.Select(order => new AdminTableRowViewModel
            {
                Cells = [order.OrderNumber, $"{order.CustomerFirstName} {order.CustomerLastName}",
                    order.CustomerEmail, order.Status.ToString(), order.PaymentStatus.ToString(),
                    $"Rs {order.TotalAmount:N0}", order.OrderDate.ToString("MMM dd, yyyy")]
            })));
    }

    public async Task<IActionResult> Users()
    {
        var totalCount = await _context.Users.CountAsync();
        var users = await _context.Users
            .AsNoTracking()
            .OrderByDescending(user => user.CreatedAt)
            .Take(PageLimit)
            .Select(user => new
            {
                user.FirstName,
                user.LastName,
                user.Email,
                user.PhoneNumber,
                user.IsActive,
                user.CreatedAt
            })
            .ToListAsync();

        return View("Index", CreateList(
            "Users", "Recently registered customer and administrator accounts", totalCount,
            ["Name", "Email", "Phone", "Status", "Joined"],
            users.Select(user => new AdminTableRowViewModel
            {
                Cells = [$"{user.FirstName} {user.LastName}", user.Email ?? "—",
                    user.PhoneNumber ?? "—", GetStatus(user.IsActive), user.CreatedAt.ToString("MMM dd, yyyy")]
            })));
    }

    public async Task<IActionResult> Reports()
    {
        return View(await _reportService.GetDashboardDataAsync());
    }

    private static AdminListViewModel CreateList(
        string title,
        string description,
        int totalCount,
        List<string> headers,
        IEnumerable<AdminTableRowViewModel> rows)
    {
        return new AdminListViewModel
        {
            Title = title,
            Description = description,
            TotalCount = totalCount,
            Headers = headers,
            Rows = rows.ToList()
        };
    }

    private static string GetStatus(bool isActive) => isActive ? "Active" : "Inactive";
}
