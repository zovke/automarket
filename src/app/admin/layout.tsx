import { getServerSession } from "next-auth/next";
import { authOptions } from "@/app/api/auth/[...nextauth]/route";
import { redirect } from "next/navigation";
import Link from "next/link";

export default async function AdminLayout({ children }: { children: React.ReactNode }) {
  const session = await getServerSession(authOptions);
  
  if (!session || (session.user as any).role !== "ADMIN") {
    redirect("/");
  }

  return (
    <div className="flex min-h-screen bg-neutral-950 text-white">
      <aside className="w-64 bg-neutral-900 border-r border-neutral-800 flex flex-col">
        <div className="p-6 border-b border-neutral-800">
          <h2 className="text-xl font-bold bg-gradient-to-br from-orange-500 to-orange-700 bg-clip-text text-transparent">
            Admin Panel
          </h2>
        </div>
        <nav className="flex-1 p-4 space-y-2">
          <Link href="/admin" className="block px-4 py-3 rounded-xl text-neutral-400 hover:text-white hover:bg-neutral-800 transition-colors">
            Gösterge Paneli
          </Link>
          <Link href="/admin/users" className="block px-4 py-3 rounded-xl text-neutral-400 hover:text-white hover:bg-neutral-800 transition-colors">
            Kullanıcı Yönetimi
          </Link>
          <Link href="/admin/listings" className="block px-4 py-3 rounded-xl text-neutral-400 hover:text-white hover:bg-neutral-800 transition-colors">
            İlan Yönetimi
          </Link>
        </nav>
        <div className="p-4 border-t border-neutral-800">
           <Link href="/" className="block px-4 py-3 rounded-xl text-neutral-400 hover:text-red-400 hover:bg-red-500/10 transition-colors">
             Siteye Dön
           </Link>
        </div>
      </aside>
      <main className="flex-1 p-8 bg-neutral-950 overflow-auto">
        <div className="max-w-6xl mx-auto">
           {children}
        </div>
      </main>
    </div>
  );
}
