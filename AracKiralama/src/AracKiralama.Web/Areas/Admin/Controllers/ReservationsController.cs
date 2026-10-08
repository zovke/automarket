using AracKiralama.Web.Data;
using AracKiralama.Web.Helpers;
using AracKiralama.Web.Models.Entities;
using AracKiralama.Web.Models.Enums;
using AracKiralama.Web.Services;
using AracKiralama.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AracKiralama.Web.Areas.Admin.Controllers;

/// <summary>
/// Rezervasyon operasyonları: onay / ret, teslim, iade, ödeme, iptal.
/// Controller sadece formu alır ve servisi çağırır; kuralların hepsi ReservationService içinde.
/// </summary>
public class ReservationsController(AppDbContext db, IReservationService reservations,
    IPricingService pricing, IContractPdfService contracts) : AdminController
{
    public async Task<IActionResult> Index(ReservationListFilter filter)
    {
        var query = db.Reservations
            .Include(r => r.Customer)
            .Include(r => r.Vehicle).ThenInclude(v => v!.Brand)
            .Include(r => r.PickupBranch).Include(r => r.ReturnBranch)
            .AsNoTracking();

        if (filter.Status.HasValue) query = query.Where(r => r.Status == filter.Status);
        if (filter.OverdueOnly) query = query.Where(r => r.Status == ReservationStatus.Active && r.EndDate < DateTime.Now);
        if (filter.BranchId.HasValue) query = query.Where(r => r.PickupBranchId == filter.BranchId || r.ReturnBranchId == filter.BranchId);
        if (!string.IsNullOrWhiteSpace(filter.Q))
        {
            var q = filter.Q.Trim();
            var qUpper = q.ToUpperInvariant();
            query = query.Where(r => r.Code.Contains(qUpper) || r.Vehicle!.Plate.Contains(qUpper)
                                     || r.Customer!.FirstName.Contains(q) || r.Customer.LastName.Contains(q)
                                     || r.Customer.Email!.Contains(q));
        }

        // Bekleyenler en üstte (en eski talep önce), sonra en yeni alış tarihi
        query = filter.Status == ReservationStatus.Pending
            ? query.OrderBy(r => r.CreatedAt)
            : query.OrderByDescending(r => r.StartDate);

        var counts = await db.Reservations.GroupBy(r => r.Status)
            .Select(g => new { g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Key, x => x.Count);

        return View(new AdminReservationListViewModel
        {
            Filter = filter,
            Reservations = await PagedList<Reservation>.CreateAsync(query, filter.Page, 20),
            Branches = await db.Branches.OrderBy(b => b.Name).AsNoTracking().ToListAsync(),
            StatusCounts = counts,
        });
    }

    public async Task<IActionResult> Details(int id)
    {
        var r = await LoadAsync(id);
        if (r is null) return NotFound();

        ViewBag.EstimatedLateFee = r.Status == ReservationStatus.Active
            ? pricing.CalculateLateFee(r.DailyPrice, r.EndDate, DateTime.Now) : 0m;
        ViewBag.Payment = new PaymentFormViewModel
        {
            ReservationId = id,
            Amount = Math.Max(0, r.Balance),
            Type = PaymentType.Rental,
            Method = PaymentMethod.CreditCard,
        };
        return View(r);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public Task<IActionResult> Approve(int id)
        => Run(id, () => reservations.ApproveAsync(id, CurrentUserId), "Rezervasyon onaylandı. Çakışan bekleyen talepler varsa otomatik reddedildi.");

    [HttpPost, ValidateAntiForgeryToken]
    public Task<IActionResult> Reject(int id, string? reason)
        => Run(id, () => reservations.RejectAsync(id, CurrentUserId, reason), "Talep reddedildi.");

    [HttpPost, ValidateAntiForgeryToken]
    public Task<IActionResult> Cancel(int id, string? reason)
        => Run(id, () => reservations.CancelAsync(id, CurrentUserId, isStaff: true, reason), "Rezervasyon iptal edildi.");

    [HttpGet]
    public async Task<IActionResult> Deliver(int id)
    {
        var r = await LoadAsync(id);
        if (r is null) return NotFound();
        if (r.Status != ReservationStatus.Approved)
        {
            ShowError("Sadece onaylanmış rezervasyonlar teslim edilebilir.");
            return RedirectToAction(nameof(Details), new { id });
        }
        return View(new DeliverViewModel { ReservationId = id, StartKm = r.Vehicle!.Kilometers, Reservation = r });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Deliver(DeliverViewModel model)
    {
        if (ModelState.IsValid)
        {
            try
            {
                await reservations.DeliverAsync(model.ReservationId, CurrentUserId, model.StartKm, model.FuelLevelOut);
                ShowSuccess("Araç müşteriye teslim edildi. İyi yolculuklar!");
                return RedirectToAction(nameof(Details), new { id = model.ReservationId });
            }
            catch (BusinessRuleException ex) { ModelState.AddModelError("", ex.Message); }
        }
        model.Reservation = await LoadAsync(model.ReservationId);
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Return(int id)
    {
        var r = await LoadAsync(id);
        if (r is null) return NotFound();
        if (r.Status != ReservationStatus.Active)
        {
            ShowError("Sadece kiradaki araçların iadesi alınabilir.");
            return RedirectToAction(nameof(Details), new { id });
        }
        var now = DateTime.Now;
        return View(new ReturnViewModel
        {
            ReservationId = id,
            EndKm = (r.StartKm ?? 0) + 150 * r.TotalDays,
            ReturnedAt = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, 0),
            FuelLevelIn = r.FuelLevelOut ?? FuelLevel.Full,
            Reservation = r,
            EstimatedLateFee = pricing.CalculateLateFee(r.DailyPrice, r.EndDate, now),
        });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Return(ReturnViewModel model)
    {
        if (ModelState.IsValid)
        {
            try
            {
                var r = await reservations.ReturnAsync(model.ReservationId, CurrentUserId, new ReturnRequest(
                    model.EndKm, model.FuelLevelIn, model.ReturnedAt, model.DamageDescription, model.DamageCost, model.Notes));

                var extras = r.LateFee + r.FuelFee + r.DamageFee;
                ShowSuccess(extras > 0
                    ? $"İade alındı. Ek ücretler: {Fmt.Money(extras)} — yeni toplam {Fmt.Money(r.TotalPrice)}."
                    : $"İade alındı. Toplam tutar {Fmt.Money(r.TotalPrice)}.");
                return RedirectToAction(nameof(Details), new { id = model.ReservationId });
            }
            catch (BusinessRuleException ex) { ModelState.AddModelError("", ex.Message); }
        }
        model.Reservation = await LoadAsync(model.ReservationId);
        model.EstimatedLateFee = pricing.CalculateLateFee(model.Reservation!.DailyPrice, model.Reservation.EndDate, model.ReturnedAt);
        return View(model);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> AddPayment(PaymentFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            ShowError("Ödeme bilgileri geçersiz.");
            return RedirectToAction(nameof(Details), new { id = model.ReservationId });
        }
        return await Run(model.ReservationId,
            () => reservations.AddPaymentAsync(model.ReservationId, CurrentUserId, model.Amount, model.Method, model.Type, model.Note),
            $"{Fmt.Money(model.Amount)} tutarında ödeme kaydedildi.");
    }

    public async Task<IActionResult> Contract(int id)
    {
        var result = await contracts.GenerateAsync(id);
        return result is null ? NotFound() : InlinePdf(result.Value.Pdf, result.Value.FileName);
    }

    /// <summary>Servis çağrısını çalıştırır; iş kuralı hatası olursa mesajı bildirim olarak gösterir.</summary>
    private async Task<IActionResult> Run(int id, Func<Task> action, string successMessage)
    {
        try
        {
            await action();
            ShowSuccess(successMessage);
        }
        catch (BusinessRuleException ex)
        {
            ShowError(ex.Message);
        }
        return RedirectToAction(nameof(Details), new { id });
    }

    private Task<Reservation?> LoadAsync(int id) => db.Reservations
        .Include(r => r.Customer)
        .Include(r => r.Vehicle).ThenInclude(v => v!.Brand)
        .Include(r => r.PickupBranch).Include(r => r.ReturnBranch)
        .Include(r => r.Extras).ThenInclude(e => e.Extra)
        .Include(r => r.Payments).ThenInclude(p => p.ReceivedBy)
        .Include(r => r.DamageReports)
        .Include(r => r.HandledBy)
        .AsSplitQuery()
        .AsNoTracking()
        .FirstOrDefaultAsync(r => r.Id == id);
}
