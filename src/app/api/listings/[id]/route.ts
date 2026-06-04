import { prisma } from "@/lib/prisma";
import { getServerSession } from "next-auth/next";
import { authOptions } from "@/app/api/auth/[...nextauth]/route";
import { NextResponse } from "next/server";

export async function PUT(req: Request, { params }: { params: Promise<{ id: string }> }) {
  const session = await getServerSession(authOptions);
  
  if (!session || !session.user) {
    return NextResponse.json({ message: "Yetkisiz erişim." }, { status: 401 });
  }

  try {
    const { id } = await params;
    const data = await req.json();
    const { title, description, price, category, brand, model, year, kilometers, fuel, transmission, isAvailable, images } = data;

    // Check ownership or admin context
    const listing = await prisma.listing.findUnique({ where: { id }});
    if (!listing) return NextResponse.json({ message: "İlan bulunamadı" }, { status: 404 });

    const isOwner = listing.userId === (session.user as any).id;
    const isAdmin = (session.user as any).role === "ADMIN";

    if (!isOwner && !isAdmin) {
      return NextResponse.json({ message: "Bu ilanı düzenleme yetkiniz yok." }, { status: 403 });
    }

    const updatedListing = await prisma.listing.update({
      where: { id },
      data: {
         title,
         description,
         price: parseFloat(price),
         category,
         brand,
         model,
         year: year ? parseInt(year) : null,
         kilometers: kilometers ? parseInt(kilometers) : null,
         fuel,
         transmission,
         isAvailable,
         images
      }
    });

    return NextResponse.json({ message: "İlan güncellendi.", listing: updatedListing }, { status: 200 });
  } catch (error) {
    console.error("İlan güncellenirken hata:", error);
    return NextResponse.json({ message: "Sunucu hatası" }, { status: 500 });
  }
}

export async function DELETE(req: Request, { params }: { params: Promise<{ id: string }> }) {
    const session = await getServerSession(authOptions);
    if (!session || !session.user) return NextResponse.json({ message: "Yetkisiz erişim." }, { status: 401 });

    try {
        const { id } = await params;
        const listing = await prisma.listing.findUnique({ where: { id }});
        if (!listing) return NextResponse.json({ message: "İlan bulunamadı" }, { status: 404 });

        const isOwner = listing.userId === (session.user as any).id;
        const isAdmin = (session.user as any).role === "ADMIN";

        if (!isOwner && !isAdmin) {
            return NextResponse.json({ message: "Silme yetkiniz yok." }, { status: 403 });
        }

        await prisma.listing.delete({ where: { id }});
        return NextResponse.json({ message: "İlan silindi." }, { status: 200 });
    } catch (e) {
        return NextResponse.json({ message: "Sunucu hatası" }, { status: 500 });
    }
}
