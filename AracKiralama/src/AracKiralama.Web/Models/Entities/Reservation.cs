using System.ComponentModel.DataAnnotations;
using AracKiralama.Web.Models.Enums;

namespace AracKiralama.Web.Models.Entities;

/// <summary>
/// Bir müşterinin bir aracı belirli tarihler arasında kiralaması.
/// Fiyat alanları oluşturulduğu anda "snapshot" olarak kaydedilir; araç fiyatı sonradan
/// değişse bile eski rezervasyonların tutarı bozulmaz.
/// </summary>
public class Reservation
{
    public int Id { get; set; }

    /// <summary>Müşteriye gösterilen kod, örn: "RZ-2026-0042".</summary>
    [MaxLength(20)] public string Code { get; set; } = "";

    public string CustomerId { get; set; } = "";
    public AppUser? Customer { get; set; }

    public int VehicleId { get; set; }
    public Vehicle? Vehicle { get; set; }

    public int PickupBranchId { get; set; }
    public Branch? PickupBranch { get; set; }
    public int ReturnBranchId { get; set; }
    public Branch? ReturnBranch { get; set; }

    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public DateTime? DeliveredAt { get; set; }
    public DateTime? ActualReturnDate { get; set; }

    public ReservationStatus Status { get; set; } = ReservationStatus.Pending;

    /// <summary>İade tarihi geçtiği halde araç teslim edilmediyse arka plan servisi true yapar.</summary>
    public bool IsOverdue { get; set; }

    // ---- Fiyat dökümü (₺) ----
    public int TotalDays { get; set; }
    public decimal DailyPrice { get; set; }
    public decimal BaseTotal { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal ExtrasTotal { get; set; }
    public decimal OneWayFee { get; set; }
    public decimal LateFee { get; set; }
    public decimal FuelFee { get; set; }
    public decimal DamageFee { get; set; }
    public decimal CancellationFee { get; set; }
    public decimal Deposit { get; set; }
    public decimal TotalPrice { get; set; }

    // ---- Teslim / iade bilgileri ----
    public int? StartKm { get; set; }
    public int? EndKm { get; set; }
    public FuelLevel? FuelLevelOut { get; set; }
    public FuelLevel? FuelLevelIn { get; set; }

    [MaxLength(500)] public string? Notes { get; set; }

    /// <summary>Ret / iptal sebebi gibi son durum açıklaması.</summary>
    [MaxLength(250)] public string? StatusReason { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    /// <summary>Son işlemi yapan personel.</summary>
    public string? HandledById { get; set; }
    public AppUser? HandledBy { get; set; }

    public ICollection<ReservationExtra> Extras { get; set; } = new List<ReservationExtra>();
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
    public ICollection<DamageReport> DamageReports { get; set; } = new List<DamageReport>();

    /// <summary>Tahsil edilen kira + ceza tutarı (depozito hareketleri hariç). Payments yüklenmiş olmalı.</summary>
    public decimal PaidAmount => Payments
        .Where(p => p.Type is PaymentType.Rental or PaymentType.Penalty)
        .Sum(p => p.Amount);

    public decimal Balance => TotalPrice - PaidAmount;

    /// <summary>Müşteri ya da personel tarafından hâlâ iptal edilebilir mi?</summary>
    public bool CanBeCancelled => Status is ReservationStatus.Pending or ReservationStatus.Approved;
}
