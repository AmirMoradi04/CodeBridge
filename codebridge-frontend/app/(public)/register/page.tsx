import type { Metadata } from "next";
import Link from "next/link";
import { RegisterForm } from "@/components/auth/RegisterForm";

export const metadata: Metadata = { title: "ثبت‌نام" };

export default function RegisterPage() {
  return (
    <div className="mx-auto w-full max-w-2xl rounded-2xl border bg-card p-6 shadow-sm">
      <h1 className="text-xl font-bold">ساخت حساب جدید</h1>
      <p className="mb-6 mt-1 text-sm text-muted-foreground">
        پس از ثبت‌نام، حساب شما توسط ادمین تایید می‌شود.
      </p>

      <RegisterForm />

      <p className="mt-6 text-center text-sm text-muted-foreground">
        قبلاً ثبت‌نام کرده‌اید؟{" "}
        <Link href="/login" className="font-medium text-primary hover:underline">
          ورود
        </Link>
      </p>
    </div>
  );
}