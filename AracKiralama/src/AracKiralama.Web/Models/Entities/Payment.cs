using System.ComponentModel.DataAnnotations;
using AracKiralama.Web.Models.Enums;

namespace AracKiralama.Web.Models.Entities;

public class Payment
{
    public int Id { get; set; }
    public int ReservationId { get; set; }
    public Reservation? Reservation { get; set; }

    public decimal Amount { get; set; }
    public PaymentMethod Method { get; set; }
    public PaymentType Type { get; set; }
    public DateTime PaidAt { get; set; } = DateTime.Now;
    [MaxLength(250)] public string? Note { get; set; }

    public string? ReceivedById { get; set; }
    public AppUser? ReceivedBy { get; set; }
}
