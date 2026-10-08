using System.ComponentModel.DataAnnotations;

namespace AracKiralama.Web.Validation;

/// <summary>
/// T.C. Kimlik Numarası doğrulama algoritması:
///  - 11 hane, ilk hane 0 olamaz
///  - 10. hane = ((1+3+5+7+9. haneler) × 7 − (2+4+6+8. haneler)) mod 10
///  - 11. hane = (ilk 10 hanenin toplamı) mod 10
/// </summary>
public static class TcKimlikNo
{
    public static bool IsValid(string? value)
    {
        if (value is null || value.Length != 11 || !value.All(char.IsAsciiDigit) || value[0] == '0')
            return false;

        var d = value.Select(c => c - '0').ToArray();
        var odd = d[0] + d[2] + d[4] + d[6] + d[8];
        var even = d[1] + d[3] + d[5] + d[7];
        var tenth = ((odd * 7 - even) % 10 + 10) % 10;   // negatif mod'a karşı +10
        var eleventh = d.Take(10).Sum() % 10;

        return d[9] == tenth && d[10] == eleventh;
    }

    /// <summary>Örnek veri için geçerli rastgele bir numara üretir (gerçek bir kişiye ait değildir).</summary>
    public static string Generate(Random random)
    {
        var d = new int[11];
        d[0] = random.Next(1, 10);
        for (var i = 1; i < 9; i++) d[i] = random.Next(0, 10);
        var odd = d[0] + d[2] + d[4] + d[6] + d[8];
        var even = d[1] + d[3] + d[5] + d[7];
        d[9] = ((odd * 7 - even) % 10 + 10) % 10;
        d[10] = d.Take(10).Sum() % 10;
        return string.Concat(d);
    }
}

/// <summary>Model alanına [TcKimlikNo] yazarak form doğrulamasında kullanılır. Boş değer serbesttir.</summary>
[AttributeUsage(AttributeTargets.Property)]
public class TcKimlikNoAttribute : ValidationAttribute
{
    public TcKimlikNoAttribute() : base("Geçerli bir T.C. Kimlik Numarası giriniz.") { }

    public override bool IsValid(object? value)
        => value is null || (value is string s && (s.Length == 0 || TcKimlikNo.IsValid(s)));
}
