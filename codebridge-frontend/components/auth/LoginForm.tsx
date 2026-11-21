"use client";

import { useEffect, useState } from "react";
import { useRouter } from "next/navigation";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { toast } from "sonner";
import { getErrorMessage } from "@/lib/api/axios";
import { useAuth } from "@/lib/hooks/useAuth";
import { loginSchema, type LoginValues } from "@/lib/validators/auth";
import { ROLE_HOME } from "@/lib/constants";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import {
  Form,
  FormControl,
  FormField,
  FormItem,
  FormLabel,
  FormMessage
} from "@/components/ui/form";

export function LoginForm() {
  const router = useRouter();
  const { login, hydrated, user } = useAuth();
  const [submitting, setSubmitting] = useState(false);

  // اگر کاربر قبلاً وارد شده، به پنل خودش هدایت می‌شود
  useEffect(() => {
    if (hydrated && user && !submitting) {
      router.replace(ROLE_HOME[user.role]);
    }
  }, [hydrated, user, submitting, router]);

  const form = useForm<LoginValues>({
    resolver: zodResolver(loginSchema),
    defaultValues: { email: "", password: "" }
  });

  async function onSubmit(values: LoginValues) {
    setSubmitting(true);

    try {
      const user = await login(values);
      toast.success(`خوش آمدید، ${user.firstName ?? user.email}`);

      const next = new URLSearchParams(window.location.search).get("next");
      router.push(next && next.startsWith("/") ? next : ROLE_HOME[user.role]);
    } catch (error) {
      toast.error(getErrorMessage(error, "ورود ناموفق بود."));
    } finally {
      setSubmitting(false);
    }
  }

  return (
    <Form {...form}>
      <form onSubmit={form.handleSubmit(onSubmit)} className="space-y-4">
        <FormField
          control={form.control}
          name="email"
          render={({ field }) => (
            <FormItem>
              <FormLabel>ایمیل</FormLabel>
              <FormControl>
                <Input
                  type="email"
                  dir="ltr"
                  placeholder="you@example.com"
                  autoComplete="email"
                  {...field}
                />
              </FormControl>
              <FormMessage />
            </FormItem>
          )}
        />

        <FormField
          control={form.control}
          name="password"
          render={({ field }) => (
            <FormItem>
              <FormLabel>رمز عبور</FormLabel>
              <FormControl>
                <Input
                  type="password"
                  dir="ltr"
                  placeholder="••••••••"
                  autoComplete="current-password"
                  {...field}
                />
              </FormControl>
              <FormMessage />
            </FormItem>
          )}
        />

        <Button type="submit" className="w-full" disabled={submitting}>
          {submitting ? "در حال ورود..." : "ورود"}
        </Button>
      </form>
    </Form>
  );
}