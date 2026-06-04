import { PrismaClient } from '../prisma/generated/client2';
const prisma = new PrismaClient();

async function main() {
  const parts = await prisma.listing.findMany({ where: { category: 'PART' } });

  for (let i = 0; i < parts.length; i++) {
    await prisma.listing.update({
      where: { id: parts[i].id },
      data: { 
        images: null
      }
    });
  }

  console.log("Parts images have been successfully reset to local fallbacks.");
}

main().catch(console.error).finally(() => prisma.$disconnect());
