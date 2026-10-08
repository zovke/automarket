using System.Globalization;
using System.Text.Encodings.Web;
using System.Text.Unicode;
using Microsoft.Extensions.WebEncoders;
using AracKiralama.Web.Data;
using AracKiralama.Web.Helpers;
using AracKiralama.Web.Models.Entities;
using AracKiralama.Web.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ---------- Ayarlar (appsettings.json) ----------
builder.Services.Configure<PricingOptions>(builder.Configuration.GetSection("Pricing"));
builder.Services.Configure<SiteOptions>(builder.Configuration.GetSection("Site"));

// ---------- Veritabanı ----------
// SQL Server'a geçmek için: UseSqlite → UseSqlServer ve connection string'i değiştirin.
builder.Services.AddDbContext<AppDbContext>(o =>
    o.UseSqlite(builder.Configuration.GetConnectionString("Default")));

// ---------- Kimlik doğrulama (ASP.NET Core Identity) ----------
builder.Services
    .AddIdentity<AppUser, IdentityRole>(o =>
    {
        o.User.RequireUniqueEmail = true;
        o.Password.RequiredLength = 6;
        o.Password.RequireNonAlphanumeric = false;
        o.Password.RequireUppercase = false;
    })
    .AddEntityFrameworkStores<AppDbContext>()
    .AddErrorDescriber<TurkishIdentityErrorDescriber>()
    .AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(o =>
{
    o.LoginPath = "/Account/Login";
    o.AccessDeniedPath = "/Account/AccessDenied";
    o.ExpireTimeSpan = TimeSpan.FromDays(7);
});

// ---------- Uygulama servisleri (iş kuralları) ----------
// Scoped = her HTTP isteği için bir örnek oluşturulur.
builder.Services.AddScoped<IPricingService, PricingService>();
builder.Services.AddScoped<IAvailabilityService, AvailabilityService>();
builder.Services.AddScoped<IReservationService, ReservationService>();
builder.Services.AddScoped<IReportService, ReportService>();
builder.Services.AddScoped<IContractPdfService, ContractPdfService>();
builder.Services.AddScoped<IImageService, ImageService>();
builder.Services.AddHostedService<OverdueReservationWorker>();

QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;
// Fontta olmayan bir karakter (ör. emoji) girilirse PDF üretimi durmasın, o karakter boş geçilsin.
QuestPDF.Settings.ThrowOnMissingTextGlyphs = false;

// ---------- MVC + API ----------
builder.Services.AddControllersWithViews(o => TurkishModelBindingMessages.Apply(o.ModelBindingMessageProvider));

// Türkçe karakterler (ş, ğ, ı, ₺...) HTML çıktısında &#x15F; gibi kodlanmadan, olduğu gibi yazılsın.
builder.Services.Configure<WebEncoderOptions>(o => o.TextEncoderSettings = new TextEncoderSettings(UnicodeRanges.All));
builder.Services.AddOpenApi();

var app = builder.Build();

// Form verisindeki sayılar "1500.50" biçiminde gelir; okuma kültürden bağımsız olsun.
// Ekrandaki Türkçe biçimlendirme Helpers/Fmt.cs ile yapılır.
CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo("tr-TR");

// ---------- Veritabanını oluştur + örnek verileri yükle ----------
using (var scope = app.Services.CreateScope())
{
    await DbSeeder.SeedAsync(scope.ServiceProvider);
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/Home/StatusCode", "?code={0}");

app.UseHttpsRedirection();
app.UseStaticFiles(); // wwwroot/uploads altına sonradan yüklenen görseller için
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

// API dokümantasyonu: /openapi/v1.json  ve arayüz: /swagger
app.MapOpenApi();
app.UseSwaggerUI(o =>
{
    o.SwaggerEndpoint("/openapi/v1.json", "Rota Araç Kiralama API");
    o.DocumentTitle = "Rota API";
});

app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();

// Test projesinin WebApplicationFactory ile erişebilmesi için.
public partial class Program;
