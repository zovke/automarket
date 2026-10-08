using System.ComponentModel.DataAnnotations;

namespace AracKiralama.Web.Models.Entities;

public class Brand
{
    public int Id { get; set; }
    [MaxLength(50)] public string Name { get; set; } = "";
    [MaxLength(250)] public string? LogoUrl { get; set; }

    public ICollection<Vehicle> Vehicles { get; set; } = new List<Vehicle>();
}
