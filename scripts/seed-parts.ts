import { PrismaClient } from '../prisma/generated/client2';
const prisma = new PrismaClient();

async function main() {
  const admin = await prisma.user.findFirst({ where: { role: 'ADMIN' } });
  if (!admin) {
     console.log("No ADMIN user found to tie the part to.");
     return;
  }

  await prisma.listing.create({
    data: {
      title: "Orijinal Audi A3 Ön Fren Balatası Seti",
      description: "Audi A3 2012-2020 modelleriyle uyumlu yüksek performanslı orijinal ön fren balatası. Kutusunda sıfır ürün.",
      price: 2450,
      category: "PART",
      userId: admin.id,
      brand: "Bosch",
      isAvailable: true
    }
  });

  await prisma.listing.create({
    data: {
      title: "BMW 3 Serisi F30 Sağ Orijinal Far",
      description: "F30 kasa BMW 3 serisi için orijinal yedek yedek far takımı. Çatlaksız ve sıfır ayarında temiz durumda. Sınırlı stok.",
      price: 4900,
      category: "PART",
      userId: admin.id,
      brand: "BMW Kırmızı",
      isAvailable: true
    }
  });

  await prisma.listing.create({
    data: {
      title: "Renault Clio 4 Sol Aynası Montajlı",
      description: "Elektrikli, isitmali ve sinyalli sol dikiz aynası. Orijinal fabrika çıkışı kutusundan alınmıştır.",
      price: 1850,
      category: "PART",
      userId: admin.id,
      brand: "Renault",
      isAvailable: true
    }
  });

  console.log("3 Yedek Parca basariyla seed edildi!");
}

main().catch(console.error).finally(() => prisma.$disconnect());
