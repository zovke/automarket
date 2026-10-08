using AracKiralama.Web.Data;
using AracKiralama.Web.Helpers;
using AracKiralama.Web.Models.Entities;
using AracKiralama.Web.Models.Enums;
using AracKiralama.Web.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AracKiralama.Web.Areas.Admin.Controllers;

public class CustomersController(AppDbContext db, UserManager<AppUser> userManager) : AdminController
{
    public async Task<IActionResult> Index(string? q, bool blacklisted = false)
    {
        var customers = await userManager.GetUsersInRoleAsync(Roles.Customer);
        IEnumerable<AppUser> list = customers;

        if (!string.IsNullOrWhiteSpace(q))
        {
            var term = q.Trim();
            list = list.Where(u => u.FullName.Contains(term, StringComparison.CurrentCultureIgnoreCase)
                                   || (u.Email ?? "").Contains(term, StringComparison.OrdinalIgnoreCase)
                                   || (u.PhoneNumber ?? "").Contains(term));
        }
        if (blacklisted) list = list.Where(u => u.IsBlacklisted);

        // Müşteri başına kiralama sayısı ve toplam harcama (tek sorguda)
        var stats = await db.Reservations
            .Where(r => r.Status == ReservationStatus.Completed)
            .GroupBy(r => r.CustomerId)
            .Select(g => new { CustomerId = g.Key, Count = g.Count(), Total = g.Sum(r => r.TotalPrice), Last = g.Max(r => r.StartDate) })
            .ToDictionaryAsync(x => x.CustomerId);

        var rows = list
            .Select(u => stats.TryGetValue(u.Id, out var s)
                ? new CustomerRow(u, s.Count, s.Total, s.Last)
                : new CustomerRow(u, 0, 0, null))
            .OrderByDescending(r => r.TotalSpent)
            .ToList();

        ViewBag.Q = q;
        ViewBag.Blacklisted = blacklisted;
        return View(rows);
    }

    public async Task<IActionResult> Details(string id)
    {
        var user = await db.Users
            .Include(u => u.Reservations).ThenInclude(r => r.Vehicle).ThenInclude(v => v!.Brand)
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == id);
        return user is null ? NotFound() : View(user);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleBlacklist(string id, string? reason)
    {
        var user = await db.Users.FindAsync(id);
        if (user is null) return NotFound();

        user.IsBlacklisted = !user.IsBlacklisted;
        user.BlacklistReason = user.IsBlacklisted ? (string.IsNullOrWhiteSpace(reason) ? "Belirtilmedi" : reason.Trim()) : null;
        await db.SaveChangesAsync();

        ShowSuccess(user.IsBlacklisted ? $"{user.FullName} kara listeye alındı." : $"{user.FullName} kara listeden çıkarıldı.");
        return RedirectToAction(nameof(Details), new { id });
    }
}
