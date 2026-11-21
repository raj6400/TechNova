using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechNova.Data;
using TechNova.Models;

namespace TechNova.Controllers
{
    public class ContactController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ContactController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Contact
        public async Task<IActionResult> Index()
        {
            var contact = await _context.ContactInfos
                .AsNoTracking()
                .FirstOrDefaultAsync();

            if (contact == null)
            {
                contact = new ContactInfo
                {
                    CompanyName = "TechNova",
                    Address = "Rajkot, Gujarat, India",
                    Phone = "+91 0000000000",
                    Email = "info@example.com",
                    WorkingHours = "Monday - Saturday: 9 AM - 6 PM",
                    MapAddress = "Rajkot, Gujarat, India"
                };
            }

            ViewBag.Contact = contact;

            var mapAddress = string.IsNullOrWhiteSpace(contact.MapAddress)
                ? contact.Address
                : contact.MapAddress;

            ViewBag.MapUrl =
                "https://maps.google.com/maps?q=" +
                Uri.EscapeDataString(mapAddress) +
                "&output=embed";

            return View(new ContactMessage());
        }

        // POST: /Contact/SendMessage
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendMessage(ContactMessage message)
        {
            if (!ModelState.IsValid)
            {
                var contact = await _context.ContactInfos
                    .AsNoTracking()
                    .FirstOrDefaultAsync();

                ViewBag.Contact = contact ?? new ContactInfo
                {
                    CompanyName = "TechNova",
                    Address = "Rajkot, Gujarat, India",
                    Phone = "+91 0000000000",
                    Email = "info@example.com",
                    MapAddress = "Rajkot, Gujarat, India"
                };

                var mapAddress = string.IsNullOrWhiteSpace(
                    ViewBag.Contact.MapAddress)
                    ? ViewBag.Contact.Address
                    : ViewBag.Contact.MapAddress;

                ViewBag.MapUrl =
                    "https://maps.google.com/maps?q=" +
                    Uri.EscapeDataString(mapAddress) +
                    "&output=embed";

                return View("Index", message);
            }

            message.CreatedAt = DateTime.UtcNow;
            message.IsRead = false;

            _context.ContactMessages.Add(message);

            await _context.SaveChangesAsync();

            TempData["Success"] =
                "Thank you! Your message has been submitted successfully.";

            return RedirectToAction(nameof(Index));
        }
    }
}