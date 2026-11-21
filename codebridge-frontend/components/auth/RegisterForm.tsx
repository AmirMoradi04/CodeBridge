"use client";

import { useEffect, useRef, useState } from "react";
import { useRouter } from "next/navigation";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { toast } from "sonner";
import { FileText, Upload } from "lucide-react";
import { getErrorMessage } from "@/lib/api/axios";
import { useAuth } from "@/lib/hooks/useAuth";
import { registerSchema, type RegisterValues } from "@/lib/validators/auth";
import { ROLE_HOME } from "@/lib/constants";
import { Role } from "@/types/auth";
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
import { RoleSelector } from "@/components/auth/RoleSelector";

const MAX_RESUME_SIZE = 10 * 1024 * 1024;

const RESUME_TYPES = new Set([
  "application/pdf",
  "application/vnd.openxmlformats-officedocument.wordprocessingml.document"
]);

export function RegisterForm() {
  const router = useRouter();
  const { register, hydrated, user } = useAuth();
  const [submitting, setSubmitting] = useState(false);
  const [referralCode, setReferralCode] = useState<string | undefined>(undefined);
  const [resumeFile, setResumeFile] = useState<File | null>(null);
  const [resumeError, setResumeError] = useState<string | null>(null);
  const resumeInputRef = useRef<HTMLInputElement>(null);

  // کد معرف از ?ref= در آدرس خوانده می‌شود تا فرم ساده بماند
  useEffect(() => {
    const value = new URLSearchParams(window.location.search).get("ref");

    if (value?.trim()) setReferralCode(value.trim());
  }, []);

  // کاربر وارد‌شده نباید صفحه‌ی ثبت‌نام را ببیند
  useEffect(() => {
    if (hydrated && user && !submitting) {
      router.replace(ROLE_HOME[user.role]);
    }
  }, [hydrated, user, submitting, router]);

  const form = useForm<RegisterValues>({
    resolver: zodResolver(registerSchema),
    defaultValues: {
      firstName: "",
      lastName: "",
      email: "",
      password: "",
      confirmPassword: "",
      userRole: Role.JobSeeker
    }
  });

  const selectedRole = form.watch("userRole");

  // با تغییر نقش، فایل و خطای رزومه پاک می‌شوند
  useEffect(() => {
    if (selectedRole !== Role.JobSeeker) {
      setResumeFile(null);
      setResumeError(null);

      if (resumeInputRef.current) resumeInputRef.current.value = "";
    }
  }, [selectedRole]);

  function handleResumeChange(file: File | null) {
    setResumeError(null);

    if (!file) {
      setResumeFile(null);
      return;
    }

    const hasAllowedType =
      RESUME_TYPES.has(file.type) || /\.(pdf|docx)$/i.test(file.name);

    if (!hasAllowedType) {
      setResumeFile(null);
      setResumeError("فقط فایل PDF یا DOCX برای رزومه مجاز است.");

      if (resumeInputRef.current) resumeInputRef.current.value = "";
      return;
    }

    if (file.size > MAX_RESUME_SIZE) {
      setResumeFile(null);
      setResumeError("حجم فایل رزومه نباید بیشتر از ۱۰ مگابایت باشد.");

      if (resumeInputRef.current) resumeInputRef.current.value = "";
      return;
    }

    setResumeFile(file);
  }

  async function onSubmit(values: RegisterValues) {
    setResumeError(null);

    // کارجو بدون رزومه ثبت‌نام نمی‌شود؛ همین قاعده در بک‌اند هم اعمال می‌شود
    if (values.userRole === Role.JobSeeker && !resumeFile) {
      setResumeError("برای ثبت‌نام کارجو، بارگذاری فایل رزومه الزامی است.");
      return;
    }

    setSubmitting(true);

    try {
      await register({
        firstName: values.firstName,
        lastName: values.lastName,
        email: values.email,
        password: values.password,
        userRole: values.userRole as Role,
        resumeFile: values.userRole === Role.JobSeeker ? resumeFile ?? undefined : undefined,
        referralCode
      });

      toast.success("حساب ساخته شد؛ پس از تایید ادمین می‌توانید وارد شوید.");
      router.push("/login");
    } catch (error) {
      toast.error(getErrorMessage(error, "ثبت‌نام ناموفق بود."));
    } finally {
      setSubmitting(false);
    }
  }

  return (
    <Form {...form}>
      <form onSubmit={form.handleSubmit(onSubmit)} className="space-y-4">
        <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
          <FormField
            control={form.control}
            name="firstName"
            render={({ field }) => (
              <FormItem>
                <FormLabel>نام</FormLabel>
                <FormControl>
                  <Input placeholder="سارا" {...field} />
                </FormControl>
                <FormMessage />
              </FormItem>
            )}
          />

          <FormField
            control={form.control}
            name="lastName"
            render={({ field }) => (
              <FormItem>
                <FormLabel>نام خانوادگی</FormLabel>
                <FormControl>
                  <Input placeholder="کریمی" {...field} />
                </FormControl>
                <FormMessage />
              </FormItem>
            )}
          />
        </div>

        <FormField
          control={form.control}
          name="email"
          render={({ field }) => (
            <FormItem>
              <FormLabel>ایمیل</FormLabel>
              <FormControl>
                <Input type="email" dir="ltr" placeholder="you@example.com" {...field} />
              </FormControl>
              <FormMessage />
            </FormItem>
          )}
        />

        <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
          <FormField
            control={form.control}
            name="password"
            render={({ field }) => (
              <FormItem>
                <FormLabel>رمز عبور</FormLabel>
                <FormControl>
                  <Input type="password" dir="ltr" placeholder="حداقل ۶ کاراکتر" {...field} />
                </FormControl>
                <FormMessage />
              </FormItem>
            )}
          />

          <FormField
            control={form.control}
            name="confirmPassword"
            render={({ field }) => (
              <FormItem>
                <FormLabel>تکرار رمز عبور</FormLabel>
                <FormControl>
                  <Input type="password" dir="ltr" placeholder="تکرار رمز" {...field} />
                </FormControl>
                <FormMessage />
              </FormItem>
            )}
          />
        </div>

        <FormField
          control={form.control}
          name="userRole"
          render={({ field }) => (
            <FormItem>
              <FormLabel>نقش شما</FormLabel>
              <FormControl>
                <RoleSelector value={field.value} onChange={field.onChange} />
              </FormControl>
              <FormMessage />
            </FormItem>
          )}
        />

        {selectedRole === Role.JobSeeker && (
          <div className="space-y-2 rounded-xl border bg-muted/30 p-4">
            <label htmlFor="resumeFile" className="flex items-center gap-2 text-sm font-medium">
              <FileText className="h-4 w-4 text-primary" />
              فایل رزومه
              <span className="text-destructive">*</span>
            </label>

            <Input
              ref={resumeInputRef}
              id="resumeFile"
              type="file"
              dir="ltr"
              accept=".pdf,.docx,application/pdf,application/vnd.openxmlformats-officedocument.wordprocessingml.document"
              onChange={(event) => handleResumeChange(event.target.files?.[0] ?? null)}
            />

            <p className="text-xs text-muted-foreground">
              فقط PDF یا DOCX و حداکثر ۱۰ مگابایت. این فایل همراه با ثبت‌نام برای
              بررسی ادمین ارسال می‌شود.
            </p>

            {resumeFile && (
              <p className="flex items-center gap-2 text-xs text-emerald-600 dark:text-emerald-400">
                <Upload className="h-3.5 w-3.5" />
                {resumeFile.name} (
                {(resumeFile.size / 1024 / 1024).toLocaleString("fa-IR", {
                  maximumFractionDigits: 1
                })}{" "}
                مگابایت)
              </p>
            )}

            {resumeError && <p className="text-sm text-destructive">{resumeError}</p>}
          </div>
        )}

        <Button type="submit" className="w-full" disabled={submitting}>
          {submitting ? "در حال ثبت‌نام..." : "ثبت‌نام"}
        </Button>
      </form>
    </Form>
  );
}
