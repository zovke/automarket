using AracKiralama.Web.Data;
using AracKiralama.Web.Models.Enums;
using AracKiralama.Web.Services;
using AracKiralama.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AracKiralama.Web.Controllers;

/// <summary>Müşterinin rezervasyon oluşturma akışı (3 adımlı sihirbaz).</summary>
[Authorize]
public class ReservationsController(AppDbContext db, IReservationService reservations) : AppController
{
    [HttpGet]
    public async Task<IActionResult> Create(int vehicleId, DateTime? start, DateTime? end,
        int? pickupBranchId, int? returnBranchId, List<int>? extraIds)
    {
        var vehicle = await db.Vehicles.Include(v => v.Brand).Include(v => v.Branch)
            .FirstOrDefaultAsync(v => v.Id == vehicleId && v.Status != VehicleStatus.Passive);
        if (vehicle is null) return NotFound();

        // Varsayılan: yarın 10:00'da al, 3 gün sonra aynı saatte iade et
        var defaultStart = DateTime.Today.AddDays(1).AddHours(10);
        var model = new ReservationCreateViewModel
        {
            VehicleId = vehicleId,
            Start = start ?? defaultStart,
            End = end ?? (start ?? defaultStart).AddDays(3),
            PickupBranchId = pickupBranchId ?? vehicle.BranchId,
            ReturnBranchId = returnBranchId ?? pickupBranchId ?? vehicle.BranchId,
            ExtraIds = extraIds ?? [],
        };
        await FillAsync(model);
        return View(model);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ReservationCreateViewModel model)
    {
        if (ModelState.IsValid)
        {
            try
            {
                var reservation = await reservations.CreateAsync(new CreateReservationRequest(
                    CurrentUserId, model.VehicleId, model.PickupBranchId, model.ReturnBranchId,
                    model.Start, model.End, model.ExtraIds, model.Notes));

                return RedirectToAction(nameof(Success), new { id = reservation.Id });
            }
            catch (BusinessRuleException ex)
            {
                ModelState.AddModelError("", ex.Message);
            }
        }

        await FillAsync(model);
        return View(model);
    }

    public async Task<IActionResult> Success(int id)
    {
        var r = await db.Reservations
            .Include(x => x.Vehicle).ThenInclude(v => v!.Brand)
            .Include(x => x.PickupBranch).Include(x => x.ReturnBranch)
            .Include(x => x.Extras).ThenInclude(e => e.Extra)
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id && x.CustomerId == CurrentUserId);
        return r is null ? NotFound() : View(r);
    }

    /// <summary>Formda gösterilecek araç, şube, ekstra ve uygunluk bilgilerini doldurur.</summary>
    private async Task FillAsync(ReservationCreateViewModel model)
    {
        model.Vehicle = await db.Vehicles.Include(v => v.Brand).Include(v => v.Branch)
            .AsNoTracking().FirstOrDefaultAsync(v => v.Id == model.VehicleId);
        model.Branches = await db.Branches.OrderBy(b => b.City).AsNoTracking().ToListAsync();
        model.Extras = await db.Extras.Where(x => x.IsActive).OrderBy(x => x.DailyPrice).AsNoTracking().ToListAsync();
        model.Customer = await db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == CurrentUserId);

        if (model.Customer is not null && model.Vehicle is not null)
            model.EligibilityErrors = ReservationService.CheckDriverEligibility(model.Customer, model.Vehicle, model.Start);
    }
}
