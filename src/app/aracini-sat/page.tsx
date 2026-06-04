import ListingForm from "@/components/ListingForm";
import { getServerSession } from "next-auth/next";
import { authOptions } from "@/app/api/auth/[...nextauth]/route";
import { redirect } from "next/navigation";
import Link from "next/link";
import Navbar from "@/components/Navbar";

export default async function SellCarPage() {
  const session = await getServerSession(authOptions);
  
  if (!session || !session.user) {
    redirect("/login?redirect=/aracini-sat");
  }

  return (
    <div className="min-h-screen bg-neutral-950 text-white overflow-x-hidden">
      <Navbar />
      <div className="max-w-4xl mx-auto p-8">
        {/* Simple Navbar back to Home */}
        <div className="mb-12">
          <Link href="/" className="text-orange-500 hover:text-orange-400 font-medium">← Ana Sayfaya Dön</Link>
        </div>
        
        <h1 className="text-4xl font-extrabold mb-4 bg-gradient-to-r from-orange-500 to-white bg-clip-text text-transparent">Aracınızı Saniyeler İçinde İlana Koyun</h1>
        <p className="text-neutral-400 text-lg mb-12">Aracınız veya yedek parçanız yüzbinlerce alıcıyla buluşsun. Lütfen formu eksiksiz doldurun.</p>
        
        <ListingForm />
      </div>
    </div>
  );
}
