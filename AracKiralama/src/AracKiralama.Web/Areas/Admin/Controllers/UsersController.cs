using AracKiralama.Web.Helpers;
using AracKiralama.Web.Models.Entities;
using AracKiralama.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace AracKiralama.Web.Areas.Admin.Controllers;

/// <summary>Personel hesapları ve roller. Sadece Admin rolü girebilir (personel göremez).</summary>
[Authorize(Roles = Roles.Admin)]
public class UsersController(UserManager<AppUser> userManager) : AdminController
{
    public async Task<IActionResult> Index()
    {
        var rows = new List<UserRow>();
        foreach (var role in new[] { Roles.Admin, Roles.Staff })
            rows.AddRange((await userManager.GetUsersInRoleAsync(role)).Select(u => new UserRow(u, role)));
        return View(rows.OrderBy(r => r.Role).ThenBy(r => r.User.FirstName).ToList());
    }

    [HttpGet]
    public IActionResult Create() => View(new StaffCreateViewModel());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(StaffCreateViewModel model)
    {
        if (model.Role is not (Roles.Admin or Roles.Staff))
            ModelState.AddModelError(nameof(model.Role), "Geçersiz rol.");
        if (!ModelState.IsValid) return View(model);

        var user = new AppUser
        {
            UserName = model.Email, Email = model.Email, EmailConfirmed = true,
            FirstName = model.FirstName.Trim(), LastName = model.LastName.Trim(),
        };
        var result = await userManager.CreateAsync(user, model.Password);
        if (!result.Succeeded)
        {
            foreach (var e in result.Errors) ModelState.AddModelError("", e.Description);
            return View(model);
        }
        await userManager.AddToRoleAsync(user, model.Role);
        ShowSuccess($"{user.FullName} ({model.Role}) hesabı oluşturuldu.");
        return RedirectToAction(nameof(Index));
    }

    /// <summary>Admin ↔ Personel rolü değiştirir. Kişi kendi rolünü değiştiremez.</summary>
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleRole(string id)
    {
        if (id == CurrentUserId)
        {
            ShowError("Kendi rolünüzü değiştiremezsiniz.");
            return RedirectToAction(nameof(Index));
        }
        var user = await userManager.FindByIdAsync(id);
        if (user is null) return NotFound();

        var isAdmin = await userManager.IsInRoleAsync(user, Roles.Admin);
        await userManager.RemoveFromRoleAsync(user, isAdmin ? Roles.Admin : Roles.Staff);
        await userManager.AddToRoleAsync(user, isAdmin ? Roles.Staff : Roles.Admin);
        ShowSuccess($"{user.FullName} artık {(isAdmin ? Roles.Staff : Roles.Admin)}.");
        return RedirectToAction(nameof(Index));
    }
}
