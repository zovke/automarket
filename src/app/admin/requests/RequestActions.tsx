"use client";

import { useRouter } from "next/navigation";
import { useState } from "react";

interface RequestActionsProps {
  requestId: string;
}

export default function RequestActions({ requestId }: RequestActionsProps) {
  const router = useRouter();
  const [loading, setLoading] = useState(false);

  const handleAction = async (status: "APPROVED" | "REJECTED") => {
    try {
      setLoading(true);
      const res = await fetch(`/api/admin/requests/${requestId}`, {
        method: 'PUT',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ status })
      });

      if (!res.ok) throw new Error("İşlem başarısız.");
      
      router.refresh();
    } catch (error) {
      alert("Hata oluştu: " + error);
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="flex justify-end gap-2">
      <button 
        onClick={() => handleAction('APPROVED')}
        disabled={loading}
        className="px-4 py-2 bg-green-500/10 text-green-500 hover:bg-green-500 hover:text-white rounded-lg font-bold text-sm transition-colors border border-green-500/20 disabled:opacity-50"
      >
        Onayla
      </button>
      <button 
        onClick={() => handleAction('REJECTED')}
        disabled={loading}
        className="px-4 py-2 bg-red-500/10 text-red-500 hover:bg-red-500 hover:text-white rounded-lg font-bold text-sm transition-colors border border-red-500/20 disabled:opacity-50"
      >
        Reddet
      </button>
    </div>
  );
}
