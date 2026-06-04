import { prisma } from "@/lib/prisma";
import { getServerSession } from "next-auth/next";
import { authOptions } from "@/app/api/auth/[...nextauth]/route";
import { NextResponse } from "next/server";

export async function POST(req: Request) {
  const session = await getServerSession(authOptions);
  
  if (!session || !session.user) {
    return NextResponse.json({ message: "Yetkisiz erişim." }, { status: 401 });
  }

  try {
    const data = await req.json();
    const { title, description, price, category, brand, model, year, kilometers, fuel, transmission, images } = data;

    const newListing = await prisma.listing.create({
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
        images,
        userId: (session.user as any).id
      }
    });

    return NextResponse.json({ message: "İlan başarıyla eklendi.", listing: newListing }, { status: 201 });
  } catch (error) {
    console.error("Listing yaratılırken hata:", error);
    return NextResponse.json({ message: "Sunucu hatası" }, { status: 500 });
  }
}
