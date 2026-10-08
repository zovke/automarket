namespace AracKiralama.Web.Services;

/// <summary>
/// İş kuralı ihlali (ör. "Araç bu tarihlerde dolu"). Mesaj doğrudan kullanıcıya gösterilir,
/// bu yüzden her zaman Türkçe ve anlaşılır yazılmalıdır.
/// </summary>
public class BusinessRuleException(string message) : Exception(message);
