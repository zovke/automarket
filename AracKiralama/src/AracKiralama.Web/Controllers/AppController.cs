using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

namespace AracKiralama.Web.Controllers;

/// <summary>
/// Tüm controller'ların ortak atası. Bildirim (toast) gösterme ve giriş yapan kullanıcının
/// kimliğini alma gibi tekrar eden işleri tek yerde toplar.
/// </summary>
public abstract class AppController : Controller
{
    /// <summary>Giriş yapan kullanıcının Id'si (giriş yoksa boş string).</summary>
    protected string CurrentUserId => User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";

    /// <summary>Bir sonraki sayfada yeşil bildirim gösterir (Views/Shared/_Toast.cshtml).</summary>
    protected void ShowSuccess(string message) => TempData["Success"] = message;

    /// <summary>Bir sonraki sayfada kırmızı bildirim gösterir.</summary>
    protected void ShowError(string message) => TempData["Error"] = message;

    /// <summary>PDF'i indirmeye zorlamadan tarayıcıda açar (kullanıcı oradan kaydedebilir).</summary>
    protected IActionResult InlinePdf(byte[] pdf, string fileName)
    {
        Response.Headers.ContentDisposition = $"inline; filename=\"{fileName}\"";
        return File(pdf, "application/pdf");
    }
}
