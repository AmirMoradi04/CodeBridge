"use client";

import { useRouter } from "next/navigation";
import { PlusCircle } from "lucide-react";
import { ProjectForm } from "@/components/projects/ProjectForm";

export default function NewProjectPage() {
  const router = useRouter();

  return (
    <div className="mx-auto max-w-2xl space-y-6">
      <div className="flex items-center gap-2">
        <PlusCircle className="h-5 w-5 text-primary" />
        <div>
          <h2 className="text-lg font-semibold">ثبت پروژه‌ی جدید</h2>
          <p className="text-sm text-muted-foreground">
            پروژه پس از ثبت، در انتظار تایید ادمین قرار می‌گیرد.
          </p>
        </div>
      </div>

      <div className="rounded-2xl border bg-card p-6 shadow-sm">
        <ProjectForm onSuccess={() => router.push("/employer")} />
      </div>
    </div>
  );
}