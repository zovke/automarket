using AracKiralama.Web.Helpers;
using AracKiralama.Web.Models.Entities;
using AracKiralama.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace AracKiralama.Web.Controllers;

/// <summary>
/// Giriş, kayıt ve çıkış. Identity'nin hazır (scaffold) sayfaları yerine sade bir controller
/// yazdık; böylece akışın tamamı tek dosyada okunabiliyor.
/// </summary>
public class AccountController(SignInManager<AppUser> signInManager, UserManager<AppUser> userManager) : AppController
{
    [HttpGet]
    public IActionResult Login(string? returnUrl = null) => View(new LoginViewModel { ReturnUrl = returnUrl });

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var result = await signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);
        if (!result.Succeeded)
        {
            ModelState.AddModelError("", "E-posta veya şifre hatalı.");
            return View(model);
        }

        var user = await userManager.FindByEmailAsync(model.Email);
        ShowSuccess($"Hoş geldiniz, {user!.FirstName}!");

        if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
            return Redirect(model.ReturnUrl);

        // Personel giriş yapınca doğrudan yönetim paneline gitsin
        if (await userManager.IsInRoleAsync(user, Roles.Admin) || await userManager.IsInRoleAsync(user, Roles.Staff))
            return RedirectToAction("Index", "Dashboard", new { area = "Admin" });

        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    public IActionResult Register(string? returnUrl = null) => View(new RegisterViewModel { ReturnUrl = returnUrl });

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var user = new AppUser
        {
            UserName = model.Email,
            Email = model.Email,
            FirstName = model.FirstName.Trim(),
            LastName = model.LastName.Trim(),
            PhoneNumber = model.PhoneNumber,
        };
        var result = await userManager.CreateAsync(user, model.Password);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors) ModelState.AddModelError("", error.Description);
            return View(model);
        }

        await userManager.AddToRoleAsync(user, Roles.Customer);
        await signInManager.SignInAsync(user, isPersistent: true);

        // Rezervasyon için doğum tarihi / ehliyet bilgisi gerekli → profili tamamlamaya yönlendir
        ShowSuccess("Hesabınız oluşturuldu! Rezervasyon yapabilmek için ehliyet bilgilerinizi tamamlayın.");
        return RedirectToAction("Edit", "Profile", new { returnUrl = model.ReturnUrl });
    }

    [HttpPost, ValidateAntiForgeryToken, Authorize]
    public async Task<IActionResult> Logout()
    {
        await signInManager.SignOutAsync();
        ShowSuccess("Çıkış yaptınız.");
        return RedirectToAction("Index", "Home");
    }

    public IActionResult AccessDenied() => View();
}
