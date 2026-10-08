using AracKiralama.Web.Data;
using AracKiralama.Web.Helpers;
using AracKiralama.Web.Models.Entities;
using AracKiralama.Web.Models.Enums;
using AracKiralama.Web.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace AracKiralama.Tests;

/// <summary>
/// Her test için bellekte (RAM) çalışan, boş bir SQLite veritabanı.
/// Gerçek uygulamayla aynı tablo yapısını kullanır ama diskte dosya oluşturmaz.
/// </summary>
public sealed class TestDb : IDisposable
{
    private readonly SqliteConnection _connection;
    public AppDbContext Db { get; }

    public Branch Istanbul { get; }
    public Branch Ankara { get; }
    public Vehicle Economy { get; }
    public Vehicle Luxury { get; }
    public AppUser Customer { get; }
    public AppUser OtherCustomer { get; }
    public AppUser YoungCustomer { get; }
    public AppUser Staff { get; }
    public Extra Insurance { get; }

    public static DateTime Future(int days, int hour = 10) => DateTime.Today.AddDays(days).AddHours(hour);

    public TestDb()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();
        Db = new AppDbContext(new DbContextOptionsBuilder<AppDbContext>().UseSqlite(_connection).Options);
        Db.Database.EnsureCreated();

        Istanbul = new Branch { Name = "İstanbul", City = "İstanbul", Address = "-", Phone = "-" };
        Ankara = new Branch { Name = "Ankara", City = "Ankara", Address = "-", Phone = "-" };
        var brand = new Brand { Name = "Renault" };

        Economy = new Vehicle { Plate = "34 AB 100", Brand = brand, Model = "Clio", Year = 2023, Category = VehicleCategory.Economy,
            DailyPrice = 1000, Deposit = 3000, MinDriverAge = 21, MinLicenseYears = 1, Branch = Istanbul, Kilometers = 20_000 };
        Luxury = new Vehicle { Plate = "34 AB 200", Brand = brand, Model = "Talisman", Year = 2024, Category = VehicleCategory.Luxury,
            DailyPrice = 3000, Deposit = 12000, MinDriverAge = 27, MinLicenseYears = 5, Branch = Istanbul, Kilometers = 10_000 };

        Customer = User("ayse@test.com", birth: new DateTime(1990, 1, 1), license: new DateTime(2010, 1, 1));
        OtherCustomer = User("mehmet@test.com", birth: new DateTime(1985, 1, 1), license: new DateTime(2005, 1, 1));
        YoungCustomer = User("genc@test.com", birth: DateTime.Today.AddYears(-20), license: DateTime.Today.AddYears(-1));
        Staff = User("personel@test.com", birth: new DateTime(1980, 1, 1), license: new DateTime(2000, 1, 1));

        Insurance = new Extra { Name = "Tam Kasko", DailyPrice = 300 };

        Db.AddRange(Istanbul, Ankara, brand, Economy, Luxury, Customer, OtherCustomer, YoungCustomer, Staff, Insurance);
        Db.SaveChanges();
    }

    private static AppUser User(string email, DateTime birth, DateTime license) => new()
    {
        UserName = email, Email = email, FirstName = "Test", LastName = email[..3],
        BirthDate = birth, LicenseIssueDate = license,
    };

    /// <summary>Varsayılan ayarlarla (appsettings.json ile aynı) fiyat servisi.</summary>
    public static PricingService Pricing() => new(Options.Create(new PricingOptions()));

    public ReservationService Reservations() => new(Db, Pricing(), new AvailabilityService(Db));

    /// <summary>Doğrudan veritabanına belirli durumda bir rezervasyon ekler.</summary>
    public Reservation AddReservation(Vehicle v, AppUser c, DateTime start, DateTime end, ReservationStatus status)
    {
        var r = new Reservation
        {
            Code = $"T-{Guid.NewGuid():N}"[..12], Vehicle = v, Customer = c, PickupBranch = Istanbul, ReturnBranch = Istanbul,
            StartDate = start, EndDate = end, Status = status, DailyPrice = v.DailyPrice, TotalDays = 1,
        };
        Db.Reservations.Add(r);
        Db.SaveChanges();
        return r;
    }

    public void Dispose()
    {
        Db.Dispose();
        _connection.Dispose();
    }
}
