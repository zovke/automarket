using AracKiralama.Web.Data;
using AracKiralama.Web.Models.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AracKiralama.Web.ViewComponents;

/// <summary>
/// Yönetim menüsündeki "onay bekleyen talep sayısı" rozeti.
/// ViewComponent = kendi verisini kendisi çeken küçük, tekrar kullanılabilir arayüz parçası.
/// Kullanım: @await Component.InvokeAsync("PendingBadge")
/// Görünüm: Views/Shared/Components/PendingBadge/Default.cshtml
/// </summary>
public class PendingBadgeViewComponent(AppDbContext db) : ViewComponent
{
    public async Task<IViewComponentResult> InvokeAsync()
    {
        var count = await db.Reservations.CountAsync(r => r.Status == ReservationStatus.Pending);
        return View(count);
    }
}
