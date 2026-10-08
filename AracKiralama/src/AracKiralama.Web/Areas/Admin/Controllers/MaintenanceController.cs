using AracKiralama.Web.Data;
using AracKiralama.Web.Models.Entities;
using AracKiralama.Web.Models.Enums;
using AracKiralama.Web.Services;
using AracKiralama.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AracKiralama.Web.Areas.Admin.Controllers;

public class MaintenanceController(AppDbContext db) : AdminController
{
    public async Task<IActionResult> Index(bool openOnly = false)
    {
        var query = db.Maintenances.Include(m => m.Vehicle).ThenInclude(v => v!.Brand).AsNoTracking();
        if (openOnly) query = query.Where(m => !m.IsCompleted);

        ViewBag.OpenOnly = openOnly;
        ViewBag.TotalCost = await db.Maintenances.SumAsync(m => m.Cost);
        return View(await query.OrderBy(m => m.IsCompleted).ThenByDescending(m => m.StartDate).Take(200).ToListAsync());
    }

    [HttpGet]
    public async Task<IActionResult> Create(int? vehicleId)
    {
        var model = new MaintenanceFormViewModel { VehicleId = vehicleId ?? 0 };
        if (vehicleId.HasValue)
            model.Kilometers = await db.Vehicles.Where(v => v.Id == vehicleId).Select(v => v.Kilometers).FirstOrDefaultAsync();
        model.Vehicles = await VehicleListAsync();
        return View(model);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(MaintenanceFormViewModel model)
    {
        if (model.EndDate <= model.StartDate)
            ModelState.AddModelError(nameof(model.EndDate), "Bitiş tarihi başlangıçtan sonra olmalıdır.");

        // Bakım, onaylı / devam eden bir kiralamayla çakışmasın
        var conflict = await db.Reservations.Where(r => r.VehicleId == model.VehicleId
                && AvailabilityService.BlockingStatuses.Contains(r.Status)
                && r.StartDate < model.EndDate && model.StartDate < r.EndDate)
            .Select(r => r.Code).FirstOrDefaultAsync();
        if (conflict is not null)
            ModelState.AddModelError("", $"Bu tarihlerde aracın {conflict} kodlu kiralaması var.");

        if (!ModelState.IsValid)
        {
            model.Vehicles = await VehicleListAsync();
            return View(model);
        }

        db.Maintenances.Add(new Maintenance
        {
            VehicleId = model.VehicleId, Type = model.Type, StartDate = model.StartDate, EndDate = model.EndDate,
            Kilometers = model.Kilometers, Cost = model.Cost, Description = model.Description,
        });

        // Bakım şu an başlıyorsa araç durumunu güncelle
        if (model.StartDate <= DateTime.Now && DateTime.Now < model.EndDate)
        {
            var vehicle = await db.Vehicles.FindAsync(model.VehicleId);
            if (vehicle is { Status: VehicleStatus.Available }) vehicle.Status = VehicleStatus.Maintenance;
        }

        await db.SaveChangesAsync();
        ShowSuccess("Bakım kaydı oluşturuldu. Bu tarihlerde araç kiralamaya kapalı.");
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Complete(int id)
    {
        var m = await db.Maintenances.Include(x => x.Vehicle).FirstOrDefaultAsync(x => x.Id == id);
        if (m is null) return NotFound();

        m.IsCompleted = true;
        if (m.EndDate > DateTime.Now) m.EndDate = DateTime.Now;
        if (m.Vehicle!.Status == VehicleStatus.Maintenance) m.Vehicle.Status = VehicleStatus.Available;

        await db.SaveChangesAsync();
        ShowSuccess($"{m.Vehicle.Plate} bakımı tamamlandı, araç tekrar kiralamaya açık.");
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var m = await db.Maintenances.Include(x => x.Vehicle).FirstOrDefaultAsync(x => x.Id == id);
        if (m is null) return NotFound();
        if (!m.IsCompleted && m.Vehicle!.Status == VehicleStatus.Maintenance) m.Vehicle.Status = VehicleStatus.Available;
        db.Maintenances.Remove(m);
        await db.SaveChangesAsync();
        ShowSuccess("Bakım kaydı silindi.");
        return RedirectToAction(nameof(Index));
    }

    private Task<List<Vehicle>> VehicleListAsync() => db.Vehicles.Include(v => v.Brand)
        .Where(v => v.Status != VehicleStatus.Passive)
        .OrderBy(v => v.Brand!.Name).ThenBy(v => v.Model)
        .AsNoTracking().ToListAsync();
}
