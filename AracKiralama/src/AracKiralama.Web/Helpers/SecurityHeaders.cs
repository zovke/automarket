namespace AracKiralama.Web.Helpers;

/// <summary>
/// Her yanıta güvenlik başlıkları ekler.
/// Content-Security-Policy (CSP): tarayıcıya "sadece bu siteden gelen script / stil / yazı tipini çalıştır" der.
/// Böylece sayfaya dışarıdan zararlı bir script enjekte edilse bile çalışmaz (XSS koruması).
/// Bu yüzden projede inline &lt;script&gt; yoktur; bütün JavaScript wwwroot/js altındadır.
/// </summary>
public static class SecurityHeaders
{
    private const string ContentSecurityPolicy =
        "default-src 'self'; " +
        "script-src 'self'; " +
        "style-src 'self' 'unsafe-inline'; " +   // style="..." öznitelikleri için
        "img-src 'self' data:; " +               // favicon data: URI olarak tanımlı
        "font-src 'self'; " +
        "connect-src 'self'; " +
        "frame-ancestors 'self'; " +
        "base-uri 'self'; " +
        "form-action 'self'";

    public static IApplicationBuilder UseSecurityHeaders(this IApplicationBuilder app)
        => app.Use(async (context, next) =>
        {
            var headers = context.Response.Headers;
            headers["X-Content-Type-Options"] = "nosniff";
            headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
            headers["X-Frame-Options"] = "SAMEORIGIN";

            // Swagger arayüzü kendi inline script'lerini kullandığı için bu yolda CSP uygulanmaz.
            if (!context.Request.Path.StartsWithSegments("/swagger"))
                headers["Content-Security-Policy"] = ContentSecurityPolicy;

            await next();
        });
}
