using AracKiralama.Web.Data;
using AracKiralama.Web.Models.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AracKiralama.Web.Areas.Admin.Controllers;

/// <summary>Ek hizmet tanımları (BranchesController ile aynı CRUD kalıbı).</summary>
public class ExtrasController(AppDbContext db) : AdminController
{
    public async Task<IActionResult> Index()
    {
        ViewBag.UsageCounts = await db.ReservationExtras.GroupBy(x => x.ExtraId)
            .Select(g => new { g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Key, x => x.Count);
        return View(await db.Extras.OrderBy(x => x.Name).AsNoTracking().ToListAsync());
    }

    [HttpGet]
    public IActionResult Create() => View("Form", new Extra());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Name,Description,Icon,DailyPrice,IsActive")] Extra extra)
    {
        if (!ModelState.IsValid) return View("Form", extra);
        db.Extras.Add(extra);
        await db.SaveChangesAsync();
        ShowSuccess($"\"{extra.Name}\" eklendi.");
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var extra = await db.Extras.FindAsync(id);
        return extra is null ? NotFound() : View("Form", extra);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description,Icon,DailyPrice,IsActive")] Extra extra)
    {
        if (id != extra.Id) return BadRequest();
        if (!ModelState.IsValid) return View("Form", extra);
        db.Extras.Update(extra);
        await db.SaveChangesAsync();
        ShowSuccess("Ek hizmet güncellendi. (Mevcut rezervasyonların fiyatı değişmez.)");
        return RedirectToAction(nameof(Index));
    }

    /// <summary>Kullanılmış ekstra silinmez, pasif yapılır.</summary>
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var extra = await db.Extras.FindAsync(id);
        if (extra is null) return NotFound();

        if (await db.ReservationExtras.AnyAsync(x => x.ExtraId == id))
        {
            extra.IsActive = false;
            ShowSuccess("Bu hizmet geçmiş rezervasyonlarda kullanıldığı için silinmedi, pasif yapıldı.");
        }
        else
        {
            db.Extras.Remove(extra);
            ShowSuccess("Ek hizmet silindi.");
        }
        await db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }
}
