"use client";

import { useState, useRef, useEffect } from "react";
import Link from "next/link";
import { signOut } from "next-auth/react";

export default function UserMenu({ user }: { user: any }) {
    const [open, setOpen] = useState(false);
    const menuRef = useRef<HTMLDivElement>(null);

    useEffect(() => {
        const handleClickOutside = (event: MouseEvent) => {
            if (menuRef.current && !menuRef.current.contains(event.target as Node)) {
                setOpen(false);
            }
        };
        document.addEventListener("mousedown", handleClickOutside);
        return () => document.removeEventListener("mousedown", handleClickOutside);
    }, []);

    return (
        <div className="relative" ref={menuRef}>
            <div 
                onClick={() => setOpen(!open)}
                className="bg-neutral-800 px-4 py-2 rounded-full border border-neutral-700 flex items-center gap-2 cursor-pointer hover:border-neutral-500 transition-colors"
            >
                <div className="w-6 h-6 bg-orange-600 rounded-full flex items-center justify-center text-xs font-bold text-white shadow-[0_0_10px_rgba(234,88,12,0.4)]">
                    {(user.firstName?.[0] || 'U')}
                </div>
                <span className="text-sm font-bold text-white pr-2">{user.firstName} {user.lastName}</span>
                <svg width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round" className={`text-neutral-400 transition-transform ${open ? 'rotate-180' : ''}`}><polyline points="6 9 12 15 18 9"></polyline></svg>
            </div>

            {open && (
                <div className="absolute top-12 right-0 w-48 bg-neutral-900 border border-neutral-800 rounded-xl shadow-2xl overflow-hidden flex flex-col z-50 animate-fade-in-up">
                    <div className="p-4 border-b border-neutral-800/80">
                        <p className="text-xs text-neutral-400">Giriş Yapıldı:</p>
                        <p className="text-sm text-white font-bold truncate">{user.email}</p>
                    </div>
                    
                    <div className="p-2 flex flex-col gap-1">
                        <Link href="/aracini-sat" onClick={() => setOpen(false)} className="px-3 py-2 text-sm text-neutral-300 hover:text-white hover:bg-neutral-800 rounded-lg flex items-center gap-2">
                           <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="orange" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round"><path d="M11 21H4a2 2 0 0 1-2-2V5c0-1.1.9-2 2-2h5l2 3h9a2 2 0 0 1 2 2v2M19 15v6M16 18h6"></path></svg>
                           İlan Ver / Sat
                        </Link>

                        {user.role === "ADMIN" && (
                            <Link href="/admin" onClick={() => setOpen(false)} className="px-3 py-2 text-sm text-orange-500 hover:text-orange-400 hover:bg-neutral-800 rounded-lg flex items-center gap-2">
                                <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round"><path d="M12 22s8-4 8-10V5l-8-3-8 3v7c0 6 8 10 8 10z"></path></svg>
                                Yönetim Paneli
                            </Link>
                        )}
                        
                    </div>

                    <div className="p-2 border-t border-neutral-800/80">
                        <button onClick={() => signOut()} className="w-full text-left px-3 py-2 text-sm text-red-500 hover:text-red-400 hover:bg-red-500/10 rounded-lg flex items-center gap-2 transition-colors">
                            <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round"><path d="M9 21H5a2 2 0 0 1-2-2V5a2 2 0 0 1 2-2h4"></path><polyline points="16 17 21 12 16 7"></polyline><line x1="21" y1="12" x2="9" y2="12"></line></svg>
                            Çıkış Yap
                        </button>
                    </div>
                </div>
            )}
        </div>
    );
}
