using AracKiralama.Web.Data;
using AracKiralama.Web.Models.Entities;
using AracKiralama.Web.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace AracKiralama.Web.Services;

public interface IAvailabilityService
{
    Task<bool> IsAvailableAsync(int vehicleId, DateTime start, DateTime end, int? excludeReservationId = null);
    IQueryable<Vehicle> WhereAvailable(IQueryable<Vehicle> vehicles, DateTime start, DateTime end);
}

/// <summary>
/// "Bu araç bu tarihlerde boş mu?" sorusunun tek cevap noktası.
///
/// Bir araç şu durumlarda DOLUDUR:
///   1) Durumu Pasif ise (filodan çıkarılmış),
///   2) Aynı tarihlerle çakışan Onaylanmış / Kirada bir rezervasyonu varsa,
///   3) Aynı tarihlerle çakışan tamamlanmamış bir bakım kaydı varsa.
///
/// Bakım tarihleri Maintenance tablosundan kontrol edilir; araç durumundaki "Bakımda" bilgisi
/// sadece "şu an" ne olduğunu gösterir, gelecekteki kiralamaları engellemez.
///
/// "Onay Bekliyor" rezervasyonlar aracı bloklamaz: aynı araca birden fazla talep gelebilir,
/// personel birini onayladığında çakışan diğer talepler otomatik reddedilir.
/// </summary>
public class AvailabilityService(AppDbContext db) : IAvailabilityService
{
    /// <summary>Aracı takvimde gerçekten kilitleyen rezervasyon durumları.</summary>
    public static readonly ReservationStatus[] BlockingStatuses = [ReservationStatus.Approved, ReservationStatus.Active];

    /// <summary>
    /// İki tarih aralığı çakışıyor mu? [aStart, aEnd) ve [bStart, bEnd)
    /// Biri tam bittiği anda diğeri başlıyorsa çakışma YOKTUR.
    /// </summary>
    public static bool Overlaps(DateTime aStart, DateTime aEnd, DateTime bStart, DateTime bEnd)
        => aStart < bEnd && bStart < aEnd;

    public async Task<bool> IsAvailableAsync(int vehicleId, DateTime start, DateTime end, int? excludeReservationId = null)
    {
        var vehicle = await db.Vehicles.AsNoTracking().FirstOrDefaultAsync(v => v.Id == vehicleId);
        if (vehicle is null || vehicle.Status == VehicleStatus.Passive)
            return false;

        var hasReservation = await db.Reservations.AnyAsync(r =>
            r.VehicleId == vehicleId
            && r.Id != excludeReservationId
            && BlockingStatuses.Contains(r.Status)
            && r.StartDate < end && start < r.EndDate);     // = Overlaps(...)

        if (hasReservation) return false;

        var hasMaintenance = await db.Maintenances.AnyAsync(m =>
            m.VehicleId == vehicleId
            && !m.IsCompleted
            && m.StartDate < end && start < m.EndDate);

        return !hasMaintenance;
    }

    /// <summary>Araç listesine "sadece bu tarihlerde müsait olanlar" filtresi ekler (tek SQL sorgusu).</summary>
    public IQueryable<Vehicle> WhereAvailable(IQueryable<Vehicle> vehicles, DateTime start, DateTime end)
        => vehicles.Where(v =>
            v.Status != VehicleStatus.Passive
            && !v.Reservations.Any(r => BlockingStatuses.Contains(r.Status) && r.StartDate < end && start < r.EndDate)
            && !v.Maintenances.Any(m => !m.IsCompleted && m.StartDate < end && start < m.EndDate));
}
