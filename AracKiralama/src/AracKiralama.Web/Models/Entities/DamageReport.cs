using System.ComponentModel.DataAnnotations;

namespace AracKiralama.Web.Models.Entities;

/// <summary>İade sırasında tespit edilen hasar.</summary>
public class DamageReport
{
    public int Id { get; set; }
    public int ReservationId { get; set; }
    public Reservation? Reservation { get; set; }

    [MaxLength(500)] public string Description { get; set; } = "";
    public decimal Cost { get; set; }
    [MaxLength(250)] public string? PhotoUrl { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}
