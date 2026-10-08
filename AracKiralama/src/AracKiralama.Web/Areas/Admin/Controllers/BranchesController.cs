using AracKiralama.Web.Data;
using AracKiralama.Web.Models.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AracKiralama.Web.Areas.Admin.Controllers;

/// <summary>
/// Şube tanımları — klasik CRUD (Listele / Ekle / Düzenle / Sil) örneği.
/// Yeni bir tanım tablosu (ör. Marka) eklemek isterseniz bu dosyayı kopyalayıp uyarlayabilirsiniz.
/// </summary>
public class BranchesController(AppDbContext db) : AdminController
{
    public async Task<IActionResult> Index()
    {
        ViewBag.VehicleCounts = await db.Vehicles.GroupBy(v => v.BranchId)
            .Select(g => new { g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Key, x => x.Count);
        return View(await db.Branches.OrderBy(b => b.City).ThenBy(b => b.Name).AsNoTracking().ToListAsync());
    }

    [HttpGet]
    public IActionResult Create() => View("Form", new Branch());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Name,City,Address,Phone")] Branch branch)
    {
        if (!ModelState.IsValid) return View("Form", branch);
        db.Branches.Add(branch);
        await db.SaveChangesAsync();
        ShowSuccess($"\"{branch.Name}\" şubesi eklendi.");
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var branch = await db.Branches.FindAsync(id);
        return branch is null ? NotFound() : View("Form", branch);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,Name,City,Address,Phone")] Branch branch)
    {
        if (id != branch.Id) return BadRequest();
        if (!ModelState.IsValid) return View("Form", branch);
        db.Branches.Update(branch);
        await db.SaveChangesAsync();
        ShowSuccess("Şube güncellendi.");
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var branch = await db.Branches.FindAsync(id);
        if (branch is null) return NotFound();

        var inUse = await db.Vehicles.AnyAsync(v => v.BranchId == id)
                    || await db.Reservations.AnyAsync(r => r.PickupBranchId == id || r.ReturnBranchId == id);
        if (inUse)
        {
            ShowError("Bu şubeye bağlı araç veya rezervasyon olduğu için silinemez.");
            return RedirectToAction(nameof(Index));
        }

        db.Branches.Remove(branch);
        await db.SaveChangesAsync();
        ShowSuccess("Şube silindi.");
        return RedirectToAction(nameof(Index));
    }
}
