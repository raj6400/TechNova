using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using TechNova.Data;
using TechNova.Models;

namespace TechNova.Controllers.Admin
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class ContactAdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ContactAdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Admin/ContactAdmin
        public IActionResult Index()
        {
            return RedirectToAction(nameof(Edit));
        }

        // GET: /Admin/ContactAdmin/Edit
        [HttpGet]
        public async Task<IActionResult> Edit()
        {
            var contact = await _context.ContactInfos
                .FirstOrDefaultAsync();

            if (contact == null)
            {
                contact = new ContactInfo
                {
                    CompanyName = "TechNova",
                    Address = "Rajkot, Gujarat, India",
                    Phone = "",
                    Email = "",
                    WorkingHours = "Monday - Saturday: 9 AM - 6 PM",
                    MapAddress = "Rajkot, Gujarat, India"
                };
            }

            return View("~/Views/Admin/Contact/Edit.cshtml", contact);
        }

        // POST: /Admin/ContactAdmin/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ContactInfo model)
        {
            if (!ModelState.IsValid)
            {
                return View("~/Views/Admin/Contact/Edit.cshtml", model);
            }

            var contact = await _context.ContactInfos
                .FirstOrDefaultAsync();

            if (contact == null)
            {
                _context.ContactInfos.Add(model);
            }
            else
            {
                contact.CompanyName = model.CompanyName;
                contact.Address = model.Address;
                contact.Phone = model.Phone;
                contact.Email = model.Email;
                contact.WhatsApp = model.WhatsApp;
                contact.MapAddress = model.MapAddress;
                contact.WorkingHours = model.WorkingHours;
                contact.Facebook = model.Facebook;
                contact.Instagram = model.Instagram;
                contact.Twitter = model.Twitter;
                contact.LinkedIn = model.LinkedIn;
                contact.Description = model.Description;
            }

            await _context.SaveChangesAsync();

            TempData["Success"] =
                "Contact information updated successfully!";

            return RedirectToAction(nameof(Edit));
        }

        // GET: /Admin/ContactAdmin/Messages
        [HttpGet]
        public async Task<IActionResult> Messages()
        {
            var messages = await _context.ContactMessages
                .AsNoTracking()
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();

            return View("~/Views/Admin/Contact/Messages.cshtml", messages);
        }

        // POST: Mark message as read
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            var message = await _context.ContactMessages.FindAsync(id);

            if (message == null)
                return NotFound();

            message.IsRead = true;

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Messages));
        }

        // POST: Delete message
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteMessage(int id)
        {
            var message = await _context.ContactMessages.FindAsync(id);

            if (message == null)
                return NotFound();

            _context.ContactMessages.Remove(message);

            await _context.SaveChangesAsync();

            TempData["Success"] = "Message deleted successfully.";

            return RedirectToAction(nameof(Messages));
        }
    }
}