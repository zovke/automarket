using AracKiralama.Web.Data;
using AracKiralama.Web.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace AracKiralama.Web.Services;

/// <summary>
/// Arka planda sürekli çalışan servis (BackgroundService).
/// Uygulama açılınca ve sonra her saat başı, iade tarihi geçtiği halde hâlâ müşteride olan
/// kiralamaları "gecikmiş" olarak işaretler. Dashboard'daki kırmızı uyarı buradan beslenir.
/// </summary>
public class OverdueReservationWorker(IServiceScopeFactory scopeFactory, ILogger<OverdueReservationWorker> logger) : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromHours(1);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(Interval);
        do
        {
            try
            {
                var count = await MarkOverdueAsync(stoppingToken);
                if (count > 0) logger.LogInformation("{Count} kiralama gecikmiş olarak işaretlendi.", count);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.LogError(ex, "Gecikme kontrolü sırasında hata oluştu.");
            }
        } while (await timer.WaitForNextTickAsync(stoppingToken));
    }

    private async Task<int> MarkOverdueAsync(CancellationToken ct)
    {
        // BackgroundService tekil (singleton) çalışır; DbContext ise istek başına oluşturulur.
        // Bu yüzden her çalışmada yeni bir "scope" açıp DbContext'i oradan alıyoruz.
        using var scope = scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var now = DateTime.Now;

        return await db.Reservations
            .Where(r => r.Status == ReservationStatus.Active && !r.IsOverdue && r.EndDate < now)
            .ExecuteUpdateAsync(s => s.SetProperty(r => r.IsOverdue, true), ct);
    }
}
