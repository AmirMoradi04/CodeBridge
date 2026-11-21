"use client";

import { FileText } from "lucide-react";
import { ResumeForm } from "@/components/resumes/ResumeForm";

export default function JobSeekerResumePage() {
  return (
    <div className="mx-auto max-w-3xl space-y-6">
      <div className="flex items-center gap-2">
        <FileText className="h-5 w-5 text-primary" />
        <div>
          <h2 className="text-lg font-semibold">رزومه‌ی من</h2>
          <p className="text-sm text-muted-foreground">
            رزومه را کامل کنید تا برای ادمین و کارفرمایان قابل مشاهده باشد.
          </p>
        </div>
      </div>

      <ResumeForm />
    </div>
  );
}