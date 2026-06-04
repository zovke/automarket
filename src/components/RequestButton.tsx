"use client";
import { useState } from "react";
import { useRouter } from "next/navigation";

export default function RequestButton({ vehicleId }: { vehicleId: string }) {
    const [loading, setLoading] = useState(false);
    const [success, setSuccess] = useState('');
    const router = useRouter();

    const handleRequest = async () => {
        setLoading(true);
        try {
            const res = await fetch("/api/requests", {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify({ listingId: vehicleId })
            });
            if(res.status === 401) {
                router.push("/login");
            } else if (res.ok) {
                setSuccess("Satın alma talebiniz başarıyla oluşturulmuştur.");
            } else {
                alert("Bir hata oluştu.");
            }
        } catch(e) {
            alert("Sunucu hatası");
        }
        setLoading(false);
    };

    return (
        <div className="w-full">
            {success ? (
                <div className="w-full bg-green-500/20 text-green-500 py-4 rounded-xl font-bold text-center border border-green-500/50">
                    {success}
                </div>
            ) : (
                <button 
                  onClick={handleRequest} 
                  disabled={loading}
                  className="w-full bg-orange-600 text-white py-4 rounded-xl font-bold text-lg hover:bg-orange-500 transition-all shadow-[0_0_20px_rgba(234,88,12,0.2)] hover:shadow-[0_0_30px_rgba(234,88,12,0.4)] flex justify-center items-center gap-2 disabled:opacity-50"
                 >
                  {loading ? "İşleniyor..." : "Satıcıyla İletişime Geç"}
                </button>
            )}
        </div>
    );
}
