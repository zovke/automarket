using AracKiralama.Web.Models.Entities;
using AracKiralama.Web.Models.Enums;

namespace AracKiralama.Tests;

/// <summary>Fiyat motorunun kuralları. Veritabanı gerekmez, sadece hesap.</summary>
public class PricingServiceTests
{
    private readonly Web.Services.PricingService _pricing = TestDb.Pricing();
    private static readonly Vehicle Car = new() { DailyPrice = 1000, Deposit = 3000 };
    private static readonly DateTime Start = new(2026, 11, 1, 10, 0, 0);

    [Theory]
    [InlineData(24, 1)]     // tam 1 gün
    [InlineData(25, 2)]     // 1 saat aşım → 2 gün
    [InlineData(1, 1)]      // 1 saatlik kiralama bile 1 gün
    [InlineData(72, 3)]
    public void Gun_sayisi_baslayan_her_24_saat_icin_artar(int hours, int expectedDays)
        => Assert.Equal(expectedDays, _pricing.CalculateDays(Start, Start.AddHours(hours)));

    [Fact]
    public void Kisa_kiralamada_indirim_yoktur()
    {
        var q = _pricing.Calculate(Car, Start, Start.AddDays(3), [], isOneWay: false);
        Assert.Equal(3, q.Days);
        Assert.Equal(3000, q.BaseTotal);
        Assert.Equal(0, q.DiscountAmount);
        Assert.Equal(3000, q.Total);
        Assert.Equal(3000, q.Deposit);
    }

    [Fact]
    public void Yedi_gun_ve_uzeri_yuzde_10_indirim()
    {
        var q = _pricing.Calculate(Car, Start, Start.AddDays(7), [], isOneWay: false);
        Assert.Equal(0.10m, q.DiscountRate);
        Assert.Equal(700, q.DiscountAmount);
        Assert.Equal(6300, q.Total);
    }

    [Fact]
    public void Otuz_gun_ve_uzeri_yuzde_20_indirim()
    {
        var q = _pricing.Calculate(Car, Start, Start.AddDays(30), [], isOneWay: false);
        Assert.Equal(0.20m, q.DiscountRate);
        Assert.Equal(24_000, q.Total);
    }

    [Fact]
    public void Ekstralar_gun_ile_carpilir_ve_indirime_girmez()
    {
        var extras = new[] { new Extra { Id = 1, Name = "Kasko", DailyPrice = 300 }, new Extra { Id = 2, Name = "GPS", DailyPrice = 100 } };
        var q = _pricing.Calculate(Car, Start, Start.AddDays(7), extras, isOneWay: false);

        Assert.Equal(2800, q.ExtrasTotal);                 // (300 + 100) × 7
        Assert.Equal(7000 - 700 + 2800, q.Total);          // indirim sadece kira bedeline
        Assert.Equal(2, q.ExtraLines.Count);
    }

    [Fact]
    public void Farkli_subeye_iade_tek_yon_ucreti_ekler()
    {
        var q = _pricing.Calculate(Car, Start, Start.AddDays(2), [], isOneWay: true);
        Assert.Equal(750, q.OneWayFee);
        Assert.Equal(2750, q.Total);
    }

    [Fact]
    public void Tolerans_icindeki_gecikme_ucretsizdir()
        => Assert.Equal(0, _pricing.CalculateLateFee(1000, Start, Start.AddMinutes(45)));

    [Fact]
    public void Gec_iade_her_gun_icin_bir_bucuk_kat_ucretlenir()
    {
        Assert.Equal(1500, _pricing.CalculateLateFee(1000, Start, Start.AddHours(3)));    // 1 gün geç
        Assert.Equal(3000, _pricing.CalculateLateFee(1000, Start, Start.AddHours(30)));   // 2 gün geç
    }

    [Theory]
    [InlineData(FuelLevel.Full, FuelLevel.Full, 0)]
    [InlineData(FuelLevel.Full, FuelLevel.Half, 900)]          // 2 çeyrek eksik × 450
    [InlineData(FuelLevel.Half, FuelLevel.Full, 0)]            // fazla yakıt ücretlendirilmez
    public void Eksik_yakit_ceyrek_depo_basina_ucretlenir(FuelLevel outLevel, FuelLevel inLevel, decimal expected)
        => Assert.Equal(expected, _pricing.CalculateFuelFee(outLevel, inLevel));

    [Fact]
    public void Son_24_saatte_iptal_bir_gunluk_ucret_keser()
    {
        var r = new Reservation { Status = ReservationStatus.Approved, DailyPrice = 1000, StartDate = Start };
        Assert.Equal(1000, _pricing.CalculateCancellationFee(r, Start.AddHours(-5)));
        Assert.Equal(0, _pricing.CalculateCancellationFee(r, Start.AddDays(-3)));
    }
}
