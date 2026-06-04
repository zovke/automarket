"use client";

import { createContext, useContext, useEffect, useState } from "react";

interface FavoritesContextType {
  favorites: string[];
  toggleFavorite: (id: string) => void;
  isFavorite: (id: string) => boolean;
}

const FavoritesContext = createContext<FavoritesContextType | undefined>(undefined);

export function FavoritesProvider({ children }: { children: React.ReactNode }) {
  const [favorites, setFavorites] = useState<string[]>([]);

  // Load from database on mount
  useEffect(() => {
    fetch("/api/favorites")
      .then(res => {
          if(res.ok) return res.json();
      })
      .then(data => {
          if(data?.favorites) {
              setFavorites(data.favorites.map((f:any) => f.listingId));
          }
      })
      .catch(e => console.error("Failed to load favorites", e));
  }, []);

  const toggleFavorite = async (id: string) => {
    // Optimistic UI update
    setFavorites(prev => 
      prev.includes(id) 
        ? prev.filter(favId => favId !== id)
        : [...prev, id]
    );

    // Persist to DB
    try {
        await fetch("/api/favorites", {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify({ listingId: id })
        });
    } catch(e) {
        console.error("Failed to sync favorite", e);
    }
  };

  const isFavorite = (id: string) => favorites.includes(id);

  return (
    <FavoritesContext.Provider value={{ favorites, toggleFavorite, isFavorite }}>
      {children}
    </FavoritesContext.Provider>
  );
}

export function useFavorites() {
  const context = useContext(FavoritesContext);
  if (context === undefined) {
    throw new Error("useFavorites must be used within a FavoritesProvider");
  }
  return context;
}
