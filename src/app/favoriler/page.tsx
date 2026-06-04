import { prisma } from "@/lib/prisma";
import { getServerSession } from "next-auth/next";
import { authOptions } from "@/app/api/auth/[...nextauth]/route";
import VehicleShowcase from "@/components/VehicleShowcase";
import { redirect } from "next/navigation";
import Navbar from "@/components/Navbar";

export default async function FavorilerPage() {
    const session = await getServerSession(authOptions);
    if (!session || !session.user) redirect("/login?redirect=/favoriler");

    const favorites = await prisma.favorite.findMany({
        where: { userId: (session.user as any).id },
        include: { listing: true },
        orderBy: { createdAt: 'desc' }
    });

    const vehicles = favorites.map((f: any) => ({
        ...f.listing,
        type: f.listing.category === 'SALE' ? 'SALE' : 'RENTAL'
    }));

    return (
        <div className="min-h-screen bg-neutral-950 text-white font-sans">
            <Navbar />
            <div className="px-6 py-24 max-w-7xl mx-auto">
                <h1 className="text-4xl text-white font-black mb-12 tracking-tight">Favorilerim</h1>
                {vehicles.length === 0 ? (
                    <div className="bg-neutral-900 border border-neutral-800 p-12 rounded-3xl text-center">
                        <p className="text-neutral-400 text-lg">Henüz favorilere eklenmiş bir ilanınız yok.</p>
                        <a href="/araclar" className="inline-block mt-6 px-8 py-3 bg-neutral-800 hover:bg-neutral-700 rounded-lg text-white font-medium transition-colors">Araçları İncele</a>
                    </div>
                ) : (
                    <VehicleShowcase initialVehicles={vehicles as any[]} />
                )}
            </div>
        </div>
    );
}
