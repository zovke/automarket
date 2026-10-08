using AracKiralama.Web.Data;
using AracKiralama.Web.Helpers;
using AracKiralama.Web.Models.Enums;
using AracKiralama.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AracKiralama.Web.Areas.Admin.Controllers;

/// <summary>
/// Doluluk takvimi: her satır bir araç, her sütun bir gün.
/// Rezervasyon ve bakımlar renkli bloklar olarak çizilir (Gantt şeması mantığı).
/// Harici kütüphane kullanılmadı: blokların hangi sütundan başlayıp kaç gün süreceği burada hesaplanır,
/// çizim CSS Grid ile yapılır (Areas/Admin/Views/Calendar/Index.cshtml).
/// </summary>
public class CalendarController(AppDbContext db) : AdminController
{
    public async Task<IActionResult> Index(DateTime? from, int days = 21, int? branchId = null, VehicleCategory? category = null)
    {
        days = Math.Clamp(days, 7, 42);
        var start = (from ?? DateTime.Today.AddDays(-3)).Date;
        var end = start.AddDays(days);

        var vehiclesQuery = db.Vehicles.Include(v => v.Brand)
            .Where(v => v.Status != VehicleStatus.Passive).AsNoTracking();
        if (branchId.HasValue) vehiclesQuery = vehiclesQuery.Where(v => v.BranchId == branchId);
        if (category.HasValue) vehiclesQuery = vehiclesQuery.Where(v => v.Category == category);
        var vehicles = await vehiclesQuery.OrderBy(v => v.Brand!.Name).ThenBy(v => v.Model).ToListAsync();
        var vehicleIds = vehicles.Select(v => v.Id).ToList();

        var shown = new[] { ReservationStatus.Pending, ReservationStatus.Approved, ReservationStatus.Active, ReservationStatus.Completed };
        var reservations = await db.Reservations.Include(r => r.Customer)
            .Where(r => vehicleIds.Contains(r.VehicleId) && shown.Contains(r.Status)
                        && r.StartDate < end && start < (r.ActualReturnDate ?? r.EndDate))
            .AsNoTracking().ToListAsync();
        var maintenances = await db.Maintenances
            .Where(m => vehicleIds.Contains(m.VehicleId) && m.StartDate < end && start < m.EndDate)
            .AsNoTracking().ToListAsync();

        var rows = vehicles.Select(v =>
        {
            var blocks = reservations.Where(r => r.VehicleId == v.Id)
                .Select(r =>
                {
                    var (col, span) = ToColumns(r.StartDate, r.ActualReturnDate ?? r.EndDate, start, days);
                    var css = r.IsOverdue ? "blk-overdue" : r.Status switch
                    {
                        ReservationStatus.Pending => "blk-pending",
                        ReservationStatus.Approved => "blk-approved",
                        ReservationStatus.Active => "blk-active",
                        _ => "blk-completed",
                    };
                    var tip = $"{r.Code} · {r.Customer!.FullName}\n{Fmt.DateTime(r.StartDate)} → {Fmt.DateTime(r.EndDate)}\n{r.Status.GetDisplayName()}";
                    return new CalendarBlock(col, span, r.Customer.FullName, tip, css,
                        Url.Action("Details", "Reservations", new { id = r.Id }));
                })
                .Concat(maintenances.Where(m => m.VehicleId == v.Id).Select(m =>
                {
                    var (col, span) = ToColumns(m.StartDate, m.EndDate, start, days);
                    return new CalendarBlock(col, span, "Bakım", $"{m.Type.GetDisplayName()}\n{Fmt.DateTime(m.StartDate)} → {Fmt.DateTime(m.EndDate)}",
                        "blk-maintenance", Url.Action("Index", "Maintenance"));
                }))
                .OrderBy(b => b.StartCol)
                .ToList();
            return new CalendarRow(v, blocks);
        }).ToList();

        return View(new CalendarViewModel
        {
            From = start, Days = days, BranchId = branchId, Category = category,
            Dates = Enumerable.Range(0, days).Select(i => start.AddDays(i)).ToList(),
            Rows = rows,
            Branches = await db.Branches.OrderBy(b => b.City).AsNoTracking().ToListAsync(),
        });
    }

    /// <summary>Tarih aralığını takvim sütunlarına çevirir: (başlangıç sütunu [1'den], kaç sütun).</summary>
    private static (int Col, int Span) ToColumns(DateTime s, DateTime e, DateTime gridStart, int days)
    {
        var first = Math.Max(0, (s.Date - gridStart).Days);
        var last = Math.Min(days - 1, (e.AddMinutes(-1).Date - gridStart).Days);
        return (first + 1, Math.Max(1, last - first + 1));
    }
}
