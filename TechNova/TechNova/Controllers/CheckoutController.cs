using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechNova.Data;
using TechNova.Models;
using TechNova.Services;
using TechNova.ViewModels;
using Stripe;
using Stripe.Checkout;
using PaymentMethod = TechNova.Models.PaymentMethod;

namespace TechNova.Controllers
{
    public class CheckoutController : Controller
    {
        private readonly IShoppingCartService _cartService;
        private readonly IOrderService _orderService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly IEmailSender _emailSender;
        private readonly ILogger<CheckoutController> _logger;

        public CheckoutController(
            IShoppingCartService cartService,
            IOrderService orderService,
            UserManager<ApplicationUser> userManager,
            ApplicationDbContext context,
            IConfiguration configuration,
            IEmailSender emailSender,
            ILogger<CheckoutController> logger)
        {
            _cartService = cartService;
            _orderService = orderService;
            _userManager = userManager;
            _context = context;
            _configuration = configuration;
            _emailSender = emailSender;
            _logger = logger;
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult TrackOrder()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TrackOrder(string? orderNumber, string? email)
        {
            ViewData["OrderNumber"] = orderNumber;
            ViewData["Email"] = email;

            if (string.IsNullOrWhiteSpace(orderNumber) || string.IsNullOrWhiteSpace(email))
            {
                ModelState.AddModelError(string.Empty, "Enter your order number and the email used at checkout.");
                return View();
            }

            var order = await _orderService.GetOrderByNumberAndEmailAsync(orderNumber, email);

            if (order == null)
            {
                ModelState.AddModelError(string.Empty, "We couldn't find an order matching those details. Check them and try again.");
                return View();
            }

            return View(order);
        }

        [HttpGet]
        public async Task<IActionResult> Index(bool paymentCancelled = false)
        {
            var cart = await _cartService.GetCartAsync();
            if (cart.CartItems.Count == 0)
            {
                TempData["Error"] = "Your cart is empty.";
                return RedirectToAction("Index", "ShoppingCart");
            }

            var user = await _userManager.GetUserAsync(User);
            ViewBag.CartItemCount = cart.ItemCount;
            ViewBag.StripeEnabled = !string.IsNullOrWhiteSpace(_configuration["Stripe:SecretKey"]);

            if (paymentCancelled)
            {
                TempData["Error"] = "Payment was cancelled. Your cart is still available.";
            }

            return View(new CheckoutViewModel
            {
                Cart = cart,
                IsAuthenticated = user is not null,
                FirstName = user?.FirstName ?? string.Empty,
                LastName = user?.LastName ?? string.Empty,
                Email = user?.Email ?? string.Empty,
                Phone = user?.PhoneNumber ?? string.Empty,
                Address = user?.Address ?? string.Empty,
                City = user?.City ?? string.Empty,
                PostalCode = user?.PostalCode ?? string.Empty,
                Country = user?.Country ?? "Pakistan"
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Process(CheckoutViewModel model)
        {
            var cart = await _cartService.GetCartAsync();
            if (cart.CartItems.Count == 0)
            {
                TempData["Error"] = "Your cart is empty.";
                return RedirectToAction("Index", "ShoppingCart");
            }

            var user = await _userManager.GetUserAsync(User);
            model.Cart = cart;
            model.IsAuthenticated = user is not null;
            ViewBag.CartItemCount = cart.ItemCount;
            ViewBag.StripeEnabled = !string.IsNullOrWhiteSpace(_configuration["Stripe:SecretKey"]);

            if (model.PaymentMethod is not PaymentMethod.CashOnDelivery and not PaymentMethod.Stripe)
            {
                ModelState.AddModelError(nameof(model.PaymentMethod), "Select a supported payment method.");
            }

            var productIds = cart.CartItems.Select(item => item.ProductId).Distinct().ToArray();
            var products = await _context.Products
                .Where(product => productIds.Contains(product.Id))
                .ToDictionaryAsync(product => product.Id);

            foreach (var item in cart.CartItems)
            {
                if (!products.TryGetValue(item.ProductId, out var product) || !product.IsActive)
                {
                    ModelState.AddModelError(string.Empty, $"{item.ProductName} is no longer available.");
                }
                else if (item.Quantity <= 0 || item.Quantity > product.StockQuantity)
                {
                    ModelState.AddModelError(string.Empty, $"There is not enough stock for {item.ProductName}. Please update your cart.");
                }
            }

            if (!ModelState.IsValid)
            {
                return View("Index", model);
            }

            if (model.PaymentMethod == PaymentMethod.Stripe)
            {
                var secretKey = _configuration["Stripe:SecretKey"];
                if (string.IsNullOrWhiteSpace(secretKey))
                {
                    ModelState.AddModelError(string.Empty, "Card payments are not configured. Please choose Cash on Delivery.");
                    return View("Index", model);
                }

                return await StartStripeCheckoutAsync(model, cart, user, secretKey);
            }

            await using var transaction = await _context.Database.BeginTransactionAsync();

            if (!await DecreaseStockAsync(cart))
            {
                await transaction.RollbackAsync();
                ModelState.AddModelError(string.Empty, "There is not enough stock for one or more items. Please update your cart.");
                model.Cart = await _cartService.GetCartAsync();
                ViewBag.CartItemCount = model.Cart.ItemCount;
                return View("Index", model);
            }

            var order = CreateOrder(model, cart, user, PaymentMethod.CashOnDelivery);
            _context.Orders.Add(order);
            await _context.SaveChangesAsync();
            await _cartService.ClearCartAsync();
            await transaction.CommitAsync();

            var invoiceEmailSent = await SendOrderConfirmationEmailAsync(order);
            TempData["OrderNumber"] = order.OrderNumber;
            TempData["Success"] = invoiceEmailSent
                ? $"Your order has been placed. Invoice sent to {order.CustomerEmail}."
                : "Your order has been placed.";
            if (!invoiceEmailSent)
            {
                TempData["Warning"] = "The order is complete, but the invoice email could not be sent. Check the SMTP settings.";
            }

            return RedirectToAction(nameof(Confirmation));
        }

        [HttpGet]
        public async Task<IActionResult> CompleteStripe(string? sessionId)
        {
            var secretKey = _configuration["Stripe:SecretKey"];
            if (string.IsNullOrWhiteSpace(secretKey) || string.IsNullOrWhiteSpace(sessionId))
            {
                TempData["Error"] = "Unable to verify the payment. Please contact support if you were charged.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                StripeConfiguration.ApiKey = secretKey;
                var session = await new SessionService().GetAsync(sessionId);
                if (!string.Equals(session.PaymentStatus, "paid", StringComparison.OrdinalIgnoreCase) ||
                    session.Metadata is null ||
                    !session.Metadata.TryGetValue("orderNumber", out var orderNumber))
                {
                    TempData["Error"] = "Payment has not been confirmed. Please try again or contact support.";
                    return RedirectToAction(nameof(Index));
                }

                if (!string.Equals(
                        HttpContext.Session.GetString($"StripeOrder:{session.Id}"),
                        orderNumber,
                        StringComparison.Ordinal))
                {
                    _logger.LogWarning("Stripe completion was requested from a session that did not start the order.");
                    TempData["Error"] = "Unable to verify this order in the current session.";
                    return RedirectToAction(nameof(Index));
                }

                var order = await _context.Orders
                    .Include(item => item.OrderItems)
                    .FirstOrDefaultAsync(item => item.OrderNumber == orderNumber && item.PaymentMethod == PaymentMethod.Stripe);
                if (order is null ||
                    (order.TransactionId != session.Id && order.TransactionId != session.PaymentIntentId) ||
                    session.ClientReferenceId != orderNumber ||
                    session.AmountTotal != ToMinorUnits(order.TotalAmount) ||
                    !string.Equals(session.Currency, "pkr", StringComparison.OrdinalIgnoreCase))
                {
                    _logger.LogWarning("Stripe session verification did not match a pending order.");
                    TempData["Error"] = "Unable to verify the payment. Please contact support if you were charged.";
                    return RedirectToAction(nameof(Index));
                }

                bool? invoiceEmailSent = null;
                if (order.PaymentStatus != PaymentStatus.Paid)
                {
                    await using var transaction = await _context.Database.BeginTransactionAsync();
                    var claimed = await _context.Orders
                        .Where(item => item.Id == order.Id && item.PaymentStatus == PaymentStatus.Pending)
                        .ExecuteUpdateAsync(setters => setters
                            .SetProperty(item => item.PaymentStatus, PaymentStatus.Paid)
                            .SetProperty(item => item.TransactionId, session.PaymentIntentId ?? session.Id));

                    if (claimed == 0)
                    {
                        await transaction.RollbackAsync();
                        var currentStatus = await _context.Orders
                            .AsNoTracking()
                            .Where(item => item.Id == order.Id)
                            .Select(item => item.PaymentStatus)
                            .SingleOrDefaultAsync();
                        if (currentStatus == PaymentStatus.Paid)
                        {
                            TempData["OrderNumber"] = order.OrderNumber;
                            return RedirectToAction(nameof(Confirmation));
                        }

                        TempData["Error"] = "Unable to process this payment. Please contact support.";
                        return RedirectToAction(nameof(Index));
                    }

                    if (!await DecreaseStockAsync(order.OrderItems))
                    {
                        await transaction.RollbackAsync();
                        var refunded = await TryRefundStripePaymentAsync(session.PaymentIntentId, order);
                        TempData["Error"] = refunded
                            ? "Inventory changed before your payment completed. The captured payment has been refunded."
                            : $"Inventory changed before your payment completed. Contact support with order number {order.OrderNumber} to resolve the payment.";
                        return RedirectToAction(nameof(Index));
                    }

                    order.PaymentStatus = PaymentStatus.Paid;
                    order.TransactionId = session.PaymentIntentId ?? session.Id;
                    await _context.SaveChangesAsync();
                    await _cartService.ClearCartAsync();
                    await transaction.CommitAsync();
                    invoiceEmailSent = await SendOrderConfirmationEmailAsync(order);
                }

                TempData["OrderNumber"] = order.OrderNumber;
                TempData["Success"] = "Your payment was received and your order has been placed.";
                if (invoiceEmailSent == true)
                {
                    TempData["Success"] = $"Your payment was received. Invoice sent to {order.CustomerEmail}.";
                }
                else if (invoiceEmailSent == false)
                {
                    TempData["Warning"] = "Your order is complete, but the invoice email could not be sent. Check the SMTP settings.";
                }

                return RedirectToAction(nameof(Confirmation));
            }
            catch (StripeException exception)
            {
                _logger.LogError(exception, "Unable to verify Stripe checkout session.");
                TempData["Error"] = "Unable to verify the payment. Please contact support if you were charged.";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpGet]
        public async Task<IActionResult> Confirmation()
        {
            var orderNumber = TempData["OrderNumber"] as string;
            if (string.IsNullOrWhiteSpace(orderNumber))
            {
                return RedirectToAction("Index", "ShoppingCart");
            }

            var order = await _context.Orders
                .Include(item => item.OrderItems)
                .ThenInclude(item => item.Product)
                .FirstOrDefaultAsync(item => item.OrderNumber == orderNumber);
            if (order is null)
            {
                return NotFound();
            }

            ViewBag.CartItemCount = await _cartService.GetCartItemCountAsync();
            return View("Confirmation", new OrderConfirmationViewModel
            {
                Order = order,
                OrderItems = order.OrderItems.ToList()
            });
        }

        private async Task<IActionResult> StartStripeCheckoutAsync(
            CheckoutViewModel model,
            ShoppingCartViewModel cart,
            ApplicationUser? user,
            string secretKey)
        {
            var order = CreateOrder(model, cart, user, PaymentMethod.Stripe);
            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            try
            {
                StripeConfiguration.ApiKey = secretKey;
                var successUrl = Url.Action(nameof(CompleteStripe), "Checkout", values: null, protocol: Request.Scheme);
                var cancelUrl = Url.Action(nameof(Index), "Checkout", values: null, protocol: Request.Scheme);
                var options = new SessionCreateOptions
                {
                    Mode = "payment",
                    SuccessUrl = $"{successUrl}?sessionId={{CHECKOUT_SESSION_ID}}",
                    CancelUrl = $"{cancelUrl}?paymentCancelled=true",
                    ClientReferenceId = order.OrderNumber,
                    Metadata = new Dictionary<string, string> { ["orderNumber"] = order.OrderNumber },
                    PaymentMethodTypes = new List<string> { "card" },
                    LineItems = cart.CartItems.Select(item => new SessionLineItemOptions
                    {
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            Currency = "pkr",
                            UnitAmount = ToMinorUnits(item.UnitPrice),
                            ProductData = new SessionLineItemPriceDataProductDataOptions { Name = item.ProductName }
                        },
                        Quantity = item.Quantity
                    }).ToList()
                };

                if (cart.Tax > 0)
                {
                    options.LineItems.Add(CreateCheckoutLine("Tax", cart.Tax));
                }

                if (cart.Shipping > 0)
                {
                    options.LineItems.Add(CreateCheckoutLine("Shipping", cart.Shipping));
                }

                var session = await new SessionService().CreateAsync(options);
                order.TransactionId = session.Id;
                await _context.SaveChangesAsync();
                HttpContext.Session.SetString($"StripeOrder:{session.Id}", order.OrderNumber);
                return Redirect(session.Url);
            }
            catch (StripeException exception)
            {
                order.PaymentStatus = PaymentStatus.Failed;
                await _context.SaveChangesAsync();
                _logger.LogError(exception, "Unable to create Stripe checkout session for order {OrderNumber}.", order.OrderNumber);
                ModelState.AddModelError(string.Empty, "Unable to start card payment. Please choose Cash on Delivery or try again later.");
                ViewBag.CartItemCount = cart.ItemCount;
                ViewBag.StripeEnabled = !string.IsNullOrWhiteSpace(_configuration["Stripe:SecretKey"]);
                return View("Index", model);
            }
        }

        private Order CreateOrder(CheckoutViewModel model, ShoppingCartViewModel cart, ApplicationUser? user, PaymentMethod paymentMethod)
        {
            return new Order
            {
                CustomerFirstName = model.FirstName,
                CustomerLastName = model.LastName,
                CustomerEmail = model.Email,
                Subtotal = cart.CartTotal,
                TaxAmount = cart.Tax,
                ShippingCost = cart.Shipping,
                TotalAmount = cart.GrandTotal,
                PaymentMethod = paymentMethod,
                PaymentStatus = PaymentStatus.Pending,
                ShippingAddress = model.Address,
                ShippingAddress2 = model.Address2,
                ShippingCity = model.City,
                ShippingState = model.State,
                ShippingPostalCode = model.PostalCode,
                ShippingCountry = model.Country,
                ShippingPhone = model.Phone,
                Notes = model.OrderNotes,
                UserId = user?.Id,
                OrderDate = DateTime.UtcNow,
                OrderItems = cart.CartItems.Select(item => new OrderItem
                {
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice
                }).ToList()
            };
        }

        private async Task<bool> DecreaseStockAsync(ShoppingCartViewModel cart)
        {
            foreach (var item in cart.CartItems)
            {
                var affectedRows = await _context.Products
                    .Where(product => product.Id == item.ProductId
                        && product.IsActive
                        && product.StockQuantity >= item.Quantity)
                    .ExecuteUpdateAsync(setters => setters
                        .SetProperty(product => product.StockQuantity, product => product.StockQuantity - item.Quantity));

                if (affectedRows == 0)
                {
                    return false;
                }
            }

            return true;
        }

        private async Task<bool> DecreaseStockAsync(IEnumerable<OrderItem> items)
        {
            foreach (var item in items)
            {
                var affectedRows = await _context.Products
                    .Where(product => product.Id == item.ProductId
                        && product.IsActive
                        && product.StockQuantity >= item.Quantity)
                    .ExecuteUpdateAsync(setters => setters
                        .SetProperty(product => product.StockQuantity, product => product.StockQuantity - item.Quantity));

                if (affectedRows == 0)
                {
                    return false;
                }
            }

            return true;
        }

        private async Task<bool> SendOrderConfirmationEmailAsync(Order order)
        {
            try
            {
                var invoiceOrder = await _context.Orders
                    .AsNoTracking()
                    .Include(item => item.OrderItems)
                    .ThenInclude(item => item.Product)
                    .SingleOrDefaultAsync(item => item.Id == order.Id);

                if (invoiceOrder is null)
                {
                    _logger.LogWarning("Unable to load invoice details for order {OrderNumber}.", order.OrderNumber);
                    return false;
                }

                await _emailSender.SendOrderConfirmationAsync(invoiceOrder);
                return true;
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Unable to send order confirmation for order {OrderNumber}.", order.OrderNumber);
                return false;
            }
        }

        private async Task<bool> TryRefundStripePaymentAsync(string? paymentIntentId, Order order)
        {
            if (string.IsNullOrWhiteSpace(paymentIntentId))
            {
                _logger.LogError("Cannot refund order {OrderNumber}: Stripe session did not include a payment intent.", order.OrderNumber);
                return false;
            }

            try
            {
                await new RefundService().CreateAsync(new RefundCreateOptions { PaymentIntent = paymentIntentId });
                order.PaymentStatus = PaymentStatus.Refunded;
                order.Status = OrderStatus.Cancelled;
                await _context.SaveChangesAsync();
                return true;
            }
            catch (StripeException exception)
            {
                _logger.LogError(exception, "Unable to refund Stripe payment for order {OrderNumber}.", order.OrderNumber);
                return false;
            }
        }

        private static SessionLineItemOptions CreateCheckoutLine(string name, decimal amount)
        {
            return new SessionLineItemOptions
            {
                PriceData = new SessionLineItemPriceDataOptions
                {
                    Currency = "pkr",
                    UnitAmount = ToMinorUnits(amount),
                    ProductData = new SessionLineItemPriceDataProductDataOptions { Name = name }
                },
                Quantity = 1
            };
        }

        private static long ToMinorUnits(decimal amount)
        {
            return (long)decimal.Round(amount * 100, 0, MidpointRounding.AwayFromZero);
        }
    }
}
