using Microsoft.EntityFrameworkCore;
using TechNova.Data;
using TechNova.Models;
using TechNova.ViewModels;

namespace TechNova.Services;

public interface IReportService
{
    Task<DashboardViewModel> GetDashboardDataAsync();
    Task<SalesReportViewModel> GetSalesReportAsync(DateTime startDate, DateTime endDate);
    Task<InventoryReportViewModel> GetInventoryReportAsync();
    Task<List<TopProductViewModel>> GetTopSellingProductsAsync(int count = 10);
}

public class ReportService : IReportService
{
    private readonly ApplicationDbContext _context;

    public ReportService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<DashboardViewModel> GetDashboardDataAsync()
    {
        var now = DateTime.Now;
        var firstMonth = new DateTime(now.Year, now.Month, 1).AddMonths(-5);
        var nextMonth = new DateTime(now.Year, now.Month, 1).AddMonths(1);

        var totalOrders = await _context.Orders.CountAsync();
        var totalProducts = await _context.Products.CountAsync();
        var totalCustomers = await _context.UserRoles
            .Join(_context.Roles, userRole => userRole.RoleId, role => role.Id,
                (userRole, role) => new { userRole.UserId, role.Name })
            .CountAsync(userRole => userRole.Name == "Customer");
        var totalRevenue = await _context.Orders
            .Where(order => order.PaymentStatus == PaymentStatus.Paid)
            .SumAsync(order => (decimal?)order.TotalAmount) ?? 0m;
        var pendingOrders = await _context.Orders
            .CountAsync(order => order.Status == OrderStatus.Pending);
        var lowStockProducts = await _context.Products
            .CountAsync(product => product.StockQuantity <= 10);

        var recentOrders = await _context.Orders
            .OrderByDescending(order => order.OrderDate)
            .Take(10)
            .Select(order => new RecentOrderViewModel
            {
                OrderId = order.Id,
                OrderNumber = order.OrderNumber,
                CustomerName = order.CustomerFirstName + " " + order.CustomerLastName,
                TotalAmount = order.TotalAmount,
                Status = order.Status.ToString(),
                OrderDate = order.OrderDate
            })
            .ToListAsync();

        var topProducts = await _context.OrderItems
            .GroupBy(item => new { item.ProductId, item.Product.Name })
            .Select(group => new TopProductViewModel
            {
                ProductId = group.Key.ProductId,
                ProductName = group.Key.Name,
                UnitsSold = group.Sum(item => item.Quantity),
                Revenue = group.Sum(item => (item.UnitPrice * item.Quantity) - item.Discount)
            })
            .OrderByDescending(product => product.Revenue)
            .Take(5)
            .ToListAsync();

        var monthlyTotals = await _context.Orders
            .Where(order => order.OrderDate >= firstMonth && order.OrderDate < nextMonth)
            .GroupBy(order => new { order.OrderDate.Year, order.OrderDate.Month })
            .Select(group => new
            {
                group.Key.Year,
                group.Key.Month,
                Sales = group.Sum(order => order.PaymentStatus == PaymentStatus.Paid ? order.TotalAmount : 0m),
                Orders = group.Count()
            })
            .ToListAsync();

        var monthlySales = Enumerable.Range(0, 6)
            .Select(offset => firstMonth.AddMonths(offset))
            .Select(month =>
            {
                var totals = monthlyTotals.FirstOrDefault(total => total.Year == month.Year && total.Month == month.Month);
                return new MonthlySalesViewModel
                {
                    Month = month.ToString("MMM yyyy"),
                    Sales = totals?.Sales ?? 0m,
                    Orders = totals?.Orders ?? 0
                };
            })
            .ToList();

        return new DashboardViewModel
        {
            TotalOrders = totalOrders,
            TotalProducts = totalProducts,
            TotalCustomers = totalCustomers,
            TotalRevenue = totalRevenue,
            PendingOrders = pendingOrders,
            LowStockProducts = lowStockProducts,
            RecentOrders = recentOrders,
            TopProducts = topProducts,
            MonthlySales = monthlySales
        };
    }

    public async Task<SalesReportViewModel> GetSalesReportAsync(DateTime startDate, DateTime endDate)
    {
        var start = startDate.Date;
        var endExclusive = endDate.Date.AddDays(1);

        var orders = await _context.Orders
            .AsNoTracking()
            .Where(order => order.OrderDate >= start && order.OrderDate < endExclusive)
            .ToListAsync();

        var dailySales = orders
            .GroupBy(order => order.OrderDate.Date)
            .Select(group => new DailySalesViewModel
            {
                Date = group.Key,
                Sales = group.Where(order => order.PaymentStatus == PaymentStatus.Paid)
                    .Sum(order => order.TotalAmount),
                OrderCount = group.Count()
            })
            .OrderBy(day => day.Date)
            .ToList();

        var totalSales = orders
            .Where(order => order.PaymentStatus == PaymentStatus.Paid)
            .Sum(order => order.TotalAmount);

        return new SalesReportViewModel
        {
            StartDate = start,
            EndDate = endDate.Date,
            TotalSales = totalSales,
            TotalOrders = orders.Count,
            AverageOrderValue = orders.Count == 0 ? 0m : totalSales / orders.Count,
            DailySales = dailySales
        };
    }

    public async Task<InventoryReportViewModel> GetInventoryReportAsync()
    {
        var lowStockProducts = await _context.Products
            .AsNoTracking()
            .Where(product => product.StockQuantity > 0 && product.StockQuantity <= 10)
            .OrderBy(product => product.StockQuantity)
            .ThenBy(product => product.Name)
            .Select(product => new LowStockProductViewModel
            {
                ProductId = product.Id,
                ProductName = product.Name,
                CurrentStock = product.StockQuantity
            })
            .ToListAsync();

        return new InventoryReportViewModel
        {
            TotalProducts = await _context.Products.CountAsync(),
            LowStockCount = lowStockProducts.Count,
            OutOfStockCount = await _context.Products.CountAsync(product => product.StockQuantity == 0),
            LowStockProducts = lowStockProducts
        };
    }

    public async Task<List<TopProductViewModel>> GetTopSellingProductsAsync(int count = 10)
    {
        if (count <= 0)
        {
            return [];
        }

        return await _context.OrderItems
            .AsNoTracking()
            .Where(item => item.Order.PaymentStatus == PaymentStatus.Paid)
            .GroupBy(item => new { item.ProductId, item.Product.Name })
            .Select(group => new TopProductViewModel
            {
                ProductId = group.Key.ProductId,
                ProductName = group.Key.Name,
                UnitsSold = group.Sum(item => item.Quantity),
                Revenue = group.Sum(item => (item.UnitPrice * item.Quantity) - item.Discount)
            })
            .OrderByDescending(product => product.UnitsSold)
            .ThenByDescending(product => product.Revenue)
            .Take(count)
            .ToListAsync();
    }
}

public class SalesReportViewModel
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal TotalSales { get; set; }
    public int TotalOrders { get; set; }
    public decimal AverageOrderValue { get; set; }
    public List<DailySalesViewModel> DailySales { get; set; } = [];
}

public class DailySalesViewModel
{
    public DateTime Date { get; set; }
    public decimal Sales { get; set; }
    public int OrderCount { get; set; }
}

public class InventoryReportViewModel
{
    public int TotalProducts { get; set; }
    public int LowStockCount { get; set; }
    public int OutOfStockCount { get; set; }
    public List<LowStockProductViewModel> LowStockProducts { get; set; } = [];
}

public class LowStockProductViewModel
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int CurrentStock { get; set; }
    public int Threshold { get; set; } = 10;
}
