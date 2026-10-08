using AracKiralama.Web.Models.Entities;
using AracKiralama.Web.Models.Enums;
using AracKiralama.Web.Services;

namespace AracKiralama.Tests;

/// <summary>"Araç bu tarihlerde boş mu?" kuralları.</summary>
public class AvailabilityServiceTests : IDisposable
{
    private readonly TestDb _t = new();
    private readonly AvailabilityService _availability;

    public AvailabilityServiceTests() => _availability = new AvailabilityService(_t.Db);

    public void Dispose() => _t.Dispose();

    [Fact]
    public void Uc_uca_eklenen_araliklar_cakismaz()
    {
        var a = TestDb.Future(5);
        Assert.False(AvailabilityService.Overlaps(a, a.AddDays(2), a.AddDays(2), a.AddDays(4)));
        Assert.True(AvailabilityService.Overlaps(a, a.AddDays(2), a.AddDays(1), a.AddDays(4)));
        Assert.True(AvailabilityService.Overlaps(a, a.AddDays(10), a.AddDays(2), a.AddDays(3)));   // içinde kalan
    }

    [Fact]
    public async Task Bos_arac_musaittir()
        => Assert.True(await _availability.IsAvailableAsync(_t.Economy.Id, TestDb.Future(5), TestDb.Future(8)));

    [Theory]
    [InlineData(ReservationStatus.Approved, false)]
    [InlineData(ReservationStatus.Active, false)]
    [InlineData(ReservationStatus.Pending, true)]      // bekleyen talep aracı kilitlemez
    [InlineData(ReservationStatus.Cancelled, true)]
    [InlineData(ReservationStatus.Rejected, true)]
    public async Task Sadece_onayli_ve_kiradaki_rezervasyonlar_araci_kilitler(ReservationStatus status, bool expectedAvailable)
    {
        _t.AddReservation(_t.Economy, _t.OtherCustomer, TestDb.Future(5), TestDb.Future(8), status);
        Assert.Equal(expectedAvailable, await _availability.IsAvailableAsync(_t.Economy.Id, TestDb.Future(6), TestDb.Future(7)));
    }

    [Fact]
    public async Task Bir_kiralama_biterken_digeri_baslayabilir()
    {
        _t.AddReservation(_t.Economy, _t.OtherCustomer, TestDb.Future(5), TestDb.Future(8), ReservationStatus.Approved);
        Assert.True(await _availability.IsAvailableAsync(_t.Economy.Id, TestDb.Future(8), TestDb.Future(10)));
    }

    [Fact]
    public async Task Tamamlanmamis_bakim_araci_kilitler_tamamlanmis_kilitlemez()
    {
        var m = new Maintenance { Vehicle = _t.Economy, StartDate = TestDb.Future(5), EndDate = TestDb.Future(7) };
        _t.Db.Maintenances.Add(m);
        await _t.Db.SaveChangesAsync();
        Assert.False(await _availability.IsAvailableAsync(_t.Economy.Id, TestDb.Future(6), TestDb.Future(9)));

        m.IsCompleted = true;
        await _t.Db.SaveChangesAsync();
        Assert.True(await _availability.IsAvailableAsync(_t.Economy.Id, TestDb.Future(6), TestDb.Future(9)));
    }

    [Fact]
    public async Task Pasif_arac_hicbir_tarihte_musait_degildir()
    {
        _t.Economy.Status = VehicleStatus.Passive;
        await _t.Db.SaveChangesAsync();
        Assert.False(await _availability.IsAvailableAsync(_t.Economy.Id, TestDb.Future(30), TestDb.Future(31)));
    }

    [Fact]
    public void Liste_filtresi_dolu_araci_cikarir()
    {
        _t.AddReservation(_t.Luxury, _t.OtherCustomer, TestDb.Future(5), TestDb.Future(8), ReservationStatus.Approved);

        var available = _availability.WhereAvailable(_t.Db.Vehicles, TestDb.Future(6), TestDb.Future(7)).Select(v => v.Id).ToList();

        Assert.Contains(_t.Economy.Id, available);
        Assert.DoesNotContain(_t.Luxury.Id, available);
    }
}
