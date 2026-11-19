using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using TechNova.Models;

namespace TechNova.Services;

public interface IOrderInvoicePdfService
{
    byte[] Generate(Order order);
}

public class OrderInvoicePdfService : IOrderInvoicePdfService
{
    private const string Navy = "#14243A";
    private const string Teal = "#12B8A6";
    private const string Ink = "#25364A";
    private const string Muted = "#6B7A90";
    private const string PageBackground = "#F3F6FA";
    private const string BorderColor = "#E2E8F0";

    public byte[] Generate(Order order)
    {
        return Document.Create(document =>
        {
            document.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(30);
                page.PageColor(PageBackground);
                page.DefaultTextStyle(style => style.FontFamily("Arial").FontSize(10).FontColor(Ink));

                page.Header()
                    .Background(Navy)
                    .Padding(22)
                    .Column(header =>
                    {
                        header.Item().Row(row =>
                        {
                            row.ConstantItem(42)
                                .Height(42)
                                .Background(Teal)
                                .AlignCenter()
                                .AlignMiddle()
                                .Text("T")
                                .FontSize(23)
                                .Bold()
                                .FontColor(Colors.White);

                            row.RelativeItem().PaddingLeft(12).AlignMiddle().Column(brand =>
                            {
                                brand.Item().Text("TECHNOVA")
                                    .FontSize(18)
                                    .Bold()
                                    .FontColor(Colors.White);
                                brand.Item().Text("MOBILE  •  TECHNOLOGY")
                                    .FontSize(8)
                                    .LetterSpacing(1)
                                    .FontColor("#B8C6D8");
                            });

                            row.ConstantItem(190).AlignRight().Column(invoice =>
                            {
                                invoice.Item().AlignRight().Text("INVOICE")
                                    .FontSize(24)
                                    .Bold()
                                    .FontColor(Colors.White);
                                invoice.Item().AlignRight().Text($"#{order.OrderNumber}")
                                    .FontSize(10)
                                    .FontColor("#B8C6D8");
                            });
                        });

                        header.Item().PaddingTop(18).Row(row =>
                        {
                            row.RelativeItem().AlignMiddle().Text("YOUR ORDER, BEAUTIFULLY SUMMARIZED")
                                .FontSize(8)
                                .Bold()
                                .LetterSpacing(1)
                                .FontColor("#B8C6D8");
                            row.ConstantItem(105)
                                .Background(StatusColor(order.Status))
                                .PaddingVertical(6)
                                .AlignCenter()
                                .Text(order.Status.ToString().ToUpperInvariant())
                                .FontSize(8)
                                .Bold()
                                .FontColor(Colors.White);
                        });
                    });

                page.Content()
                    .Border(1)
                    .BorderColor(BorderColor)
                    .Background(Colors.White)
                    .Padding(22)
                    .Column(content =>
                {
                    content.Spacing(18);

                    content.Item().Row(row =>
                    {
                        row.RelativeItem().Column(heading =>
                        {
                            heading.Item().Text("INVOICE SUMMARY")
                                .FontSize(9)
                                .Bold()
                                .LetterSpacing(1)
                                .FontColor(Muted);
                            heading.Item().PaddingTop(4).Text("Thank you for your order")
                                .FontSize(17)
                                .Bold()
                                .FontColor(Navy);
                        });

                        row.ConstantItem(145).AlignRight().Column(date =>
                        {
                            date.Item().AlignRight().Text("INVOICE DATE")
                                .FontSize(8)
                                .Bold()
                                .LetterSpacing(1)
                                .FontColor(Muted);
                            date.Item().PaddingTop(4).AlignRight().Text(order.OrderDate.ToString("MMM dd, yyyy"))
                                .FontSize(11)
                                .SemiBold()
                                .FontColor(Navy);
                        });
                    });

                    content.Item().Row(row =>
                    {
                        row.RelativeItem().Background(PageBackground).Padding(14).Column(billing =>
                        {
                            billing.Spacing(5);
                            billing.Item().Text("BILLED TO").FontSize(8).Bold().LetterSpacing(1).FontColor(Muted);
                            billing.Item().Text($"{order.CustomerFirstName} {order.CustomerLastName}")
                                .FontSize(12)
                                .SemiBold()
                                .FontColor(Navy);
                            billing.Item().Text(order.CustomerEmail).FontColor(Muted);
                            billing.Item().PaddingTop(4).Text("SHIPPING ADDRESS")
                                .FontSize(8)
                                .Bold()
                                .LetterSpacing(1)
                                .FontColor(Muted);
                            billing.Item().Text(order.ShippingAddress).SemiBold();
                            if (!string.IsNullOrWhiteSpace(order.ShippingAddress2))
                                billing.Item().Text(order.ShippingAddress2).FontColor(Muted);
                            billing.Item().Text(string.Join(", ", new[]
                            {
                                order.ShippingCity,
                                order.ShippingState,
                                order.ShippingPostalCode,
                                order.ShippingCountry
                            }.Where(part => !string.IsNullOrWhiteSpace(part)))).FontColor(Muted);
                            if (!string.IsNullOrWhiteSpace(order.ShippingPhone))
                                billing.Item().Text($"Phone: {order.ShippingPhone}").FontColor(Muted);
                        });

                        row.ConstantItem(14);

                        row.RelativeItem().Background(PageBackground).Padding(14).Column(details =>
                        {
                            details.Spacing(9);
                            details.Item().Text("ORDER DETAILS").FontSize(8).Bold().LetterSpacing(1).FontColor(Muted);
                            details.Item().Row(line =>
                            {
                                line.RelativeItem().Text("Order number").FontColor(Muted);
                                line.ConstantItem(110).AlignRight().Text(order.OrderNumber).SemiBold();
                            });
                            details.Item().Row(line =>
                            {
                                line.RelativeItem().Text("Payment status").FontColor(Muted);
                                line.ConstantItem(110).AlignRight().Text(order.PaymentStatus.ToString()).SemiBold();
                            });
                            details.Item().Row(line =>
                            {
                                line.RelativeItem().Text("Payment method").FontColor(Muted);
                                line.ConstantItem(110).AlignRight().Text(order.PaymentMethod.ToString()).SemiBold();
                            });
                        });
                    });

                    content.Item().PaddingTop(2).Text("ITEMIZED PURCHASE")
                        .FontSize(10)
                        .Bold()
                        .LetterSpacing(1)
                        .FontColor(Navy);
                    content.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(5);
                            columns.RelativeColumn(1);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(2);
                        });

                        table.Header(header =>
                        {
                            header.Cell().Element(HeaderCell).Text("PRODUCT");
                            header.Cell().Element(HeaderCell).AlignCenter().Text("QTY");
                            header.Cell().Element(HeaderCell).AlignRight().Text("UNIT PRICE");
                            header.Cell().Element(HeaderCell).AlignRight().Text("AMOUNT");
                        });

                        var rowIndex = 0;
                        foreach (var item in order.OrderItems)
                        {
                            var productName = item.Product?.Name ?? "Product unavailable";
                            var productModel = item.Product?.Model;
                            var rowBackground = rowIndex++ % 2 == 0 ? "#FFFFFF" : PageBackground;

                            table.Cell().Element(container => BodyCell(container, rowBackground)).Column(product =>
                            {
                                product.Item().Text(productName).SemiBold();
                                if (!string.IsNullOrWhiteSpace(productModel))
                                    product.Item().Text(productModel).FontSize(8).FontColor(Muted);
                            });
                            table.Cell().Element(container => BodyCell(container, rowBackground)).AlignCenter().Text(item.Quantity.ToString());
                            table.Cell().Element(container => BodyCell(container, rowBackground)).AlignRight().Text(FormatMoney(item.UnitPrice));
                            table.Cell().Element(container => BodyCell(container, rowBackground)).AlignRight().Text(FormatMoney(item.TotalPrice));
                        }
                    });

                    content.Item().AlignRight().Width(265).Background(PageBackground).Padding(14).Column(totals =>
                    {
                        totals.Spacing(9);
                        AddTotalRow(totals, "Subtotal", FormatMoney(order.Subtotal));
                        if (order.DiscountAmount > 0)
                            AddTotalRow(totals, "Discount", $"− {FormatMoney(order.DiscountAmount)}");
                        AddTotalRow(totals, "Tax", FormatMoney(order.TaxAmount));
                        AddTotalRow(totals, "Shipping", order.ShippingCost == 0 ? "FREE" : FormatMoney(order.ShippingCost));
                        totals.Item().PaddingTop(8).Background(Navy).Padding(12).Row(row =>
                            {
                                row.RelativeItem().Text("TOTAL PAID")
                                    .FontSize(9)
                                    .Bold()
                                    .FontColor(Colors.White);
                                row.ConstantItem(125).AlignRight().Text(FormatMoney(order.TotalAmount))
                                    .FontSize(13)
                                    .Bold()
                                    .FontColor(Teal);
                            });
                    });

                    if (!string.IsNullOrWhiteSpace(order.Notes))
                    {
                        content.Item().BorderLeft(3).BorderColor(Teal).Padding(12)
                            .Background(PageBackground).Column(notes =>
                            {
                                notes.Item().Text("ORDER NOTES").FontSize(8).Bold().LetterSpacing(1).FontColor(Muted);
                                notes.Item().PaddingTop(4).Text(order.Notes);
                            });
                    }
                });

                page.Footer()
                    .PaddingTop(12)
                    .AlignCenter()
                    .Text("THANK YOU FOR CHOOSING TECHNOVA  •  THIS IS YOUR OFFICIAL ORDER INVOICE")
                    .FontSize(9)
                    .Bold()
                    .LetterSpacing(0.5f)
                    .FontColor(Muted);
            });
        }).GeneratePdf();
    }

    private static void AddTotalRow(ColumnDescriptor column, string label, string value)
    {
        column.Item().Row(row =>
        {
            row.RelativeItem().Text(label).FontColor(Muted);
            row.ConstantItem(125).AlignRight().Text(value).SemiBold().FontColor(Ink);
        });
    }

    private static IContainer HeaderCell(IContainer container)
    {
        return container.Background(Teal)
            .PaddingVertical(10)
            .PaddingHorizontal(8)
            .DefaultTextStyle(style => style.FontSize(8).FontColor(Colors.White).SemiBold());
    }

    private static IContainer BodyCell(IContainer container, string backgroundColor)
    {
        return container.Background(backgroundColor)
            .BorderBottom(1)
            .BorderColor(BorderColor)
            .PaddingVertical(10)
            .PaddingHorizontal(8);
    }

    private static string FormatMoney(decimal amount) => $"Rs. {amount:N2}";

    private static string StatusColor(OrderStatus status) => status switch
    {
        OrderStatus.Delivered => "#2E9D73",
        OrderStatus.Cancelled or OrderStatus.Refunded => "#C54B55",
        OrderStatus.Shipped => "#3677C8",
        OrderStatus.Processing => "#BE8A2C",
        _ => "#75849A"
    };
}
