using System.ComponentModel.DataAnnotations;
using AracKiralama.Web.Helpers;
using AracKiralama.Web.Models.Entities;
using AracKiralama.Web.Models.Enums;

namespace AracKiralama.Web.ViewModels;

/// <summary>
/// Araç arama / filtreleme kriterleri. URL'deki query string'den doldurulur
/// (örn: /Vehicles?Start=...&amp;Category=Suv) → filtreli sayfa linki paylaşılabilir.
/// </summary>
public class VehicleFilter
{
    public DateTime? Start { get; set; }
    public DateTime? End { get; set; }
    public int? PickupBranchId { get; set; }
    public int? ReturnBranchId { get; set; }
    public VehicleCategory? Category { get; set; }
    public TransmissionType? Transmission { get; set; }
    public FuelType? Fuel { get; set; }
    public int? MinSeats { get; set; }
    public decimal? MaxPrice { get; set; }
    public string Sort { get; set; } = "price-asc";
    public int Page { get; set; } = 1;

    public bool HasDates => Start.HasValue && End.HasValue && End > Start;

    /// <summary>Linklerde kullanmak için sadece dolu alanları döndürür.</summary>
    public Dictionary<string, string> ToRouteValues(int? page = null)
    {
        var d = new Dictionary<string, string>();
        if (Start.HasValue) d[nameof(Start)] = Fmt.Input(Start.Value);
        if (End.HasValue) d[nameof(End)] = Fmt.Input(End.Value);
        if (PickupBranchId.HasValue) d[nameof(PickupBranchId)] = PickupBranchId.Value.ToString();
        if (ReturnBranchId.HasValue) d[nameof(ReturnBranchId)] = ReturnBranchId.Value.ToString();
        if (Category.HasValue) d[nameof(Category)] = Category.Value.ToString();
        if (Transmission.HasValue) d[nameof(Transmission)] = Transmission.Value.ToString();
        if (Fuel.HasValue) d[nameof(Fuel)] = Fuel.Value.ToString();
        if (MinSeats.HasValue) d[nameof(MinSeats)] = MinSeats.Value.ToString();
        if (MaxPrice.HasValue) d[nameof(MaxPrice)] = MaxPrice.Value.ToString(System.Globalization.CultureInfo.InvariantCulture);
        if (Sort != "price-asc") d[nameof(Sort)] = Sort;
        if (page is > 1) d[nameof(Page)] = page.Value.ToString();
        return d;
    }

    /// <summary>Sadece tarih + şube bilgisini taşır (araç kartından detay sayfasına geçerken).</summary>
    public Dictionary<string, string> ToSearchRouteValues()
        => ToRouteValues().Where(kv => kv.Key is nameof(Start) or nameof(End) or nameof(PickupBranchId) or nameof(ReturnBranchId))
                          .ToDictionary(kv => kv.Key, kv => kv.Value);
}

public record CategorySummary(VehicleCategory Category, int Count, decimal MinPrice, string ImageUrl);

public class HomeViewModel
{
    public List<Vehicle> Featured { get; set; } = [];
    public List<CategorySummary> Categories { get; set; } = [];
    public List<Branch> Branches { get; set; } = [];
    public int VehicleCount { get; set; }
    public int CompletedRentals { get; set; }
}

public class VehicleListViewModel
{
    public required VehicleFilter Filter { get; init; }
    public required PagedList<Vehicle> Vehicles { get; init; }
    public List<Branch> Branches { get; init; } = [];
    public int? Days { get; init; }
}

public class VehicleCardViewModel
{
    public required Vehicle Vehicle { get; init; }
    public VehicleFilter? Search { get; init; }
    public int? Days { get; init; }
}

public class VehicleDetailsViewModel
{
    public required Vehicle Vehicle { get; init; }
    public required VehicleFilter Search { get; init; }
    public List<Extra> Extras { get; init; } = [];
    public List<Branch> Branches { get; init; } = [];
    public List<Vehicle> Similar { get; init; } = [];
    public bool? IsAvailable { get; init; }
}

/// <summary>Rezervasyon sihirbazının formu.</summary>
public class ReservationCreateViewModel
{
    public int VehicleId { get; set; }

    [Display(Name = "Alış şubesi"), Required(ErrorMessage = "Alış şubesi seçiniz.")]
    public int PickupBranchId { get; set; }

    [Display(Name = "İade şubesi"), Required(ErrorMessage = "İade şubesi seçiniz.")]
    public int ReturnBranchId { get; set; }

    [Display(Name = "Alış tarihi"), Required(ErrorMessage = "Alış tarihi seçiniz.")]
    public DateTime Start { get; set; }

    [Display(Name = "İade tarihi"), Required(ErrorMessage = "İade tarihi seçiniz.")]
    public DateTime End { get; set; }

    public List<int> ExtraIds { get; set; } = [];

    [Display(Name = "Notunuz"), MaxLength(500)]
    public string? Notes { get; set; }

    [Display(Name = "Kiralama koşullarını okudum, kabul ediyorum")]
    [Range(typeof(bool), "true", "true", ErrorMessage = "Devam etmek için kiralama koşullarını kabul etmelisiniz.")]
    public bool AcceptTerms { get; set; }

    // ---- Sadece ekranda gösterim için ----
    public Vehicle? Vehicle { get; set; }
    public List<Branch> Branches { get; set; } = [];
    public List<Extra> Extras { get; set; } = [];
    public List<string> EligibilityErrors { get; set; } = [];
    public AppUser? Customer { get; set; }
}
