using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TechNova.Services;

namespace TechNova.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Policy = "AdminOnly")]
public class DashboardController : Controller
{
    private readonly IReportService _reportService;

    public DashboardController(IReportService reportService)
    {
        _reportService = reportService;
    }

    public async Task<IActionResult> Index()
    {
        var dashboardData = await _reportService.GetDashboardDataAsync();
        return View(dashboardData);
    }

    public async Task<IActionResult> SalesReport(DateTime? startDate, DateTime? endDate)
    {
        var end = (endDate ?? DateTime.Today).Date;
        var start = (startDate ?? end.AddDays(-30)).Date;

        if (start > end)
        {
            (start, end) = (end, start);
        }

        var report = await _reportService.GetSalesReportAsync(start, end);
        return View(report);
    }

    public async Task<IActionResult> InventoryReport()
    {
        var report = await _reportService.GetInventoryReportAsync();
        return View(report);
    }

    public async Task<IActionResult> TopProducts()
    {
        var products = await _reportService.GetTopSellingProductsAsync(20);
        return View(products);
    }
}
