using AracKiralama.Web.Data;
using AracKiralama.Web.Helpers;
using AracKiralama.Web.Models.Entities;
using AracKiralama.Web.Models.Enums;
using AracKiralama.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AracKiralama.Web.Controllers.Api;

/// <summary>API'nin döndürdüğü sade araç bilgisi (entity'nin tamamını dışarı açmıyoruz).</summary>
public record VehicleDto(int Id, string Brand, string Model, int Year, string Category, string Fuel, string Transmission,
    int Seats, decimal DailyPrice, decimal Deposit, string Branch, string? ImageUrl);

public record AvailabilityDto(int VehicleId, DateTime Start, DateTime End, bool IsAvailable);

/// <summary>
/// REST API. Swagger arayüzünden denenebilir: /swagger
/// Mobil uygulama veya başka bir sistem bu uç noktaları kullanarak araçları listeleyebilir.
/// </summary>
[ApiController]
[Route("api/vehicles")]
[Produces("application/json")]
public class VehiclesApiController(AppDbContext db, IAvailabilityService availability) : ControllerBase
{
    /// <summary>Araçları listeler. Tarih verilirse sadece müsait olanlar döner.</summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<VehicleDto>>> GetAll(
        [FromQuery] DateTime? start, [FromQuery] DateTime? end,
        [FromQuery] VehicleCategory? category, [FromQuery] int? branchId)
    {
        var query = db.Vehicles.Include(v => v.Brand).Include(v => v.Branch)
            .Where(v => v.Status != VehicleStatus.Passive).AsNoTracking();

        if (category.HasValue) query = query.Where(v => v.Category == category);
        if (branchId.HasValue) query = query.Where(v => v.BranchId == branchId);
        if (start.HasValue && end.HasValue)
        {
            if (end <= start) return BadRequest(new { error = "end, start'tan büyük olmalıdır." });
            query = availability.WhereAvailable(query, start.Value, end.Value);
        }

        var list = await query.OrderBy(v => v.DailyPrice).ToListAsync();
        return Ok(list.Select(ToDto));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<VehicleDto>> Get(int id)
    {
        var v = await db.Vehicles.Include(x => x.Brand).Include(x => x.Branch).AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id && x.Status != VehicleStatus.Passive);
        return v is null ? NotFound() : Ok(ToDto(v));
    }

    /// <summary>Aracın verilen tarihlerde müsait olup olmadığını döndürür.</summary>
    [HttpGet("{id:int}/availability")]
    public async Task<ActionResult<AvailabilityDto>> Availability(int id, [FromQuery] DateTime start, [FromQuery] DateTime end)
    {
        if (end <= start) return BadRequest(new { error = "end, start'tan büyük olmalıdır." });
        if (!await db.Vehicles.AnyAsync(v => v.Id == id)) return NotFound();
        return Ok(new AvailabilityDto(id, start, end, await availability.IsAvailableAsync(id, start, end)));
    }

    private static VehicleDto ToDto(Vehicle v) => new(v.Id, v.Brand!.Name, v.Model, v.Year,
        v.Category.GetDisplayName(), v.Fuel.GetDisplayName(), v.Transmission.GetDisplayName(),
        v.Seats, v.DailyPrice, v.Deposit, v.Branch!.Name, v.ImageUrl);
}
