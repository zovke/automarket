import { NextResponse } from 'next/server';
import { prisma } from '@/lib/prisma';
import { getServerSession } from 'next-auth/next';
import { authOptions } from '@/app/api/auth/[...nextauth]/route';

export async function PUT(req: Request, { params }: { params: Promise<{ id: string }> }) {
  try {
    const session = await getServerSession(authOptions);
    if (!session || (session.user as any).role !== 'ADMIN') {
      return new NextResponse('Unauthorized', { status: 403 });
    }

    const { id } = await params;
    const { status } = await req.json();

    if (!['APPROVED', 'REJECTED'].includes(status)) {
      return new NextResponse('Invalid status', { status: 400 });
    }

    const request = await prisma.request.update({
      where: { id },
      data: { status }
    });

    if (status === 'APPROVED') {
      // Mark listing as unavailable (SOLD)
      await prisma.listing.update({
        where: { id: request.listingId },
        data: { isAvailable: false }
      });

      // Reject all other pending requests for this same listing
      await prisma.request.updateMany({
        where: { 
          listingId: request.listingId, 
          id: { not: request.id }, 
          status: 'PENDING' 
        },
        data: { status: 'REJECTED' }
      });
    } else if (status === 'REJECTED') {
      // If rejecting, maybe ensure listing is available (in case it was previously approved)
      // Usually, just letting it be rejected is fine.
    }

    return NextResponse.json({ success: true, request });
  } catch (error) {
    console.error('[ADMIN_REQUEST_ERROR]', error);
    return new NextResponse('Internal Error', { status: 500 });
  }
}
