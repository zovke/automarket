import { prisma } from "@/lib/prisma";
import Link from "next/link";

export default async function ListingsPage() {
  const listings = await prisma.listing.findMany({
    orderBy: { createdAt: "desc" },
    include: { user: true }
  });

  return (
    <div>
      <div className="flex justify-between items-center mb-8">
        <h1 className="text-3xl font-bold text-white">İlan Yönetimi</h1>
        <Link href="/admin/listings/new" className="bg-orange-600 hover:bg-orange-500 text-white px-6 py-3 rounded-xl font-bold transition-all shadow-lg hover:shadow-orange-500/20 active:scale-95">
          Admin Olarak İlan Ekle
        </Link>
      </div>

      <div className="bg-neutral-900 rounded-2xl overflow-hidden border border-neutral-800">
        <table className="w-full text-left">
          <thead className="bg-neutral-800/50 text-neutral-400 text-sm">
            <tr>
              <th className="p-4 font-medium">Başlık</th>
              <th className="p-4 font-medium">Kategori</th>
              <th className="p-4 font-medium">Fiyat</th>
              <th className="p-4 font-medium">Sahibi (Email)</th>
              <th className="p-4 font-medium text-right">İşlemler</th>
            </tr>
          </thead>
          <tbody>
            {listings.length === 0 && (
              <tr>
                <td colSpan={5} className="p-8 text-center text-neutral-500">Henüz hiç ilan eklenmemiş.</td>
              </tr>
            )}
            {listings.map(l => (
              <tr key={l.id} className="border-t border-neutral-800 hover:bg-neutral-800/30 transition-colors">
                <td className="p-4">
                   <p className="font-bold text-white">{l.title}</p>
                   {l.brand && <p className="text-xs text-neutral-400 mt-1">{l.brand} {l.model}</p>}
                </td>
                <td className="p-4">
                   <span className={`px-2 py-1 rounded-sm text-xs font-bold ${
                     l.category === 'SALE' ? 'bg-green-500/20 text-green-500' :
                     l.category === 'RENT' ? 'bg-blue-500/20 text-blue-500' : 
                     'bg-purple-500/20 text-purple-500'
                   }`}>
                     {l.category === 'SALE' ? 'Satılık' : l.category === 'RENT' ? 'Kiralık' : 'Y. Parça'}
                   </span>
                </td>
                <td className="p-4 font-semibold text-orange-500">
                  {new Intl.NumberFormat('tr-TR').format(l.price)} ₺
                </td>
                <td className="p-4 text-neutral-300 text-sm">
                  {l.user?.email || 'Bilinmiyor'}
                </td>
                <td className="p-4 text-right">
                   <Link href={`/admin/listings/edit/${l.id}`} className="text-sm font-semibold text-orange-500 hover:text-orange-400 mr-2">Düzenle</Link>
                   <span className="text-neutral-600 mx-2">|</span>
                   <button className="text-sm font-semibold text-red-500 hover:text-red-400">Sil</button>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </div>
  );
}
