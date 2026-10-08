using System.ComponentModel.DataAnnotations;

namespace AracKiralama.Web.Models.Enums;

// Not: [Display(Name = "...")] değerleri arayüzde gösterilen Türkçe isimlerdir.
// Kodda enum.GetDisplayName() ile okunur (bkz. Helpers/EnumExtensions.cs).

public enum VehicleCategory
{
    [Display(Name = "Ekonomi")] Economy,
    [Display(Name = "Orta Sınıf")] Midsize,
    [Display(Name = "SUV")] Suv,
    [Display(Name = "Lüks")] Luxury,
    [Display(Name = "Ticari")] Commercial
}

public enum FuelType
{
    [Display(Name = "Benzin")] Gasoline,
    [Display(Name = "Dizel")] Diesel,
    [Display(Name = "Hibrit")] Hybrid,
    [Display(Name = "Elektrik")] Electric,
    [Display(Name = "LPG")] Lpg
}

public enum TransmissionType
{
    [Display(Name = "Manuel")] Manual,
    [Display(Name = "Otomatik")] Automatic
}

/// <summary>Aracın şu anki fiziksel durumu.</summary>
public enum VehicleStatus
{
    [Display(Name = "Müsait")] Available,
    [Display(Name = "Kirada")] Rented,
    [Display(Name = "Bakımda")] Maintenance,
    [Display(Name = "Pasif")] Passive
}

/// <summary>Rezervasyonun yaşam döngüsü. Geçiş kuralları: Services/ReservationService.cs</summary>
public enum ReservationStatus
{
    [Display(Name = "Onay Bekliyor")] Pending,
    [Display(Name = "Onaylandı")] Approved,
    [Display(Name = "Kirada")] Active,
    [Display(Name = "Tamamlandı")] Completed,
    [Display(Name = "İptal Edildi")] Cancelled,
    [Display(Name = "Reddedildi")] Rejected
}

public enum FuelLevel
{
    [Display(Name = "Boş")] Empty = 0,
    [Display(Name = "1/4")] Quarter = 1,
    [Display(Name = "1/2")] Half = 2,
    [Display(Name = "3/4")] ThreeQuarters = 3,
    [Display(Name = "Dolu")] Full = 4
}

public enum PaymentMethod
{
    [Display(Name = "Nakit")] Cash,
    [Display(Name = "Kredi Kartı")] CreditCard,
    [Display(Name = "Havale / EFT")] BankTransfer
}

public enum PaymentType
{
    [Display(Name = "Kira Bedeli")] Rental,
    [Display(Name = "Depozito")] Deposit,
    [Display(Name = "Ceza / Ek Ücret")] Penalty,
    [Display(Name = "Depozito İadesi")] DepositRefund
}

public enum MaintenanceType
{
    [Display(Name = "Periyodik Bakım")] Periodic,
    [Display(Name = "Onarım")] Repair,
    [Display(Name = "Lastik Değişimi")] Tire,
    [Display(Name = "Muayene")] Inspection,
    [Display(Name = "Temizlik")] Cleaning
}
