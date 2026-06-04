import Link from "next/link";
import Navbar from "@/components/Navbar";

export default function Hakkimizda() {
    return (
        <div className="min-h-screen bg-neutral-950 text-white font-sans overflow-x-hidden">
            <Navbar />

            <div className="px-6 py-32 text-center flex flex-col items-center">
                <h1 className="text-6xl font-black text-transparent bg-clip-text bg-gradient-to-r from-orange-500 to-amber-300 mb-8 max-w-4xl tracking-tight">
                    Türkiye'nin Yeni Nesil İlan Platformu
                </h1>
                <p className="max-w-2xl mx-auto text-xl text-neutral-400 font-light leading-relaxed mb-12">
                    Auto Marketplace olarak, otomotiv dünyasının nabzını tutuyor, en seçkin araçları saniyeler içinde sizinle buluşturuyoruz. Güvenilir, hızlı ve dinamik altyapımızla hem alıcıların hem satıcıların ortak buluşma noktasıyız.
                </p>
                <Link href="/register" className="px-10 py-5 bg-orange-600 hover:bg-orange-500 text-white font-bold rounded-2xl shadow-2xl transition-transform hover:-translate-y-1">
                    Bize Katılın
                </Link>
            </div>
        </div>
    )
}
