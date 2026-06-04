"use client";

import { useRouter } from "next/navigation";
import { useState } from "react";

interface UserActionsProps {
  userId: string;
  currentRole: string;
}

export default function UserActions({ userId, currentRole }: UserActionsProps) {
  const router = useRouter();
  const [loading, setLoading] = useState(false);

  const toggleRole = async () => {
    try {
      setLoading(true);
      const newRole = currentRole === 'ADMIN' ? 'USER' : 'ADMIN';
      const res = await fetch(`/api/admin/users/${userId}`, {
        method: 'PUT',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ role: newRole })
      });

      if (!res.ok) {
        const errorMsg = await res.text();
        throw new Error(errorMsg || "Rol değiştirilemedi.");
      }
      
      router.refresh();
    } catch (error: any) {
      alert("Hata oluştu: " + error.message);
    } finally {
      setLoading(false);
    }
  };

  const deleteUser = async () => {
    if (!confirm("Bu kullanıcıyı tamamen silmek istediğinize emin misiniz? (Bu işlem geri alınamaz ve kullanıcının ilanlarını/taleplerini silebilir)")) return;
    
    try {
      setLoading(true);
      const res = await fetch(`/api/admin/users/${userId}`, {
        method: 'DELETE'
      });

      if (!res.ok) {
        const errorMsg = await res.text();
        throw new Error(errorMsg || "Kullanıcı silinemedi.");
      }
      
      router.refresh();
    } catch (error: any) {
      alert("Hata oluştu: " + error.message);
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="flex justify-end gap-2 text-right">
      <button 
        onClick={toggleRole}
        disabled={loading}
        className="text-sm font-semibold text-orange-500 hover:text-orange-400 disabled:opacity-50 transition-colors"
      >
        {currentRole === 'ADMIN' ? 'Yetkiyi Al' : 'Admin Yap'}
      </button>
      <span className="text-neutral-600 mx-2">|</span>
      <button 
        onClick={deleteUser}
        disabled={loading}
        className="text-sm font-semibold text-red-500 hover:text-red-400 disabled:opacity-50 transition-colors"
      >
        Sil
      </button>
    </div>
  );
}
