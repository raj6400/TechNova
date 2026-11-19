using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechNova.Data;
using TechNova.Models;
using TechNova.Services;

namespace TechNova.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Policy = "AdminOnly")]
public class OrdersController : Controller
{
    private const int PageSize = 20;
    private readonly ApplicationDbContext _context;
    private readonly IOrderInvoicePdfService _invoicePdfService;
    private readonly IOrderService _orderService;
    private readonly IEmailSender _emailSender;

    public OrdersController(
        ApplicationDbContext context,
        IOrderInvoicePdfService invoicePdfService,
        IOrderService orderService,
        IEmailSender emailSender)
    {
        _context = context;
        _invoicePdfService = invoicePdfService;
        _orderService = orderService;
        _emailSender = emailSender;
    }

    public async Task<IActionResult> Index(string? status, string? search, int page = 1)
    {
        page = Math.Max(page, 1);
        IQueryable<Order> query = _context.Orders
            .AsNoTracking()
            .Include(order => order.OrderItems);

        if (Enum.TryParse<OrderStatus>(status, true, out var orderStatus) && Enum.IsDefined(orderStatus))
            query = query.Where(order => order.Status == orderStatus);

        if (!string.IsNullOrWhiteSpace(search))
        {
            search = search.Trim();
            query = query.Where(order =>
                order.OrderNumber.Contains(search) ||
                order.CustomerEmail.Contains(search) ||
                order.CustomerFirstName.Contains(search) ||
                order.CustomerLastName.Contains(search));
        }

        var totalItems = await query.CountAsync();
        var totalPages = (int)Math.Ceiling(totalItems / (double)PageSize);
        if (totalPages > 0)
            page = Math.Min(page, totalPages);

        var orders = await query
            .OrderByDescending(order => order.OrderDate)
            .ThenByDescending(order => order.Id)
            .Skip((page - 1) * PageSize)
            .Take(PageSize)
            .ToListAsync();

        ViewBag.Statuses = Enum.GetNames<OrderStatus>();
        ViewBag.CurrentStatus = status;
        ViewBag.Search = search;
        ViewBag.CurrentPage = page;
        ViewBag.TotalPages = totalPages;

        return View(orders);
    }

    public async Task<IActionResult> Details(int id)
    {
        var order = await _context.Orders
            .AsNoTracking()
            .Include(order => order.User)
            .Include(order => order.OrderItems)
                .ThenInclude(item => item.Product)
            .FirstOrDefaultAsync(order => order.Id == id);

        if (order == null)
            return NotFound();

        ViewBag.Statuses = Enum.GetValues<OrderStatus>();
        return View(order);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStatus(int id, OrderStatus status)
    {
        if (!ModelState.IsValid || !Enum.IsDefined(status))
        {
            TempData["Error"] = "Select a valid order status.";
            return RedirectToAction(nameof(Details), new { id });
        }

        var order = await _context.Orders.FindAsync(id);
        if (order == null)
            return NotFound();

        await using var transaction = await _context.Database.BeginTransactionAsync();
        var updated = await _orderService.UpdateOrderStatusAsync(id, status);
        if (!updated)
        {
            await transaction.RollbackAsync();
            TempData["Error"] = "Order status was not changed.";
            return RedirectToAction(nameof(Details), new { id });
        }

        if (status == OrderStatus.Delivered &&
            order.PaymentMethod is PaymentMethod.CashOnDelivery or
                PaymentMethod.UPI or
                PaymentMethod.CreditCard or
                PaymentMethod.DebitCard &&
            order.PaymentStatus != PaymentStatus.Paid)
        {
            order.PaymentStatus = PaymentStatus.Paid;
            await _context.SaveChangesAsync();
        }

        await transaction.CommitAsync();

        TempData["Success"] = $"Order status updated to {status}.";
        if (order.PaymentStatus == PaymentStatus.Paid && status == OrderStatus.Delivered)
            TempData["Success"] += " Payment marked as Paid.";

        if (!string.IsNullOrWhiteSpace(order.UserId))
        {
            var user = await _context.Users.FindAsync(order.UserId);
            if (!string.IsNullOrWhiteSpace(user?.Email))
            {
                try
                {
                    await _emailSender.SendOrderStatusUpdateAsync(user.Email, order.OrderNumber, status.ToString());
                }
                catch
                {
                    TempData["Warning"] = "Order status updated, but the notification email could not be sent.";
                }
            }
        }

        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CancelOrder(int id)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync();
        var order = await _context.Orders
            .Include(order => order.OrderItems)
                .ThenInclude(item => item.Product)
            .FirstOrDefaultAsync(order => order.Id == id);

        if (order == null)
            return NotFound();

        if (order.Status is OrderStatus.Cancelled or OrderStatus.Delivered)
        {
            await transaction.RollbackAsync();
            TempData["Error"] = "This order can no longer be cancelled.";
            return RedirectToAction(nameof(Details), new { id });
        }

        if (!await _orderService.UpdateOrderStatusAsync(id, OrderStatus.Cancelled))
        {
            await transaction.RollbackAsync();
            TempData["Error"] = "Failed to cancel order.";
            return RedirectToAction(nameof(Details), new { id });
        }

        foreach (var item in order.OrderItems)
        {
            if (item.Product != null)
                item.Product.StockQuantity += item.Quantity;
        }

        await _context.SaveChangesAsync();
        await transaction.CommitAsync();

        TempData["Success"] = "Order cancelled successfully.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Invoice(int id)
    {
        var order = await GetOrderForInvoice(id);
        return order == null ? NotFound() : View(order);
    }

    public async Task<IActionResult> InvoicePdf(int id)
    {
        var order = await GetOrderForInvoice(id);
        if (order == null)
            return NotFound();

        var pdf = _invoicePdfService.Generate(order);
        return File(pdf, "application/pdf", $"Invoice-{order.OrderNumber}.pdf");
    }

    private Task<Order?> GetOrderForInvoice(int id)
    {
        return _context.Orders
            .AsNoTracking()
            .Include(order => order.OrderItems)
                .ThenInclude(item => item.Product)
            .FirstOrDefaultAsync(order => order.Id == id);
    }
}
