"use client";

import { useState } from "react";
import { useRouter } from "next/navigation";
import Link from "next/link";

export default function RegisterPage() {
  const router = useRouter();
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [firstName, setFirstName] = useState("");
  const [lastName, setLastName] = useState("");
  const [error, setError] = useState("");
  const [loading, setLoading] = useState(false);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError("");
    setLoading(true);

    try {
      const res = await fetch("/api/auth/register", {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
        },
        body: JSON.stringify({ email, password, firstName, lastName }),
      });

      if (res.ok) {
        router.push("/login?registered=true");
      } else {
        const data = await res.json();
        setError(data.message || "Bir hata oluştu");
      }
    } catch (err) {
      setError("Sunucu bağlantı hatası");
    }

    setLoading(false);
  };

  return (
    <div className="min-h-screen flex items-center justify-center bg-neutral-900 pb-20">
      <div className="bg-neutral-800 p-8 rounded-2xl w-full max-w-md border border-neutral-700">
        <h1 className="text-3xl font-bold text-center text-white mb-6">Kayıt Ol</h1>
        {error && <p className="text-red-500 text-center mb-4">{error}</p>}
        <form onSubmit={handleSubmit} className="space-y-4">
          <div className="grid grid-cols-2 gap-4">
            <div>
              <label className="block text-neutral-300 mb-2">Ad</label>
              <input
                type="text"
                value={firstName}
                onChange={(e) => setFirstName(e.target.value)}
                className="w-full bg-neutral-900 text-white p-3 rounded-lg border border-neutral-700 focus:outline-none focus:border-red-500"
                required
              />
            </div>
            <div>
              <label className="block text-neutral-300 mb-2">Soyad</label>
              <input
                type="text"
                value={lastName}
                onChange={(e) => setLastName(e.target.value)}
                className="w-full bg-neutral-900 text-white p-3 rounded-lg border border-neutral-700 focus:outline-none focus:border-red-500"
                required
              />
            </div>
          </div>
          <div>
            <label className="block text-neutral-300 mb-2">Email</label>
            <input
              type="email"
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              className="w-full bg-neutral-900 text-white p-3 rounded-lg border border-neutral-700 focus:outline-none focus:border-red-500"
              required
            />
          </div>
          <div>
            <label className="block text-neutral-300 mb-2">Şifre</label>
            <input
              type="password"
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              className="w-full bg-neutral-900 text-white p-3 rounded-lg border border-neutral-700 focus:outline-none focus:border-red-500"
              required
            />
          </div>
          <button
            type="submit"
            disabled={loading}
            className="w-full bg-red-600 hover:bg-red-700 text-white p-3 rounded-lg font-bold transition duration-300 disabled:opacity-50"
          >
            {loading ? "Kaydediliyor..." : "Kayıt Ol"}
          </button>
        </form>
        <p className="mt-4 text-center text-neutral-400">
          Zaten hesabınız var mı?{" "}
          <Link href="/login" className="text-red-500 hover:text-red-400">
            Giriş Yap
          </Link>
        </p>
      </div>
    </div>
  );
}
