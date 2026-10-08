using System.ComponentModel.DataAnnotations;
using AracKiralama.Web.Models.Enums;

namespace AracKiralama.Web.Models.Entities;

/// <summary>Bakım kaydı. Tamamlanmamış bakım tarihleri arasında araç kiralanamaz.</summary>
public class Maintenance
{
    public int Id { get; set; }
    public int VehicleId { get; set; }
    public Vehicle? Vehicle { get; set; }

    public MaintenanceType Type { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int Kilometers { get; set; }
    public decimal Cost { get; set; }
    [MaxLength(500)] public string? Description { get; set; }
    public bool IsCompleted { get; set; }
}
