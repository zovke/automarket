import { NextResponse } from 'next/server';
import { prisma } from '@/lib/prisma';
import { getServerSession } from 'next-auth/next';
import { authOptions } from '@/app/api/auth/[...nextauth]/route';

export async function PUT(req: Request, { params }: { params: { id: string } }) {
  try {
    const session = await getServerSession(authOptions);
    if (!session || (session.user as any).role !== 'ADMIN') {
      return new NextResponse('Unauthorized', { status: 403 });
    }

    const { role } = await req.json();

    if (!['ADMIN', 'USER'].includes(role)) {
      return new NextResponse('Invalid role', { status: 400 });
    }

    const targetUser = await prisma.user.findUnique({ where: { id: params.id } });
    
    if (!targetUser) return new NextResponse('Not found', { status: 404 });
    // Prevent an admin from demoting themselves by accident to avoid locking out the system.
    if (targetUser.email === session.user?.email) {
       return new NextResponse('Cannot demote yourself', { status: 400 });
    }

    const user = await prisma.user.update({
      where: { id: params.id },
      data: { role }
    });

    return NextResponse.json({ success: true, user });
  } catch (error) {
    console.error('[ADMIN_USER_ROLE_ERROR]', error);
    return new NextResponse('Internal Error', { status: 500 });
  }
}

export async function DELETE(req: Request, { params }: { params: { id: string } }) {
   try {
    const session = await getServerSession(authOptions);
    if (!session || (session.user as any).role !== 'ADMIN') {
      return new NextResponse('Unauthorized', { status: 403 });
    }

    const targetUser = await prisma.user.findUnique({ where: { id: params.id } });
    if (!targetUser) return new NextResponse('Not found', { status: 404 });

    if (targetUser.email === session.user?.email) {
       return new NextResponse('Cannot delete yourself', { status: 400 });
    }

    await prisma.user.delete({
      where: { id: params.id }
    });

    return NextResponse.json({ success: true });
  } catch (error) {
    console.error('[ADMIN_USER_DELETE_ERROR]', error);
    return new NextResponse('Internal Error', { status: 500 });
  }
}
