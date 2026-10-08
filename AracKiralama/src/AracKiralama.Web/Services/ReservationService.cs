using AracKiralama.Web.Helpers;
using AracKiralama.Web.Data;
using AracKiralama.Web.Models.Entities;
using AracKiralama.Web.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace AracKiralama.Web.Services;

public record CreateReservationRequest(
    string CustomerId,
    int VehicleId,
    int PickupBranchId,
    int ReturnBranchId,
    DateTime Start,
    DateTime End,
    IReadOnlyCollection<int> ExtraIds,
    string? Notes = null);

public record ReturnRequest(
    int EndKm,
    FuelLevel FuelLevelIn,
    DateTime ReturnedAt,
    string? DamageDescription,
    decimal DamageCost,
    string? Notes);

public interface IReservationService
{
    Task<(Vehicle Vehicle, PriceQuote Quote)> BuildQuoteAsync(int vehicleId, DateTime start, DateTime end,
        IEnumerable<int> extraIds, int pickupBranchId, int returnBranchId);
    Task<Reservation> CreateAsync(CreateReservationRequest request);
    Task ApproveAsync(int reservationId, string staffId);
    Task RejectAsync(int reservationId, string staffId, string? reason);
    Task CancelAsync(int reservationId, string actorId, bool isStaff, string? reason = null);
    Task DeliverAsync(int reservationId, string staffId, int startKm, FuelLevel fuelLevelOut);
    Task<Reservation> ReturnAsync(int reservationId, string staffId, ReturnRequest request);
    Task AddPaymentAsync(int reservationId, string staffId, decimal amount, PaymentMethod method, PaymentType type, string? note);
}

/// <summary>
/// Rezervasyonun tüm yaşam döngüsü burada yönetilir (durum makinesi):
///
///   Pending ──onay──▶ Approved ──teslim──▶ Active ──iade──▶ Completed
///     │ ret             │ iptal
///     ▼                 ▼
///   Rejected         Cancelled        (Pending de iptal edilebilir)
///
/// Kurallar ihlal edilirse BusinessRuleException fırlatılır; controller bu mesajı kullanıcıya gösterir.
/// </summary>
public class ReservationService(AppDbContext db, IPricingService pricing, IAvailabilityService availability) : IReservationService
{
    /// <summary>Hangi durumdan hangi durumlara geçilebilir?</summary>
    public static readonly IReadOnlyDictionary<ReservationStatus, ReservationStatus[]> AllowedTransitions =
        new Dictionary<ReservationStatus, ReservationStatus[]>
        {
            [ReservationStatus.Pending] = [ReservationStatus.Approved, ReservationStatus.Rejected, ReservationStatus.Cancelled],
            [ReservationStatus.Approved] = [ReservationStatus.Active, ReservationStatus.Cancelled],
            [ReservationStatus.Active] = [ReservationStatus.Completed],
            [ReservationStatus.Completed] = [],
            [ReservationStatus.Cancelled] = [],
            [ReservationStatus.Rejected] = [],
        };

    public const int MaxRentalDays = 90;

    public static bool CanTransition(ReservationStatus from, ReservationStatus to)
        => AllowedTransitions[from].Contains(to);

    private static void EnsureTransition(Reservation r, ReservationStatus to)
    {
        if (!CanTransition(r.Status, to))
            throw new BusinessRuleException(
                $"Bu işlem yapılamaz: \"{r.Status.GetDisplayName()}\" durumundaki rezervasyon " +
                $"\"{to.GetDisplayName()}\" durumuna geçirilemez.");
    }

    /// <summary>Tarih aralığı kuralları. Hata yoksa boş liste döner.</summary>
    public static List<string> ValidateDates(DateTime start, DateTime end, DateTime now)
    {
        var errors = new List<string>();
        if (start < now.AddMinutes(-5)) errors.Add("Alış tarihi geçmişte olamaz.");
        if (end <= start) errors.Add("İade tarihi alış tarihinden sonra olmalıdır.");
        else if ((end - start).TotalDays > MaxRentalDays) errors.Add($"En fazla {MaxRentalDays} günlük kiralama yapılabilir.");
        return errors;
    }

    /// <summary>Sürücü yaşı ve ehliyet süresi aracın şartlarını karşılıyor mu? Hata yoksa boş liste döner.</summary>
    public static List<string> CheckDriverEligibility(AppUser user, Vehicle vehicle, DateTime atDate)
    {
        var errors = new List<string>();
        if (user.IsBlacklisted)
        {
            errors.Add("Hesabınız rezervasyon yapmaya kapalıdır. Lütfen bizimle iletişime geçin.");
            return errors;
        }
        if (user.BirthDate is null || user.LicenseIssueDate is null)
        {
            errors.Add("Rezervasyon için profilinizde doğum tarihi ve ehliyet bilgileri eksiksiz olmalıdır.");
            return errors;
        }

        var age = YearsBetween(user.BirthDate.Value, atDate);
        if (age < vehicle.MinDriverAge)
            errors.Add($"Bu aracı kiralamak için en az {vehicle.MinDriverAge} yaşında olmalısınız.");

        var licenseYears = YearsBetween(user.LicenseIssueDate.Value, atDate);
        if (licenseYears < vehicle.MinLicenseYears)
            errors.Add($"Bu araç için en az {vehicle.MinLicenseYears} yıllık ehliyet gereklidir.");

        return errors;
    }

    /// <summary>İki tarih arasındaki tam yıl sayısı (yaş hesabı).</summary>
    public static int YearsBetween(DateTime from, DateTime to)
    {
        var years = to.Year - from.Year;
        if (from.Date > to.Date.AddYears(-years)) years--;
        return years;
    }

    public async Task<(Vehicle Vehicle, PriceQuote Quote)> BuildQuoteAsync(int vehicleId, DateTime start, DateTime end,
        IEnumerable<int> extraIds, int pickupBranchId, int returnBranchId)
    {
        var vehicle = await db.Vehicles.Include(v => v.Brand).FirstOrDefaultAsync(v => v.Id == vehicleId)
                      ?? throw new BusinessRuleException("Araç bulunamadı.");

        var ids = extraIds.Distinct().ToList();
        var extras = await db.Extras.Where(x => x.IsActive && ids.Contains(x.Id)).OrderBy(x => x.Name).ToListAsync();

        var quote = pricing.Calculate(vehicle, start, end, extras, isOneWay: pickupBranchId != returnBranchId);
        return (vehicle, quote);
    }

    public async Task<Reservation> CreateAsync(CreateReservationRequest req)
    {
        var dateErrors = ValidateDates(req.Start, req.End, DateTime.Now);
        if (dateErrors.Count > 0) throw new BusinessRuleException(dateErrors[0]);

        var customer = await db.Users.FindAsync(req.CustomerId)
                       ?? throw new BusinessRuleException("Müşteri bulunamadı.");

        var (vehicle, quote) = await BuildQuoteAsync(req.VehicleId, req.Start, req.End, req.ExtraIds,
            req.PickupBranchId, req.ReturnBranchId);

        var eligibility = CheckDriverEligibility(customer, vehicle, req.Start);
        if (eligibility.Count > 0) throw new BusinessRuleException(eligibility[0]);

        if (!await db.Branches.AnyAsync(b => b.Id == req.PickupBranchId) ||
            !await db.Branches.AnyAsync(b => b.Id == req.ReturnBranchId))
            throw new BusinessRuleException("Geçersiz şube seçimi.");

        if (!await availability.IsAvailableAsync(vehicle.Id, req.Start, req.End))
            throw new BusinessRuleException("Üzgünüz, araç seçtiğiniz tarihlerde müsait değil.");

        // Aynı müşteri aynı araç için çakışan ikinci bir talep açamasın.
        var duplicate = await db.Reservations.AnyAsync(r =>
            r.CustomerId == req.CustomerId && r.VehicleId == vehicle.Id
            && r.Status == ReservationStatus.Pending
            && r.StartDate < req.End && req.Start < r.EndDate);
        if (duplicate) throw new BusinessRuleException("Bu araç için bu tarihlerde zaten bekleyen bir talebiniz var.");

        var reservation = new Reservation
        {
            Code = await GenerateCodeAsync(),
            CustomerId = customer.Id,
            VehicleId = vehicle.Id,
            PickupBranchId = req.PickupBranchId,
            ReturnBranchId = req.ReturnBranchId,
            StartDate = req.Start,
            EndDate = req.End,
            Status = ReservationStatus.Pending,
            Notes = req.Notes,
            CreatedAt = DateTime.Now,
        };
        ApplyQuote(reservation, quote);

        db.Reservations.Add(reservation);
        await db.SaveChangesAsync();
        return reservation;
    }

    /// <summary>Fiyat teklifini rezervasyon kaydına kopyalar (snapshot).</summary>
    public static void ApplyQuote(Reservation r, PriceQuote q)
    {
        r.TotalDays = q.Days;
        r.DailyPrice = q.DailyPrice;
        r.BaseTotal = q.BaseTotal;
        r.DiscountAmount = q.DiscountAmount;
        r.ExtrasTotal = q.ExtrasTotal;
        r.OneWayFee = q.OneWayFee;
        r.Deposit = q.Deposit;
        r.Extras = q.ExtraLines
            .Select(x => new ReservationExtra { ExtraId = x.ExtraId, DailyPrice = x.DailyPrice, Total = x.Total })
            .ToList();
        RecalculateTotal(r);
    }

    /// <summary>Toplam = kira - indirim + ekstralar + tek yön + gecikme + yakıt + hasar.</summary>
    public static void RecalculateTotal(Reservation r)
    {
        r.TotalPrice = r.BaseTotal - r.DiscountAmount + r.ExtrasTotal + r.OneWayFee
                       + r.LateFee + r.FuelFee + r.DamageFee;
    }

    public async Task ApproveAsync(int reservationId, string staffId)
    {
        // Transaction: kontrol ve güncellemeler ya hep birlikte olur ya hiç olmaz.
        // Böylece aynı araç aynı tarihlerde iki farklı müşteriye onaylanamaz.
        await using var tx = await db.Database.BeginTransactionAsync();

        var r = await GetAsync(reservationId);
        EnsureTransition(r, ReservationStatus.Approved);

        if (r.StartDate < DateTime.Now.AddHours(-1))
            throw new BusinessRuleException("Alış tarihi geçmiş bir talep onaylanamaz. Lütfen reddedin.");

        if (!await availability.IsAvailableAsync(r.VehicleId, r.StartDate, r.EndDate, excludeReservationId: r.Id))
            throw new BusinessRuleException("Araç bu tarihlerde başka bir rezervasyona ayrılmış veya bakımda.");

        r.Status = ReservationStatus.Approved;
        r.HandledById = staffId;

        // Aynı araç için çakışan diğer bekleyen talepleri otomatik reddet.
        var conflicting = await db.Reservations
            .Where(o => o.Id != r.Id && o.VehicleId == r.VehicleId && o.Status == ReservationStatus.Pending
                        && o.StartDate < r.EndDate && r.StartDate < o.EndDate)
            .ToListAsync();
        foreach (var o in conflicting)
        {
            o.Status = ReservationStatus.Rejected;
            o.StatusReason = $"Araç aynı tarihler için başka bir rezervasyona ({r.Code}) ayrıldı.";
            o.HandledById = staffId;
        }

        await db.SaveChangesAsync();
        await tx.CommitAsync();
    }

    public async Task RejectAsync(int reservationId, string staffId, string? reason)
    {
        var r = await GetAsync(reservationId);
        EnsureTransition(r, ReservationStatus.Rejected);
        r.Status = ReservationStatus.Rejected;
        r.StatusReason = string.IsNullOrWhiteSpace(reason) ? "Talep uygun bulunmadı." : reason.Trim();
        r.HandledById = staffId;
        await db.SaveChangesAsync();
    }

    public async Task CancelAsync(int reservationId, string actorId, bool isStaff, string? reason = null)
    {
        var r = await GetAsync(reservationId);
        if (!isStaff && r.CustomerId != actorId)
            throw new BusinessRuleException("Bu rezervasyonu iptal etme yetkiniz yok.");
        EnsureTransition(r, ReservationStatus.Cancelled);

        // Müşteri son 24 saatte iptal ederse 1 günlük ücret kesilir (personel iptalinde kesilmez).
        r.CancellationFee = isStaff ? 0m : pricing.CalculateCancellationFee(r, DateTime.Now);
        r.Status = ReservationStatus.Cancelled;
        r.StatusReason = reason ?? (isStaff ? "Personel tarafından iptal edildi." : "Müşteri tarafından iptal edildi.");
        if (isStaff) r.HandledById = actorId;
        await db.SaveChangesAsync();
    }

    public async Task DeliverAsync(int reservationId, string staffId, int startKm, FuelLevel fuelLevelOut)
    {
        var r = await GetAsync(reservationId);
        EnsureTransition(r, ReservationStatus.Active);

        var vehicle = r.Vehicle!;
        if (vehicle.Status == VehicleStatus.Rented)
            throw new BusinessRuleException("Araç şu anda başka bir müşteride görünüyor; önce o kiralamanın iadesini alın.");
        if (startKm < vehicle.Kilometers)
            throw new BusinessRuleException($"Teslim kilometresi aracın kayıtlı kilometresinden ({vehicle.Kilometers:N0}) küçük olamaz.");

        r.Status = ReservationStatus.Active;
        r.DeliveredAt = DateTime.Now;
        r.StartKm = startKm;
        r.FuelLevelOut = fuelLevelOut;
        r.HandledById = staffId;

        vehicle.Status = VehicleStatus.Rented;
        vehicle.Kilometers = startKm;

        await db.SaveChangesAsync();
    }

    public async Task<Reservation> ReturnAsync(int reservationId, string staffId, ReturnRequest req)
    {
        var r = await GetAsync(reservationId);
        EnsureTransition(r, ReservationStatus.Completed);

        if (req.EndKm < (r.StartKm ?? 0))
            throw new BusinessRuleException($"İade kilometresi teslim kilometresinden ({r.StartKm:N0}) küçük olamaz.");
        if (req.DamageCost < 0)
            throw new BusinessRuleException("Hasar tutarı negatif olamaz.");

        r.Status = ReservationStatus.Completed;
        r.ActualReturnDate = req.ReturnedAt;
        r.EndKm = req.EndKm;
        r.FuelLevelIn = req.FuelLevelIn;
        r.IsOverdue = false;
        r.HandledById = staffId;

        // Otomatik ek ücretler
        r.LateFee = pricing.CalculateLateFee(r.DailyPrice, r.EndDate, req.ReturnedAt);
        r.FuelFee = pricing.CalculateFuelFee(r.FuelLevelOut ?? FuelLevel.Full, req.FuelLevelIn);
        r.DamageFee = req.DamageCost;

        if (!string.IsNullOrWhiteSpace(req.DamageDescription) || req.DamageCost > 0)
        {
            r.DamageReports.Add(new DamageReport
            {
                Description = string.IsNullOrWhiteSpace(req.DamageDescription) ? "Hasar" : req.DamageDescription.Trim(),
                Cost = req.DamageCost,
            });
        }
        if (!string.IsNullOrWhiteSpace(req.Notes))
            r.Notes = string.IsNullOrWhiteSpace(r.Notes) ? req.Notes : $"{r.Notes}\n{req.Notes}";

        RecalculateTotal(r);

        // Araç iade şubesinde, yeni kilometresiyle tekrar müsait.
        var vehicle = r.Vehicle!;
        vehicle.Status = VehicleStatus.Available;
        vehicle.Kilometers = req.EndKm;
        vehicle.BranchId = r.ReturnBranchId;

        await db.SaveChangesAsync();
        return r;
    }

    public async Task AddPaymentAsync(int reservationId, string staffId, decimal amount, PaymentMethod method, PaymentType type, string? note)
    {
        if (amount <= 0) throw new BusinessRuleException("Ödeme tutarı sıfırdan büyük olmalıdır.");
        var r = await GetAsync(reservationId);
        if (r.Status is ReservationStatus.Rejected)
            throw new BusinessRuleException("Reddedilmiş rezervasyona ödeme eklenemez.");

        db.Payments.Add(new Payment
        {
            ReservationId = r.Id,
            Amount = amount,
            Method = method,
            Type = type,
            Note = note,
            ReceivedById = staffId,
            PaidAt = DateTime.Now,
        });
        await db.SaveChangesAsync();
    }

    private async Task<Reservation> GetAsync(int id)
        => await db.Reservations
               .Include(r => r.Vehicle)
               .Include(r => r.DamageReports)
               .FirstOrDefaultAsync(r => r.Id == id)
           ?? throw new BusinessRuleException("Rezervasyon bulunamadı.");

    /// <summary>"RZ-2026-0001" biçiminde, yıl içinde artan benzersiz kod üretir.</summary>
    private async Task<string> GenerateCodeAsync()
    {
        var prefix = $"RZ-{DateTime.Now.Year}-";
        var lastCode = await db.Reservations
            .Where(r => r.Code.StartsWith(prefix))
            .OrderByDescending(r => r.Code)
            .Select(r => r.Code)
            .FirstOrDefaultAsync();

        var next = lastCode is null ? 1 : int.Parse(lastCode[prefix.Length..]) + 1;
        return $"{prefix}{next:D4}";
    }
}
