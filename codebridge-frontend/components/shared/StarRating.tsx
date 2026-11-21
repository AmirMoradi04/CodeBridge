"use client";

import { Star } from "lucide-react";
import { cn } from "@/lib/utils";

interface StarRatingProps {
  /** امتیاز انتخاب‌شده یا میانگین نمایشی (می‌تواند اعشاری باشد). */
  value: number;
  max?: number;
  /** اگر تعریف شود، ستاره‌ها قابل انتخاب می‌شوند. */
  onChange?: (value: number) => void;
  readOnly?: boolean;
  size?: "sm" | "md";
  showValue?: boolean;
  className?: string;
  id?: string;
}

const SIZE_CLASSES = {
  sm: "h-4 w-4",
  md: "h-5 w-5"
};

/** ستاره‌ی خالی به‌عنوان پس‌زمینه + نسخه‌ی پرشده با عرض دلخواه برای نیم‌ستاره. */
function StarGlyph({ filled, percent, size }: { filled: boolean; percent: number; size: string }) {
  return (
    <span className="relative inline-block">
      <Star className={cn(size, filled ? "text-amber-400" : "text-muted-foreground/40")} />
      {percent > 0 && (
        <span
          className="absolute inset-0 overflow-hidden"
          style={{ width: `${percent}%` }}
          aria-hidden
        >
          <Star className={cn(size, "fill-amber-400 text-amber-400")} />
        </span>
      )}
    </span>
  );
}

export function StarRating({
  value,
  max = 5,
  onChange,
  readOnly = false,
  size = "md",
  showValue = false,
  className,
  id
}: StarRatingProps) {
  const interactive = !readOnly && typeof onChange === "function";
  const sizeClass = SIZE_CLASSES[size];

  return (
    <div
      id={id}
      className={cn("inline-flex items-center gap-1", className)}
      role={interactive ? "radiogroup" : undefined}
      aria-label={interactive ? "انتخاب امتیاز" : undefined}
    >
      {Array.from({ length: max }).map((_, index) => {
        const starValue = index + 1;
        // برای امتیاد اعشاری، هر ستاره به نسبت پر می‌شود
        const percent = Math.max(0, Math.min(1, value - index)) * 100;

        if (interactive) {
          return (
            <button
              key={starValue}
              type="button"
              role="radio"
              aria-checked={Math.round(value) === starValue}
              aria-label={`${starValue} از ${max}`}
              className="rounded p-0.5 transition-transform hover:scale-110"
              onClick={() => onChange?.(starValue)}
            >
              <StarGlyph filled={value >= starValue - 0.25} percent={percent} size={sizeClass} />
            </button>
          );
        }

        return (
          <span key={starValue} aria-hidden>
            <StarGlyph filled={value >= starValue - 0.25} percent={percent} size={sizeClass} />
          </span>
        );
      })}

      {showValue && (
        <span className="ms-1 text-sm font-medium tabular-nums" dir="ltr">
          {value > 0 ? value.toFixed(value % 1 === 0 ? 0 : 1) : "بدون امتیاز"}
        </span>
      )}
    </div>
  );
}
