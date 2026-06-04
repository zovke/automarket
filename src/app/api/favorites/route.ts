import { prisma } from "@/lib/prisma";
import { getServerSession } from "next-auth/next";
import { authOptions } from "@/app/api/auth/[...nextauth]/route";
import { NextResponse } from "next/server";

export async function POST(req: Request) { // Toggles favorite
  const session = await getServerSession(authOptions);
  if (!session || !session.user) return NextResponse.json({ message: "Giriş yapmanız gerekiyor." }, { status: 401 });
  
  try {
    const { listingId } = await req.json();
    const userId = (session.user as any).id;

    const existing = await prisma.favorite.findUnique({
        where: { userId_listingId: { userId, listingId } }
    });

    if (existing) {
        await prisma.favorite.delete({ where: { id: existing.id } });
        return NextResponse.json({ message: "Favorilerden çıkarıldı", isFavorite: false }, { status: 200 });
    } else {
        await prisma.favorite.create({ data: { userId, listingId } });
        return NextResponse.json({ message: "Favorilere eklendi", isFavorite: true }, { status: 201 });
    }
  } catch (e) {
      return NextResponse.json({ message: "Hata" }, { status: 500 });
  }
}

export async function GET(req: Request) {
    const session = await getServerSession(authOptions);
    if (!session || !session.user) return NextResponse.json({ message: "Giriş yapmanız gerekiyor." }, { status: 401 });
    try {
        const userId = (session.user as any).id;
        const favorites = await prisma.favorite.findMany({
            where: { userId },
            include: { listing: true }
        });
        return NextResponse.json({ favorites }, { status: 200 });
    } catch(e) {
        return NextResponse.json({ message: "Hata" }, { status: 500 });
    }
}
