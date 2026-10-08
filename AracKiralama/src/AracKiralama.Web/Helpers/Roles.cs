namespace AracKiralama.Web.Helpers;

/// <summary>Rol adları. [Authorize(Roles = ...)] içinde sabit olarak kullanılır.</summary>
public static class Roles
{
    public const string Admin = "Admin";
    public const string Staff = "Personel";
    public const string Customer = "Musteri";

    /// <summary>Yönetim paneline girebilen roller.</summary>
    public const string AdminOrStaff = Admin + "," + Staff;
}
