import { prisma } from "@/lib/prisma";

export default async function AdminDashboard() {
  const userCount = await prisma.user.count();
  const listingCount = await prisma.listing.count();
  
  const saleCount = await prisma.listing.count({ where: { category: "SALE" }});
  const rentCount = await prisma.listing.count({ where: { category: "RENT" }});
  const partCount = await prisma.listing.count({ where: { category: "PART" }});

  return (
    <div>
      <h1 className="text-3xl font-bold mb-8 text-white">Gösterge Paneli</h1>
      
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6">
        <div className="bg-neutral-900 border border-neutral-800 rounded-2xl p-6">
          <p className="text-neutral-400 text-sm font-medium mb-2">Toplam Kullanıcı</p>
          <div className="text-4xl font-black text-white">{userCount}</div>
        </div>
        
        <div className="bg-neutral-900 border border-neutral-800 rounded-2xl p-6">
          <p className="text-neutral-400 text-sm font-medium mb-2">Toplam İlan</p>
          <div className="text-4xl font-black text-orange-500">{listingCount}</div>
        </div>

        <div className="bg-neutral-900 border border-neutral-800 rounded-2xl p-6">
          <p className="text-neutral-400 text-sm font-medium mb-2">Satılık Araçlar</p>
          <div className="text-3xl font-bold text-white">{saleCount}</div>
        </div>

        <div className="bg-neutral-900 border border-neutral-800 rounded-2xl p-6">
          <p className="text-neutral-400 text-sm font-medium mb-2">Kiralık & Parça</p>
          <div className="text-3xl font-bold text-white">{rentCount + partCount}</div>
        </div>
      </div>
    </div>
  );
}
