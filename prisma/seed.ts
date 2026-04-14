import { PrismaClient } from '@prisma/client';

const prisma = new PrismaClient();

async function main() {
  // Orijinal oluşturduğumuz json dosyasını referans alalım (ya da doğrudan burada tanımlayabiliriz).
  // Hızlı çalışması için doğrudan js nesnesi olarak aktarıyoruz:
  const brandsData = [
    { name: "Renault", models: ["Clio", "Megane", "Talisman", "Symbol", "Broadway", "Express"] },
    { name: "Volkswagen", models: ["Passat", "Jetta", "Transporter", "Caddy", "Arteon", "Golf", "Polo", "Tiguan"] },
    { name: "Audi", models: ["A3", "A4", "A5", "A6", "A7", "Q3", "Q5", "Q7"] },
    { name: "Peugeot", models: ["208", "206", "307", "3008", "5008", "308", "301"] },
    { name: "Fiat", models: ["Linea", "Egea", "Palio", "Doblo", "500L", "Fiorino"] },
    { name: "Hyundai", models: ["i20", "i30", "i20N", "Tucson", "Accent", "Accent Blue", "Bayon"] },
    { name: "Ford", models: ["Focus", "Mondeo", "Fiesta", "Tourneo Courier", "Transit"] }
  ];

  console.log('Seeding vehicles...');
  
  // Real-world approximate baseline prices in TRY for 2020 models
  const basePrices: Record<string, number> = {
    "Clio": 850000, "Megane": 1150000, "Talisman": 1600000, "Symbol": 650000, "Broadway": 250000, "Express": 700000,
    "Passat": 2100000, "Jetta": 1200000, "Transporter": 1400000, "Caddy": 1050000, "Arteon": 2800000, "Golf": 1450000, "Polo": 1000000, "Tiguan": 2300000,
    "A3": 1700000, "A4": 2600000, "A5": 3200000, "A6": 4500000, "A7": 5500000, "Q3": 2400000, "Q5": 3800000, "Q7": 6500000,
    "208": 950000, "206": 350000, "307": 450000, "3008": 1800000, "5008": 2200000, "308": 1300000, "301": 700000,
    "Linea": 550000, "Egea": 850000, "Palio": 300000, "Doblo": 750000, "500L": 900000, "Fiorino": 600000,
    "i20": 900000, "i30": 1100000, "i20N": 1600000, "Tucson": 2100000, "Accent": 600000, "Accent Blue": 750000, "Bayon": 1200000,
    "Focus": 1250000, "Mondeo": 1700000, "Fiesta": 850000, "Tourneo Courier": 850000, "Transit": 1300000
  };

  for (const brandObj of brandsData) {
    for (const [index, modelName] of brandObj.models.entries()) {
      // Rastgelelik katıyoruz (Satılık / Kiralık)
      const isRental = Math.random() > 0.8; // %20 kiralık
      
      const year = 2015 + Math.floor(Math.random() * 10); // 2015-2024
      const kilometers = Math.floor(Math.random() * 200000);
      
      let basePrice = basePrices[modelName] || 1000000;
      
      // Yıla ve kilometreye göre fiyatı gerçekçi şekilde ayarla
      const yearFactor = 1 - ((2024 - year) * 0.05); // Her yıl için %5 değer kaybı
      const kmFactor = 1 - (kilometers / 300000); // 300 bin km'de %100 değer kaybı varsayımı üzerinden orantı
      let calculatedPrice = Math.floor(basePrice * yearFactor * kmFactor);
      
      if (calculatedPrice < 200000) calculatedPrice = 200000; // Minimum fiyat
      
      // Kiralık fiyatı (günlük, satış fiyatının ortalama 1500'de 1'i)
      const rentPrice = Math.floor(calculatedPrice / 1500);

      await prisma.vehicle.create({
        data: {
          brand: brandObj.name,
          model: modelName,
          year: year,
          kilometers: kilometers,
          price: isRental ? rentPrice : calculatedPrice,
          type: isRental ? "RENTAL" : "SALE",
          fuel: ["Diesel", "Petrol", "Hybrid", "Electric"][Math.floor(Math.random() * 4)],
          transmission: ["Manual", "Automatic"][Math.floor(Math.random() * 2)],
          images: JSON.stringify(["/placeholder-car.jpg"]), // Varsayılan görsel
          description: `Sahibinden tertemiz ${brandObj.name} ${modelName}. ${year} model, ${new Intl.NumberFormat('tr-TR').format(kilometers)} km'de. Tüm bakımları yetkili serviste yapılmıştır.`,
          isAvailable: true
        }
      });
    }
  }

  console.log('Vehicles seeded successfully!');
  
  // Seed sample spare parts
  console.log('Seeding spare parts...');
  const parts = [
    { name: "Fren Balatası", category: "Fren Sistemi", price: 1250, stock: 50, compatibleBrands: "Renault, Fiat, Ford" },
    { name: "Hava Filtresi", category: "Filtreler", price: 350, stock: 100, compatibleBrands: "Volkswagen, Audi, Peugeot" },
    { name: "Triger Kayışı", category: "Motor Parçaları", price: 3200, stock: 15, compatibleBrands: "Hyundai, Ford" },
    { name: "Motor Yağı (5W-30)", category: "Sıvı & Bakım", price: 1500, stock: 200, compatibleBrands: "Tüm Markalar" },
  ];

  for (const part of parts) {
    await prisma.sparePart.create({
      data: {
        ...part,
        imageUrl: "/placeholder-part.jpg",
        description: `Yüksek kaliteli ${part.name}. Uyumlu araçlar: ${part.compatibleBrands}.`,
      }
    });
  }

  console.log('Spare parts seeded successfully!');
}

main()
  .catch((e) => {
    console.error(e);
    process.exit(1);
  })
  .finally(async () => {
    await prisma.$disconnect();
  });
