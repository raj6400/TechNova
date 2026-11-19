using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechNova.Data;
using TechNova.Models;
using TechNova.ViewModels;

namespace TechNova.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Policy = "AdminOnly")]
public class UsersController : Controller
{
    private const int PageSize = 20;

    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly ApplicationDbContext _context;

    public UsersController(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        ApplicationDbContext context)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _context = context;
    }

    public async Task<IActionResult> Index(string? search, string? role, int page = 1)
    {
        page = Math.Max(page, 1);
        search = string.IsNullOrWhiteSpace(search) ? null : search.Trim();
        role = string.IsNullOrWhiteSpace(role) ? null : role.Trim();

        IQueryable<ApplicationUser> query = _userManager.Users.AsNoTracking();
        if (search != null)
        {
            query = query.Where(user =>
                (user.Email != null && user.Email.Contains(search)) ||
                user.FirstName.Contains(search) ||
                user.LastName.Contains(search));
        }

        var users = await query
            .OrderByDescending(user => user.CreatedAt)
            .ThenBy(user => user.Id)
            .ToListAsync();

        var orderCounts = await _context.Orders
            .AsNoTracking()
            .Where(order => order.UserId != null)
            .GroupBy(order => order.UserId!)
            .Select(group => new { UserId = group.Key, Count = group.Count() })
            .ToDictionaryAsync(item => item.UserId, item => item.Count);

        var userViewModels = new List<UserManagementViewModel>(users.Count);
        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            if (role != null && !roles.Contains(role, StringComparer.OrdinalIgnoreCase))
                continue;

            userViewModels.Add(new UserManagementViewModel
            {
                UserId = user.Id,
                FullName = user.FullName,
                Email = user.Email ?? string.Empty,
                PhoneNumber = user.PhoneNumber ?? string.Empty,
                Roles = roles.ToArray(),
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt,
                OrderCount = orderCounts.GetValueOrDefault(user.Id)
            });
        }

        var totalItems = userViewModels.Count;
        var totalPages = (int)Math.Ceiling(totalItems / (double)PageSize);
        if (totalPages > 0)
            page = Math.Min(page, totalPages);

        ViewBag.Roles = await _roleManager.Roles
            .Where(identityRole => identityRole.Name != null)
            .OrderBy(identityRole => identityRole.Name)
            .Select(identityRole => identityRole.Name!)
            .ToListAsync();
        ViewBag.CurrentRole = role;
        ViewBag.Search = search;
        ViewBag.CurrentPage = page;
        ViewBag.TotalPages = totalPages;

        return View(userViewModels
            .Skip((page - 1) * PageSize)
            .Take(PageSize)
            .ToList());
    }

    public async Task<IActionResult> Details(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            return NotFound();

        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
            return NotFound();

        ViewBag.Roles = await _userManager.GetRolesAsync(user);
        ViewBag.AvailableRoles = await _roleManager.Roles
            .Where(identityRole => identityRole.Name != null)
            .OrderBy(identityRole => identityRole.Name)
            .Select(identityRole => identityRole.Name!)
            .ToListAsync();
        ViewBag.Orders = await _context.Orders
            .AsNoTracking()
            .Where(order => order.UserId == user.Id)
            .OrderByDescending(order => order.OrderDate)
            .ThenByDescending(order => order.Id)
            .Take(10)
            .ToListAsync();

        return View(user);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleStatus(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
            return NotFound();

        user.IsActive = !user.IsActive;
        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            TempData["Error"] = string.Join(" ", result.Errors.Select(error => error.Description));
            return RedirectToAction(nameof(Details), new { id });
        }

        TempData["Success"] = $"User {(user.IsActive ? "activated" : "deactivated")} successfully.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateRole(string id, string role, bool add)
    {
        if (string.IsNullOrWhiteSpace(role) || !await _roleManager.RoleExistsAsync(role))
        {
            TempData["Error"] = "Select a valid role.";
            return RedirectToAction(nameof(Details), new { id });
        }

        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
            return NotFound();

        IdentityResult result;
        if (add)
        {
            if (await _userManager.IsInRoleAsync(user, role))
            {
                TempData["Error"] = "The user already has this role.";
                return RedirectToAction(nameof(Details), new { id });
            }

            result = await _userManager.AddToRoleAsync(user, role);
        }
        else
        {
            if (!await _userManager.IsInRoleAsync(user, role))
            {
                TempData["Error"] = "The user does not have this role.";
                return RedirectToAction(nameof(Details), new { id });
            }

            if (string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase) &&
                await _userManager.GetUsersInRoleAsync("Admin") is { Count: <= 1 })
            {
                TempData["Error"] = "The last administrator role cannot be removed.";
                return RedirectToAction(nameof(Details), new { id });
            }

            result = await _userManager.RemoveFromRoleAsync(user, role);
        }

        if (!result.Succeeded)
        {
            TempData["Error"] = string.Join(" ", result.Errors.Select(error => error.Description));
            return RedirectToAction(nameof(Details), new { id });
        }

        TempData["Success"] = $"Role '{role}' {(add ? "added to" : "removed from")} user.";
        return RedirectToAction(nameof(Details), new { id });
    }
}
