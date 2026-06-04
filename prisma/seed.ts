import { PrismaClient } from "./generated/client2";
import * as fs from 'fs';
import * as path from 'path';

const prisma = new PrismaClient();

async function main() {
  console.log("Veritabanı sıfırlanıyor ve yeniden tohumlanıyor (Seeding)...");

  // Wipe previous data
  await prisma.request.deleteMany({});
  await prisma.favorite.deleteMany({});
  await prisma.listing.deleteMany({});
  await prisma.user.deleteMany({});

  // Create an admin user first
  const admin = await prisma.user.create({
    data: {
      email: 'admin@example.com',
      password: '$2b$10$eW4E5yYlIHE2yJ9UuFhW1u9T0QzK85S2I.j6xYfT21O75bL82.e.y', // admin123
      role: 'ADMIN',
      firstName: 'Süper',
      lastName: 'Admin'
    },
  });

  const carsDir = path.join(process.cwd(), 'public', 'cars');
  let files: string[] = [];
  try {
     files = fs.readdirSync(carsDir);
  } catch(e) {
     console.log("No cars directory found.");
  }

  for(const file of files) {
      if(!file.endsWith('.jpg') && !file.endsWith('.png')) continue;
      
      const fileName = file.replace('.jpg', '').replace('.png', '');
      const parts = fileName.split('_');
      // e.g "Audi_A3" -> brand: Audi, model: A3
      const brand = parts[0];
      const model = parts.slice(1).join(' ').replace(/_/g, ' ');

      const category = Math.random() > 0.8 ? 'RENT' : 'SALE'; // 20% Rent, 80% Sale
      const price = category === 'RENT' ? Math.floor(Math.random() * 2000) + 1000 : Math.floor(Math.random() * 2000000) + 500000;
      
      await prisma.listing.create({
          data: {
              title: `${brand} ${model} Özel Fırsat! Temiz Araç`,
              description: `Orijinal kilometrede, kapalı garaj arabası, tertemiz bir ${brand} ${model}. Detaylı bilgi için taleplerinizi bekliyoruz.`,
              price,
              category,
              brand,
              model,
              year: Math.floor(Math.random() * 10) + 2014,
              kilometers: Math.floor(Math.random() * 150000),
              fuel: Math.random() > 0.5 ? "Diesel" : "Petrol",
              transmission: Math.random() > 0.5 ? "Automatic" : "Manual",
              images: JSON.stringify([`/cars/${file}`]), // Storing the image correctly
              isAvailable: true,
              userId: admin.id
          }
      });
  }

  console.log(`Tamamlandı. Toplam ${files.length} ilan oluşturuldu.`);
}

main()
  .catch((e) => {
    console.error(e);
    process.exit(1);
  })
  .finally(async () => {
    await prisma.$disconnect();
  });
