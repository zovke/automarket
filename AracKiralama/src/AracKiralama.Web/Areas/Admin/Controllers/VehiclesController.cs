using AracKiralama.Web.Data;
using AracKiralama.Web.Helpers;
using AracKiralama.Web.Models.Entities;
using AracKiralama.Web.Models.Enums;
using AracKiralama.Web.Services;
using AracKiralama.Web.Validation;
using AracKiralama.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AracKiralama.Web.Areas.Admin.Controllers;

public class VehiclesController(AppDbContext db, IImageService images) : AdminController
{
    public async Task<IActionResult> Index(string? q, VehicleCategory? category, VehicleStatus? status, int? branchId, int page = 1)
    {
        var query = db.Vehicles.Include(v => v.Brand).Include(v => v.Branch).AsNoTracking();

        if (!string.IsNullOrWhiteSpace(q))
        {
            var term = q.Trim().ToUpperInvariant();
            query = query.Where(v => v.Plate.Contains(term) || v.Model.ToUpper().Contains(term) || v.Brand!.Name.ToUpper().Contains(term));
        }
        if (category.HasValue) query = query.Where(v => v.Category == category);
        if (status.HasValue) query = query.Where(v => v.Status == status);
        if (branchId.HasValue) query = query.Where(v => v.BranchId == branchId);

        ViewBag.Q = q;
        ViewBag.Category = category;
        ViewBag.Status = status;
        ViewBag.BranchId = branchId;
        ViewBag.Branches = await db.Branches.OrderBy(b => b.Name).ToListAsync();

        var list = await PagedList<Vehicle>.CreateAsync(query.OrderBy(v => v.Brand!.Name).ThenBy(v => v.Model), page, 15);
        return View(list);
    }

    public async Task<IActionResult> Details(int id)
    {
        var v = await db.Vehicles
            .Include(x => x.Brand).Include(x => x.Branch)
            .Include(x => x.Maintenances.OrderByDescending(m => m.StartDate))
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id);
        if (v is null) return NotFound();

        // Son 15 kiralama ayrı sorguyla alınır (SQLite, Include içinde Take kullanımını desteklemez).
        v.Reservations = await db.Reservations.Include(r => r.Customer)
            .Where(r => r.VehicleId == id)
            .OrderByDescending(r => r.StartDate).Take(15)
            .AsNoTracking().ToListAsync();

        ViewBag.Revenue = await db.Reservations
            .Where(r => r.VehicleId == id && r.Status == ReservationStatus.Completed)
            .SumAsync(r => r.TotalPrice);
        ViewBag.RentalCount = await db.Reservations
            .CountAsync(r => r.VehicleId == id && r.Status == ReservationStatus.Completed);
        return View(v);
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var model = new VehicleFormViewModel { DailyPrice = 1500, Deposit = 5000 };
        await FillListsAsync(model);
        return View("Form", model);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(VehicleFormViewModel form)
    {
        form.Plate = Plate.Normalize(form.Plate);
        if (await db.Vehicles.AnyAsync(v => v.Plate == form.Plate))
            ModelState.AddModelError(nameof(form.Plate), "Bu plaka ile kayıtlı bir araç zaten var.");

        if (!ModelState.IsValid)
        {
            await FillListsAsync(form);
            return View("Form", form);
        }

        var vehicle = new Vehicle();
        if (!await TryApplyAsync(form, vehicle))
        {
            await FillListsAsync(form);
            return View("Form", form);
        }

        db.Vehicles.Add(vehicle);
        await db.SaveChangesAsync();
        ShowSuccess($"{vehicle.Plate} plakalı araç eklendi.");
        return RedirectToAction(nameof(Details), new { id = vehicle.Id });
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var v = await db.Vehicles.FindAsync(id);
        if (v is null) return NotFound();

        var model = new VehicleFormViewModel
        {
            Id = v.Id, Plate = v.Plate, BrandId = v.BrandId, Model = v.Model, Year = v.Year, Category = v.Category,
            Fuel = v.Fuel, Transmission = v.Transmission, Seats = v.Seats, Color = v.Color, Kilometers = v.Kilometers,
            DailyPrice = v.DailyPrice, Deposit = v.Deposit, MinDriverAge = v.MinDriverAge, MinLicenseYears = v.MinLicenseYears,
            Status = v.Status, BranchId = v.BranchId, Description = v.Description, ImageUrl = v.ImageUrl,
        };
        await FillListsAsync(model);
        return View("Form", model);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, VehicleFormViewModel form)
    {
        var vehicle = await db.Vehicles.FindAsync(id);
        if (vehicle is null) return NotFound();

        form.Plate = Plate.Normalize(form.Plate);
        if (await db.Vehicles.AnyAsync(v => v.Plate == form.Plate && v.Id != id))
            ModelState.AddModelError(nameof(form.Plate), "Bu plaka ile kayıtlı başka bir araç var.");

        if (!ModelState.IsValid || !await TryApplyAsync(form, vehicle))
        {
            form.ImageUrl = vehicle.ImageUrl;
            await FillListsAsync(form);
            return View("Form", form);
        }

        await db.SaveChangesAsync();
        ShowSuccess("Araç bilgileri güncellendi.");
        return RedirectToAction(nameof(Details), new { id });
    }

    /// <summary>
    /// Kiralama geçmişi olan araç silinmez (raporlar bozulmasın diye) → "Pasif" yapılır.
    /// Hiç kiralanmamış araç tamamen silinir.
    /// </summary>
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var vehicle = await db.Vehicles.FindAsync(id);
        if (vehicle is null) return NotFound();

        var hasHistory = await db.Reservations.AnyAsync(r => r.VehicleId == id);
        if (hasHistory)
        {
            var hasOpen = await db.Reservations.AnyAsync(r => r.VehicleId == id &&
                (r.Status == ReservationStatus.Approved || r.Status == ReservationStatus.Active));
            if (hasOpen)
            {
                ShowError("Aracın onaylı veya devam eden kiralaması var; önce bunları sonuçlandırın.");
                return RedirectToAction(nameof(Details), new { id });
            }
            vehicle.Status = VehicleStatus.Passive;
            ShowSuccess("Aracın kiralama geçmişi olduğu için silinmedi, pasif duruma alındı.");
        }
        else
        {
            db.Vehicles.Remove(vehicle);
            ShowSuccess("Araç silindi.");
        }
        await db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    /// <summary>Formdaki değerleri entity'ye kopyalar ve varsa görseli yükler.</summary>
    private async Task<bool> TryApplyAsync(VehicleFormViewModel m, Vehicle v)
    {
        if (m.Image is not null)
        {
            try { v.ImageUrl = await images.SaveAsync(m.Image); }
            catch (BusinessRuleException ex)
            {
                ModelState.AddModelError(nameof(m.Image), ex.Message);
                return false;
            }
        }

        v.Plate = m.Plate; v.BrandId = m.BrandId; v.Model = m.Model.Trim(); v.Year = m.Year;
        v.Category = m.Category; v.Fuel = m.Fuel; v.Transmission = m.Transmission; v.Seats = m.Seats;
        v.Color = m.Color.Trim(); v.Kilometers = m.Kilometers; v.DailyPrice = m.DailyPrice; v.Deposit = m.Deposit;
        v.MinDriverAge = m.MinDriverAge; v.MinLicenseYears = m.MinLicenseYears; v.Status = m.Status;
        v.BranchId = m.BranchId; v.Description = m.Description;
        return true;
    }

    private async Task FillListsAsync(VehicleFormViewModel model)
    {
        model.Brands = await db.Brands.OrderBy(b => b.Name).AsNoTracking().ToListAsync();
        model.Branches = await db.Branches.OrderBy(b => b.City).ThenBy(b => b.Name).AsNoTracking().ToListAsync();
    }
}
