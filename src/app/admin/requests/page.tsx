import { prisma } from "@/lib/prisma";
import RequestActions from "./RequestActions";

export default async function RequestsPage() {
  const requests = await prisma.request.findMany({
    orderBy: { createdAt: "desc" },
    include: {
      user: true,
      listing: true
    }
  });

  return (
    <div>
      <div className="flex justify-between items-center mb-8">
        <h1 className="text-3xl font-bold text-white">Alım/Kiralama Talepleri</h1>
      </div>

      <div className="bg-neutral-900 rounded-2xl overflow-hidden border border-neutral-800">
        <table className="w-full text-left">
          <thead className="bg-neutral-800/50 text-neutral-400 text-sm">
            <tr>
              <th className="p-4 font-medium">Kullanıcı</th>
              <th className="p-4 font-medium">Araç</th>
              <th className="p-4 font-medium">Tarih</th>
              <th className="p-4 font-medium">Durum</th>
              <th className="p-4 font-medium text-right">İşlemler</th>
            </tr>
          </thead>
          <tbody>
            {requests.map(r => (
              <tr key={r.id} className="border-t border-neutral-800 hover:bg-neutral-800/30 transition-colors">
                <td className="p-4">
                  <div className="font-bold text-white">{r.user.firstName} {r.user.lastName}</div>
                  <div className="text-sm text-neutral-400">{r.user.email}</div>
                </td>
                <td className="p-4">
                  <div className="font-bold text-orange-500">{r.listing.brand} {r.listing.model}</div>
                  <div className="text-sm text-neutral-400 mb-2">{new Intl.NumberFormat('tr-TR').format(r.listing.price)} ₺</div>
                  <a href={`/arac/${r.listing.id}`} target="_blank" className="inline-block px-3 py-1 bg-neutral-800 hover:bg-neutral-700 text-xs text-white rounded-lg transition-colors border border-neutral-700">İlana Git ↗</a>
                </td>
                <td className="p-4 text-neutral-400">
                  {new Date(r.createdAt).toLocaleDateString("tr-TR")}
                </td>
                <td className="p-4">
                  <span className={`px-3 py-1 rounded-full text-xs font-bold ${
                    r.status === 'PENDING' ? 'bg-yellow-500/10 text-yellow-500 border border-yellow-500/20' :
                    r.status === 'APPROVED' ? 'bg-green-500/10 text-green-500 border border-green-500/20' :
                    'bg-red-500/10 text-red-500 border border-red-500/20'
                  }`}>
                    {r.status === 'PENDING' ? 'Bekliyor' : r.status === 'APPROVED' ? 'Onaylandı' : 'Reddedildi'}
                  </span>
                </td>
                <td className="p-4">
                  {r.status === 'PENDING' ? (
                     <RequestActions requestId={r.id} />
                  ) : (
                     <div className="text-right text-xs text-neutral-500 font-medium">
                       İşlem Tamamlandı
                     </div>
                  )}
                </td>
              </tr>
            ))}
            {requests.length === 0 && (
              <tr>
                <td colSpan={5} className="p-8 text-center text-neutral-500">Hiç talep bulunmuyor.</td>
              </tr>
            )}
          </tbody>
        </table>
      </div>
    </div>
  );
}
