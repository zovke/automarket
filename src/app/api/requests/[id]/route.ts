import { prisma } from "@/lib/prisma";
import { getServerSession } from "next-auth/next";
import { authOptions } from "@/app/api/auth/[...nextauth]/route";
import { NextResponse } from "next/server";

export async function PUT(req: Request, { params }: { params: Promise<{ id: string }> }) {
  const session = await getServerSession(authOptions);
  if (!session || !session.user || (session.user as any).role !== "ADMIN") {
      return NextResponse.json({ message: "Yetkisiz" }, { status: 401 });
  }
  const { status } = await req.json(); // PENDING, APPROVED, REJECTED
  const { id } = await params;

  try {
      const updated = await prisma.request.update({
          where: { id },
          data: { status }
      });
      
      // If approved, make listing unavailable to prevent double selling/renting
      if(status === "APPROVED") {
          await prisma.listing.update({
              where: { id: updated.listingId },
              data: { isAvailable: false }
          });
      }
      return NextResponse.json({ message: "Durum güncellendi.", updated }, { status: 200 });
  } catch(e) {
      return NextResponse.json({ message: "Sunucu hatası" }, { status: 500 });
  }
}
