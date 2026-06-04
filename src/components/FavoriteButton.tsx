"use client";

import { useState, useEffect } from "react";
import { useRouter } from "next/navigation";

export default function FavoriteButton({ vehicleId }: { vehicleId: string }) {
  const [isFavorited, setIsFavorited] = useState(false);
  const [loading, setLoading] = useState(true);
  const router = useRouter();

  useEffect(() => {
    fetch("/api/favorites")
        .then(res => res.json())
        .then(data => {
            if(data.favorites) {
                const found = data.favorites.some((f: any) => f.listingId === vehicleId);
                setIsFavorited(found);
            }
            setLoading(false);
        })
        .catch(() => setLoading(false));
  }, [vehicleId]);

  const toggleFavorite = async () => {
    try {
        const res = await fetch("/api/favorites", {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify({ listingId: vehicleId })
        });
        if(res.status === 401) {
            router.push("/login?redirect=/arac/" + vehicleId);
            return;
        }
        if(res.ok) {
            const data = await res.json();
            setIsFavorited(data.isFavorite);
        }
    } catch(e) {
        console.error(e);
    }
  };

  return (
    <button 
      onClick={toggleFavorite}
      disabled={loading}
      className={`w-full py-4 rounded-xl font-bold text-lg transition-all flex justify-center items-center gap-2 border-2 ${
        isFavorited 
          ? 'bg-neutral-800 border-red-500 text-red-500 hover:bg-neutral-900 shadow-[0_0_15px_rgba(239,68,68,0.2)]' 
          : 'bg-transparent border-neutral-700 text-white hover:bg-neutral-800 hover:border-neutral-500'
      }`}
    >
      <svg width="24" height="24" viewBox="0 0 24 24" fill={isFavorited ? "currentColor" : "none"} stroke="currentColor" strokeWidth="2.5" strokeLinecap="round" strokeLinejoin="round">
        <path d="M20.84 4.61a5.5 5.5 0 0 0-7.78 0L12 5.67l-1.06-1.06a5.5 5.5 0 0 0-7.78 7.78l1.06 1.06L12 21.23l7.78-7.78 1.06-1.06a5.5 5.5 0 0 0 0-7.78z"></path>
      </svg>
      {loading ? 'Yükleniyor...' : isFavorited ? 'Favorilerden Çıkar' : 'Favorilere Ekle'}
    </button>
  );
}
