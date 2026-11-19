
using System.Net;
using System.Net.Mail;
using System.Text;
using TechNova.Models;

namespace TechNova.Services;

public sealed class SmtpEmailSender : IEmailSender
{
    private readonly IConfiguration _configuration;

    public SmtpEmailSender(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task SendEmailAsync(string to, string subject, string htmlBody)
    {
        var host = _configuration["Smtp:Host"] ?? "smtp.gmail.com";
        var port = _configuration.GetValue<int>("Smtp:Port", 587);
        var username = _configuration["Smtp:Username"];
        var password = _configuration["Smtp:Password"];
        var from = _configuration["Smtp:From"] ?? username;
        var fromName = _configuration["Smtp:FromName"] ?? "TechNova";

        if (string.IsNullOrWhiteSpace(username) ||
            string.IsNullOrWhiteSpace(password) ||
            string.IsNullOrWhiteSpace(from))
        {
            throw new InvalidOperationException(
                "SMTP settings are missing. Configure Smtp:Username and Smtp:Password; Smtp:From is optional.");
        }

        using var message = new MailMessage
        {
            From = new MailAddress(from, fromName),
            Subject = subject,
            Body = htmlBody,
            IsBodyHtml = true
        };
        message.To.Add(new MailAddress(to));

#pragma warning disable SYSLIB0014
        using var client = new SmtpClient(host, port)
        {
            EnableSsl = true,
            UseDefaultCredentials = false,
            Credentials = new NetworkCredential(username, password),
            DeliveryMethod = SmtpDeliveryMethod.Network
        };
#pragma warning restore SYSLIB0014

        await client.SendMailAsync(message);
    }

    public Task SendOrderConfirmationAsync(Order order)
    {
        var items = new StringBuilder();
        foreach (var item in order.OrderItems)
        {
            var productName = WebUtility.HtmlEncode(item.Product?.Name ?? $"Product #{item.ProductId}");
            items.Append($"<tr><td>{productName}</td><td style=\"text-align:center\">{item.Quantity}</td><td style=\"text-align:right\">RS {item.UnitPrice:N2}</td><td style=\"text-align:right\">RS {item.TotalPrice:N2}</td></tr>");
        }

        var customerName = WebUtility.HtmlEncode($"{order.CustomerFirstName} {order.CustomerLastName}");
        var orderNumber = WebUtility.HtmlEncode(order.OrderNumber);
        var customerEmail = WebUtility.HtmlEncode(order.CustomerEmail);
        var phone = WebUtility.HtmlEncode(order.ShippingPhone);
        var addressLines = new[]
        {
            order.ShippingAddress,
            order.ShippingAddress2,
            string.Join(", ", new[] { order.ShippingCity, order.ShippingState, order.ShippingPostalCode }
                .Where(value => !string.IsNullOrWhiteSpace(value))),
            order.ShippingCountry
        };
        var shippingAddress = string.Join("<br />", addressLines
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Select(WebUtility.HtmlEncode));
        var transactionId = string.IsNullOrWhiteSpace(order.TransactionId)
            ? ""
            : $"<p><strong>Transaction ID:</strong> {WebUtility.HtmlEncode(order.TransactionId)}</p>";
        var notes = string.IsNullOrWhiteSpace(order.Notes)
            ? ""
            : $"<p><strong>Order notes:</strong> {WebUtility.HtmlEncode(order.Notes)}</p>";

        var body = $"""
            <html><body style="font-family:Arial,sans-serif;color:#222">
            <h2>Thank you for your order, {customerName}!</h2>
            <p>Your order has been placed. This email is your order invoice.</p>
            <h3>Order details</h3>
            <p><strong>Order number:</strong> {orderNumber}<br />
            <strong>Order date:</strong> {order.OrderDate.ToUniversalTime():yyyy-MM-dd HH:mm} UTC<br />
            <strong>Order status:</strong> {order.Status}<br />
            <strong>Payment method:</strong> {order.PaymentMethod}<br />
            <strong>Payment status:</strong> {order.PaymentStatus}</p>
            {transactionId}
            <h3>Customer and shipping details</h3>
            <p><strong>Name:</strong> {customerName}<br />
            <strong>Email:</strong> {customerEmail}<br />
            <strong>Phone:</strong> {phone}<br />
            <strong>Shipping address:</strong><br />{shippingAddress}</p>
            <h3>Items</h3>
            <table cellpadding="8" cellspacing="0" border="1" style="border-collapse:collapse;width:100%">
            <thead><tr><th>Product</th><th>Qty</th><th>Unit price</th><th>Line total</th></tr></thead>
            <tbody>{items}</tbody></table>
            <table cellpadding="6" cellspacing="0" style="margin-left:auto;margin-top:12px">
            <tr><td>Subtotal</td><td style="text-align:right">RS {order.Subtotal:N2}</td></tr>
            <tr><td>Discount</td><td style="text-align:right">RS {order.DiscountAmount:N2}</td></tr>
            <tr><td>Tax</td><td style="text-align:right">RS {order.TaxAmount:N2}</td></tr>
            <tr><td>Shipping</td><td style="text-align:right">RS {order.ShippingCost:N2}</td></tr>
            <tr><td><strong>Total</strong></td><td style="text-align:right"><strong>RS {order.TotalAmount:N2}</strong></td></tr>
            </table>
            {notes}
            </body></html>
            """;

        return SendEmailAsync(order.CustomerEmail, $"Order Invoice - {order.OrderNumber}", body);
    }

    public Task SendOrderStatusUpdateAsync(string to, string orderNumber, string status)
    {
        var safeOrderNumber = WebUtility.HtmlEncode(orderNumber);
        var safeStatus = WebUtility.HtmlEncode(status);
        var body = $"<h2>Order Status Update</h2><p>Your order <strong>{safeOrderNumber}</strong> status has been updated to <strong>{safeStatus}</strong>.</p><p>You can track your order on our website.</p>";
        return SendEmailAsync(to, $"Order Status Update - {orderNumber}", body);
    }
}
