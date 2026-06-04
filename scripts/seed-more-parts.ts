import { PrismaClient } from '../prisma/generated/client2';
const prisma = new PrismaClient();

async function main() {
  const admin = await prisma.user.findFirst({ where: { role: 'ADMIN' } }) || await prisma.user.findFirst();
  
  if (!admin) return;

  const newParts = [
    { brand: "Castrol", title: "Castrol Edge 5W-30 Tam Sentetik Motor Yağı", description: "4 Litre tam sentetik yüksek performanslı motor yağı. Turbo şarjlı modern motorlara uygundur.", price: 1250, model: "C-100" },
    { brand: "Marelli", title: "Magneti Marelli Orijinal Akü 72 Amper", description: "Binek araçlar için uzun ömürlü, start-stop uyumlu 72 Ah kapasiteli güvenilir akü.", price: 2850, model: "M-72A" },
    { brand: "Bosch", title: "Bosch Orijinal İridyum Buji Takımı (4'lü)", description: "Daha verimli ateşleme, daha düşük yakıt tüketimi. Tüm VAG grubu araçlarla uyumludur.", price: 890, model: "B-IR-4" },
    { brand: "Valeo", title: "Valeo Sessiz Kauçuk Silecek Takımı", description: "Aero özellikli, camsil suyu entegreli esnek cam sileceği. Soğuğa ve ısıya dayanıklıdır.", price: 420, model: "V-Sil-01" },
    { brand: "Continental", title: "Continental Triger Eksantrik Kayış Seti", description: "V kayışı, gergiler ve orijinal triger seti tam takım. Dayanıklı kauçuk malzemeden üretilmiştir.", price: 3400, model: "CT-E-1" },
    { brand: "Sachs", title: "Sachs Orijinal Ön Amortisör Takımı", description: "Gelişmiş süspansiyon konforu. Çukurlu yollarda üstün darbe emici gazlı yapı.", price: 4100, model: "SA-O-AM" },
    { brand: "Denso", title: "Denso Klima Kompresörü A Kalite", description: "Araç içi soğutmayı ilk günkü seviyesine çıkaran performanslı OEM klima kompresörü.", price: 8500, model: "DN-KLM-9" },
    { brand: "Hella", title: "Hella Standart H7 Halojen Ampul Çifti", description: "Güçlendirilmiş beyaz ışık. Uzun gece görüşleri için optimal ışın hüzmesi.", price: 350, model: "H7-HL" },
    { brand: "Mann", title: "Mann Filter Orijinal Karbonlu Polen Filtresi", description: "Araç içini toz, polen ve kötü kokulardan %99 oranında temizleyen aktif karbon filtre.", price: 450, model: "MN-KF-55" },
    { brand: "LUK", title: "LUK Baskı Balata Volant Debriyaj Seti", description: "Ağır hizmete dayanıklı, vites geçişlerini yumuşatan sıfır OEM debriyaj kiti.", price: 6200, model: "LK-BB-22" }
  ];

  for (const part of newParts) {
    await prisma.listing.create({
      data: {
        title: part.title,
        description: part.description,
        price: part.price,
        category: "PART",
        brand: part.brand,
        model: part.model,
        year: 2024,
        kilometers: 0,
        fuel: "OTHER",
        transmission: "MANUAL",
        isAvailable: true,
        userId: admin.id
      }
    });
  }

  console.log("Added 10 new spare parts exclusively for the catalog!");
}

main().catch(console.error).finally(() => prisma.$disconnect());
