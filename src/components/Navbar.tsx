import Link from "next/link";
import { getServerSession } from "next-auth/next";
import { authOptions } from "@/app/api/auth/[...nextauth]/route";
import UserMenu from "@/components/UserMenu";

export default async function Navbar() {
  const session = await getServerSession(authOptions);

  return (
    <nav className="sticky top-0 z-50 bg-neutral-950/80 backdrop-blur-md border-b border-neutral-900/50">
      <div className="flex items-center justify-between p-6 max-w-7xl mx-auto">
        <Link href="/" className="font-black text-2xl tracking-tighter cursor-pointer flex items-center gap-1">
          <div className="w-8 h-8 bg-gradient-to-br from-orange-500 to-orange-700 rounded-lg flex items-center justify-center shadow-lg shadow-orange-500/20">
            <span className="text-white text-lg">A</span>
          </div>
          AUTO<span className="text-orange-500">MARKET</span>
        </Link>
        <div className="hidden md:flex gap-8 font-medium text-sm text-neutral-300 items-center">
          <Link href="/" className="hover:text-orange-400 transition-colors">Ana Sayfa</Link>
          <Link href="/araclar" className="hover:text-orange-400 transition-colors">Araçlar</Link>
          <Link href="/yedek-parca" className="hover:text-orange-400 transition-colors">Yedek Parça</Link>
          <Link href="/hakkimizda" className="hover:text-orange-400 transition-colors">Hakkımızda</Link>
          {session && session.user && (
              <Link href="/favoriler" className="text-orange-500 font-bold hover:text-orange-400 transition-colors flex items-center gap-1">
                 Favorilerim
              </Link>
          )}
        </div>
        
        <div className="flex gap-4 items-center">
          {session && session.user ? (
             <UserMenu user={session.user} />
          ) : (
            <>
              <Link href="/register" className="text-sm font-medium hover:text-white transition-colors text-neutral-300 hidden sm:block">Üye Ol</Link>
              <Link href="/login" className="bg-white text-black hover:bg-neutral-200 px-6 py-2 rounded-full font-bold text-sm transition-transform hover:scale-105 active:scale-95 shadow-[0_0_20px_rgba(255,255,255,0.1)] inline-block">
                Giriş Yap
              </Link>
            </>
          )}
        </div>
      </div>
    </nav>
  );
}
