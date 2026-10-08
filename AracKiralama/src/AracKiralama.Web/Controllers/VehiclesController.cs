using AracKiralama.Web.Data;
using AracKiralama.Web.Helpers;
using AracKiralama.Web.Models.Enums;
using AracKiralama.Web.Services;
using AracKiralama.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AracKiralama.Web.Controllers;

public class VehiclesController(AppDbContext db, IAvailabilityService availability, IPricingService pricing) : AppController
{
    private const int PageSize = 12;

    /// <summary>Araç listesi + filtreler. Tüm filtreler URL'den gelir: /Vehicles?Category=Suv&amp;Start=...</summary>
    public async Task<IActionResult> Index(VehicleFilter filter)
    {
        var query = db.Vehicles
            .Include(v => v.Brand).Include(v => v.Branch)
            .Where(v => v.Status != VehicleStatus.Passive)
            .AsNoTracking();

        if (filter.PickupBranchId.HasValue) query = query.Where(v => v.BranchId == filter.PickupBranchId);
        if (filter.Category.HasValue) query = query.Where(v => v.Category == filter.Category);
        if (filter.Transmission.HasValue) query = query.Where(v => v.Transmission == filter.Transmission);
        if (filter.Fuel.HasValue) query = query.Where(v => v.Fuel == filter.Fuel);
        if (filter.MinSeats.HasValue) query = query.Where(v => v.Seats >= filter.MinSeats);
        if (filter.MaxPrice.HasValue) query = query.Where(v => v.DailyPrice <= filter.MaxPrice);

        // Tarih seçildiyse sadece o tarihlerde boş olan araçlar
        if (filter.HasDates) query = availability.WhereAvailable(query, filter.Start!.Value, filter.End!.Value);

        query = filter.Sort switch
        {
            "price-desc" => query.OrderByDescending(v => v.DailyPrice),
            "newest" => query.OrderByDescending(v => v.Year).ThenBy(v => v.DailyPrice),
            _ => query.OrderBy(v => v.DailyPrice),
        };

        var model = new VehicleListViewModel
        {
            Filter = filter,
            Vehicles = await PagedList<Models.Entities.Vehicle>.CreateAsync(query, filter.Page, PageSize),
            Branches = await db.Branches.OrderBy(b => b.City).AsNoTracking().ToListAsync(),
            Days = filter.HasDates ? pricing.CalculateDays(filter.Start!.Value, filter.End!.Value) : null,
        };
        return View(model);
    }

    public async Task<IActionResult> Details(int id, VehicleFilter search)
    {
        var vehicle = await db.Vehicles
            .Include(v => v.Brand).Include(v => v.Branch)
            .AsNoTracking()
            .FirstOrDefaultAsync(v => v.Id == id && v.Status != VehicleStatus.Passive);
        if (vehicle is null) return NotFound();

        // Aynı kategoride fiyatı en yakın 4 araç (sıralama bellekte yapılır, kategori başına az araç var)
        var similar = (await db.Vehicles
                .Include(v => v.Brand).Include(v => v.Branch)
                .Where(v => v.Category == vehicle.Category && v.Id != id && v.Status != VehicleStatus.Passive)
                .AsNoTracking()
                .ToListAsync())
            .OrderBy(v => Math.Abs(v.DailyPrice - vehicle.DailyPrice))
            .Take(4)
            .ToList();

        var model = new VehicleDetailsViewModel
        {
            Vehicle = vehicle,
            Search = search,
            Extras = await db.Extras.Where(x => x.IsActive).OrderBy(x => x.DailyPrice).AsNoTracking().ToListAsync(),
            Branches = await db.Branches.OrderBy(b => b.City).AsNoTracking().ToListAsync(),
            Similar = similar,
            IsAvailable = search.HasDates ? await availability.IsAvailableAsync(id, search.Start!.Value, search.End!.Value) : null,
        };
        return View(model);
    }
}
