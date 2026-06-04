import { prisma } from "@/lib/prisma";
import UserActions from "./UserActions";
export default async function UsersPage() {
  const users = await prisma.user.findMany({
    orderBy: { createdAt: "desc" }
  });

  return (
    <div>
      <div className="flex justify-between items-center mb-8">
        <h1 className="text-3xl font-bold text-white">Kullanıcı Yönetimi</h1>
      </div>

      <div className="bg-neutral-900 rounded-2xl overflow-hidden border border-neutral-800">
        <table className="w-full text-left">
          <thead className="bg-neutral-800/50 text-neutral-400 text-sm">
            <tr>
              <th className="p-4 font-medium">Email</th>
              <th className="p-4 font-medium">Kayıt Tarihi</th>
              <th className="p-4 font-medium">Rol</th>
              <th className="p-4 font-medium text-right">İşlemler</th>
            </tr>
          </thead>
          <tbody>
            {users.map(u => (
              <tr key={u.id} className="border-t border-neutral-800 hover:bg-neutral-800/30 transition-colors">
                <td className="p-4">
                  <div className="font-bold text-white">{u.firstName} {u.lastName}</div>
                  <div className="text-sm text-neutral-400">{u.email}</div>
                </td>
                <td className="p-4 text-neutral-400">
                  {new Date(u.createdAt).toLocaleDateString("tr-TR")}
                </td>
                <td className="p-4">
                  <span className={`px-3 py-1 rounded-full text-xs font-bold ${
                    u.role === 'ADMIN' 
                      ? 'bg-orange-500/10 text-orange-500 border border-orange-500/20' 
                      : 'bg-neutral-800 text-neutral-300 border border-neutral-700'
                  }`}>
                    {u.role}
                  </span>
                </td>
                <td className="p-4 text-right">
                   <UserActions userId={u.id} currentRole={u.role} />
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </div>
  );
}
