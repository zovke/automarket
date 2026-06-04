import { prisma } from "@/lib/prisma";
import ListingForm from "@/components/ListingForm";
import { notFound } from "next/navigation";

export default async function EditListingPage({ params }: { params: Promise<{ id: string }> }) {
  const { id } = await params;
  
  const listing = await prisma.listing.findUnique({
    where: { id }
  });

  if (!listing) return notFound();

  return (
    <div>
      <h1 className="text-3xl font-bold mb-8 text-white">İlanı Düzenle</h1>
      <ListingForm initialData={listing} />
    </div>
  );
}
