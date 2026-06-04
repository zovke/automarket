import { prisma } from "@/lib/prisma";
import Image from "next/image";
import Link from "next/link";
import { notFound } from "next/navigation";
import FavoriteButton from "@/components/FavoriteButton";
import Navbar from "@/components/Navbar";
import RequestButton from "@/components/RequestButton";

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
    type: listing.category === 'SALE' ? 'SALE' : listing.category === 'RENT' ? 'RENTAL' : 'PART'
  } as any;

  if (!vehicle) {
    notFound();
  }

  const brandInfo = brandsData.find(b => b.name === vehicle.brand);
  const formattedPrice = new Intl.NumberFormat('tr-TR').format(vehicle.price);
  const formattedKm = new Intl.NumberFormat('tr-TR').format(vehicle.kilometers);

  let finalImageSrc = brandInfo?.logo || "/next.svg";
  
  if (vehicle.images) {
    try {
      const parsedImages = JSON.parse(vehicle.images);
      if (Array.isArray(parsedImages) && parsedImages.length > 0) {
        finalImageSrc = parsedImages[0];
      } else {
        finalImageSrc = vehicle.images;
      }
    } catch {
      finalImageSrc = vehicle.images;
    }
  } else {
    const imageFilename = `${vehicle.brand}_${(vehicle.model || '').replace(/\s+/g, '_')}.jpg`;
    const fs = require('fs');
    const path = require('path');
    const imagePath = path.join(process.cwd(), 'public', 'cars', imageFilename);
    if (fs.existsSync(imagePath)) {
      finalImageSrc = `/cars/${imageFilename}`;
    }
  }

  // Validate the finalImageSrc path to prevent next/image runtime errors
  if (finalImageSrc && !(finalImageSrc.startsWith("/") || finalImageSrc.startsWith("http://") || finalImageSrc.startsWith("https://"))) {
    finalImageSrc = brandInfo?.logo || "/next.svg";
  }

  return (
    <div className="min-h-screen bg-neutral-950 text-neutral-50 font-sans selection:bg-orange-500/30">
      {/* Navbar Section */}
      <Navbar />

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
            {/* Image Gallery */}
            <div className="relative aspect-[16/9] w-full rounded-3xl overflow-hidden bg-neutral-900 border border-neutral-800 shadow-2xl">
              {!vehicle.isAvailable && (
                <div className="absolute top-6 -right-12 z-20 bg-red-600 text-white font-black py-2 px-14 transform rotate-45 shadow-2xl border-y-2 border-red-500/50">
                  SATILDI
                </div>
              )}
              
              <div className={`absolute inset-0 bg-neutral-800 flex items-center justify-center ${!vehicle.isAvailable ? 'grayscale opacity-70' : ''}`}>
                <Image 
                  src={finalImageSrc}
                  alt={`${vehicle.brand} ${vehicle.model}`}
                  fill
                  className={finalImageSrc !== (brandInfo?.logo || "/next.svg") ? "object-cover" : "object-contain p-12 opacity-50"}
                  unoptimized
                />
                
                {/* Fallback solid gradient overlay */}
                <div className="absolute inset-x-0 bottom-0 h-1/3 bg-gradient-to-t from-neutral-900/90 to-transparent"></div>
              </div>
              
              <div className="absolute top-6 left-6 flex gap-3">
                {vehicle.type !== 'PART' && (
                  <div className="bg-black/60 backdrop-blur-md px-4 py-2 rounded-full text-sm font-bold text-white border border-white/10 shadow-lg">
                    {vehicle.year} Model
                  </div>
                )}
                <div className="bg-orange-600 px-4 py-2 rounded-full text-sm font-bold text-white shadow-[0_4px_12px_rgba(234,88,12,0.4)]">
                  {vehicle.type === 'SALE' ? 'Satılık' : vehicle.type === 'RENTAL' ? 'Kiralık' : 'Orijinal Yedek Parça'}
                </div>
              </div>
            </div>

            {/* Description */}
            <div className="bg-neutral-900/50 border border-neutral-800/80 rounded-3xl p-8 backdrop-blur-sm">
              <h2 className="text-2xl font-bold mb-6 text-white">{vehicle.type === 'PART' ? 'Yedek Parça Açıklaması' : 'Araç Açıklaması'}</h2>
              <p className="text-neutral-300 leading-relaxed text-lg font-light">
                {vehicle.description}
              </p>
              
              {vehicle.type !== 'PART' && (
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
              )}
            </div>
          </div>

          {/* Right Column: Pricing & Specs Box */}
          <div className="space-y-6 animate-fade-in-up delay-200 sticky top-[120px] h-fit">
            <div className="bg-neutral-900/80 border border-neutral-800 rounded-3xl p-8 shadow-2xl backdrop-blur-sm relative overflow-hidden">
               {/* Ambient glow in pricing card */}
               <div className="absolute -top-24 -right-24 w-48 h-48 bg-orange-600/20 blur-[60px] rounded-full pointer-events-none" />

              <div className="mb-8">
                <p className="text-neutral-400 text-sm font-medium mb-1 uppercase tracking-wider">{vehicle.type === 'PART' ? 'Fiyat' : 'İlan Fiyatı'}</p>
                <div className="text-4xl font-black text-white tracking-tight">
                  <span className="text-orange-500">{formattedPrice}</span> {vehicle.type === 'SALE' || vehicle.type === 'PART' ? '₺' : '₺/gün'}
                </div>
              </div>

              <div className="space-y-4 mb-8">
                <div className="flex justify-between items-center py-3 border-b border-neutral-800/60">
                  <span className="text-neutral-400 flex items-center gap-2">Marka</span>
                  <span className="font-bold text-white flex items-center gap-2">
                    {brandInfo?.name || vehicle.brand}
                  </span>
                </div>
                
                {vehicle.type !== 'PART' && (
                  <>
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
                  </>
                )}
              </div>

              {/* Actions */}
              <div className="space-y-3">
                {vehicle.isAvailable ? (
                  <RequestButton vehicleId={vehicle.id} />
                ) : (
                  <button disabled className="w-full py-4 rounded-2xl font-bold bg-neutral-800 text-neutral-500 cursor-not-allowed">
                    Bu İlan Satılmıştır
                  </button>
                )}
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
