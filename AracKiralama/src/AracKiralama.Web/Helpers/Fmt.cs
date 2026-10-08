using System.Globalization;

namespace AracKiralama.Web.Helpers;

/// <summary>
/// Ekranda gösterilen para / tarih biçimleri tek yerden yönetilir.
/// (Uygulama form verilerini "nokta ondalık" ile okur; bu yüzden Türkçe biçim sadece gösterimde kullanılır.)
/// </summary>
public static class Fmt
{
    public static readonly CultureInfo Tr = new("tr-TR");

    public static string Money(decimal value) => value.ToString("N0", Tr) + " ₺";
    public static string MoneyExact(decimal value) => value.ToString("N2", Tr) + " ₺";
    public static string Number(int value) => value.ToString("N0", Tr);
    public static string Date(DateTime value) => value.ToString("dd MMM yyyy", Tr);
    public static string DateTime(DateTime value) => value.ToString("dd MMM yyyy HH:mm", Tr);
    public static string DateTime(DateTime? value) => value is null ? "-" : DateTime(value.Value);
    public static string Month(DateTime value) => value.ToString("MMM yy", Tr);
    public static string DayName(DateTime value) => value.ToString("ddd", Tr);

    /// <summary>datetime-local input'unun beklediği biçim.</summary>
    public static string Input(DateTime value) => value.ToString("yyyy-MM-ddTHH:mm", CultureInfo.InvariantCulture);
}
