import { prisma } from "@/lib/prisma";
import VehicleShowcase from "@/components/VehicleShowcase";
import Link from "next/link";

import Navbar from "@/components/Navbar";

export default async function AraclarPage() {
  const listings = await prisma.listing.findMany({
    where: { category: { in: ['SALE', 'RENT'] } },
    orderBy: { createdAt: 'desc' },
  });
  
  const vehicles = listings.map((l: any) => ({
    ...l,
    type: l.category === 'SALE' ? 'SALE' : 'RENTAL'
  }));

  return (
    <div className="min-h-screen bg-neutral-950 text-white font-sans overflow-x-hidden">
      <Navbar />

      <div className="mt-8">
        <h1 className="text-4xl text-white font-black text-center mb-2 tracking-tight">Geniş Araç Portföyümüz</h1>
        <p className="text-neutral-400 text-center mb-12">Yüzlerce elit satılık ve kiralık aracı inceleyin.</p>
        <VehicleShowcase initialVehicles={vehicles as any[]} />
      </div>
    </div>
  );
}
