using TechNova.Models;

namespace TechNova.ViewModels
{
    public class OrderConfirmationViewModel
    {
        public Order Order { get; set; } = null!;
        public List<OrderItem> OrderItems { get; set; } = new();
    }
}
