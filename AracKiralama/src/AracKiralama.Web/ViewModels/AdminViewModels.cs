using System.ComponentModel.DataAnnotations;
using AracKiralama.Web.Helpers;
using AracKiralama.Web.Models.Entities;
using AracKiralama.Web.Models.Enums;
using AracKiralama.Web.Services;
using AracKiralama.Web.Validation;

namespace AracKiralama.Web.ViewModels;

/// <summary>
/// Araç ekleme / düzenleme formu. Entity yerine ayrı bir form modeli kullanıyoruz çünkü:
/// formda dosya (IFormFile) var, doğrulama kuralları ekrana özel, ve kullanıcının
/// değiştirmemesi gereken alanlar (CreatedAt, ilişkiler) formda yer almamalı.
/// </summary>
public class VehicleFormViewModel
{
    public int? Id { get; set; }

    [Display(Name = "Plaka"), Required(ErrorMessage = "Plaka zorunludur."), Plate]
    public string Plate { get; set; } = "";

    [Display(Name = "Marka"), Required(ErrorMessage = "Marka seçiniz.")]
    public int BrandId { get; set; }

    [Display(Name = "Model"), Required(ErrorMessage = "Model zorunludur."), MaxLength(50)]
    public string Model { get; set; } = "";

    [Display(Name = "Model yılı"), Range(2000, 2030, ErrorMessage = "Model yılı 2000-2030 arasında olmalıdır.")]
    public int Year { get; set; } = DateTime.Today.Year;

    [Display(Name = "Kategori")] public VehicleCategory Category { get; set; }
    [Display(Name = "Yakıt")] public FuelType Fuel { get; set; }
    [Display(Name = "Vites")] public TransmissionType Transmission { get; set; }

    [Display(Name = "Koltuk sayısı"), Range(2, 9, ErrorMessage = "Koltuk sayısı 2-9 arasında olmalıdır.")]
    public int Seats { get; set; } = 5;

    [Display(Name = "Renk"), Required(ErrorMessage = "Renk zorunludur."), MaxLength(30)]
    public string Color { get; set; } = "";

    [Display(Name = "Kilometre"), Range(0, 2_000_000, ErrorMessage = "Geçerli bir kilometre giriniz.")]
    public int Kilometers { get; set; }

    [Display(Name = "Günlük fiyat (₺)"), Range(100, 100_000, ErrorMessage = "Günlük fiyat 100 - 100.000 ₺ arasında olmalıdır.")]
    public decimal DailyPrice { get; set; }

    [Display(Name = "Depozito (₺)"), Range(0, 500_000, ErrorMessage = "Geçerli bir depozito giriniz.")]
    public decimal Deposit { get; set; }

    [Display(Name = "Min. sürücü yaşı"), Range(18, 40)]
    public int MinDriverAge { get; set; } = 21;

    [Display(Name = "Min. ehliyet yılı"), Range(0, 20)]
    public int MinLicenseYears { get; set; } = 1;

    [Display(Name = "Durum")] public VehicleStatus Status { get; set; }

    [Display(Name = "Bulunduğu şube"), Required(ErrorMessage = "Şube seçiniz.")]
    public int BranchId { get; set; }

    [Display(Name = "Açıklama"), MaxLength(1000)]
    public string? Description { get; set; }

    public string? ImageUrl { get; set; }

    [Display(Name = "Görsel")]
    public IFormFile? Image { get; set; }

    public List<Brand> Brands { get; set; } = [];
    public List<Branch> Branches { get; set; } = [];
}

public class ReservationListFilter
{
    public ReservationStatus? Status { get; set; }
    public string? Q { get; set; }
    public int? BranchId { get; set; }
    public bool OverdueOnly { get; set; }
    public int Page { get; set; } = 1;
}

public class AdminReservationListViewModel
{
    public required ReservationListFilter Filter { get; init; }
    public required PagedList<Reservation> Reservations { get; init; }
    public List<Branch> Branches { get; init; } = [];
    public Dictionary<ReservationStatus, int> StatusCounts { get; init; } = [];
}

public class DeliverViewModel
{
    public int ReservationId { get; set; }

    [Display(Name = "Teslim kilometresi"), Range(0, 2_000_000, ErrorMessage = "Geçerli bir kilometre giriniz.")]
    public int StartKm { get; set; }

    [Display(Name = "Yakıt seviyesi")] public FuelLevel FuelLevelOut { get; set; } = FuelLevel.Full;

    public Reservation? Reservation { get; set; }
}

public class ReturnViewModel
{
    public int ReservationId { get; set; }

    [Display(Name = "İade kilometresi"), Range(0, 2_000_000, ErrorMessage = "Geçerli bir kilometre giriniz.")]
    public int EndKm { get; set; }

    [Display(Name = "Yakıt seviyesi")] public FuelLevel FuelLevelIn { get; set; } = FuelLevel.Full;

    [Display(Name = "İade zamanı"), Required]
    public DateTime ReturnedAt { get; set; } = DateTime.Now;

    [Display(Name = "Hasar açıklaması"), MaxLength(500)]
    public string? DamageDescription { get; set; }

    [Display(Name = "Hasar bedeli (₺)"), Range(0, 1_000_000, ErrorMessage = "Geçerli bir tutar giriniz.")]
    public decimal DamageCost { get; set; }

    [Display(Name = "Not"), MaxLength(500)]
    public string? Notes { get; set; }

    public Reservation? Reservation { get; set; }
    public decimal EstimatedLateFee { get; set; }
}

public class PaymentFormViewModel
{
    public int ReservationId { get; set; }

    [Display(Name = "Tutar (₺)"), Range(1, 10_000_000, ErrorMessage = "Geçerli bir tutar giriniz.")]
    public decimal Amount { get; set; }

    [Display(Name = "Ödeme yöntemi")] public PaymentMethod Method { get; set; }
    [Display(Name = "Ödeme türü")] public PaymentType Type { get; set; }

    [Display(Name = "Not"), MaxLength(250)]
    public string? Note { get; set; }
}

public class MaintenanceFormViewModel
{
    [Display(Name = "Araç"), Required(ErrorMessage = "Araç seçiniz.")]
    public int VehicleId { get; set; }

    [Display(Name = "Bakım türü")] public MaintenanceType Type { get; set; }

    [Display(Name = "Başlangıç"), Required] public DateTime StartDate { get; set; } = DateTime.Today.AddHours(9);
    [Display(Name = "Bitiş"), Required] public DateTime EndDate { get; set; } = DateTime.Today.AddDays(1).AddHours(18);

    [Display(Name = "Kilometre"), Range(0, 2_000_000)] public int Kilometers { get; set; }
    [Display(Name = "Maliyet (₺)"), Range(0, 10_000_000)] public decimal Cost { get; set; }

    [Display(Name = "Açıklama"), MaxLength(500)] public string? Description { get; set; }

    public List<Vehicle> Vehicles { get; set; } = [];
}

public class StaffCreateViewModel
{
    [Display(Name = "Ad"), Required(ErrorMessage = "Ad zorunludur.")] public string FirstName { get; set; } = "";
    [Display(Name = "Soyad"), Required(ErrorMessage = "Soyad zorunludur.")] public string LastName { get; set; } = "";

    [Display(Name = "E-posta"), Required(ErrorMessage = "E-posta zorunludur."), EmailAddress(ErrorMessage = "Geçerli bir e-posta giriniz.")]
    public string Email { get; set; } = "";

    [Display(Name = "Geçici şifre"), Required(ErrorMessage = "Şifre zorunludur."), MinLength(6, ErrorMessage = "Şifre en az 6 karakter olmalıdır.")]
    public string Password { get; set; } = "";

    [Display(Name = "Rol")] public string Role { get; set; } = Roles.Staff;
}

public record UserRow(AppUser User, string Role);

public record CustomerRow(AppUser User, int RentalCount, decimal TotalSpent, DateTime? LastRental);

// ---------- Doluluk takvimi ----------

public record CalendarBlock(int StartCol, int Span, string Label, string Tooltip, string CssClass, string? Url);

public record CalendarRow(Vehicle Vehicle, List<CalendarBlock> Blocks);

public class CalendarViewModel
{
    public DateTime From { get; init; }
    public int Days { get; init; }
    public int? BranchId { get; init; }
    public VehicleCategory? Category { get; init; }
    public List<DateTime> Dates { get; init; } = [];
    public List<CalendarRow> Rows { get; init; } = [];
    public List<Branch> Branches { get; init; } = [];
}

public class ReportsViewModel
{
    public DateTime From { get; init; }
    public DateTime To { get; init; }
    public required ReportData Report { get; init; }
}

/// <summary>Dashboard'daki küçük gösterge kartı (Views/Shared/_KpiCard.cshtml).</summary>
public record KpiModel(string Label, string Value, string Icon, string Tone, string? Hint = null, string? Url = null);
