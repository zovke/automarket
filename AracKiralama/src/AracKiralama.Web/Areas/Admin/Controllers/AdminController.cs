using AracKiralama.Web.Controllers;
using AracKiralama.Web.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AracKiralama.Web.Areas.Admin.Controllers;

/// <summary>
/// Yönetim paneli controller'larının ortak atası.
/// [Area("Admin")] → adresler /Admin/... ile başlar.
/// [Authorize(Roles = ...)] → sadece Admin ve Personel rolleri girebilir; müşteri girerse "Erişim Engellendi" sayfası.
/// </summary>
[Area("Admin")]
[Authorize(Roles = Roles.AdminOrStaff)]
public abstract class AdminController : AppController;
