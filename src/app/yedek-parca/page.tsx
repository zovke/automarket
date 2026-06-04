import { prisma } from "@/lib/prisma";
import Link from "next/link";
import Navbar from "@/components/Navbar";

export default async function YedekParcaPage() {
  const parts = await prisma.listing.findMany({
    where: { category: 'PART' },
    orderBy: { createdAt: 'desc' }
  });

  return (
    <div className="min-h-screen bg-neutral-950 text-white font-sans overflow-x-hidden">
      <Navbar />

      <div className="px-6 py-24 max-w-7xl mx-auto">
        <h1 className="text-4xl font-extrabold text-center mb-12">Orijinal Yedek Parçalar</h1>
        {parts.length === 0 ? (
           <p className="text-center text-neutral-500">Şu anda hiç yedek parça ilanı bulunmuyor.</p>
        ) : (
          <div className="grid grid-cols-1 md:grid-cols-3 gap-8">
              {parts.map(p => (
                  <div key={p.id} className="bg-neutral-900 border border-neutral-800 overflow-hidden rounded-3xl hover:border-orange-500/50 transition-colors flex flex-col group p-6">
                      <div className="flex justify-between items-start mb-4">
                        <h2 className="text-xl font-bold text-white line-clamp-2 pr-4">{p.title}</h2>
                        {!p.isAvailable && (
                           <div className="bg-red-600 text-white text-xs font-black px-3 py-1 rounded-full shadow-lg shrink-0">SATILDI</div>
                        )}
                      </div>
                      <p className="text-neutral-400 mb-8 line-clamp-3 text-sm flex-1">{p.description}</p>
                      <div className="flex justify-between items-end mt-auto pt-4 border-t border-neutral-800/60">
                              <div>
                                <span className="block text-xs font-medium text-neutral-500 mb-1">Fiyat</span>
                                <span className="text-2xl font-black text-orange-500">{new Intl.NumberFormat('tr-TR').format(p.price)} ₺</span>
                              </div>
                              <Link href={`/arac/${p.id}`} className="px-5 py-2 bg-neutral-800 border border-neutral-700 group-hover:border-orange-500 group-hover:bg-orange-500 hover:text-white transition-colors text-white font-medium rounded-lg text-sm">İncele</Link>
                          </div>
                  </div>
              ))}
          </div>
        )}
      </div>
    </div>
  );
}
