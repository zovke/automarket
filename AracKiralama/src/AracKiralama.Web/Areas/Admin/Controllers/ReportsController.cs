using AracKiralama.Web.Services;
using AracKiralama.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace AracKiralama.Web.Areas.Admin.Controllers;

public class ReportsController(IReportService reports) : AdminController
{
    public async Task<IActionResult> Index(DateTime? from, DateTime? to)
    {
        var (start, end) = Range(from, to);
        return View(new ReportsViewModel { From = start, To = end, Report = await reports.GetReportAsync(start, end) });
    }

    /// <summary>Aynı raporu Excel dosyası olarak indirir.</summary>
    public async Task<IActionResult> Excel(DateTime? from, DateTime? to)
    {
        var (start, end) = Range(from, to);
        var bytes = reports.ExportToExcel(await reports.GetReportAsync(start, end));
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"Rapor_{start:yyyyMMdd}_{end:yyyyMMdd}.xlsx");
    }

    /// <summary>Varsayılan aralık: son 30 gün.</summary>
    private static (DateTime, DateTime) Range(DateTime? from, DateTime? to)
    {
        var end = (to ?? DateTime.Today).Date;
        var start = (from ?? end.AddDays(-29)).Date;
        return start > end ? (end, start) : (start, end);
    }
}
