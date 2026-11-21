import type { Metadata } from "next";
import Link from "next/link";
import { LoginForm } from "@/components/auth/LoginForm";

export const metadata: Metadata = { title: "ورود" };

export default function LoginPage() {
  return (
    <div className="mx-auto w-full max-w-md rounded-2xl border bg-card p-6 shadow-sm">
      <h1 className="text-xl font-bold">ورود به حساب</h1>
      <p className="mb-6 mt-1 text-sm text-muted-foreground">
        برای دسترسی به پنل خود وارد شوید.
      </p>

      <LoginForm />

      <p className="mt-6 text-center text-sm text-muted-foreground">
        حساب ندارید؟{" "}
        <Link href="/register" className="font-medium text-primary hover:underline">
          ثبت‌نام
        </Link>
      </p>
    </div>
  );
}