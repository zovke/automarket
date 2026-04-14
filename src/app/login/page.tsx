"use client";

import { useState } from "react";
import { signIn } from "next-auth/react";
import { useRouter } from "next/navigation";
import Link from "next/link";

export default function LoginPage() {
  const router = useRouter();
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [error, setError] = useState("");

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError("");

    const res = await signIn("credentials", {
      email,
      password,
      redirect: false,
    });

    if (res?.error) {
      setError("Email veya şifre hatalı");
    } else {
      router.push("/");
      router.refresh();
    }
  };

  return (
    <div className="min-h-screen flex items-center justify-center bg-neutral-900 pb-20">
      <div className="bg-neutral-800 p-8 rounded-2xl w-full max-w-md border border-neutral-700">
        <h1 className="text-3xl font-bold text-center text-white mb-6">Giriş Yap</h1>
        {error && <p className="text-red-500 text-center mb-4">{error}</p>}
        <form onSubmit={handleSubmit} className="space-y-4">
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
            className="w-full bg-red-600 hover:bg-red-700 text-white p-3 rounded-lg font-bold transition duration-300"
          >
            Giriş Yap
          </button>
        </form>
        <p className="mt-4 text-center text-neutral-400">
          Hesabınız yok mu?{" "}
          <Link href="/register" className="text-red-500 hover:text-red-400">
            Kayıt Ol
          </Link>
        </p>
      </div>
    </div>
  );
}
