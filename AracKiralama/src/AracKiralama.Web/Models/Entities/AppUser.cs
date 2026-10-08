using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace AracKiralama.Web.Models.Entities;

/// <summary>
/// Sistemdeki her kullanıcı (müşteri, personel, admin).
/// E-posta, şifre hash'i ve telefon IdentityUser sınıfından gelir; burada sadece ek alanlar var.
/// </summary>
public class AppUser : IdentityUser
{
    [MaxLength(50)] public string FirstName { get; set; } = "";
    [MaxLength(50)] public string LastName { get; set; } = "";
    [MaxLength(11)] public string? TcNo { get; set; }
    public DateTime? BirthDate { get; set; }
    [MaxLength(30)] public string? LicenseNumber { get; set; }
    public DateTime? LicenseIssueDate { get; set; }

    /// <summary>Kara listedeki müşteri yeni rezervasyon yapamaz.</summary>
    public bool IsBlacklisted { get; set; }
    [MaxLength(250)] public string? BlacklistReason { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();

    [NotMapped] public string FullName => $"{FirstName} {LastName}";
}
