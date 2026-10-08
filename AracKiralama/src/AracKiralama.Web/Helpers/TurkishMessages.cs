using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;

namespace AracKiralama.Web.Helpers;

/// <summary>Identity'nin (kayıt / şifre) hata mesajlarının Türkçesi.</summary>
public class TurkishIdentityErrorDescriber : IdentityErrorDescriber
{
    public override IdentityError DuplicateEmail(string email) => new() { Code = nameof(DuplicateEmail), Description = $"'{email}' adresiyle kayıtlı bir hesap zaten var." };
    public override IdentityError DuplicateUserName(string userName) => new() { Code = nameof(DuplicateUserName), Description = $"'{userName}' zaten kullanılıyor." };
    public override IdentityError InvalidEmail(string? email) => new() { Code = nameof(InvalidEmail), Description = "Geçersiz e-posta adresi." };
    public override IdentityError InvalidUserName(string? userName) => new() { Code = nameof(InvalidUserName), Description = "Kullanıcı adı geçersiz karakter içeriyor." };
    public override IdentityError PasswordTooShort(int length) => new() { Code = nameof(PasswordTooShort), Description = $"Şifre en az {length} karakter olmalıdır." };
    public override IdentityError PasswordRequiresDigit() => new() { Code = nameof(PasswordRequiresDigit), Description = "Şifre en az bir rakam içermelidir." };
    public override IdentityError PasswordRequiresLower() => new() { Code = nameof(PasswordRequiresLower), Description = "Şifre en az bir küçük harf içermelidir." };
    public override IdentityError PasswordRequiresUpper() => new() { Code = nameof(PasswordRequiresUpper), Description = "Şifre en az bir büyük harf içermelidir." };
    public override IdentityError PasswordRequiresNonAlphanumeric() => new() { Code = nameof(PasswordRequiresNonAlphanumeric), Description = "Şifre en az bir sembol içermelidir." };
    public override IdentityError PasswordMismatch() => new() { Code = nameof(PasswordMismatch), Description = "Mevcut şifre hatalı." };
}

/// <summary>Model binding ("'abc' geçerli bir sayı değil" gibi) mesajlarının Türkçesi.</summary>
public static class TurkishModelBindingMessages
{
    public static void Apply(DefaultModelBindingMessageProvider p)
    {
        p.SetValueMustNotBeNullAccessor(_ => "Bu alan zorunludur.");
        p.SetMissingBindRequiredValueAccessor(name => $"'{name}' alanı zorunludur.");
        p.SetMissingKeyOrValueAccessor(() => "Değer zorunludur.");
        p.SetAttemptedValueIsInvalidAccessor((value, _) => $"'{value}' geçerli bir değer değil.");
        p.SetUnknownValueIsInvalidAccessor(_ => "Girilen değer geçersiz.");
        p.SetValueIsInvalidAccessor(value => $"'{value}' geçersiz.");
        p.SetValueMustBeANumberAccessor(_ => "Bu alan sayı olmalıdır.");
        p.SetNonPropertyAttemptedValueIsInvalidAccessor(value => $"'{value}' geçerli bir değer değil.");
        p.SetNonPropertyUnknownValueIsInvalidAccessor(() => "Girilen değer geçersiz.");
        p.SetNonPropertyValueMustBeANumberAccessor(() => "Bu alan sayı olmalıdır.");
        p.SetMissingRequestBodyRequiredValueAccessor(() => "İstek gövdesi boş olamaz.");
    }
}
