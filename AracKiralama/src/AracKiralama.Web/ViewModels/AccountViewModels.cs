using System.ComponentModel.DataAnnotations;
using AracKiralama.Web.Models.Entities;
using AracKiralama.Web.Validation;

namespace AracKiralama.Web.ViewModels;

public class LoginViewModel
{
    [Display(Name = "E-posta"), Required(ErrorMessage = "E-posta zorunludur."), EmailAddress(ErrorMessage = "Geçerli bir e-posta giriniz.")]
    public string Email { get; set; } = "";

    [Display(Name = "Şifre"), Required(ErrorMessage = "Şifre zorunludur."), DataType(DataType.Password)]
    public string Password { get; set; } = "";

    [Display(Name = "Beni hatırla")]
    public bool RememberMe { get; set; } = true;

    public string? ReturnUrl { get; set; }
}

public class RegisterViewModel
{
    [Display(Name = "Ad"), Required(ErrorMessage = "Ad zorunludur."), MaxLength(50)]
    public string FirstName { get; set; } = "";

    [Display(Name = "Soyad"), Required(ErrorMessage = "Soyad zorunludur."), MaxLength(50)]
    public string LastName { get; set; } = "";

    [Display(Name = "E-posta"), Required(ErrorMessage = "E-posta zorunludur."), EmailAddress(ErrorMessage = "Geçerli bir e-posta giriniz.")]
    public string Email { get; set; } = "";

    [Display(Name = "Telefon"), Phone(ErrorMessage = "Geçerli bir telefon giriniz.")]
    public string? PhoneNumber { get; set; }

    [Display(Name = "Şifre"), Required(ErrorMessage = "Şifre zorunludur."), DataType(DataType.Password)]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Şifre en az 6 karakter olmalıdır.")]
    public string Password { get; set; } = "";

    [Display(Name = "Şifre (tekrar)"), DataType(DataType.Password)]
    [Compare(nameof(Password), ErrorMessage = "Şifreler eşleşmiyor.")]
    public string ConfirmPassword { get; set; } = "";

    [Range(typeof(bool), "true", "true", ErrorMessage = "KVKK aydınlatma metnini onaylamalısınız.")]
    public bool AcceptKvkk { get; set; }

    public string? ReturnUrl { get; set; }
}

public class ProfileViewModel
{
    [Display(Name = "Ad"), Required(ErrorMessage = "Ad zorunludur."), MaxLength(50)]
    public string FirstName { get; set; } = "";

    [Display(Name = "Soyad"), Required(ErrorMessage = "Soyad zorunludur."), MaxLength(50)]
    public string LastName { get; set; } = "";

    [Display(Name = "E-posta")]
    public string? Email { get; set; }

    [Display(Name = "Telefon"), Phone(ErrorMessage = "Geçerli bir telefon giriniz.")]
    public string? PhoneNumber { get; set; }

    [Display(Name = "T.C. Kimlik No"), TcKimlikNo]
    public string? TcNo { get; set; }

    [Display(Name = "Doğum tarihi"), DataType(DataType.Date), Required(ErrorMessage = "Doğum tarihi zorunludur.")]
    public DateTime? BirthDate { get; set; }

    [Display(Name = "Ehliyet no"), MaxLength(30)]
    public string? LicenseNumber { get; set; }

    [Display(Name = "Ehliyet veriliş tarihi"), DataType(DataType.Date), Required(ErrorMessage = "Ehliyet tarihi zorunludur.")]
    public DateTime? LicenseIssueDate { get; set; }

    public string? ReturnUrl { get; set; }
}

public class MyReservationsViewModel
{
    public required AppUser User { get; init; }
    public List<Reservation> Upcoming { get; init; } = [];
    public List<Reservation> Past { get; init; } = [];
    public bool ProfileComplete => User.BirthDate.HasValue && User.LicenseIssueDate.HasValue;
}
