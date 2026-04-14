"use client";

import { useFavorites } from "@/context/FavoritesContext";

export default function FavoriteButton({ vehicleId }: { vehicleId: string }) {
  const { isFavorite, toggleFavorite } = useFavorites();
  const favorite = isFavorite(vehicleId);

  return (
    <button
      onClick={() => toggleFavorite(vehicleId)}
      className={`w-full py-4 rounded-xl font-bold text-lg transition-all border flex justify-center items-center gap-2 ${
        favorite 
          ? "bg-red-500/10 text-red-500 border-red-500/50 hover:bg-red-500/20" 
          : "bg-neutral-800 text-white border-neutral-700 hover:bg-neutral-700"
      }`}
    >
      <svg 
        width="20" height="20" viewBox="0 0 24 24" 
        fill={favorite ? "currentColor" : "none"} 
        stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round"
      >
        <path d="M20.84 4.61a5.5 5.5 0 0 0-7.78 0L12 5.67l-1.06-1.06a5.5 5.5 0 0 0-7.78 7.78l1.06 1.06L12 21.23l7.78-7.78 1.06-1.06a5.5 5.5 0 0 0 0-7.78z"></path>
      </svg>
      {favorite ? "Favorilerden Çıkar" : "Favorilere Ekle"}
    </button>
  );
}
