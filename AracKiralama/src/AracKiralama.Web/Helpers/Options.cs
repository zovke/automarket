namespace AracKiralama.Web.Helpers;

/// <summary>appsettings.json → "Pricing" bölümü. Kod değiştirmeden fiyat kuralları ayarlanabilir.</summary>
public class PricingOptions
{
    public int WeeklyDiscountMinDays { get; set; } = 7;
    public decimal WeeklyDiscountRate { get; set; } = 0.10m;
    public int MonthlyDiscountMinDays { get; set; } = 30;
    public decimal MonthlyDiscountRate { get; set; } = 0.20m;

    /// <summary>Alış ve iade şubesi farklıysa alınan sabit ücret.</summary>
    public decimal OneWayFee { get; set; } = 750m;

    /// <summary>Geç iadede her gün için: günlük fiyat × bu çarpan.</summary>
    public decimal LateFeeMultiplier { get; set; } = 1.5m;

    /// <summary>Planlanan iade saatinden sonra ceza başlamadan önceki tolerans (dakika).</summary>
    public int LateGraceMinutes { get; set; } = 59;

    /// <summary>Eksik her 1/4 depo için yakıt ücreti.</summary>
    public decimal FuelFeePerQuarter { get; set; } = 450m;

    /// <summary>Alış saatine bu kadar saatten az kala yapılan iptallerde 1 günlük ücret kesilir.</summary>
    public int FreeCancellationHours { get; set; } = 24;
}

/// <summary>appsettings.json → "Site" bölümü. Firma adını/iletişim bilgilerini buradan değiştirin.</summary>
public class SiteOptions
{
    public string Name { get; set; } = "Rota";
    public string Tagline { get; set; } = "Rent a Car";
    public string Phone { get; set; } = "";
    public string Email { get; set; } = "";
    public string Address { get; set; } = "";
}
