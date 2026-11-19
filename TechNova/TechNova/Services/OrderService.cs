using Microsoft.EntityFrameworkCore;
using TechNova.Data;
using TechNova.Models;

namespace TechNova.Services
{
    public interface IOrderService
    {
        Task<List<Order>> GetUserOrdersAsync(string userId);
        Task<Order?> GetUserOrderByIdAsync(int orderId, string userId);
        Task<Order?> GetOrderByNumberAndEmailAsync(string orderNumber, string email);
        Task<bool> UpdateOrderStatusAsync(int orderId, OrderStatus status);
    }

    public class OrderService : IOrderService
    {
        private readonly ApplicationDbContext _context;

        public OrderService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Order>> GetUserOrdersAsync(string userId)
        {
            return await _context.Orders
                .AsNoTracking()
                .Include(order => order.OrderItems)
                    .ThenInclude(orderItem => orderItem.Product)
                .Where(order => order.UserId == userId)
                .OrderByDescending(order => order.OrderDate)
                .ToListAsync();
        }

        public async Task<Order?> GetUserOrderByIdAsync(int orderId, string userId)
        {
            return await _context.Orders
                .AsNoTracking()
                .Include(order => order.OrderItems)
                    .ThenInclude(orderItem => orderItem.Product)
                .Include(order => order.User)
                .FirstOrDefaultAsync(order => order.Id == orderId && order.UserId == userId);
        }

        public async Task<Order?> GetOrderByNumberAndEmailAsync(string orderNumber, string email)
        {
            var normalizedOrderNumber = orderNumber.Trim().ToUpperInvariant();
            var normalizedEmail = email.Trim().ToUpperInvariant();

            return await _context.Orders
                .AsNoTracking()
                .Include(order => order.OrderItems)
                    .ThenInclude(orderItem => orderItem.Product)
                .FirstOrDefaultAsync(order =>
                    order.OrderNumber.ToUpper() == normalizedOrderNumber &&
                    order.CustomerEmail.ToUpper() == normalizedEmail);
        }

        public async Task<bool> UpdateOrderStatusAsync(int orderId, OrderStatus status)
        {
            if (!Enum.IsDefined(status))
                return false;

            var order = await _context.Orders.FindAsync(orderId);
            if (order == null || order.Status == status)
                return false;

            order.Status = status;
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
