import Navbar from "@/components/Navbar";

export const metadata = {
  title: 'Gizlilik Politikası | Auto Marketplace',
};

export default function GizlilikPolitikasiPage() {
  return (
    <div className="min-h-screen bg-neutral-950 text-neutral-300 font-sans selection:bg-orange-500/30">
      <Navbar />
      <div className="max-w-4xl mx-auto px-6 py-24">
        <h1 className="text-4xl font-black text-white mb-8">Gizlilik Politikası</h1>
        
        <div className="space-y-8 text-neutral-400 leading-relaxed">
          <section className="bg-neutral-900/50 border border-neutral-800/80 p-8 rounded-3xl backdrop-blur-sm">
            <h2 className="text-2xl font-bold text-white mb-4">Veri Toplama</h2>
            <p>
              Auto Marketplace, hizmetlerimizi sağlamak, sürdürmek ve geliştirmek amacıyla; kayıt sırasında vermiş olduğunuz isim, 
              soyisim, e-posta adresi gibi kişisel verilerinizi toplar ve KVKK kapsamında güvenli sunucularında yüksek kriptolama ile muhafaza eder.
            </p>
          </section>

          <section className="bg-neutral-900/50 border border-neutral-800/80 p-8 rounded-3xl backdrop-blur-sm">
             <h2 className="text-2xl font-bold text-white mb-4">Verilerin İşlenmesi ve Kullanımı</h2>
             <p>
               Toplanan kişisel verileriniz;
             </p>
             <ul className="list-disc ml-6 mt-4 space-y-2 text-neutral-400">
               <li>Platform içi ilan iletişimlerinin sağlanması,</li>
               <li>Yetkisiz erişimlerin engellenmesi,</li>
               <li>Yasal taleplere yetkili makamlara dönüş sağlanması</li>
             </ul>
             <p className="mt-4">koşullarıyla işlenir ve şirket dışı kurumlarla paylaşılmaz.</p>
          </section>

          <section className="bg-neutral-900/50 border border-neutral-800/80 p-8 rounded-3xl backdrop-blur-sm">
             <h2 className="text-2xl font-bold text-white mb-4">Veri Silme Talepleri</h2>
             <p>
               Mevcut üyeliğinizi sonlandırmak veya kişisel verilerinizin veri tabanımızdan tamamen silinmesini istemek gibi 
               hususlarda dilediğiniz zaman iletişim panellerimizden sitemize başvurabilirsiniz. Talebiniz en fazla 15 gün içinde yasal 
               standartlarda yerine getirilecektir.
             </p>
          </section>
        </div>
      </div>
    </div>
  );
}
