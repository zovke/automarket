"use client";

import { useFavorites } from "@/context/FavoritesContext";

export default function HeartButton({ vehicleId }: { vehicleId: string }) {
  const { isFavorite, toggleFavorite } = useFavorites();
  const favorite = isFavorite(vehicleId);

  return (
    <div
      role="button"
      onClick={(e) => {
        e.preventDefault(); // Kartın içine link tıklandığında çalışmaması için
        toggleFavorite(vehicleId);
      }}
      className={`absolute top-4 right-24 z-10 w-8 h-8 rounded-full flex items-center justify-center transition-all bg-black/60 backdrop-blur-md shadow-lg border ${
        favorite ? "text-red-500 border-red-500/50" : "text-white border-white/10 hover:bg-black/80"
      } hover:scale-110 active:scale-95 cursor-pointer`}
    >
      <svg 
        width="16" height="16" viewBox="0 0 24 24" 
        fill={favorite ? "currentColor" : "none"} 
        stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round"
      >
        <path d="M20.84 4.61a5.5 5.5 0 0 0-7.78 0L12 5.67l-1.06-1.06a5.5 5.5 0 0 0-7.78 7.78l1.06 1.06L12 21.23l7.78-7.78 1.06-1.06a5.5 5.5 0 0 0 0-7.78z"></path>
      </svg>
    </div>
  );
}
