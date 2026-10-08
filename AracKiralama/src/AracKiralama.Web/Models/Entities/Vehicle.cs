using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AracKiralama.Web.Models.Enums;

namespace AracKiralama.Web.Models.Entities;

public class Vehicle
{
    public int Id { get; set; }

    /// <summary>Örn: "34 ABC 123". Benzersizdir (bkz. AppDbContext).</summary>
    [MaxLength(15)] public string Plate { get; set; } = "";

    public int BrandId { get; set; }
    public Brand? Brand { get; set; }

    [MaxLength(50)] public string Model { get; set; } = "";
    public int Year { get; set; }
    public VehicleCategory Category { get; set; }
    public FuelType Fuel { get; set; }
    public TransmissionType Transmission { get; set; }
    public int Seats { get; set; } = 5;
    [MaxLength(30)] public string Color { get; set; } = "";
    public int Kilometers { get; set; }

    /// <summary>Günlük kiralama ücreti (₺).</summary>
    public decimal DailyPrice { get; set; }

    /// <summary>Teslimde alınan, iadede geri verilen güvence bedeli (₺).</summary>
    public decimal Deposit { get; set; }

    public int MinDriverAge { get; set; } = 21;
    public int MinLicenseYears { get; set; } = 1;

    public VehicleStatus Status { get; set; } = VehicleStatus.Available;

    /// <summary>Aracın şu an bulunduğu şube.</summary>
    public int BranchId { get; set; }
    public Branch? Branch { get; set; }

    [MaxLength(250)] public string? ImageUrl { get; set; }
    [MaxLength(1000)] public string? Description { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
    public ICollection<Maintenance> Maintenances { get; set; } = new List<Maintenance>();

    [NotMapped] public string DisplayName => $"{Brand?.Name} {Model}".Trim();
}
