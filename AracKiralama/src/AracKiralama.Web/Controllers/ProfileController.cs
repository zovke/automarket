using AracKiralama.Web.Data;
using AracKiralama.Web.Models.Entities;
using AracKiralama.Web.Models.Enums;
using AracKiralama.Web.Services;
using AracKiralama.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AracKiralama.Web.Controllers;

/// <summary>"Hesabım": rezervasyonlarım, profil bilgileri, iptal ve sözleşme indirme.</summary>
[Authorize]
public class ProfileController(AppDbContext db, UserManager<AppUser> userManager,
    IReservationService reservations, IContractPdfService contracts) : AppController
{
    public async Task<IActionResult> Index()
    {
        var user = await userManager.GetUserAsync(User);
        if (user is null) return Challenge();

        var all = await db.Reservations
            .Where(r => r.CustomerId == user.Id)
            .Include(r => r.Vehicle).ThenInclude(v => v!.Brand)
            .Include(r => r.PickupBranch).Include(r => r.ReturnBranch)
            .OrderByDescending(r => r.StartDate)
            .AsNoTracking()
            .ToListAsync();

        var openStatuses = new[] { ReservationStatus.Pending, ReservationStatus.Approved, ReservationStatus.Active };
        return View(new MyReservationsViewModel
        {
            User = user,
            Upcoming = all.Where(r => openStatuses.Contains(r.Status)).OrderBy(r => r.StartDate).ToList(),
            Past = all.Where(r => !openStatuses.Contains(r.Status)).ToList(),
        });
    }

    [HttpGet]
    public async Task<IActionResult> Edit(string? returnUrl = null)
    {
        var user = await userManager.GetUserAsync(User);
        if (user is null) return Challenge();

        return View(new ProfileViewModel
        {
            FirstName = user.FirstName, LastName = user.LastName, Email = user.Email,
            PhoneNumber = user.PhoneNumber, TcNo = user.TcNo, BirthDate = user.BirthDate,
            LicenseNumber = user.LicenseNumber, LicenseIssueDate = user.LicenseIssueDate,
            ReturnUrl = returnUrl,
        });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(ProfileViewModel model)
    {
        if (model.BirthDate > DateTime.Today.AddYears(-18))
            ModelState.AddModelError(nameof(model.BirthDate), "18 yaşından küçükler kayıt olamaz.");
        if (model.LicenseIssueDate > DateTime.Today)
            ModelState.AddModelError(nameof(model.LicenseIssueDate), "Ehliyet tarihi gelecekte olamaz.");
        if (model.BirthDate.HasValue && model.LicenseIssueDate < model.BirthDate.Value.AddYears(17))
            ModelState.AddModelError(nameof(model.LicenseIssueDate), "Ehliyet tarihi doğum tarihiyle uyumlu değil.");

        var user = await userManager.GetUserAsync(User);
        if (user is null) return Challenge();
        if (!ModelState.IsValid)
        {
            model.Email = user.Email;
            return View(model);
        }

        user.FirstName = model.FirstName.Trim();
        user.LastName = model.LastName.Trim();
        user.PhoneNumber = model.PhoneNumber;
        user.TcNo = string.IsNullOrWhiteSpace(model.TcNo) ? null : model.TcNo;
        user.BirthDate = model.BirthDate;
        user.LicenseNumber = model.LicenseNumber;
        user.LicenseIssueDate = model.LicenseIssueDate;
        await userManager.UpdateAsync(user);

        ShowSuccess("Profil bilgileriniz güncellendi.");
        if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
            return Redirect(model.ReturnUrl);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(int id)
    {
        try
        {
            await reservations.CancelAsync(id, CurrentUserId, isStaff: false);
            var fee = await db.Reservations.Where(r => r.Id == id).Select(r => r.CancellationFee).FirstAsync();
            ShowSuccess(fee > 0
                ? $"Rezervasyonunuz iptal edildi. Alış saatine az kaldığı için {Helpers.Fmt.Money(fee)} iptal ücreti uygulanır."
                : "Rezervasyonunuz ücretsiz olarak iptal edildi.");
        }
        catch (BusinessRuleException ex)
        {
            ShowError(ex.Message);
        }
        return RedirectToAction(nameof(Index));
    }

    /// <summary>Müşteri sadece KENDİ rezervasyonunun sözleşmesini indirebilir.</summary>
    public async Task<IActionResult> Contract(int id)
    {
        var isOwner = await db.Reservations.AnyAsync(r => r.Id == id && r.CustomerId == CurrentUserId);
        if (!isOwner) return NotFound();

        var result = await contracts.GenerateAsync(id);
        return result is null ? NotFound() : InlinePdf(result.Value.Pdf, result.Value.FileName);
    }
}
