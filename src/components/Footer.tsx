"use client";

import Link from "next/link";
import { usePathname } from "next/navigation";

export default function Footer() {
  const pathname = usePathname();

  // Hide footer on dashboard/admin routes where full screen height layout is critical
  if (pathname?.startsWith("/admin")) return null;

  return (
    <footer className="bg-neutral-950 border-t border-neutral-900 pt-16 pb-8 mt-auto z-10 relative">
      <div className="max-w-7xl mx-auto px-6 grid grid-cols-1 md:grid-cols-4 gap-12">
        <div className="space-y-4">
          <Link href="/" className="font-black text-2xl tracking-tighter cursor-pointer flex items-center gap-1">
            <div className="w-8 h-8 bg-gradient-to-br from-orange-500 to-orange-700 rounded-lg flex items-center justify-center shadow-lg shadow-orange-500/20">
              <span className="text-white text-lg">A</span>
            </div>
            <span className="text-white">AUTO</span><span className="text-orange-500">MARKET</span>
          </Link>
          <p className="text-neutral-500 text-sm leading-relaxed">
            Türkiye'nin seçkin otomobil ilan platformu. Yüzlerce premium araç saniyeler uzağınızda.
          </p>
        </div>
        
        <div>
          <h4 className="text-white font-bold mb-4">Hızlı Bağlantılar</h4>
          <ul className="space-y-2 text-sm text-neutral-400">
            <li><Link href="/araclar" className="hover:text-orange-500 transition-colors">Tüm Araçlar</Link></li>
            <li><Link href="/aracini-sat" className="hover:text-orange-500 transition-colors">İlan Ver</Link></li>
            <li><Link href="/yedek-parca" className="hover:text-orange-500 transition-colors">Yedek Parça</Link></li>
          </ul>
        </div>

        <div>
          <h4 className="text-white font-bold mb-4">Kurumsal</h4>
          <ul className="space-y-2 text-sm text-neutral-400">
            <li><Link href="/hakkimizda" className="hover:text-orange-500 transition-colors">Hakkımızda</Link></li>
            <li><Link href="/iletisim" className="hover:text-orange-500 transition-colors">İletişim</Link></li>
            <li><Link href="/kullanim-kosullari" className="hover:text-orange-500 transition-colors">Kullanım Koşulları</Link></li>
          </ul>
        </div>

         <div>
           <h4 className="text-white font-bold mb-4">Bize Katılın</h4>
           <div className="flex gap-4">
             <a href="https://instagram.com" target="_blank" rel="noopener noreferrer" className="w-10 h-10 rounded-full bg-neutral-900 border border-neutral-800 flex items-center justify-center hover:bg-orange-500 hover:text-white transition-colors cursor-pointer text-neutral-400">
               <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round"><rect x="2" y="2" width="20" height="20" rx="5" ry="5"></rect><path d="M16 11.37A4 4 0 1 1 12.63 8 4 4 0 0 1 16 11.37z"></path><line x1="17.5" y1="6.5" x2="17.51" y2="6.5"></line></svg>
             </a>
             <a href="https://facebook.com" target="_blank" rel="noopener noreferrer" className="w-10 h-10 rounded-full bg-neutral-900 border border-neutral-800 flex items-center justify-center hover:bg-orange-500 hover:text-white transition-colors cursor-pointer text-neutral-400">
               <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round"><path d="M18 2h-3a5 5 0 0 0-5 5v3H7v4h3v8h4v-8h3l1-4h-4V7a1 1 0 0 1 1-1h3z"></path></svg>
             </a>
             <a href="https://x.com" target="_blank" rel="noopener noreferrer" className="w-10 h-10 rounded-full bg-neutral-900 border border-neutral-800 flex items-center justify-center hover:bg-orange-500 hover:text-white transition-colors cursor-pointer text-neutral-400">
               <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round"><path d="M4 4l11.733 16h4.267l-11.733 -16z"></path><path d="M4 20l6.768 -6.768m2.46 -2.46l6.772 -6.772"></path></svg>
             </a>
           </div>
        </div>
      </div>
      
      <div className="max-w-7xl mx-auto px-6 mt-16 pt-8 border-t border-neutral-900/50 flex flex-col md:flex-row items-center justify-between text-sm text-neutral-500">
        <p>&copy; {new Date().getFullYear()} Auto Marketplace. Tüm Hakları Saklıdır.</p>
        <div className="flex flex-wrap gap-8 mt-4 md:mt-0 font-medium">
          <Link href="/gizlilik-politikasi" className="hover:text-orange-500 transition-colors">Gizlilik Politikası</Link>
          <Link href="/cerez-politikasi" className="hover:text-orange-500 transition-colors">Çerez Politikası</Link>
        </div>
      </div>
    </footer>
  );
}
