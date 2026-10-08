namespace AracKiralama.Web.Models.Entities;

/// <summary>Rezervasyon ↔ Ekstra çoka-çok ilişki tablosu (fiyat snapshot'ı ile).</summary>
public class ReservationExtra
{
    public int ReservationId { get; set; }
    public Reservation? Reservation { get; set; }

    public int ExtraId { get; set; }
    public Extra? Extra { get; set; }

    public decimal DailyPrice { get; set; }
    public decimal Total { get; set; }
}
