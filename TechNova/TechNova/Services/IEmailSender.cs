using TechNova.Models;

namespace TechNova.Services;

public interface IEmailSender
{
    Task SendEmailAsync(string to, string subject, string htmlBody);
    Task SendOrderConfirmationAsync(Order order);
    Task SendOrderStatusUpdateAsync(string to, string orderNumber, string status);
}
