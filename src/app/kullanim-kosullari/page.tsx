import Navbar from "@/components/Navbar";

export const metadata = {
  title: 'Kullanım Koşulları | Auto Marketplace',
};

export default function KullanimKosullariPage() {
  return (
    <div className="min-h-screen bg-neutral-950 text-neutral-300 font-sans selection:bg-orange-500/30">
      <Navbar />
      <div className="max-w-4xl mx-auto px-6 py-24">
        <h1 className="text-4xl font-black text-white mb-8">Kullanım Koşulları</h1>
        
        <div className="space-y-8 text-neutral-400 leading-relaxed">
          <section className="bg-neutral-900/50 border border-neutral-800/80 p-8 rounded-3xl backdrop-blur-sm">
            <h2 className="text-2xl font-bold text-white mb-4">1. Kabul Edilme Şartları</h2>
            <p>
              Auto Marketplace platformunu kullanarak, işbu koşul ve şartların tamamını kabul etmiş sayılırsınız. 
              Sistemimizde yer alan tüm alım, satım ve kiralama prosedürleri, aşağıda belirtilen kurallara tabidir.
            </p>
          </section>

          <section className="bg-neutral-900/50 border border-neutral-800/80 p-8 rounded-3xl backdrop-blur-sm">
             <h2 className="text-2xl font-bold text-white mb-4">2. İlan Verme Sorumluluğu</h2>
             <p>
               Sistemimize yüklenen araç ve yedek parça ilanlarının doğruluğundan tamamen kullanıcı (satıcı) sorumludur. 
               Yanlış veya yanıltıcı bilgi barındıran ilanlar, şikayet veya tespit durumunda sistem yöneticileri tarafından 
               önceden haber verilmeksizin kalıcı olarak silinecektir.
             </p>
          </section>

          <section className="bg-neutral-900/50 border border-neutral-800/80 p-8 rounded-3xl backdrop-blur-sm">
             <h2 className="text-2xl font-bold text-white mb-4">3. Ödeme ve Kapora Güvenliği</h2>
             <p>
               Auto Marketplace, taraflar arası tahsilat sürecine müdahil olmaz. Platform üzerinden alıcılar ile satıcıları 
               birleştiren bir aracı servis olarak çalışmaktadır. Araçları veya belgelerini fiziksel olarak görmeden hiçbir şekil ve şartta kapora ödemesi 
               yapılmamasını önemle hatırlatırız. Doğacak mali uyuşmazlıklarda platformumuz yasal olarak sorumluluk kabul etmez.
             </p>
          </section>
        </div>
      </div>
    </div>
  );
}
