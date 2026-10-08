using AracKiralama.Web.Helpers;
using AracKiralama.Web.Models.Entities;
using AracKiralama.Web.Models.Enums;
using Microsoft.Extensions.Options;

namespace AracKiralama.Web.Services;

/// <summary>Bir ekstranın fiyat dökümündeki satırı.</summary>
public record ExtraLine(int ExtraId, string Name, decimal DailyPrice, decimal Total);

/// <summary>Fiyat teklifinin kalem kalem dökümü. Hem ekranda hem rezervasyon kaydında kullanılır.</summary>
public record PriceQuote(
    int Days,
    decimal DailyPrice,
    decimal BaseTotal,
    decimal DiscountRate,
    decimal DiscountAmount,
    IReadOnlyList<ExtraLine> ExtraLines,
    decimal ExtrasTotal,
    decimal OneWayFee,
    decimal Total,
    decimal Deposit);

public interface IPricingService
{
    int CalculateDays(DateTime start, DateTime end);
    PriceQuote Calculate(Vehicle vehicle, DateTime start, DateTime end, IEnumerable<Extra> extras, bool isOneWay);
    decimal CalculateLateFee(decimal dailyPrice, DateTime plannedEnd, DateTime actualReturn);
    decimal CalculateFuelFee(FuelLevel levelOut, FuelLevel levelIn);
    decimal CalculateCancellationFee(Reservation reservation, DateTime now);
}

/// <summary>
/// Fiyat motoru. Veritabanına erişmez; sadece hesap yapar → birim testi çok kolaydır
/// (bkz. tests/AracKiralama.Tests/PricingServiceTests.cs).
/// Oranlar appsettings.json → "Pricing" bölümünden okunur.
/// </summary>
public class PricingService(IOptions<PricingOptions> options) : IPricingService
{
    private readonly PricingOptions _o = options.Value;

    /// <summary>Kiralama gün sayısı: başlayan her 24 saat 1 gündür, en az 1 gün.</summary>
    public int CalculateDays(DateTime start, DateTime end)
    {
        if (end <= start) return 1;
        var days = (int)Math.Ceiling((end - start).TotalHours / 24.0);
        return Math.Max(1, days);
    }

    public PriceQuote Calculate(Vehicle vehicle, DateTime start, DateTime end, IEnumerable<Extra> extras, bool isOneWay)
    {
        var days = CalculateDays(start, end);
        var baseTotal = vehicle.DailyPrice * days;

        // Uzun kiralama indirimi: önce aylık, sonra haftalık kontrol edilir (büyük olan geçerli).
        var discountRate = days >= _o.MonthlyDiscountMinDays ? _o.MonthlyDiscountRate
                         : days >= _o.WeeklyDiscountMinDays ? _o.WeeklyDiscountRate
                         : 0m;
        var discount = Math.Round(baseTotal * discountRate, 2);

        // Ekstralar gün sayısıyla çarpılır, indirim ekstralara uygulanmaz.
        var extraLines = extras
            .Select(x => new ExtraLine(x.Id, x.Name, x.DailyPrice, x.DailyPrice * days))
            .ToList();
        var extrasTotal = extraLines.Sum(x => x.Total);

        var oneWayFee = isOneWay ? _o.OneWayFee : 0m;
        var total = baseTotal - discount + extrasTotal + oneWayFee;

        return new PriceQuote(days, vehicle.DailyPrice, baseTotal, discountRate, discount,
            extraLines, extrasTotal, oneWayFee, total, vehicle.Deposit);
    }

    /// <summary>Geç iade: tolerans süresinden sonra başlayan her gün için günlük fiyat × çarpan.</summary>
    public decimal CalculateLateFee(decimal dailyPrice, DateTime plannedEnd, DateTime actualReturn)
    {
        var late = actualReturn - plannedEnd;
        if (late.TotalMinutes <= _o.LateGraceMinutes) return 0m;

        var lateDays = (int)Math.Ceiling(late.TotalHours / 24.0);
        return lateDays * dailyPrice * _o.LateFeeMultiplier;
    }

    /// <summary>Araç teslim edildiğinden daha az yakıtla dönerse eksik her çeyrek depo ücretlendirilir.</summary>
    public decimal CalculateFuelFee(FuelLevel levelOut, FuelLevel levelIn)
    {
        var missingQuarters = (int)levelOut - (int)levelIn;
        return missingQuarters > 0 ? missingQuarters * _o.FuelFeePerQuarter : 0m;
    }

    /// <summary>Alış saatine belirlenen süreden az kala yapılan iptalde 1 günlük ücret kesilir.</summary>
    public decimal CalculateCancellationFee(Reservation reservation, DateTime now)
    {
        if (reservation.Status != ReservationStatus.Approved) return 0m;
        var hoursLeft = (reservation.StartDate - now).TotalHours;
        return hoursLeft < _o.FreeCancellationHours ? reservation.DailyPrice : 0m;
    }
}
