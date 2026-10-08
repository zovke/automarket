# Geliştirici Rehberi

Bu rehber, projeye yeni bir şey eklerken hangi dosyalara hangi sırayla dokunacağınızı gösterir. Her örnek gerçekten çalışan, küçük bir değişikliktir.

---

## 1. Araca yeni bir alan eklemek (örnek: "Bagaj hacmi")

**① Entity** — `src/AracKiralama.Web/Models/Entities/Vehicle.cs`

```csharp
/// <summary>Bagaj hacmi (litre).</summary>
public int LuggageLiters { get; set; }
```

**② Migration** — veritabanı tablosuna kolonu ekler (proje klasörü `AracKiralama/` iken):

```bash
dotnet ef migrations add AddLuggageLiters --project src/AracKiralama.Web --output-dir Data/Migrations
```

Uygulama açılırken `DbSeeder.SeedAsync` → `MigrateAsync()` migration'ı otomatik uygular.

**③ Form modeli** — `ViewModels/AdminViewModels.cs` → `VehicleFormViewModel`

```csharp
[Display(Name = "Bagaj (lt)"), Range(0, 3000)]
public int LuggageLiters { get; set; }
```

**④ Controller eşlemesi** — `Areas/Admin/Controllers/VehiclesController.cs`
- `Edit` (GET) içinde: `LuggageLiters = v.LuggageLiters,`
- `TryApplyAsync` içinde: `v.LuggageLiters = m.LuggageLiters;`

**⑤ Form** — `Areas/Admin/Views/Vehicles/Form.cshtml` içine diğer alanların yanına:

```html
<div class="col-md-3">
    <label asp-for="LuggageLiters" class="form-label"></label>
    <input asp-for="LuggageLiters" class="form-control" />
    <span asp-validation-for="LuggageLiters"></span>
</div>
```

**⑥ Gösterim** — `Views/Vehicles/Details.cshtml` içindeki özellik kutularına bir tane daha:

```html
<div class="col-6 col-md-3"><div class="spec-tile"><i class="bi bi-suitcase2"></i><div class="label">Bagaj</div><div class="value">@v.LuggageLiters lt</div></div></div>
```

Bitti. Toplam 5 dosya, her biri birkaç satır.

---

## 2. Yeni bir tanım sayfası eklemek (örnek: "Marka yönetimi")

Şube sayfası (`BranchesController`) klasik bir CRUD örneğidir; kopyalayıp uyarlamak en hızlı yoldur.

1. `Areas/Admin/Controllers/BranchesController.cs` → `BrandsController.cs` olarak kopyalayın, `Branch` → `Brand`, `Branches` → `Brands` değiştirin. `[Bind("...")]` içindeki alan adlarını `Name,LogoUrl` yapın.
2. `Areas/Admin/Views/Branches/` klasörünü `Brands/` olarak kopyalayın ve formdaki alanları `Name`, `LogoUrl` yapın.
3. `Models/Entities/Brand.cs` alanlarına `[Display(Name = "...")]` ve `[Required(...)]` ekleyin (Branch.cs'teki gibi).
4. Menüye bağlantı: `Views/Shared/_AdminLayout.cshtml` içinde "Şubeler" satırının altına:
   ```html
   <a class="@Active("Brands")" asp-area="Admin" asp-controller="Brands" asp-action="Index"><i class="bi bi-tags"></i>Markalar</a>
   ```

Veritabanı değişmediği için migration gerekmez.

---

## 3. Yeni bir fiyat kuralı eklemek (örnek: "Hafta sonu %15 zam")

Bütün fiyat mantığı `Services/PricingService.cs` → `Calculate` metodunda.

```csharp
// Alış günü cumartesi veya pazarsa kira bedeline %15 eklenir
var weekendRate = start.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday ? 0.15m : 0m;
var weekendFee = Math.Round(baseTotal * weekendRate, 2);
```

`PriceQuote` kaydına `WeekendFee` alanını ekleyip `total` hesabına dahil edin. Oranı kodda sabit tutmak yerine `Helpers/Options.cs` → `PricingOptions` içine `WeekendRate` ekleyip `appsettings.json` → `Pricing` bölümünden okuyabilirsiniz.

Sonra **test yazın** (`tests/AracKiralama.Tests/PricingServiceTests.cs`):

```csharp
[Fact]
public void Hafta_sonu_alista_yuzde_15_zam()
{
    var saturday = new DateTime(2026, 11, 7, 10, 0, 0);
    var q = _pricing.Calculate(Car, saturday, saturday.AddDays(2), [], isOneWay: false);
    Assert.Equal(300, q.WeekendFee);
}
```

`dotnet test` ile çalıştırın.

---

## 4. Sık karşılaşılan durumlar

| Durum | Çözüm |
|---|---|
| Örnek verileri baştan oluşturmak | Uygulamayı kapatın, `src/AracKiralama.Web/arackiralama.db` dosyasını silin, tekrar çalıştırın. |
| `dotnet test` "dosya kullanımda" hatası veriyor | Çalışan uygulamayı kapatın (Windows açık DLL'i kilitler). |
| Ekrandaki bir metni değiştirmek | Metinler doğrudan `.cshtml` dosyalarındadır; Visual Studio / VS Code'da "Find in Files" ile arayın. |
| Durum adlarını (ör. "Onay Bekliyor") değiştirmek | `Models/Enums/Enums.cs` içindeki `[Display(Name = "...")]` değerleri. |
| Bir iş kuralı hatasını kullanıcıya göstermek | Serviste `throw new BusinessRuleException("Türkçe mesaj")`; controller zaten yakalayıp bildirim gösteriyor. |
| Yeni bir bildirim göstermek | Controller'da `ShowSuccess("...")` veya `ShowError("...")`. |

---

## 5. Kod kuralları (ekip içi)

- **Controller ince, servis kalın**: Hesap ve kural kodu controller'a değil `Services/` altına.
- Para alanları her zaman `decimal`; ekranda `Fmt.Money(x)` ile gösterilir.
- Tarih gösterimi `Fmt.Date(...)` / `Fmt.DateTime(...)`.
- Formlar için ayrı ViewModel; basit tanım tabloları (Şube, Ekstra) entity'yi doğrudan kullanabilir.
- Enum'a yeni değer eklerken `[Display(Name = "...")]` vermeyi unutmayın.
- Her yeni iş kuralı için en az bir test.
- **View içine `<script>...</script>` veya `onclick="..."` yazmayın.** Güvenlik politikası (CSP) bunları engeller. JavaScript'i `wwwroot/js/` altına koyup `<script src="~/js/...">` ile ekleyin; tekrar kullanılan davranışlar için `site.js`'teki `data-*` kalıplarını (`data-confirm`, `data-auto-submit`, `data-image-preview`...) kullanın.
- Yeni bir kütüphane CDN'den değil, `wwwroot/lib/` altına indirilerek eklenir.
