using AracKiralama.Web.Data;
using AracKiralama.Web.Helpers;
using AracKiralama.Web.Models.Entities;
using AracKiralama.Web.Models.Enums;
using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;

namespace AracKiralama.Web.Services;

public record ChartPoint(string Label, decimal Value);

public record DashboardData(
    int TotalVehicles, int RentedNow, int InMaintenance, double OccupancyRate,
    int PendingCount, int OverdueCount,
    decimal MonthRevenue, decimal PreviousPeriodRevenue,
    List<Reservation> TodayPickups, List<Reservation> TodayReturns, List<Reservation> Overdue, List<Reservation> LatestPending,
    List<ChartPoint> MonthlyRevenue, List<ChartPoint> CategoryShare, List<ChartPoint> TopVehicles);

public record VehicleReportRow(int VehicleId, string Plate, string Name, string Category, string Branch,
    int RentalCount, int RentedDays, double Utilization, decimal Revenue);

public record BranchReportRow(string Branch, int RentalCount, decimal Revenue);

public record ReportData(DateTime From, DateTime To, decimal TotalRevenue, int RentalCount, int RentedDays,
    decimal AverageRental, double AverageUtilization, List<VehicleReportRow> Vehicles, List<BranchReportRow> Branches);

public interface IReportService
{
    Task<DashboardData> GetDashboardAsync();
    Task<ReportData> GetReportAsync(DateTime from, DateTime to);
    byte[] ExportToExcel(ReportData report);
}

/// <summary>
/// Dashboard ve rapor ekranlarının verileri.
/// Gelir tanımı: TAMAMLANAN kiralamaların toplam tutarı, iade tarihine göre.
/// </summary>
public class ReportService(AppDbContext db) : IReportService
{
    public async Task<DashboardData> GetDashboardAsync()
    {
        var now = DateTime.Now;
        var today = DateTime.Today;
        var tomorrow = today.AddDays(1);
        var monthStart = new DateTime(today.Year, today.Month, 1);

        var vehicleStats = await db.Vehicles
            .Where(v => v.Status != VehicleStatus.Passive)
            .GroupBy(v => v.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToListAsync();
        var total = vehicleStats.Sum(x => x.Count);
        var rented = vehicleStats.FirstOrDefault(x => x.Status == VehicleStatus.Rented)?.Count ?? 0;
        var maintenance = vehicleStats.FirstOrDefault(x => x.Status == VehicleStatus.Maintenance)?.Count ?? 0;

        IQueryable<Reservation> WithDetails() => db.Reservations
            .Include(r => r.Customer).Include(r => r.Vehicle).ThenInclude(v => v!.Brand)
            .Include(r => r.PickupBranch).Include(r => r.ReturnBranch);

        var todayPickups = await WithDetails()
            .Where(r => r.Status == ReservationStatus.Approved && r.StartDate >= today && r.StartDate < tomorrow)
            .OrderBy(r => r.StartDate).ToListAsync();
        var todayReturns = await WithDetails()
            .Where(r => r.Status == ReservationStatus.Active && r.EndDate >= today && r.EndDate < tomorrow)
            .OrderBy(r => r.EndDate).ToListAsync();
        var overdue = await WithDetails()
            .Where(r => r.Status == ReservationStatus.Active && r.EndDate < now)
            .OrderBy(r => r.EndDate).ToListAsync();
        var latestPending = await WithDetails()
            .Where(r => r.Status == ReservationStatus.Pending)
            .OrderBy(r => r.CreatedAt).Take(6).ToListAsync();
        var pendingCount = await db.Reservations.CountAsync(r => r.Status == ReservationStatus.Pending);

        // Son 12 ayın geliri
        var yearAgo = monthStart.AddMonths(-11);
        var completed = await db.Reservations
            .Where(r => r.Status == ReservationStatus.Completed && r.ActualReturnDate >= yearAgo)
            .Select(r => new { r.ActualReturnDate, r.TotalPrice })
            .ToListAsync();
        var monthly = Enumerable.Range(0, 12)
            .Select(i => yearAgo.AddMonths(i))
            .Select(m => new ChartPoint(Fmt.Month(m),
                completed.Where(r => r.ActualReturnDate >= m && r.ActualReturnDate < m.AddMonths(1)).Sum(r => r.TotalPrice)))
            .ToList();

        // Kategori dağılımı ve en çok kiralanan araçlar (iptal / ret hariç)
        var counted = new[] { ReservationStatus.Approved, ReservationStatus.Active, ReservationStatus.Completed };
        var byCategory = await db.Reservations
            .Where(r => counted.Contains(r.Status))
            .GroupBy(r => r.Vehicle!.Category)
            .Select(g => new { g.Key, Count = g.Count() })
            .ToListAsync();
        var topVehicles = await db.Reservations
            .Where(r => counted.Contains(r.Status))
            .GroupBy(r => new { r.Vehicle!.Brand!.Name, r.Vehicle.Model })
            .Select(g => new { g.Key.Name, g.Key.Model, Count = g.Count() })
            .OrderByDescending(x => x.Count).Take(5)
            .ToListAsync();

        // Adil karşılaştırma: bu ayın ilk N günü ↔ geçen ayın ilk N günü
        var elapsed = now - monthStart;
        var previousMonthStart = monthStart.AddMonths(-1);
        var previousSamePeriod = completed
            .Where(r => r.ActualReturnDate >= previousMonthStart && r.ActualReturnDate < previousMonthStart + elapsed)
            .Sum(r => r.TotalPrice);

        return new DashboardData(
            total, rented, maintenance,
            OccupancyRate: total == 0 ? 0 : (double)rented / total,
            PendingCount: pendingCount,
            OverdueCount: overdue.Count,
            MonthRevenue: monthly[^1].Value,
            PreviousPeriodRevenue: previousSamePeriod,
            todayPickups, todayReturns, overdue, latestPending,
            monthly,
            byCategory.OrderByDescending(x => x.Count).Select(x => new ChartPoint(x.Key.GetDisplayName(), x.Count)).ToList(),
            topVehicles.Select(x => new ChartPoint($"{x.Name} {x.Model}", x.Count)).ToList());
    }

    public async Task<ReportData> GetReportAsync(DateTime from, DateTime to)
    {
        var start = from.Date;
        var end = to.Date.AddDays(1);              // "to" günü de dahil
        var rangeDays = Math.Max(1, (end - start).Days);

        var vehicles = await db.Vehicles.Include(v => v.Brand).Include(v => v.Branch)
            .Where(v => v.Status != VehicleStatus.Passive).AsNoTracking().ToListAsync();

        // Aralıkla çakışan kirada / tamamlanmış kiralamalar (kullanım oranı için)
        var rentals = await db.Reservations
            .Where(r => (r.Status == ReservationStatus.Completed || r.Status == ReservationStatus.Active)
                        && r.StartDate < end && start < (r.ActualReturnDate ?? r.EndDate))
            .Include(r => r.PickupBranch)
            .AsNoTracking().ToListAsync();

        // Gelir: aralık içinde iade edilmiş (tamamlanmış) kiralamalar
        var revenueRentals = rentals
            .Where(r => r.Status == ReservationStatus.Completed && r.ActualReturnDate >= start && r.ActualReturnDate < end)
            .ToList();

        var vehicleRows = vehicles.Select(v =>
        {
            var vr = rentals.Where(r => r.VehicleId == v.Id).ToList();
            var rentedDays = vr.Sum(r => OverlapDays(r.StartDate, r.ActualReturnDate ?? r.EndDate, start, end));
            var revenue = revenueRentals.Where(r => r.VehicleId == v.Id).Sum(r => r.TotalPrice);
            return new VehicleReportRow(v.Id, v.Plate, v.DisplayName, v.Category.GetDisplayName(), v.Branch!.Name,
                vr.Count, rentedDays, Math.Min(1.0, (double)rentedDays / rangeDays), revenue);
        })
        .OrderByDescending(x => x.Revenue)
        .ToList();

        var branchRows = revenueRentals
            .GroupBy(r => r.PickupBranch!.Name)
            .Select(g => new BranchReportRow(g.Key, g.Count(), g.Sum(r => r.TotalPrice)))
            .OrderByDescending(x => x.Revenue)
            .ToList();

        var totalRevenue = revenueRentals.Sum(r => r.TotalPrice);
        return new ReportData(start, end.AddDays(-1), totalRevenue, revenueRentals.Count,
            vehicleRows.Sum(x => x.RentedDays),
            revenueRentals.Count == 0 ? 0 : totalRevenue / revenueRentals.Count,
            vehicleRows.Count == 0 ? 0 : vehicleRows.Average(x => x.Utilization),
            vehicleRows, branchRows);
    }

    /// <summary>[aStart, aEnd) aralığının [bStart, bEnd) içinde kalan gün sayısı (yukarı yuvarlanır).</summary>
    private static int OverlapDays(DateTime aStart, DateTime aEnd, DateTime bStart, DateTime bEnd)
    {
        var s = aStart > bStart ? aStart : bStart;
        var e = aEnd < bEnd ? aEnd : bEnd;
        return e <= s ? 0 : (int)Math.Ceiling((e - s).TotalDays);
    }

    /// <summary>Raporu iki sayfalı bir Excel dosyasına (xlsx) dönüştürür.</summary>
    public byte[] ExportToExcel(ReportData report)
    {
        using var wb = new XLWorkbook();

        var ws = wb.Worksheets.Add("Araç Bazlı");
        ws.Cell(1, 1).Value = $"Araç Kullanım Raporu ({Fmt.Date(report.From)} - {Fmt.Date(report.To)})";
        ws.Cell(1, 1).Style.Font.SetBold().Font.SetFontSize(14);

        string[] headers = ["Plaka", "Araç", "Kategori", "Şube", "Kiralama", "Kiralı Gün", "Kullanım %", "Gelir (₺)"];
        for (var i = 0; i < headers.Length; i++) ws.Cell(3, i + 1).Value = headers[i];
        ws.Range(3, 1, 3, headers.Length).Style.Font.SetBold().Fill.SetBackgroundColor(XLColor.FromHtml("#E8EEFF"));

        var row = 4;
        foreach (var v in report.Vehicles)
        {
            ws.Cell(row, 1).Value = v.Plate;
            ws.Cell(row, 2).Value = v.Name;
            ws.Cell(row, 3).Value = v.Category;
            ws.Cell(row, 4).Value = v.Branch;
            ws.Cell(row, 5).Value = v.RentalCount;
            ws.Cell(row, 6).Value = v.RentedDays;
            ws.Cell(row, 7).Value = v.Utilization;
            ws.Cell(row, 8).Value = v.Revenue;
            row++;
        }
        ws.Column(7).Style.NumberFormat.Format = "0.0%";
        ws.Column(8).Style.NumberFormat.Format = "#,##0.00";
        ws.Cell(row, 7).Value = "Toplam";
        ws.Cell(row, 8).FormulaA1 = $"SUM(H4:H{row - 1})";
        ws.Range(row, 7, row, 8).Style.Font.SetBold();
        ws.Columns().AdjustToContents();

        var ws2 = wb.Worksheets.Add("Şube Bazlı");
        ws2.Cell(1, 1).Value = "Şube";
        ws2.Cell(1, 2).Value = "Kiralama";
        ws2.Cell(1, 3).Value = "Gelir (₺)";
        ws2.Range(1, 1, 1, 3).Style.Font.SetBold().Fill.SetBackgroundColor(XLColor.FromHtml("#E8EEFF"));
        row = 2;
        foreach (var b in report.Branches)
        {
            ws2.Cell(row, 1).Value = b.Branch;
            ws2.Cell(row, 2).Value = b.RentalCount;
            ws2.Cell(row, 3).Value = b.Revenue;
            row++;
        }
        ws2.Column(3).Style.NumberFormat.Format = "#,##0.00";
        ws2.Columns().AdjustToContents();

        using var ms = new MemoryStream();
        wb.SaveAs(ms);
        return ms.ToArray();
    }
}
