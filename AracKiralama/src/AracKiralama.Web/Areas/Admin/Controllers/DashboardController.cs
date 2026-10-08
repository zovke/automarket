using AracKiralama.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace AracKiralama.Web.Areas.Admin.Controllers;

public class DashboardController(IReportService reports) : AdminController
{
    public async Task<IActionResult> Index() => View(await reports.GetDashboardAsync());
}
