using AracKiralama.Web.Data;
using AracKiralama.Web.Helpers;
using AracKiralama.Web.Models.Entities;
using AracKiralama.Web.Models.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AracKiralama.Web.Areas.Admin.Controllers;

/// <summary>Kasa hareketleri: tüm tahsilatlar ve depozito hareketleri.</summary>
public class PaymentsController(AppDbContext db) : AdminController
{
    public async Task<IActionResult> Index(DateTime? from, DateTime? to, PaymentMethod? method, PaymentType? type, int page = 1)
    {
        var start = (from ?? DateTime.Today.AddDays(-30)).Date;
        var end = (to ?? DateTime.Today).Date.AddDays(1);

        var query = db.Payments
            .Include(p => p.Reservation).ThenInclude(r => r!.Customer)
            .Include(p => p.ReceivedBy)
            .Where(p => p.PaidAt >= start && p.PaidAt < end)
            .AsNoTracking();
        if (method.HasValue) query = query.Where(p => p.Method == method);
        if (type.HasValue) query = query.Where(p => p.Type == type);

        var byMethod = await query.GroupBy(p => p.Method)
            .Select(g => new { g.Key, Total = g.Sum(p => p.Amount) })
            .ToDictionaryAsync(x => x.Key, x => x.Total);

        ViewBag.From = start;
        ViewBag.To = end.AddDays(-1);
        ViewBag.Method = method;
        ViewBag.Type = type;
        ViewBag.ByMethod = byMethod;
        ViewBag.Total = byMethod.Values.Sum();

        return View(await PagedList<Payment>.CreateAsync(query.OrderByDescending(p => p.PaidAt), page, 25));
    }
}
