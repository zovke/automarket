"use client";

import { useState, useRef } from "react";
import { useRouter } from "next/navigation";

export default function ListingForm({ initialData }: { initialData?: any }) {
  const router = useRouter();
  const [step, setStep] = useState(initialData ? 2 : 1);
  const [loading, setLoading] = useState(false);
  const [formData, setFormData] = useState({
    title: initialData?.title || "",
    description: initialData?.description || "",
    price: initialData?.price?.toString() || "",
    category: initialData?.category || "SALE",
    brand: initialData?.brand || "",
    model: initialData?.model || "",
    year: initialData?.year?.toString() || "",
    kilometers: initialData?.kilometers?.toString() || "",
    fuel: initialData?.fuel || "Petrol",
    transmission: initialData?.transmission || "Manual",
    images: initialData?.images || "",
    isAvailable: initialData?.isAvailable ?? true
  });
  const [isDragging, setIsDragging] = useState(false);
  const [uploading, setUploading] = useState(false);
  const [showManualUrl, setShowManualUrl] = useState(false);
  const fileInputRef = useRef<HTMLInputElement>(null);

  const handleChange = (e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement | HTMLSelectElement>) => {
    const { name, value, type } = e.target;
    // Handle checkbox
    if (type === "checkbox") {
      setFormData(prev => ({ ...prev, [name]: (e.target as HTMLInputElement).checked }));
    } else {
      setFormData(prev => ({ ...prev, [name]: value }));
    }
  };

  const handleFileChange = async (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    if (file) {
      await handleUpload(file);
    }
  };

  const handleDragOver = (e: React.DragEvent) => {
    e.preventDefault();
    setIsDragging(true);
  };

  const handleDragLeave = (e: React.DragEvent) => {
    e.preventDefault();
    setIsDragging(false);
  };

  const handleDrop = async (e: React.DragEvent) => {
    e.preventDefault();
    setIsDragging(false);
    const file = e.dataTransfer.files?.[0];
    if (file) {
      await handleUpload(file);
    }
  };

  const handleUpload = async (file: File) => {
    if (!file.type.startsWith("image/")) {
      alert("Lütfen geçerli bir görsel dosyası seçin (PNG, JPG, JPEG, WEBP vb.).");
      return;
    }
    setUploading(true);
    const uData = new FormData();
    uData.append("file", file);
    try {
      const res = await fetch("/api/upload", {
        method: "POST",
        body: uData,
      });
      const data = await res.json();
      if (res.ok && data.success) {
        setFormData(prev => ({ ...prev, images: data.url }));
      } else {
        alert(data.error || "Yükleme sırasında bir hata oluştu.");
      }
    } catch (err) {
      console.error("Yükleme hatası:", err);
      alert("Sunucuya bağlanırken bir hata oluştu.");
    } finally {
      setUploading(false);
    }
  };

  const handleRemoveImage = () => {
    setFormData(prev => ({ ...prev, images: "" }));
    if (fileInputRef.current) {
      fileInputRef.current.value = "";
    }
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setLoading(true);

    const url = initialData ? `/api/listings/${initialData.id}` : "/api/listings";
    const method = initialData ? "PUT" : "POST";

    try {
      const res = await fetch(url, {
        method,
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(formData)
      });
      if (res.ok) {
        router.push("/admin/listings");
        router.refresh();
      } else {
        alert("Bir hata oluştu");
      }
    } catch (err) {
      console.error(err);
    }
    setLoading(false);
  };

  if (step === 1) {
    return (
      <div className="bg-neutral-900 border border-neutral-800 p-8 rounded-2xl max-w-4xl space-y-6 flex flex-col items-center">
        <h2 className="text-3xl font-black text-white mb-6">Ne İlanı Vermek İstiyorsunuz?</h2>
        <div className="grid grid-cols-1 md:grid-cols-3 gap-6 w-full">
           <button type="button" onClick={() => { setFormData(p => ({...p, category: 'SALE'})); setStep(2); }} className="p-8 border border-neutral-700 hover:border-orange-500 hover:bg-orange-500/10 rounded-2xl transition-all flex flex-col items-center gap-4 group">
             <div className="text-4xl transform group-hover:scale-110 transition-transform">🚗</div>
             <div className="font-bold text-white text-lg">Araç Satmak</div>
           </button>
           <button type="button" onClick={() => { setFormData(p => ({...p, category: 'RENT'})); setStep(2); }} className="p-8 border border-neutral-700 hover:border-orange-500 hover:bg-orange-500/10 rounded-2xl transition-all flex flex-col items-center gap-4 group">
             <div className="text-4xl transform group-hover:scale-110 transition-transform">🔑</div>
             <div className="font-bold text-white text-lg">Araç Kiralamak</div>
           </button>
           <button type="button" onClick={() => { setFormData(p => ({...p, category: 'PART'})); setStep(2); }} className="p-8 border border-neutral-700 hover:border-orange-500 hover:bg-orange-500/10 rounded-2xl transition-all flex flex-col items-center gap-4 group">
             <div className="text-4xl transform group-hover:scale-110 transition-transform">⚙️</div>
             <div className="font-bold text-white text-lg">Yedek Parça Satmak</div>
           </button>
        </div>
      </div>
    );
  }

  return (
    <form onSubmit={handleSubmit} className="bg-neutral-900 border border-neutral-800 p-8 rounded-2xl max-w-4xl space-y-6 animate-fade-in-up">
      <div className="flex justify-between items-center mb-6 border-b border-neutral-800 pb-4">
         <h2 className="text-xl font-bold text-white">İlan Detayları ({formData.category === 'SALE' ? 'Satılık' : formData.category === 'RENT' ? 'Kiralık' : 'Yedek Parça'})</h2>
         {!initialData && (
           <button type="button" onClick={() => setStep(1)} className="text-sm font-bold text-orange-500 hover:text-orange-400 underline transition-colors">← Kategori Değiştir</button>
         )}
      </div>
      <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
        <div className="md:col-span-2">
          <label className="block text-neutral-400 mb-2">Başlık</label>
          <input required name="title" value={formData.title} onChange={handleChange} className="w-full bg-neutral-950 border border-neutral-800 rounded-lg p-3 text-white focus:border-orange-500 focus:outline-none" />
        </div>

        <div>
          <label className="block text-neutral-400 mb-2">Fiyat (₺)</label>
          <input required type="number" name="price" value={formData.price} onChange={handleChange} className="w-full bg-neutral-950 border border-neutral-800 rounded-lg p-3 text-white focus:border-orange-500 focus:outline-none" />
        </div>

        <div>
          <label className="block text-neutral-400 mb-2">Marka</label>
          <input name="brand" value={formData.brand} onChange={handleChange} className="w-full bg-neutral-950 border border-neutral-800 rounded-lg p-3 text-white focus:border-orange-500 focus:outline-none" />
        </div>

        <div>
          <label className="block text-neutral-400 mb-2">Model</label>
          <input name="model" value={formData.model} onChange={handleChange} className="w-full bg-neutral-950 border border-neutral-800 rounded-lg p-3 text-white focus:border-orange-500 focus:outline-none" />
        </div>

        {formData.category !== "PART" && (
          <>
            <div>
              <label className="block text-neutral-400 mb-2">Yıl</label>
              <input type="number" name="year" value={formData.year} onChange={handleChange} className="w-full bg-neutral-950 border border-neutral-800 rounded-lg p-3 text-white focus:border-orange-500 focus:outline-none" />
            </div>

            <div>
              <label className="block text-neutral-400 mb-2">Kilometre</label>
              <input type="number" name="kilometers" value={formData.kilometers} onChange={handleChange} className="w-full bg-neutral-950 border border-neutral-800 rounded-lg p-3 text-white focus:border-orange-500 focus:outline-none" />
            </div>

            <div>
              <label className="block text-neutral-400 mb-2">Yakıt Tipi</label>
              <select name="fuel" value={formData.fuel} onChange={handleChange} className="w-full bg-neutral-950 border border-neutral-800 rounded-lg p-3 text-white focus:border-orange-500 focus:outline-none">
                 <option value="Petrol">Benzin</option>
                 <option value="Diesel">Dizel</option>
                 <option value="Hybrid">Hibrit</option>
                 <option value="Electric">Elektrikli</option>
              </select>
            </div>

            <div>
              <label className="block text-neutral-400 mb-2">Vites Tipi</label>
              <select name="transmission" value={formData.transmission} onChange={handleChange} className="w-full bg-neutral-950 border border-neutral-800 rounded-lg p-3 text-white focus:border-orange-500 focus:outline-none">
                 <option value="Manual">Manuel</option>
                 <option value="Automatic">Otomatik</option>
              </select>
            </div>
          </>
        )}

        <div className="md:col-span-2">
          <label className="block text-neutral-400 mb-2">Açıklama</label>
          <textarea required rows={5} name="description" value={formData.description} onChange={handleChange} className="w-full bg-neutral-950 border border-neutral-800 rounded-lg p-3 text-white focus:border-orange-500 focus:outline-none" />
        </div>

        <div className="md:col-span-2 space-y-4">
          <label className="block text-neutral-400 mb-1">Araç Fotoğrafı</label>
          
          {formData.images ? (
            <div className="relative group w-full max-w-lg aspect-[16/10] rounded-2xl overflow-hidden border border-neutral-800 bg-neutral-950 shadow-inner">
              {/* eslint-disable-next-line @next/next/no-img-element */}
              <img 
                src={formData.images} 
                alt="Araç Önizleme" 
                className="w-full h-full object-cover transition-transform duration-500 group-hover:scale-105"
              />
              <div className="absolute inset-0 bg-black/60 opacity-0 group-hover:opacity-100 transition-opacity duration-300 flex items-center justify-center gap-4">
                <button
                  type="button"
                  onClick={() => fileInputRef.current?.click()}
                  className="bg-white/90 hover:bg-white text-neutral-900 px-4 py-2 rounded-xl text-sm font-bold shadow-lg transition-transform hover:scale-105 active:scale-95"
                >
                  Fotoğrafı Değiştir
                </button>
                <button
                  type="button"
                  onClick={handleRemoveImage}
                  className="bg-red-600 hover:bg-red-500 text-white px-4 py-2 rounded-xl text-sm font-bold shadow-lg transition-transform hover:scale-105 active:scale-95"
                >
                  Kaldır
                </button>
              </div>
            </div>
          ) : (
            <div
              onDragOver={handleDragOver}
              onDragLeave={handleDragLeave}
              onDrop={handleDrop}
              onClick={() => fileInputRef.current?.click()}
              className={`w-full max-w-lg aspect-[16/10] border-2 border-dashed rounded-2xl flex flex-col items-center justify-center p-6 text-center cursor-pointer transition-all duration-300 ${
                isDragging 
                  ? "border-orange-500 bg-orange-500/10 text-orange-400 scale-[1.01]" 
                  : "border-neutral-800 hover:border-neutral-700 bg-neutral-950/50 hover:bg-neutral-950 text-neutral-400"
              }`}
            >
              {uploading ? (
                <div className="flex flex-col items-center gap-3">
                  <div className="w-8 h-8 border-4 border-orange-500 border-t-transparent rounded-full animate-spin"></div>
                  <p className="text-sm font-semibold text-neutral-300">Fotoğraf yükleniyor...</p>
                </div>
              ) : (
                <div className="flex flex-col items-center gap-3">
                  <div className="text-4xl text-neutral-600 transition-transform">📷</div>
                  <div>
                    <p className="font-semibold text-neutral-200">
                      Fotoğrafı sürükleyip bırakın veya <span className="text-orange-500 underline">göz atın</span>
                    </p>
                    <p className="text-xs text-neutral-500 mt-1">PNG, JPG, JPEG, WEBP (Maks. 5MB)</p>
                  </div>
                </div>
              )}
            </div>
          )}

          <input
            type="file"
            ref={fileInputRef}
            onChange={handleFileChange}
            accept="image/*"
            className="hidden"
          />

          <div>
            <button
              type="button"
              onClick={() => setShowManualUrl(!showManualUrl)}
              className="text-xs text-orange-500/80 hover:text-orange-500 transition-colors font-medium flex items-center gap-1"
            >
              {showManualUrl ? "▲ Görsel URL alanını gizle" : "▼ Manuel dosya yolu veya URL girmek istiyorum"}
            </button>

            {showManualUrl && (
              <div className="mt-3 space-y-2">
                <input
                  name="images"
                  value={formData.images}
                  onChange={handleChange}
                  placeholder="Örn: /cars/Audi_A3.jpg veya https://..."
                  className="w-full bg-neutral-950 border border-neutral-800 rounded-lg p-3 text-white focus:border-orange-500 focus:outline-none text-sm"
                />
                <p className="text-xs text-neutral-500">
                  Fotoğrafın tam bağlantısını veya sunucudaki konumunu elle de belirtebilirsiniz.
                </p>
              </div>
            )}
          </div>
        </div>

        <div className="md:col-span-2 flex items-center gap-3">
          <input type="checkbox" id="isAvailable" name="isAvailable" checked={formData.isAvailable} onChange={handleChange} className="w-5 h-5 accent-orange-500" />
          <label htmlFor="isAvailable" className="text-white font-medium">Bu İlan Yayında (Aktif)</label>
        </div>
      </div>

      <div className="pt-6 border-t border-neutral-800 flex justify-end gap-4">
        <button type="button" onClick={() => router.back()} className="px-6 py-3 font-semibold text-neutral-400 hover:text-white transition-colors">
          İptal
        </button>
        <button type="submit" disabled={loading} className="bg-orange-600 hover:bg-orange-500 text-white font-bold py-3 px-8 rounded-xl transition-all shadow-[0_0_20px_rgba(234,88,12,0.3)] disabled:opacity-50">
          {loading ? "Kaydediliyor..." : initialData ? "Değişiklikleri Kaydet" : "İlanı Oluştur"}
        </button>
      </div>
    </form>
  );
}
