

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Encodings.Web;
using TechNova.Data;
using TechNova.Models;
using TechNova.Services;
using TechNova.ViewModels;

namespace TechNova.AccountControllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _context;
        private readonly IShoppingCartService _cartService;
        private readonly IOrderService _orderService;
        private readonly IOrderInvoicePdfService _orderInvoicePdfService;
        private readonly FileService _fileService;
        private readonly IEmailSender _emailSender;

        // Constructor Injection
        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            RoleManager<IdentityRole> roleManager,
            ApplicationDbContext context,
            IShoppingCartService cartService,
            IOrderService orderService,
            IOrderInvoicePdfService orderInvoicePdfService,
            FileService fileService,
            IEmailSender emailSender)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _context = context;
            _cartService = cartService;
            _orderService = orderService;
            _orderInvoicePdfService = orderInvoicePdfService;
            _fileService = fileService;
            _emailSender = emailSender;
        }

        // ==========================================
        // GET: Account/Register
        // ==========================================

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        // ==========================================
        // POST: Account/Register
        // ==========================================

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(
            RegisterViewModel model,
            string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var existingUser =
                await _userManager.FindByEmailAsync(model.Email);

            if (existingUser != null)
            {
                ModelState.AddModelError(
                    "Email",
                    "This email address is already registered."
                );

                return View(model);
            }

            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                FirstName = model.FirstName,
                LastName = model.LastName,
                PhoneNumber = model.PhoneNumber,
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(
                user,
                model.Password
            );

            if (result.Succeeded)
            {
                if (!await _roleManager.RoleExistsAsync("Customer"))
                {
                    await _roleManager.CreateAsync(
                        new IdentityRole("Customer")
                    );
                }

                var roleResult = await _userManager.AddToRoleAsync(
                    user,
                    "Customer"
                );

                if (!roleResult.Succeeded)
                {
                    foreach (var error in roleResult.Errors)
                    {
                        ModelState.AddModelError(
                            string.Empty,
                            error.Description
                        );
                    }

                    await _userManager.DeleteAsync(user);

                    return View(model);
                }

                await _signInManager.SignInAsync(
                    user,
                    isPersistent: false
                );

                if (!string.IsNullOrEmpty(returnUrl) &&
                    Url.IsLocalUrl(returnUrl))
                {
                    return Redirect(returnUrl);
                }

                return RedirectToAction("Index", "Home");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(
                    string.Empty,
                    error.Description
                );
            }

            return View(model);
        }

        // ==========================================
        // POST: Account/Logout
        // ==========================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();

            return RedirectToAction("Index", "Home");
        }

        // ==========================================
        // GET: Account/Login
        // ==========================================

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            return View();
        }

        // ==========================================
        // POST: Account/Login
        // ==========================================

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(
            LoginViewModel model,
            string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var result = await _signInManager.PasswordSignInAsync(
                model.Email,
                model.Password,
                model.RememberMe,
                lockoutOnFailure: false
            );

            if (result.Succeeded)
            {
                // Migrate cart
                var user =
                    await _userManager.FindByEmailAsync(model.Email);

                if (user != null)
                {
                    await _cartService.MigrateCartAsync(user.Id);
                }

                if (!string.IsNullOrEmpty(returnUrl) &&
                    Url.IsLocalUrl(returnUrl))
                {
                    return Redirect(returnUrl);
                }

                return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError(
                string.Empty,
                "Invalid login attempt."
            );

            return View(model);
        }

        // ==========================================
        // GET: Account/Profile
        // ==========================================

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Profile()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return NotFound();
            }

            var model = new ProfileViewModel
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email!,
                PhoneNumber = user.PhoneNumber,
                DateOfBirth = user.DateOfBirth,
                Address = user.Address,
                City = user.City,
                PostalCode = user.PostalCode,
                Country = user.Country,
                ProfileImageUrl = user.ProfileImageUrl
            };

            ViewBag.CartItemCount =
                await _cartService.GetCartItemCountAsync();

            return View(model);
        }

        // ==========================================
        // POST: Account/Profile
        // ==========================================

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Profile(
            ProfileViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.CartItemCount =
                    await _cartService.GetCartItemCountAsync();

                return View(model);
            }

            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return NotFound();
            }

            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.PhoneNumber = model.PhoneNumber;
            user.DateOfBirth = model.DateOfBirth;
            user.Address = model.Address;
            user.City = model.City;
            user.PostalCode = model.PostalCode;
            user.Country = model.Country;

            if (model.ProfileImage != null)
            {
                var imagePath = await _fileService.SaveFileAsync(
                    model.ProfileImage,
                    "images/profiles"
                );

                if (!string.IsNullOrEmpty(imagePath))
                {
                    if (!string.IsNullOrEmpty(user.ProfileImageUrl))
                    {
                        _fileService.DeleteFile(
                            user.ProfileImageUrl
                        );
                    }

                    user.ProfileImageUrl = imagePath;
                }
            }

            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                TempData["Success"] =
                    "Profile updated successfully.";

                return RedirectToAction(nameof(Profile));
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(
                    string.Empty,
                    error.Description
                );
            }

            ViewBag.CartItemCount =
                await _cartService.GetCartItemCountAsync();

            return View(model);
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Orders()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return NotFound();
            }

            var orders = await _orderService.GetUserOrdersAsync(user.Id);
            ViewBag.CartItemCount = await _cartService.GetCartItemCountAsync();

            return View(orders);
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> OrderDetails(int id)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return NotFound();
            }

            var order = await _orderService.GetUserOrderByIdAsync(id, user.Id);

            if (order == null)
            {
                return NotFound();
            }

            ViewBag.CartItemCount = await _cartService.GetCartItemCountAsync();
            return View(order);
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> DownloadInvoice(int id)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return NotFound();
            }

            var order = await _orderService.GetUserOrderByIdAsync(id, user.Id);

            if (order == null)
            {
                return NotFound();
            }

            var invoicePdf = _orderInvoicePdfService.Generate(order);
            return File(invoicePdf, "application/pdf", $"Invoice-{order.OrderNumber}.pdf");
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> TrackOrder(int id)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return NotFound();
            }

            var order = await _context.Orders
                .AsNoTracking()
                .FirstOrDefaultAsync(order => order.Id == id && order.UserId == user.Id);

            if (order == null)
            {
                return NotFound();
            }

            ViewBag.CartItemCount = await _cartService.GetCartItemCountAsync();
            return View(order);
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Wishlist()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return NotFound();
            }

            var wishlistItems = await _context.WishlistItems
                .Include(item => item.Product)
                    .ThenInclude(product => product.Brand)
                .Where(item => item.UserId == user.Id)
                .OrderByDescending(item => item.AddedAt)
                .ToListAsync();

            ViewBag.CartItemCount = await _cartService.GetCartItemCountAsync();
            return View(wishlistItems);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddToWishlist(int productId)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return Unauthorized();
            }

            if (!await _context.Products.AnyAsync(product => product.Id == productId))
            {
                return NotFound();
            }

            var alreadyAdded = await _context.WishlistItems
                .AnyAsync(item => item.ProductId == productId && item.UserId == user.Id);

            if (alreadyAdded)
            {
                return Json(new { success = true, message = "Already added!" });
            }

            _context.WishlistItems.Add(new WishlistItem
            {
                ProductId = productId,
                UserId = user.Id
            });

            await _context.SaveChangesAsync();
            return Json(new { success = true, message = "Added to wishlist!" });
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveFromWishlist(int productId)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return Unauthorized();
            }

            var item = await _context.WishlistItems
                .FirstOrDefaultAsync(wishlistItem =>
                    wishlistItem.UserId == user.Id && wishlistItem.ProductId == productId);

            if (item != null)
            {
                _context.WishlistItems.Remove(item);
                await _context.SaveChangesAsync();
            }

            return Json(new { success = true, message = "Removed from wishlist!" });
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ForgotPassword()
        {
            return View();
        }


        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            const string confirmationMessage = "If your email is registered, you will receive password reset instructions.";
            var user = await _userManager.FindByEmailAsync(model.Email);

            // Keep responses identical whether or not an account exists.
            if (user is not null && await _userManager.IsEmailConfirmedAsync(user))
            {
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var resetUrl = Url.Action(
                    nameof(ResetPassword),
                    "Account",
                    new { email = user.Email, code = token },
                    Request.Scheme);

                if (string.IsNullOrWhiteSpace(resetUrl))
                {
                    throw new InvalidOperationException("Could not generate the password reset URL.");
                }

                var safeResetUrl = HtmlEncoder.Default.Encode(resetUrl);
                var body = $"<p>We received a request to reset your TechNova password.</p><p><a href=\"{safeResetUrl}\">Reset your password</a></p><p>If you did not request this, you can ignore this email.</p>";
                await _emailSender.SendEmailAsync(user.Email!, "Reset your TechNova password", body);
            }

            TempData["SuccessMessage"] = confirmationMessage;
            return RedirectToAction(nameof(Login));
        }
        [HttpGet]
        [Authorize]
        public IActionResult ChangePassword()
        {
            return View();
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return NotFound();

            var result = await _userManager.ChangePasswordAsync(user, model.OldPassword, model.NewPassword);
            if (result.Succeeded)
            {
                await _signInManager.RefreshSignInAsync(user);
                TempData["Success"] = "Password changed successfully.";
                return RedirectToAction(nameof(Profile));
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
        }
        [HttpGet]
        [AllowAnonymous]
        public IActionResult ResetPassword(string? code = null)
        {
            if (code == null)
                return BadRequest("A code must be supplied for password reset.");

            var model = new ResetPasswordViewModel
            {
                Code = code
            };

            return View(model);
        }
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                return RedirectToAction(nameof(ResetPasswordConfirmation));
            }

            var result = await _userManager.ResetPasswordAsync(user, model.Code, model.Password);
            if (result.Succeeded)
            {
                return RedirectToAction(nameof(ResetPasswordConfirmation));
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ResetPasswordConfirmation()
        {
            return View();
        }


        [HttpGet]
        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
