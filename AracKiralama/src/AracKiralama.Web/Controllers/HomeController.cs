using AracKiralama.Web.Data;
using AracKiralama.Web.Models.Enums;
using AracKiralama.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AracKiralama.Web.Controllers;

public class HomeController(AppDbContext db) : AppController
{
    public async Task<IActionResult> Index()
    {
        var vehicles = await db.Vehicles
            .Include(v => v.Brand).Include(v => v.Branch)
            .Where(v => v.Status != VehicleStatus.Passive)
            .AsNoTracking()
            .ToListAsync();

        // Her kategoriden en yeni araç + kalan yerlere en uygun fiyatlılar → 8 öne çıkan araç
        var featured = vehicles
            .GroupBy(v => v.Category)
            .Select(g => g.OrderByDescending(v => v.Year).First())
            .Concat(vehicles.OrderBy(v => v.DailyPrice))
            .DistinctBy(v => v.Id)
            .Take(8)
            .ToList();

        var categories = vehicles
            .GroupBy(v => v.Category)
            .OrderBy(g => g.Key)
            .Select(g => new CategorySummary(g.Key, g.Count(), g.Min(v => v.DailyPrice),
                g.OrderByDescending(v => v.DailyPrice).First().ImageUrl ?? ""))
            .ToList();

        var model = new HomeViewModel
        {
            Featured = featured,
            Categories = categories,
            Branches = await db.Branches.OrderBy(b => b.City).ThenBy(b => b.Name).AsNoTracking().ToListAsync(),
            VehicleCount = vehicles.Count,
            CompletedRentals = await db.Reservations.CountAsync(r => r.Status == ReservationStatus.Completed),
        };
        return View(model);
    }

    public IActionResult About() => View();

    public IActionResult Contact() => View();

    public IActionResult Privacy() => View();

    public IActionResult Terms() => View();

    /// <summary>404, 403 gibi durum kodları için özel sayfa (Program.cs → UseStatusCodePagesWithReExecute).</summary>
    [ActionName("StatusCode")]
    public IActionResult HttpStatus(int code) => View("StatusCode", code);

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error() => View();
}
