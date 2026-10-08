using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace AracKiralama.Web.Helpers;

public static class EnumExtensions
{
    /// <summary>Enum değerinin [Display(Name = "...")] ile verilen Türkçe adını döndürür.</summary>
    public static string GetDisplayName(this Enum value)
    {
        var member = value.GetType().GetMember(value.ToString()).FirstOrDefault();
        return member?.GetCustomAttribute<DisplayAttribute>()?.GetName() ?? value.ToString();
    }
}
