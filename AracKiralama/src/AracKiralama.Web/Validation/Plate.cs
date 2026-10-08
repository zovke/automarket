using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace AracKiralama.Web.Validation;

/// <summary>
/// Türkiye plaka biçimi: il kodu (01-81) + 1-3 harf + 2-4 rakam. Örn: "34 ABC 123", "06 A 1234".
/// </summary>
public static partial class Plate
{
    [GeneratedRegex(@"^(0[1-9]|[1-7][0-9]|8[01]) ?([A-Z]{1,3}) ?(\d{2,4})$")]
    private static partial Regex PlateRegex();

    public static bool IsValid(string? value) => value is not null && PlateRegex().IsMatch(Normalize(value));

    /// <summary>"34abc123" → "34 ABC 123"</summary>
    public static string Normalize(string value)
    {
        var compact = new string(value.ToUpperInvariant().Where(char.IsLetterOrDigit).ToArray());
        var m = PlateRegex().Match(compact);
        return m.Success ? $"{m.Groups[1].Value} {m.Groups[2].Value} {m.Groups[3].Value}" : value.Trim().ToUpperInvariant();
    }
}

[AttributeUsage(AttributeTargets.Property)]
public class PlateAttribute : ValidationAttribute
{
    public PlateAttribute() : base("Geçerli bir plaka giriniz (örn: 34 ABC 123).") { }

    public override bool IsValid(object? value) => value is string s && Plate.IsValid(s);
}
