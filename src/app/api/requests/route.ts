import { prisma } from "@/lib/prisma";
import { getServerSession } from "next-auth/next";
import { authOptions } from "@/app/api/auth/[...nextauth]/route";
import { NextResponse } from "next/server";

export async function POST(req: Request) {
  const session = await getServerSession(authOptions);
  if (!session || !session.user) return NextResponse.json({ message: "Giriş yapmanız gerekiyor." }, { status: 401 });

  try {
    const { listingId } = await req.json();
    if (!listingId) return NextResponse.json({ message: "İlan kimliği gerekli." }, { status: 400 });

    const request = await prisma.request.create({
      data: {
        listingId,
        userId: (session.user as any).id
      }
    });

    return NextResponse.json({ message: "Talebiniz başarıyla alındı.", request }, { status: 201 });
  } catch (e) {
      console.error(e);
      return NextResponse.json({ message: "Sunucu hatası" }, { status: 500 });
  }
}

export async function GET(req: Request) {
    const session = await getServerSession(authOptions);
    if (!session || !session.user || (session.user as any).role !== "ADMIN") {
        return NextResponse.json({ message: "Yetkisiz Erişim" }, { status: 401 });
    }
    const requests = await prisma.request.findMany({ 
        include: { user: true, listing: true },
        orderBy: { createdAt: 'desc' }
    });
    return NextResponse.json({ requests }, { status: 200 });
}
