using System.ComponentModel.DataAnnotations;

namespace AracKiralama.Web.Models.Entities;

/// <summary>
/// Kiralamaya eklenebilen ek hizmet (bebek koltuğu, navigasyon, tam kasko...).
/// Basit tanım tablosu → admin formunda doğrudan kullanılır.
/// </summary>
public class Extra
{
    public int Id { get; set; }

    [Display(Name = "Ad"), Required(ErrorMessage = "Ad zorunludur."), MaxLength(60)]
    public string Name { get; set; } = "";

    [Display(Name = "Açıklama"), MaxLength(250)]
    public string? Description { get; set; }

    /// <summary>Bootstrap Icons sınıf adı, örn: "bi-shield-check". Liste: https://icons.getbootstrap.com</summary>
    [Display(Name = "İkon"), Required(ErrorMessage = "İkon zorunludur."), MaxLength(40)]
    public string Icon { get; set; } = "bi-plus-circle";

    [Display(Name = "Günlük fiyat (₺)"), Range(0, 100_000, ErrorMessage = "Geçerli bir fiyat giriniz.")]
    public decimal DailyPrice { get; set; }

    [Display(Name = "Aktif")]
    public bool IsActive { get; set; } = true;
}
