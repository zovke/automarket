using AracKiralama.Web.Validation;

namespace AracKiralama.Tests;

public class ValidationTests
{
    [Theory]
    [InlineData("10000000146", true)]     // algoritmaya uygun örnek numara
    [InlineData("10000000147", false)]    // son hane yanlış
    [InlineData("01234567890", false)]    // 0 ile başlayamaz
    [InlineData("1234567890", false)]     // 10 hane
    [InlineData("1234567890a", false)]
    [InlineData(null, false)]
    public void Tc_kimlik_algoritmasi(string? value, bool expected) => Assert.Equal(expected, TcKimlikNo.IsValid(value));

    [Fact]
    public void Uretilen_tc_numaralari_gecerlidir()
    {
        var rnd = new Random(1);
        for (var i = 0; i < 200; i++) Assert.True(TcKimlikNo.IsValid(TcKimlikNo.Generate(rnd)));
    }

    [Theory]
    [InlineData("34 ABC 123", true)]
    [InlineData("06 A 1234", true)]
    [InlineData("35abc12", true)]         // boşluksuz ve küçük harf de kabul
    [InlineData("82 ABC 123", false)]     // il kodu 01-81
    [InlineData("34 ABCD 12", false)]     // en fazla 3 harf
    [InlineData("ABC 123", false)]
    public void Plaka_bicimi(string value, bool expected) => Assert.Equal(expected, Plate.IsValid(value));

    [Fact]
    public void Plaka_standart_bicime_donusturulur() => Assert.Equal("34 ABC 123", Plate.Normalize("34abc123"));
}
