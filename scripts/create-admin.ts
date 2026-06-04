import { PrismaClient } from '../prisma/generated/client2';
import bcrypt from 'bcrypt';

const prisma = new PrismaClient();

async function main() {
  const hash = await bcrypt.hash("admin123", 10);
  await prisma.user.upsert({
    where: { email: "admin@example.com" },
    update: { password: hash, role: "ADMIN", firstName: "Sistem", lastName: "Yöneticisi" },
    create: { email: "admin@example.com", password: hash, role: "ADMIN", firstName: "Sistem", lastName: "Yöneticisi" }
  });
  console.log("Admin hesabı oluşturuldu/güncellendi: admin@example.com / admin123");
}

main().catch(console.error).finally(() => prisma.$disconnect());
