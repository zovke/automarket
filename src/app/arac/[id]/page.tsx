import { prisma } from "@/lib/prisma";
import Image from "next/image";
import Link from "next/link";
import { notFound } from "next/navigation";
import FavoriteButton from "@/components/FavoriteButton";

// The static list of brands directly matching our vehicles models JSON
const brandsData = [
  { name: "Renault", logo: "/brands/renault.png" },
  { name: "Volkswagen", logo: "/brands/volkswagen.png" },
  { name: "Audi", logo: "/brands/audi.png" },
  { name: "Peugeot", logo: "/brands/peugeot.png" },
  { name: "Fiat", logo: "/brands/fiat.png" },
  { name: "Hyundai", logo: "/brands/hyundai.png" },
  { name: "Ford", logo: "/brands/ford.png" }
];

export default async function VehicleDetailPage({ params }: { params: Promise<{ id: string }> }) {
  const { id } = await params;
  
  const listing = await prisma.listing.findUnique({
    where: { id }
  });
  
  if (!listing) notFound();
  
  const vehicle = {
    ...listing,
    type: listing.category === 'SALE' ? 'SALE' : 'RENTAL'
  } as any;

  if (!vehicle) {
    notFound();
  }

  const brandInfo = brandsData.find(b => b.name === vehicle.brand);
  const formattedPrice = new Intl.NumberFormat('tr-TR').format(vehicle.price);
  const formattedKm = new Intl.NumberFormat('tr-TR').format(vehicle.kilometers);

  return (
    <div className="min-h-screen bg-neutral-950 text-neutral-50 font-sans selection:bg-orange-500/30">
      {/* Navbar Section */}
      <nav className="flex items-center justify-between p-6 max-w-7xl mx-auto border-b border-neutral-900/50">
        <Link href="/" className="font-black text-2xl tracking-tighter cursor-pointer flex items-center gap-1">
          <div className="w-8 h-8 bg-gradient-to-br from-orange-500 to-orange-700 rounded-lg flex items-center justify-center shadow-lg shadow-orange-500/20">
            <span className="text-white text-lg">A</span>
          </div>
          AUTO<span className="text-orange-500">MARKET</span>
        </Link>
        <div className="hidden md:flex gap-8 font-medium text-sm text-neutral-300">
          <Link href="/" className="hover:text-orange-400 transition-colors">Ana Sayfa</Link>
          <a href="#" className="text-white hover:text-orange-400 transition-colors">Araçlar</a>
          <a href="#" className="hover:text-orange-400 transition-colors">Yedek Parça</a>
          <a href="#" className="hover:text-orange-400 transition-colors">Hakkımızda</a>
        </div>
        <div className="flex gap-4 items-center">
          <a href="#" className="text-sm font-medium hover:text-white transition-colors text-neutral-300 hidden sm:block">Üye Ol</a>
          <button className="bg-white text-black hover:bg-neutral-200 px-6 py-2 rounded-full font-bold text-sm transition-transform hover:scale-105 active:scale-95 shadow-[0_0_20px_rgba(255,255,255,0.1)]">
            Giriş Yap
          </button>
        </div>
      </nav>

      {/* Main Content */}
      <main className="max-w-7xl mx-auto px-6 py-12 animate-fade-in">
        {/* Breadcrumb */}
        <div className="flex items-center gap-2 text-sm text-neutral-400 mb-8">
          <Link href="/" className="hover:text-white transition-colors">Ana Sayfa</Link>
          <span>/</span>
          <Link href="/" className="hover:text-white transition-colors">İlanlar</Link>
          <span>/</span>
          <span className="text-orange-500 font-medium">{vehicle.brand} {vehicle.model}</span>
        </div>

        <div className="grid grid-cols-1 lg:grid-cols-3 gap-12">
          {/* Left Column: Image & Details */}
          <div className="lg:col-span-2 space-y-8 animate-fade-in-up delay-100">
            {/* Image Gallery (Placeholder for one large image) */}
            <div className="relative aspect-[16/9] w-full rounded-3xl overflow-hidden bg-neutral-900 border border-neutral-800 shadow-2xl">
              <div className="absolute inset-0 bg-neutral-800 flex items-center justify-center">
                <Image 
                  src={`/cars/${vehicle.brand}_${vehicle.model.replace(/\s+/g, '_')}.jpg`}
                  alt={`${vehicle.brand} ${vehicle.model}`}
                  fill
                  className="object-cover"
                  unoptimized
                />
                
                {/* Fallback solid gradient overlay */}
                <div className="absolute inset-x-0 bottom-0 h-1/3 bg-gradient-to-t from-neutral-900/90 to-transparent"></div>
              </div>
              
              <div className="absolute top-6 left-6 flex gap-3">
                <div className="bg-black/60 backdrop-blur-md px-4 py-2 rounded-full text-sm font-bold text-white border border-white/10 shadow-lg">
                  {vehicle.year} Model
                </div>
                <div className="bg-orange-600 px-4 py-2 rounded-full text-sm font-bold text-white shadow-[0_4px_12px_rgba(234,88,12,0.4)]">
                  {vehicle.type === 'SALE' ? 'Satılık' : 'Kiralık'}
                </div>
              </div>
            </div>

            {/* Description */}
            <div className="bg-neutral-900/50 border border-neutral-800/80 rounded-3xl p-8 backdrop-blur-sm">
              <h2 className="text-2xl font-bold mb-6 text-white">Araç Açıklaması</h2>
              <p className="text-neutral-300 leading-relaxed text-lg font-light">
                {vehicle.description}
              </p>
              
              <div className="mt-8 pt-8 border-t border-neutral-800/80">
                <h3 className="text-lg font-bold mb-4 text-white">Donanım ve Özellikler</h3>
                <div className="grid grid-cols-2 md:grid-cols-3 gap-y-4 gap-x-8">
                  {['Yol Bilgisayarı', 'Klima', 'Hız Sabitleyici', 'Bluetooth', 'Park Sensörü', 'Alaşımlı Jant'].map((feature, i) => (
                    <div key={i} className="flex items-center gap-2 text-neutral-400">
                      <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" className="text-orange-500" strokeWidth="3" strokeLinecap="round" strokeLinejoin="round"><polyline points="20 6 9 17 4 12"></polyline></svg>
                      {feature}
                    </div>
                  ))}
                </div>
              </div>
            </div>
          </div>

          {/* Right Column: Pricing & Specs Box */}
          <div className="space-y-6 animate-fade-in-up delay-200">
            <div className="bg-neutral-900/80 border border-neutral-800 rounded-3xl p-8 sticky top-8 shadow-2xl backdrop-blur-sm relative overflow-hidden">
               {/* Ambient glow in pricing card */}
               <div className="absolute -top-24 -right-24 w-48 h-48 bg-orange-600/20 blur-[60px] rounded-full pointer-events-none" />

              <div className="mb-8">
                <p className="text-neutral-400 text-sm font-medium mb-1 uppercase tracking-wider">İlan Fiyatı</p>
                <div className="text-4xl font-black text-white tracking-tight">
                  <span className="text-orange-500">{formattedPrice}</span> {vehicle.type === 'SALE' ? '₺' : '₺/gün'}
                </div>
              </div>

              <div className="space-y-4 mb-8">
                <div className="flex justify-between items-center py-3 border-b border-neutral-800/60">
                  <span className="text-neutral-400 flex items-center gap-2">Marka</span>
                  <span className="font-bold text-white flex items-center gap-2">
                    {brandInfo?.name || vehicle.brand}
                  </span>
                </div>
                <div className="flex justify-between items-center py-3 border-b border-neutral-800/60">
                  <span className="text-neutral-400 flex items-center gap-2">Seri / Model</span>
                  <span className="font-bold text-white">{vehicle.model}</span>
                </div>
                <div className="flex justify-between items-center py-3 border-b border-neutral-800/60">
                  <span className="text-neutral-400 flex items-center gap-2">Kilometre</span>
                  <span className="font-bold text-white">{formattedKm} km</span>
                </div>
                <div className="flex justify-between items-center py-3 border-b border-neutral-800/60">
                  <span className="text-neutral-400 flex items-center gap-2">Vites Tipi</span>
                  <span className="font-bold text-white">{vehicle.transmission === 'Automatic' ? 'Otomatik' : 'Manuel'}</span>
                </div>
                <div className="flex justify-between items-center py-3 border-b border-neutral-800/60">
                  <span className="text-neutral-400 flex items-center gap-2">Yakıt Tipi</span>
                  <span className="font-bold text-white">{vehicle.fuel === 'Petrol' ? 'Benzin' : vehicle.fuel === 'Diesel' ? 'Dizel' : vehicle.fuel}</span>
                </div>
              </div>

              {/* Actions */}
              <div className="space-y-3">
                <button className="w-full bg-orange-600 text-white py-4 rounded-xl font-bold text-lg hover:bg-orange-500 transition-all shadow-[0_0_20px_rgba(234,88,12,0.2)] hover:shadow-[0_0_30px_rgba(234,88,12,0.4)] flex justify-center items-center gap-2">
                  Satıcıyla İletişime Geç
                </button>
                <FavoriteButton vehicleId={vehicle.id} />
              </div>
            </div>
            
            {/* Safety Note */}
            <div className="bg-neutral-900/30 border border-neutral-800/50 rounded-2xl p-6 flex items-start gap-4">
               <div className="text-orange-500 shrink-0">
                  <svg width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round"><path d="M12 22s8-4 8-10V5l-8-3-8 3v7c0 6 8 10 8 10z"></path></svg>
               </div>
               <p className="text-xs text-neutral-400 leading-relaxed">
                 Güvenliğiniz için aracı ve belgeleri görmeden asla kapora veya herhangi bir ön ödeme yapmayın.
               </p>
            </div>
          </div>
        </div>
      </main>
    </div>
  );
}
