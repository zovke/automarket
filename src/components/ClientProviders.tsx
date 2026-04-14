"use client";
import { FavoritesProvider } from "@/context/FavoritesContext";
import { SessionProvider } from "next-auth/react";

export default function ClientProviders({ children }: { children: React.ReactNode }) {
  return (
    <SessionProvider>
      <FavoritesProvider>{children}</FavoritesProvider>
    </SessionProvider>
  );
}
