using AracKiralama.Web.Models.Enums;
using AracKiralama.Web.Services;
using Microsoft.EntityFrameworkCore;

namespace AracKiralama.Tests;

/// <summary>Rezervasyonun yaşam döngüsü (durum makinesi) ve iş kuralları.</summary>
public class ReservationServiceTests : IDisposable
{
    private readonly TestDb _t = new();
    private readonly ReservationService _service;

    public ReservationServiceTests() => _service = _t.Reservations();

    public void Dispose() => _t.Dispose();

    private CreateReservationRequest Request(string customerId, int vehicleId, int startDay = 10, int days = 3, int? returnBranchId = null, int[]? extras = null)
        => new(customerId, vehicleId, _t.Istanbul.Id, returnBranchId ?? _t.Istanbul.Id,
            TestDb.Future(startDay), TestDb.Future(startDay + days), extras ?? []);

    [Fact]
    public async Task Rezervasyon_olusturulur_kod_ve_fiyat_kaydedilir()
    {
        var r = await _service.CreateAsync(Request(_t.Customer.Id, _t.Economy.Id, extras: [_t.Insurance.Id], returnBranchId: _t.Ankara.Id));

        Assert.Equal(ReservationStatus.Pending, r.Status);
        Assert.Matches(@"^RZ-\d{4}-0001$", r.Code);
        Assert.Equal(3, r.TotalDays);
        Assert.Equal(3000 + 900 + 750, r.TotalPrice);    // kira + kasko + tek yön
        Assert.Single(r.Extras);
    }

    [Fact]
    public async Task Kodlar_sirayla_artar()
    {
        await _service.CreateAsync(Request(_t.Customer.Id, _t.Economy.Id, startDay: 10));
        var second = await _service.CreateAsync(Request(_t.Customer.Id, _t.Economy.Id, startDay: 20));
        Assert.EndsWith("-0002", second.Code);
    }

    [Fact]
    public async Task Gecmis_tarihe_rezervasyon_yapilamaz()
    {
        var req = Request(_t.Customer.Id, _t.Economy.Id) with { Start = DateTime.Now.AddDays(-2), End = DateTime.Now.AddDays(1) };
        var ex = await Assert.ThrowsAsync<BusinessRuleException>(() => _service.CreateAsync(req));
        Assert.Contains("geçmişte", ex.Message);
    }

    [Fact]
    public async Task Genc_surucu_luks_arac_kiralayamaz()
    {
        var ex = await Assert.ThrowsAsync<BusinessRuleException>(() => _service.CreateAsync(Request(_t.YoungCustomer.Id, _t.Luxury.Id)));
        Assert.Contains("27 yaşında", ex.Message);
    }

    [Fact]
    public async Task Kara_listedeki_musteri_rezervasyon_yapamaz()
    {
        _t.Customer.IsBlacklisted = true;
        await _t.Db.SaveChangesAsync();
        await Assert.ThrowsAsync<BusinessRuleException>(() => _service.CreateAsync(Request(_t.Customer.Id, _t.Economy.Id)));
    }

    [Fact]
    public async Task Onayli_rezervasyonla_cakisan_talep_reddedilir()
    {
        _t.AddReservation(_t.Economy, _t.OtherCustomer, TestDb.Future(9), TestDb.Future(12), ReservationStatus.Approved);
        var ex = await Assert.ThrowsAsync<BusinessRuleException>(() => _service.CreateAsync(Request(_t.Customer.Id, _t.Economy.Id)));
        Assert.Contains("müsait değil", ex.Message);
    }

    [Fact]
    public async Task Onaylaninca_cakisan_bekleyen_talepler_otomatik_reddedilir()
    {
        var mine = await _service.CreateAsync(Request(_t.Customer.Id, _t.Economy.Id));
        var other = await _service.CreateAsync(Request(_t.OtherCustomer.Id, _t.Economy.Id, startDay: 11));
        var notOverlapping = await _service.CreateAsync(Request(_t.OtherCustomer.Id, _t.Economy.Id, startDay: 30));

        await _service.ApproveAsync(mine.Id, _t.Staff.Id);

        _t.Db.ChangeTracker.Clear();
        Assert.Equal(ReservationStatus.Approved, (await _t.Db.Reservations.FindAsync(mine.Id))!.Status);
        Assert.Equal(ReservationStatus.Rejected, (await _t.Db.Reservations.FindAsync(other.Id))!.Status);
        Assert.Equal(ReservationStatus.Pending, (await _t.Db.Reservations.FindAsync(notOverlapping.Id))!.Status);
    }

    [Fact]
    public async Task Gecersiz_durum_gecisi_engellenir()
    {
        var r = await _service.CreateAsync(Request(_t.Customer.Id, _t.Economy.Id));

        // Onaylanmamış araç teslim edilemez
        await Assert.ThrowsAsync<BusinessRuleException>(() => _service.DeliverAsync(r.Id, _t.Staff.Id, 20_000, FuelLevel.Full));
        // Teslim edilmemiş araç iade alınamaz
        await Assert.ThrowsAsync<BusinessRuleException>(() =>
            _service.ReturnAsync(r.Id, _t.Staff.Id, new ReturnRequest(21_000, FuelLevel.Full, DateTime.Now, null, 0, null)));
    }

    [Theory]
    [InlineData(ReservationStatus.Pending, ReservationStatus.Approved, true)]
    [InlineData(ReservationStatus.Approved, ReservationStatus.Active, true)]
    [InlineData(ReservationStatus.Active, ReservationStatus.Completed, true)]
    [InlineData(ReservationStatus.Completed, ReservationStatus.Cancelled, false)]
    [InlineData(ReservationStatus.Active, ReservationStatus.Cancelled, false)]
    [InlineData(ReservationStatus.Rejected, ReservationStatus.Approved, false)]
    public void Durum_gecis_tablosu(ReservationStatus from, ReservationStatus to, bool allowed)
        => Assert.Equal(allowed, ReservationService.CanTransition(from, to));

    [Fact]
    public async Task Tam_kiralama_dongusu_gec_iade_ve_yakit_ucretiyle()
    {
        var r = await _service.CreateAsync(Request(_t.Customer.Id, _t.Economy.Id, returnBranchId: _t.Ankara.Id));
        await _service.ApproveAsync(r.Id, _t.Staff.Id);
        await _service.DeliverAsync(r.Id, _t.Staff.Id, startKm: 20_000, FuelLevel.Full);

        var vehicle = await _t.Db.Vehicles.FindAsync(_t.Economy.Id);
        Assert.Equal(VehicleStatus.Rented, vehicle!.Status);

        // Planlanandan 5 saat geç, yarım depo ve 500 ₺ hasarla iade
        var returned = await _service.ReturnAsync(r.Id, _t.Staff.Id,
            new ReturnRequest(20_850, FuelLevel.Half, r.EndDate.AddHours(5), "Çizik", 500, null));

        Assert.Equal(ReservationStatus.Completed, returned.Status);
        Assert.Equal(1500, returned.LateFee);     // 1 gün × 1000 × 1.5
        Assert.Equal(900, returned.FuelFee);      // 2 çeyrek × 450
        Assert.Equal(500, returned.DamageFee);
        Assert.Equal(3000 + 750 + 1500 + 900 + 500, returned.TotalPrice);
        Assert.Single(returned.DamageReports);

        // Araç iade şubesinde, yeni kilometresiyle tekrar müsait
        Assert.Equal(VehicleStatus.Available, vehicle.Status);
        Assert.Equal(20_850, vehicle.Kilometers);
        Assert.Equal(_t.Ankara.Id, vehicle.BranchId);
    }

    [Fact]
    public async Task Iade_kilometresi_teslimden_kucuk_olamaz()
    {
        var r = await _service.CreateAsync(Request(_t.Customer.Id, _t.Economy.Id));
        await _service.ApproveAsync(r.Id, _t.Staff.Id);
        await _service.DeliverAsync(r.Id, _t.Staff.Id, 20_000, FuelLevel.Full);

        await Assert.ThrowsAsync<BusinessRuleException>(() =>
            _service.ReturnAsync(r.Id, _t.Staff.Id, new ReturnRequest(19_000, FuelLevel.Full, r.EndDate, null, 0, null)));
    }

    [Fact]
    public async Task Musteri_baskasinin_rezervasyonunu_iptal_edemez()
    {
        var r = await _service.CreateAsync(Request(_t.Customer.Id, _t.Economy.Id));
        await Assert.ThrowsAsync<BusinessRuleException>(() => _service.CancelAsync(r.Id, _t.OtherCustomer.Id, isStaff: false));

        await _service.CancelAsync(r.Id, _t.Customer.Id, isStaff: false);
        Assert.Equal(ReservationStatus.Cancelled, (await _t.Db.Reservations.AsNoTracking().FirstAsync(x => x.Id == r.Id)).Status);
    }
}
