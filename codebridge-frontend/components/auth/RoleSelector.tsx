"use client";

import { Briefcase, GraduationCap } from "lucide-react";
import { cn } from "@/lib/utils";
import { ROLE_LABELS } from "@/lib/constants";
import { Role } from "@/types/auth";

const ROLE_ICONS: Record<number, React.ElementType> = {
  [Role.JobSeeker]: GraduationCap,
  [Role.Employer]: Briefcase
};

// فقط کارجو و کارفرما می‌توانند ثبت‌نام کنند
const ROLE_OPTIONS: { value: Role; hint: string }[] = [
  { value: Role.JobSeeker, hint: "رزومه بسازم و پروژه پیدا کنم" },
  { value: Role.Employer, hint: "پروژه ثبت کنم و تیم بسازم" }
];

/** انتخاب نقش هنگام ثبت‌نام؛ ادمین و برنامه‌نویس از اینجا قابل ساختن نیستند. */
export function RoleSelector({
  value,
  onChange
}: {
  value: number;
  onChange: (value: number) => void;
}) {
  return (
    <div className="grid grid-cols-1 gap-3 sm:grid-cols-2">
      {ROLE_OPTIONS.map((option) => {
        const Icon = ROLE_ICONS[option.value];
        const selected = value === option.value;

        return (
          <button
            key={option.value}
            type="button"
            onClick={() => onChange(option.value)}
            className={cn(
              "flex flex-col items-center gap-2 rounded-xl border p-4 text-center transition-colors",
              selected
                ? "border-primary bg-primary/5 text-primary ring-2 ring-primary"
                : "border-input bg-background hover:bg-accent"
            )}
          >
            <Icon className="h-6 w-6" />
            <span className="font-semibold">{ROLE_LABELS[option.value]}</span>
            <span className="text-xs text-muted-foreground">{option.hint}</span>
          </button>
        );
      })}
    </div>
  );
}
