using AracKiralama.Web.Helpers;
using AracKiralama.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace AracKiralama.Web.Controllers.Api;

public record QuoteRequest(int VehicleId, DateTime Start, DateTime End, int PickupBranchId, int ReturnBranchId, List<int>? ExtraIds);

public record QuoteResponse(bool IsAvailable, List<string> Errors, PriceQuote? Quote, QuoteText? Text);

/// <summary>Arayüzde doğrudan gösterilecek, Türkçe biçimlendirilmiş tutarlar.</summary>
public record QuoteText(string BaseTotal, string Discount, string ExtrasTotal, string OneWayFee, string Total, string Deposit, string PerDay);

/// <summary>
/// Canlı fiyat hesaplama. Araç detay sayfası ve rezervasyon sihirbazı, kullanıcı tarih / ekstra
/// değiştirdikçe bu uç noktaya istek atar (bkz. wwwroot/js/price-quote.js).
/// </summary>
[ApiController]
[Route("api/quote")]
[Produces("application/json")]
public class QuoteApiController(IReservationService reservations, IAvailabilityService availability) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<QuoteResponse>> Post(QuoteRequest req)
    {
        var errors = ReservationService.ValidateDates(req.Start, req.End, DateTime.Now);
        if (errors.Count > 0) return Ok(new QuoteResponse(false, errors, null, null));

        try
        {
            var (_, quote) = await reservations.BuildQuoteAsync(req.VehicleId, req.Start, req.End,
                req.ExtraIds ?? [], req.PickupBranchId, req.ReturnBranchId);
            var available = await availability.IsAvailableAsync(req.VehicleId, req.Start, req.End);
            if (!available) errors.Add("Araç seçtiğiniz tarihlerde müsait değil. Lütfen farklı tarih deneyin.");

            var text = new QuoteText(Fmt.Money(quote.BaseTotal), Fmt.Money(quote.DiscountAmount), Fmt.Money(quote.ExtrasTotal),
                Fmt.Money(quote.OneWayFee), Fmt.Money(quote.Total), Fmt.Money(quote.Deposit),
                Fmt.Money(quote.Total / quote.Days));
            return Ok(new QuoteResponse(available, errors, quote, text));
        }
        catch (BusinessRuleException ex)
        {
            return NotFound(new QuoteResponse(false, [ex.Message], null, null));
        }
    }
}
