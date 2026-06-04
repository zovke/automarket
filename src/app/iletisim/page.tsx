import Navbar from "@/components/Navbar";

export const metadata = {
  title: 'İletişim | Auto Marketplace',
};

export default function IletisimPage() {
  return (
    <div className="min-h-screen bg-neutral-950 text-neutral-200 font-sans selection:bg-orange-500/30">
      <Navbar />
      <div className="max-w-4xl mx-auto px-6 py-24">
        <h1 className="text-4xl font-black text-white mb-8">İletişim</h1>
        <p className="text-lg text-neutral-400 mb-12">
          Soru, görüş ve önerileriniz için bizimle iletişime geçmekten çekinmeyin. Ekibimiz en kısa sürede size dönüş yapacaktır.
        </p>

        <div className="grid grid-cols-1 md:grid-cols-2 gap-12">
          {/* İletişim Formu */}
          <div className="bg-neutral-900 border border-neutral-800 p-8 rounded-3xl">
            <h2 className="text-2xl font-bold text-white mb-6">Bize Ulaşın</h2>
            <form className="space-y-4">
              <div>
                <label className="block text-sm text-neutral-400 mb-1">Ad Soyad</label>
                <input type="text" className="w-full bg-neutral-950 border border-neutral-800 rounded-xl px-4 py-3 text-white focus:outline-none focus:border-orange-500 transition-colors" />
              </div>
              <div>
                <label className="block text-sm text-neutral-400 mb-1">E-posta</label>
                <input type="email" className="w-full bg-neutral-950 border border-neutral-800 rounded-xl px-4 py-3 text-white focus:outline-none focus:border-orange-500 transition-colors" />
              </div>
              <div>
                <label className="block text-sm text-neutral-400 mb-1">Mesajınız</label>
                <textarea rows={4} className="w-full bg-neutral-950 border border-neutral-800 rounded-xl px-4 py-3 text-white focus:outline-none focus:border-orange-500 transition-colors resize-none"></textarea>
              </div>
              <button type="button" className="w-full bg-orange-600 hover:bg-orange-500 text-white font-bold py-3 rounded-xl transition-colors mt-2">
                Gönder
              </button>
            </form>
          </div>

          {/* Adres ve Bilgiler */}
          <div className="space-y-8">
            <div className="bg-neutral-900 border border-neutral-800 p-6 rounded-2xl">
              <h3 className="text-orange-500 font-bold mb-2">Merkez Ofis</h3>
              <p className="text-neutral-400 leading-relaxed">
                Levent Mah. Büyükdere Cad. No:1<br/>
                Beşiktaş, İstanbul, Türkiye
              </p>
            </div>
            <div className="bg-neutral-900 border border-neutral-800 p-6 rounded-2xl">
              <h3 className="text-orange-500 font-bold mb-2">Telefon</h3>
              <p className="text-neutral-400 leading-relaxed">+90 (850) 123 45 67</p>
            </div>
            <div className="bg-neutral-900 border border-neutral-800 p-6 rounded-2xl">
              <h3 className="text-orange-500 font-bold mb-2">E-Posta</h3>
              <p className="text-neutral-400 leading-relaxed">destek@automarket.com.tr</p>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}
