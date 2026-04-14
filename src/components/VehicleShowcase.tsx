"use client";

import { Vehicle } from "@prisma/client";
import Image from "next/image";
import Link from "next/link";
import { useState } from "react";
import HeartButton from "@/components/HeartButton";

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

interface Props {
  initialVehicles: Vehicle[];
}

export default function VehicleShowcase({ initialVehicles }: Props) {
  const [selectedBrand, setSelectedBrand] = useState<string | null>(null);
  const [selectedFuel, setSelectedFuel] = useState<string | null>(null);
  const [selectedTransmission, setSelectedTransmission] = useState<string | null>(null);
  const [selectedType, setSelectedType] = useState<string | null>(null);

  // Filter vehicles by selected states
  const displayedVehicles = initialVehicles.filter(v => {
    if (selectedBrand && v.brand !== selectedBrand) return false;
    if (selectedFuel && v.fuel !== selectedFuel) return false;
    if (selectedTransmission && v.transmission !== selectedTransmission) return false;
    if (selectedType && v.type !== selectedType) return false;
    return true;
  });

  return (
    <div className="max-w-7xl mx-auto px-6 pb-24">
      {/* Brands Section */}
      <div className="mb-16">
        <h2 className="text-3xl font-bold tracking-tight mb-8 text-center text-white">Markaya Göre Keşfet</h2>
        <div className="flex flex-wrap justify-center gap-6">
          <button 
            onClick={() => setSelectedBrand(null)}
            className={`flex flex-col items-center gap-3 p-4 rounded-2xl border transition-all duration-300 w-28 hover:transform hover:-translate-y-1 ${
              selectedBrand === null 
                ? "bg-orange-600/10 border-orange-500 shadow-[0_0_20px_rgba(234,88,12,0.15)] ring-1 ring-orange-500" 
                : "bg-neutral-900 border-neutral-800 hover:border-neutral-600"
            }`}
          >
            <div className="w-12 h-12 flex items-center justify-center rounded-full bg-neutral-800 text-neutral-300 font-bold text-lg border border-neutral-700">Tümü</div>
            <span className={`text-sm font-medium ${selectedBrand === null ? "text-orange-500" : "text-neutral-400"}`}>Hepsi</span>
          </button>

          {brandsData.map((brand) => (
            <button
              key={brand.name}
              onClick={() => setSelectedBrand(brand.name)}
              className={`flex flex-col items-center gap-3 p-4 rounded-2xl border transition-all duration-300 w-28 hover:transform hover:-translate-y-1 ${
                selectedBrand === brand.name 
                  ? "bg-orange-600/10 border-orange-500 shadow-[0_0_20px_rgba(234,88,12,0.15)] ring-1 ring-orange-500" 
                  : "bg-neutral-900 border-neutral-800 hover:border-neutral-600"
              }`}
            >
              <div className="w-12 h-12 relative overflow-hidden rounded-full border border-neutral-700 bg-neutral-950 p-2 flex items-center justify-center">
                <Image src={brand.logo} alt={brand.name} fill className="object-contain p-1.5" />
              </div>
              <span className={`text-sm font-medium ${selectedBrand === brand.name ? "text-orange-500" : "text-neutral-400"}`}>{brand.name}</span>
            </button>
          ))}
        </div>
      </div>

      {/* Advanced Filters Section */}
      <div className="mb-12 flex flex-wrap gap-4 items-center justify-center p-6 bg-neutral-900/50 rounded-2xl border border-neutral-800 backdrop-blur-sm">
        <div className="flex flex-col gap-2">
          <label className="text-xs font-medium text-neutral-400 uppercase tracking-wider">Durum</label>
          <div className="flex bg-neutral-950 rounded-lg p-1 border border-neutral-800">
            <button onClick={() => setSelectedType(null)} className={`px-4 py-2 text-sm font-medium rounded-md transition-colors ${!selectedType ? 'bg-orange-600 text-white' : 'text-neutral-400 hover:text-white'}`}>Tümü</button>
            <button onClick={() => setSelectedType('SALE')} className={`px-4 py-2 text-sm font-medium rounded-md transition-colors ${selectedType === 'SALE' ? 'bg-orange-600 text-white' : 'text-neutral-400 hover:text-white'}`}>Satılık</button>
            <button onClick={() => setSelectedType('RENTAL')} className={`px-4 py-2 text-sm font-medium rounded-md transition-colors ${selectedType === 'RENTAL' ? 'bg-orange-600 text-white' : 'text-neutral-400 hover:text-white'}`}>Kiralık</button>
          </div>
        </div>

        <div className="flex flex-col gap-2">
          <label className="text-xs font-medium text-neutral-400 uppercase tracking-wider">Vites Tipi</label>
          <div className="flex bg-neutral-950 rounded-lg p-1 border border-neutral-800">
            <button onClick={() => setSelectedTransmission(null)} className={`px-4 py-2 text-sm font-medium rounded-md transition-colors ${!selectedTransmission ? 'bg-neutral-800 text-white' : 'text-neutral-400 hover:text-white'}`}>Tümü</button>
            <button onClick={() => setSelectedTransmission('Automatic')} className={`px-4 py-2 text-sm font-medium rounded-md transition-colors ${selectedTransmission === 'Automatic' ? 'bg-neutral-800 text-white' : 'text-neutral-400 hover:text-white'}`}>Otomatik</button>
            <button onClick={() => setSelectedTransmission('Manual')} className={`px-4 py-2 text-sm font-medium rounded-md transition-colors ${selectedTransmission === 'Manual' ? 'bg-neutral-800 text-white' : 'text-neutral-400 hover:text-white'}`}>Manuel</button>
          </div>
        </div>

        <div className="flex flex-col gap-2">
          <label className="text-xs font-medium text-neutral-400 uppercase tracking-wider">Yakıt Tipi</label>
          <div className="flex bg-neutral-950 rounded-lg p-1 border border-neutral-800">
            <button onClick={() => setSelectedFuel(null)} className={`px-4 py-2 text-sm font-medium rounded-md transition-colors ${!selectedFuel ? 'bg-neutral-800 text-white' : 'text-neutral-400 hover:text-white'}`}>Tümü</button>
            <button onClick={() => setSelectedFuel('Petrol')} className={`px-4 py-2 text-sm font-medium rounded-md transition-colors ${selectedFuel === 'Petrol' ? 'bg-neutral-800 text-white' : 'text-neutral-400 hover:text-white'}`}>Benzin</button>
            <button onClick={() => setSelectedFuel('Diesel')} className={`px-4 py-2 text-sm font-medium rounded-md transition-colors ${selectedFuel === 'Diesel' ? 'bg-neutral-800 text-white' : 'text-neutral-400 hover:text-white'}`}>Dizel</button>
            <button onClick={() => setSelectedFuel('Electric')} className={`px-4 py-2 text-sm font-medium rounded-md transition-colors ${selectedFuel === 'Electric' ? 'bg-neutral-800 text-white' : 'text-neutral-400 hover:text-white'}`}>Elektrikli</button>
            <button onClick={() => setSelectedFuel('Hybrid')} className={`px-4 py-2 text-sm font-medium rounded-md transition-colors ${selectedFuel === 'Hybrid' ? 'bg-neutral-800 text-white' : 'text-neutral-400 hover:text-white'}`}>Hibrit</button>
          </div>
        </div>
      </div>

      {/* Title */}
      <div className="flex justify-between items-end mb-8 pt-4 border-t border-neutral-800/50">
        <div>
          <h2 className="text-3xl font-bold tracking-tight text-white">{selectedBrand ? `${selectedBrand} İlanları` : "Tüm İlanlar"}</h2>
          <p className="text-neutral-500 mt-2">
            Toplam <span className="text-orange-500 font-bold">{displayedVehicles.length}</span> araç listeleniyor
          </p>
        </div>
        <button className="text-orange-500 hover:text-orange-400 font-medium hidden sm:block transition-colors">Tümünü gör &rarr;</button>
      </div>

      {/* Grid */}
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-8">
        {displayedVehicles.map((car: Vehicle, index: number) => (
          <Link 
            href={`/arac/${car.id}`} 
            key={car.id} 
            className="group relative rounded-2xl bg-neutral-900 border border-neutral-800 overflow-hidden hover:border-orange-500/50 hover:shadow-2xl transition-all duration-300 flex flex-col items-stretch transform hover:-translate-y-1 block animate-fade-in-up"
            style={{ animationDelay: `${index * 100}ms` }}
          >
            <div className="aspect-[4/3] bg-neutral-800 relative w-full overflow-hidden">
               {/* 
                 For a real effect, we're using a gradient + brand logo as a placeholder for the real car image 
               */}
              <div className="absolute inset-0 bg-neutral-800 flex items-center justify-center opacity-80 group-hover:opacity-100 transition-opacity">
                <Image 
                  src={`/cars/${car.brand}_${car.model.replace(/\s+/g, '_')}.jpg`}
                  alt={`${car.brand} ${car.model}`}
                  fill
                  className="object-cover transition-transform duration-700 group-hover:scale-110"
                  onError={(e) => {
                    // Fallback to logo if the car image fails 
                    (e.currentTarget as HTMLImageElement).src = brandsData.find(b => b.name === car.brand)?.logo || "/next.svg";
                    (e.currentTarget as HTMLImageElement).classList.remove("object-cover");
                    (e.currentTarget as HTMLImageElement).classList.add("object-contain", "p-8", "opacity-50");
                  }}
                />
                
                {/* Fallback solid gradient overlay to ensure text readability */}
                <div className="absolute inset-0 bg-gradient-to-t from-neutral-900/90 via-neutral-900/20 to-transparent"></div>
              </div>

              {/* Badges */}
              <div className="absolute top-4 left-4 flex gap-2">
                <div className="bg-black/70 backdrop-blur-md px-3 py-1.5 rounded-full text-xs font-bold text-white border border-white/10 shadow-lg">
                  {car.year}
                </div>
              </div>
              
              <HeartButton vehicleId={car.id} />
              
              <div className="absolute top-4 right-4 bg-orange-600 px-3 py-1.5 rounded-full text-xs font-bold text-white shadow-[0_4px_12px_rgba(234,88,12,0.4)]">
                {car.type === 'SALE' ? 'Satılık' : 'Kiralık'}
              </div>
            </div>
            
            <div className="p-6 flex-1 flex flex-col bg-gradient-to-b from-transparent to-neutral-950/50">
              <div className="flex justify-between items-start mb-3 gap-2">
                <h3 className="text-xl font-bold text-white leading-tight group-hover:text-orange-400 transition-colors">{car.brand} <span className="font-light">{car.model}</span></h3>
                <div className="text-xl font-black text-orange-500 whitespace-nowrap">
                  {car.type === 'SALE' 
                    ? `${new Intl.NumberFormat('tr-TR').format(car.price)} ₺` 
                    : `${new Intl.NumberFormat('tr-TR').format(car.price)} ₺/g`}
                </div>
              </div>
              <p className="text-neutral-400 text-sm mb-6 flex-1">
                {car.description}
              </p>
              
              <div className="mt-auto pt-6 border-t border-neutral-800/80 grid grid-cols-3 gap-2">
                <div className="flex flex-col items-center justify-center bg-neutral-950/80 px-2 py-2 rounded-lg border border-neutral-800/50">
                   <span className="text-[10px] text-neutral-500 uppercase tracking-wider mb-1">Kilometre</span>
                   <span className="text-xs font-bold text-neutral-200">{new Intl.NumberFormat('tr-TR').format(car.kilometers)}</span>
                </div>
                <div className="flex flex-col items-center justify-center bg-neutral-950/80 px-2 py-2 rounded-lg border border-neutral-800/50">
                   <span className="text-[10px] text-neutral-500 uppercase tracking-wider mb-1">Vites</span>
                   <span className="text-xs font-bold text-neutral-200">{car.transmission === 'Automatic' ? 'Otomatik' : 'Manuel'}</span>
                </div>
                <div className="flex flex-col items-center justify-center bg-neutral-950/80 px-2 py-2 rounded-lg border border-neutral-800/50">
                   <span className="text-[10px] text-neutral-500 uppercase tracking-wider mb-1">Yakıt</span>
                   <span className="text-xs font-bold text-neutral-200">{car.fuel === 'Petrol' ? 'Benzin' : car.fuel === 'Diesel' ? 'Dizel' : car.fuel}</span>
                </div>
              </div>
            </div>
          </Link>
        ))}

        {displayedVehicles.length === 0 && (
          <div className="col-span-full py-24 flex flex-col items-center justify-center text-center text-neutral-500 bg-neutral-900/50 border border-dashed border-neutral-800 rounded-3xl">
            <div className="w-16 h-16 bg-neutral-800 rounded-full flex items-center justify-center mb-4 text-2xl">🚗</div>
            <h3 className="text-xl font-bold text-white mb-2">Araç Bulunamadı</h3>
            <p className="max-w-md">Seçtiğiniz {selectedBrand} markasına ait ilan şu anda sistemimizde bulunmuyor.</p>
            <button 
              onClick={() => setSelectedBrand(null)}
              className="mt-6 text-orange-500 hover:text-orange-400 font-medium underline underline-offset-4"
            >
              Tüm araçlara geri dön
            </button>
          </div>
        )}
      </div>
    </div>
  );
}
