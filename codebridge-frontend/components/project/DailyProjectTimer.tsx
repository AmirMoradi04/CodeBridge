"use client";

import { useEffect, useState } from "react";
import { formatDistanceToNow, isPast } from "date-fns";
import { Clock } from "lucide-react";

interface DailyProjectTimerProps {
  /** ISO date string or Date — the moment the task expires */
  expiresAt: string | Date;
  /** Called when the countdown reaches zero */
  onExpire?: () => void;
  className?: string;
}

/** Formats seconds into H:MM:SS or M:SS display */
function formatDuration(totalSeconds: number): string {
  if (totalSeconds <= 0) return "00:00";
  const h = Math.floor(totalSeconds / 3600);
  const m = Math.floor((totalSeconds % 3600) / 60);
  const s = totalSeconds % 60;
  if (h > 0) {
    return `${h}:${String(m).padStart(2, "0")}:${String(s).padStart(2, "0")}`;
  }
  return `${String(m).padStart(2, "0")}:${String(s).padStart(2, "0")}`;
}

export function DailyProjectTimer({
  expiresAt,
  onExpire,
  className
}: DailyProjectTimerProps) {
  const targetDate = typeof expiresAt === "string" ? new Date(expiresAt) : expiresAt;

  const [secondsLeft, setSecondsLeft] = useState<number>(() => {
    const diff = targetDate.getTime() - Date.now();
    return Math.max(0, Math.floor(diff / 1000));
  });

  useEffect(() => {
    // Recalculate if expiresAt prop changes (e.g. new task started)
    const diff = targetDate.getTime() - Date.now();
    setSecondsLeft(Math.max(0, Math.floor(diff / 1000)));
  }, [targetDate]);

  useEffect(() => {
    if (secondsLeft <= 0) {
      onExpire?.();
      return;
    }

    const interval = setInterval(() => {
      const diff = targetDate.getTime() - Date.now();
      const remaining = Math.max(0, Math.floor(diff / 1000));
      setSecondsLeft(remaining);

      if (remaining <= 0) {
        clearInterval(interval);
        onExpire?.();
      }
    }, 1000);

    return () => clearInterval(interval);
  }, [targetDate, secondsLeft, onExpire]);

  const expired = secondsLeft <= 0 || isPast(targetDate);

  if (expired) {
    return (
      <span
        className={`inline-flex items-center gap-1 font-mono text-xs text-red-500 ${className ?? ""}`}
        title={formatDistanceToNow(targetDate, { addSuffix: true })}
      >
        <Clock className="h-3 w-3" />
        منقضی شد
      </span>
    );
  }

  return (
    <span
      className={`inline-flex items-center gap-1 font-mono text-sm font-semibold text-orange-500 ${className ?? ""}`}
      title={`منقضی ${formatDistanceToNow(targetDate, { addSuffix: true })}`}
    >
      <Clock className="h-3 w-3" />
      {formatDuration(secondsLeft)}
    </span>
  );
}
