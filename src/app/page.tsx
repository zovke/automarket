import { prisma } from "@/lib/prisma";
import VehicleShowcase from "@/components/VehicleShowcase";
import Link from "next/link";
import Navbar from "@/components/Navbar";

export default async function Home() {
  const listings = await prisma.listing.findMany({
    where: { category: { in: ['SALE', 'RENT'] } },
    take: 50,
    orderBy: { createdAt: 'desc' },
  });
  
  const vehicles = listings.map((l: any) => ({
    ...l,
    type: l.category === 'SALE' ? 'SALE' : 'RENTAL'
  })) as any[];

  return (
    <div className="min-h-screen bg-neutral-950 text-neutral-50 font-sans selection:bg-orange-500/30">
      {/* Navbar Section */}
      <Navbar />

      {/* Hero Section */}
      <header className="relative mt-8 max-w-7xl mx-auto px-6 mb-24 flex flex-col items-center text-center animate-fade-in-up delay-100">
        {/* Ambient background glow */}
        <div className="absolute top-1/2 left-1/2 -translate-x-1/2 -translate-y-1/2 w-[600px] h-[400px] bg-orange-600/10 blur-[120px] rounded-full pointer-events-none -z-10" />
        
        <div className="inline-flex items-center gap-2 px-3 py-1 rounded-full bg-neutral-900/50 border border-neutral-800 text-xs font-medium text-orange-500 mb-8 backdrop-blur-sm animate-fade-in delay-200">
          <span className="w-2 h-2 rounded-full bg-orange-500 animate-pulse"></span>
          Yeni Sezon Araçlar Eklendi
        </div>
        
        <h1 className="text-5xl md:text-7xl font-extrabold tracking-tight mb-8 mt-4 bg-gradient-to-b from-white via-neutral-200 to-neutral-500 text-transparent bg-clip-text drop-shadow-sm max-w-4xl leading-tight">
          Hayalinizdeki Aracı <br className="hidden md:block"/> Saniyeler İçinde Keşfedin
        </h1>
        <p className="max-w-2xl text-neutral-400 text-lg md:text-xl mb-12 font-light">
          Türkiye'nin en seçkin otomobil ilan platformu. Yüzlerce premium <b className="text-neutral-200 font-medium">satılık</b> ve <b className="text-neutral-200 font-medium">kiralık</b> araç saniyeler uzağınızda.
        </p>
        <div className="flex flex-col sm:flex-row gap-4 w-full sm:w-auto">
          <Link href="/araclar" className="bg-orange-600 text-white px-10 py-4 lg:py-4 rounded-full font-bold text-lg hover:bg-orange-500 transition-all hover:shadow-[0_0_30px_rgba(234,88,12,0.3)] hover:-translate-y-0.5 active:translate-y-0 flex items-center justify-center gap-2">
            İlanları Gör
            <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2.5" strokeLinecap="round" strokeLinejoin="round"><path d="M5 12h14"/><path d="m12 5 7 7-7 7"/></svg>
          </Link>
          <Link href="/aracini-sat" className="bg-neutral-900 border border-neutral-700 hover:border-neutral-500 text-white px-10 py-4 lg:py-4 rounded-full font-bold text-lg transition-all hover:bg-neutral-800 flex items-center justify-center gap-2">
            Aracını Sat
          </Link>
        </div>
      </header>

      {/* Interactive React Component for Brands & Vehicles */}
      <VehicleShowcase initialVehicles={vehicles} />
    </div>
  );
}
