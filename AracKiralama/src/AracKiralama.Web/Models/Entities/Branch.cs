using System.ComponentModel.DataAnnotations;

namespace AracKiralama.Web.Models.Entities;

/// <summary>
/// Kiralama şubesi (araç alma / iade noktası).
/// Basit bir tanım tablosu olduğu için admin formunda doğrudan bu sınıf kullanılır;
/// [Display] ve [Required] etiketleri form doğrulamasını sağlar.
/// </summary>
public class Branch
{
    public int Id { get; set; }

    [Display(Name = "Şube adı"), Required(ErrorMessage = "Şube adı zorunludur."), MaxLength(100)]
    public string Name { get; set; } = "";

    [Display(Name = "Şehir"), Required(ErrorMessage = "Şehir zorunludur."), MaxLength(50)]
    public string City { get; set; } = "";

    [Display(Name = "Adres"), Required(ErrorMessage = "Adres zorunludur."), MaxLength(250)]
    public string Address { get; set; } = "";

    [Display(Name = "Telefon"), Required(ErrorMessage = "Telefon zorunludur."), MaxLength(20)]
    public string Phone { get; set; } = "";

    public ICollection<Vehicle> Vehicles { get; set; } = new List<Vehicle>();
}
