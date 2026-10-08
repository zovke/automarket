using AracKiralama.Web.Models.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AracKiralama.Web.Data;

/// <summary>
/// Veritabanı bağlamı. Her DbSet bir tabloya karşılık gelir.
/// IdentityDbContext; kullanıcı, rol ve giriş tablolarını (AspNetUsers, AspNetRoles...) otomatik ekler.
/// </summary>
public class AppDbContext(DbContextOptions<AppDbContext> options) : IdentityDbContext<AppUser>(options)
{
    public DbSet<Branch> Branches => Set<Branch>();
    public DbSet<Brand> Brands => Set<Brand>();
    public DbSet<Vehicle> Vehicles => Set<Vehicle>();
    public DbSet<Extra> Extras => Set<Extra>();
    public DbSet<Reservation> Reservations => Set<Reservation>();
    public DbSet<ReservationExtra> ReservationExtras => Set<ReservationExtra>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<Maintenance> Maintenances => Set<Maintenance>();
    public DbSet<DamageReport> DamageReports => Set<DamageReport>();

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        // SQLite'ta decimal tipi yok; varsayılan olarak TEXT saklanır ve SUM / ORDER BY
        // veritabanında çalışmaz. Bu yüzden decimal'leri REAL (double) olarak saklıyoruz.
        // SQL Server'a geçerseniz bu satırı silebilirsiniz.
        if (Database.IsSqlite())
            configurationBuilder.Properties<decimal>().HaveConversion<double>();

        // Enum'lar veritabanında okunabilir olsun diye metin olarak saklanır ("Pending", "Approved"...).
        configurationBuilder.Properties<Enum>().HaveConversion<string>().HaveMaxLength(30);
    }

    protected override void OnModelCreating(ModelBuilder b)
    {
        base.OnModelCreating(b);

        b.Entity<Vehicle>(e =>
        {
            e.HasIndex(v => v.Plate).IsUnique();
            e.HasOne(v => v.Brand).WithMany(br => br.Vehicles).HasForeignKey(v => v.BrandId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(v => v.Branch).WithMany(br => br.Vehicles).HasForeignKey(v => v.BranchId).OnDelete(DeleteBehavior.Restrict);
        });

        b.Entity<Reservation>(e =>
        {
            e.HasIndex(r => r.Code).IsUnique();
            // Müsaitlik sorgusu bu üç kolona göre arama yapar.
            e.HasIndex(r => new { r.VehicleId, r.StartDate, r.EndDate });
            e.HasIndex(r => r.Status);

            e.HasOne(r => r.Customer).WithMany(u => u.Reservations).HasForeignKey(r => r.CustomerId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(r => r.HandledBy).WithMany().HasForeignKey(r => r.HandledById).OnDelete(DeleteBehavior.SetNull);
            e.HasOne(r => r.Vehicle).WithMany(v => v.Reservations).HasForeignKey(r => r.VehicleId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(r => r.PickupBranch).WithMany().HasForeignKey(r => r.PickupBranchId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(r => r.ReturnBranch).WithMany().HasForeignKey(r => r.ReturnBranchId).OnDelete(DeleteBehavior.Restrict);
        });

        b.Entity<ReservationExtra>(e =>
        {
            e.HasKey(x => new { x.ReservationId, x.ExtraId });
            e.HasOne(x => x.Reservation).WithMany(r => r.Extras).HasForeignKey(x => x.ReservationId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.Extra).WithMany().HasForeignKey(x => x.ExtraId).OnDelete(DeleteBehavior.Restrict);
        });

        b.Entity<Payment>(e =>
        {
            e.HasOne(p => p.Reservation).WithMany(r => r.Payments).HasForeignKey(p => p.ReservationId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(p => p.ReceivedBy).WithMany().HasForeignKey(p => p.ReceivedById).OnDelete(DeleteBehavior.SetNull);
        });

        b.Entity<Maintenance>()
            .HasOne(m => m.Vehicle).WithMany(v => v.Maintenances).HasForeignKey(m => m.VehicleId).OnDelete(DeleteBehavior.Cascade);

        b.Entity<DamageReport>()
            .HasOne(d => d.Reservation).WithMany(r => r.DamageReports).HasForeignKey(d => d.ReservationId).OnDelete(DeleteBehavior.Cascade);
    }
}
