# Auto Marketplace

Türkiye'nin modern, dinamik ve yenilikçi araç & yedek parça ilan platformu. Yüzlerce aracı saniyeler uzaklığınızda buluşturan, yüksek performanslı ve tam yığın (full-stack) Next.js uygulamasıdır.

## 🚀 Teknolojiler
- **Framework:** Next.js (App Router)
- **Veritabanı:** Neon PostgreSQL & Prisma ORM
- **Kimlik Doğrulama:** NextAuth.js (Custom Credentials - Bcrypt Hashing)
- **CSS / UI:** Tailwind CSS (Modern Animations & Glassmorphism)
- **Versiyon Kontrolü:** Git & GitHub

---

## ✅ Şu Ana Kadar Yapılanlar

- [x] **Veritabanı Entegrasyonu:**
  - Neon PostgreSQL bağlantısı kuruldu.
  - Prisma aracılığıyla `User` (Kullanıcı) ve `Listing` (İlan/Araç) modelleri tek bir yapıda (`Category` enum: `SALE`, `RENT`, `PART` uyumlu) birleştirildi.
- [x] **Yetkilendirme ve Güvenlik:**
  - `NextAuth.js` kullanılarak kullanıcı giriş yapma (`/login`) ve kayıt olma (`/register`) sayfaları entegre edildi.
  - Şifreler `bcrypt` ile hash'lenerek güvenceye alındı.
  - Session state yönetimi için global `SessionProvider` eklendi.
- [x] **Gelişmiş Admin Ekranı:**
  - `/admin` dizini sadece `ADMIN` yetkisine sahip kullanıcıların erişimine açıldı.
  - **Dashboard:** Sistemdeki toplam kullanıcı ve ilan sayılarının takibi.
  - **Kullanıcı Yönetimi:** Sisteme kayıtlı kullanıcıların rolleriyle birlikte listelenmesi.
  - **İlan Yönetimi:** Eklenen her ilanın listelenmesi ve admin üzerinden kontrol edilebilir altyapı paneli hazırlandı.
- [x] **Arayüz Entegrasyonu:**
  - Anasayfa listelemeleri ve `/arac/[id]` detay sayfaları veritabanında yer alan `Listing` modeli ile uyumlu hale getirildi.

---

## 🔮 Yapılacaklar Listesi (To-Do)

Uygulamanın vizyonunu tamamlamak ve fonksiyonel bir pazara dönüştürmek için aşağıdaki adımlar hedeflenmektedir:

1. **İlan Ekleme Altyapısı (Kullanıcı)**
   - Normal `USER` yetkisindeki kullanıcıların site üzerinden araç / kiralık araç / yedek parça ilanı verebilmesi için form oluşturulması.
2. **Gelişmiş Filtreleme ve Arama Sistemi**
   - Anasayfada Marka, Model, Vites, Yakıt ve Fiyat Aralığına göre dinamik Prisma sorgularıyla arama yapılması.
3. **Favoriler Sistemini Veritabanına Taşıma**
   - Şu anda Context API (Client-Side) ile çalışan Favori Ekle/Çıkar yeteneğinin, kayıtlı kullanıcılar için veritabanında saklanması.
4. **Fotoğraf ve Dosya Yükleme (Upload)**
   - Yeni ilan eklerken kullanıcıların fotoğraf yükleyebilmesi (Cloudinary veya AWS S3 entegrasyonu).
5. **Mesajlaşma ve Satıcıyla İletişim**
   - İlan detay sayfasından güvenli bir şekilde satıcıyla mail/uygulama-içi sohbet yapılması.

---

## 🛠 Kurulum ve Çalıştırma

Projeyi yerel bilgisayarınızda çalıştırmak için aşağıdaki adımları izleyin:

1. Gerekli paketleri indirin:
   ```bash
   npm install
   ```

2. Prisma ve Veritabanı istemcisini oluşturun:
   ```bash
   npx prisma generate
   ```

3. Geliştirme sunucusunu başlatın:
   ```bash
   npm run dev
   ```

Projeye [http://localhost:3000](http://localhost:3000) bağlantısından erişebilirsiniz.
