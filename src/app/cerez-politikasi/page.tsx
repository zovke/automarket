import Navbar from "@/components/Navbar";

export const metadata = {
  title: 'Çerez Politikası | Auto Marketplace',
};

export default function CerezPolitikasiPage() {
  return (
    <div className="min-h-screen bg-neutral-950 text-neutral-300 font-sans selection:bg-orange-500/30">
      <Navbar />
      <div className="max-w-4xl mx-auto px-6 py-24">
        <h1 className="text-4xl font-black text-white mb-8">Çerez (Cookie) Politikası</h1>
        
        <div className="space-y-8 text-neutral-400 leading-relaxed">
          <section className="bg-neutral-900/50 border border-neutral-800/80 p-8 rounded-3xl backdrop-blur-sm">
            <h2 className="text-2xl font-bold text-white mb-4">Çerez Nedir?</h2>
            <p>
              Çerezler, ziyaret ettiğiniz web siteleri tarafından tarayıcınız aracılığıyla bilgisayarınıza (veya akıllı telefon, tablet vb. 
              mobil cihazınıza) kaydedilen küçük boyutlu metin dosyalarıdır. Çerezlerin temel amacı kullanıcının cihazına ve tarayıcı hareketlerine 
              ilişkin veri depolayarak bir sonraki ziyarette deneyimi hızlandırmaktır.
            </p>
          </section>

          <section className="bg-neutral-900/50 border border-neutral-800/80 p-8 rounded-3xl backdrop-blur-sm">
             <h2 className="text-2xl font-bold text-white mb-4">Neden Çerez Kullanıyoruz?</h2>
             <ul className="list-none space-y-4 text-neutral-400">
               <li className="flex items-start gap-4">
                 <div className="w-8 h-8 rounded-full bg-orange-600/20 text-orange-500 flex items-center justify-center shrink-0">1</div>
                 <p>Oturum Yönetimi: Oturum açık kalma sürenizi ve Admin/User rollerinizin tutarlılığını sağlamak için sistemimiz zorunlu JWT (NextAuth) çerezlerine ihtiyaç duyar.</p>
               </li>
               <li className="flex items-start gap-4">
                 <div className="w-8 h-8 rounded-full bg-orange-600/20 text-orange-500 flex items-center justify-center shrink-0">2</div>
                 <p>Kişiselleştirme: Beğendiğiniz, favorilere eklediğiniz araç kayıtlarının local storage (hafıza) ve çerezler aracıyla hatırlanmasını sağlamak.</p>
               </li>
               <li className="flex items-start gap-4">
                 <div className="w-8 h-8 rounded-full bg-orange-600/20 text-orange-500 flex items-center justify-center shrink-0">3</div>
                 <p>Güvenlik: Web tabanlı saldırı tiplerine karşı bot koruması ve limitleri düzenlemek amcacıya metin dosyası loglamak.</p>
               </li>
             </ul>
          </section>

          <section className="bg-neutral-900/50 border border-neutral-800/80 p-8 rounded-3xl backdrop-blur-sm">
             <h2 className="text-2xl font-bold text-white mb-4">Çerez Optimizasyonu ve İptali</h2>
             <p>
               Tarayıcınızın ayarlarını değiştirerek çerezlere ilişkin tercihlerinizi kişiselleştirme imkanına sahipsiniz.  
               Lakin çerezleri tamamen devre dışı bırakmanız durumunda platformumuz içerisindeki Satın Alım, İlan Yükleme ve Yorum atma 
               özelliklerinin işlevini yitirebileceğini bilmelisiniz.
             </p>
          </section>
        </div>
      </div>
    </div>
  );
}
