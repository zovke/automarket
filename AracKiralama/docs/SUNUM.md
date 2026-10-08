# Sunum Notları (≈ 10 dakika)

> **Sunumdan 2 dakika önce** https://automarket.ardcek.com adresini bir kez açın (ücretsiz sunucu uykudan uyansın). İnternet yoksa yerelde `dotnet run` ile çalıştırın; hiçbir dosya internetten yüklenmez.

## Açılış (1 dk)
"Araç kiralama firmasının hem müşteri sitesini hem de şube personelinin kullandığı otomasyonu C# ile geliştirdik. Asıl odağımız gerçek iş kuralları: aynı araç aynı tarihte iki kişiye verilemez, fiyat otomatik hesaplanır, geç iade ve eksik yakıt otomatik ücretlendirilir."

## Canlı demo akışı

**1. Müşteri (3 dk)** — `musteri@rota.com`
1. Ana sayfa → şube ve tarih seç → **Araç Bul**. Sadece o tarihlerde boş araçlar gelir.
2. Filtre: SUV + Otomatik. URL'nin değiştiğini göster (link paylaşılabilir).
3. Araç detayı → tarihi 8 güne çıkar: **%10 indirim** satırı anında belirir. Tam kasko ekle, iade şubesini değiştir → tek yön ücreti.
4. **Rezervasyon Yap** → 3 adımlı sihirbaz → talebi gönder → rezervasyon kodu.
5. (İsteğe bağlı) `genc@rota.com` ile lüks araç dene → "En az 27 yaşında olmalısınız" uyarısı.

**2. Personel (4 dk)** — `admin@rota.com`
1. Gösterge paneli: KPI'lar, aylık gelir grafiği, gecikme uyarısı, bugünün işleri.
2. **Doluluk takvimi**: araç × gün; renkler durumları gösterir.
3. Rezervasyonlar → az önceki talep → **Onayla**. Aynı araca çakışan başka talep varsa otomatik reddedilir.
4. Onaylanan rezervasyonu **Teslim et** (km, yakıt, kontrol listesi).
5. Dashboard'daki kırmızı **gecikme uyarısı** → gecikmiş bir kiralamayı aç → **İade al**: tahmini gecikme ücreti hazır gelir; yakıtı 1/2 seç → gecikme ve yakıt ücretleri otomatik eklenir.
6. Ödeme al → kalan bakiye güncellenir. **Sözleşme PDF**'i aç.
7. Raporlar → **Excel'e aktar**.

**3. Teknik (2 dk)**
1. `/swagger` → `POST /api/quote` dene: "Sitedeki canlı fiyat hesaplayıcı da bu API'yi kullanıyor."
2. `dotnet test` → 57 test yeşil.
3. Kod: `Services/ReservationService.cs` içindeki durum geçiş tablosu ve `ApproveAsync` transaction'ı.

## Hocanın sorabileceği sorular

| Soru | Cevap |
|---|---|
| Aynı araç iki kişiye kiralanabilir mi? | Hayır. `AvailabilityService` çakışma kontrolü yapar; onay işlemi **transaction** içinde tekrar kontrol eder ve çakışan bekleyen talepleri reddeder. Testi: `Onaylaninca_cakisan_bekleyen_talepler_otomatik_reddedilir`. |
| Fiyat değişirse eski faturalar ne olur? | Fiyatlar rezervasyon anında kopyalanır (snapshot); eski kayıtlar etkilenmez. |
| Neden SQLite? | Kurulum gerektirmez, proje her bilgisayarda `dotnet run` ile açılır. EF Core sayesinde SQL Server'a geçiş birkaç satır (README). |
| Şifreler nasıl saklanıyor? | ASP.NET Core Identity — PBKDF2 ile hash'lenir, düz metin saklanmaz. |
| Yetkilendirme nasıl? | Rol bazlı: `[Authorize(Roles = "Admin,Personel")]`. Müşteri `/Admin`'e girerse "Erişim engellendi". Personel, "Personel & Roller" sayfasını göremez. |
| Neden servis katmanı? | Kurallar tek yerde, controller'dan bağımsız → test edilebilir. Aynı servis hem MVC sayfaları hem API tarafından kullanılıyor. |
| Gecikmeler nasıl tespit ediliyor? | `OverdueReservationWorker` (BackgroundService) saatte bir çalışır, iade tarihi geçmiş kiralamaları işaretler. |
| Nerede yayında, nasıl? | Docker imajı olarak Render.com'da; GitHub'a push edince otomatik yayınlanır. Hostinger paketi C# çalıştıramadığı için ardcek.com/automarket alt alan adına yönlendirir. |
| Güvenlik başlıkları? | Her sayfada Content-Security-Policy: sadece kendi script'lerimiz çalışır, inline script ve dış CDN yok (XSS koruması). `Helpers/SecurityHeaders.cs`. |
| T.C. kimlik doğrulaması? | `Validation/TcKimlikNo.cs` — resmi algoritma (10. ve 11. hane kontrolü). |

## Ekip iş bölümü (öneri)
- **Backend**: Entity'ler, servisler, testler, API
- **Müşteri arayüzü**: ana sayfa, araç listesi/detay, rezervasyon sihirbazı, hesabım
- **Yönetim paneli**: dashboard, takvim, rezervasyon operasyonları, raporlar

Her kişi kendi bölümünü sunarsa hoca "kim ne yaptı" sorusunun cevabını doğrudan görür.
