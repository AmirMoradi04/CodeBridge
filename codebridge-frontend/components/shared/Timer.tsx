"use client";

import { useEffect, useState } from "react";

interface TimerProps {
  expiresAt: string | Date;
  onExpire?: () => void;
  className?: string;
}

function calculateTimeLeft(target: Date): number {
  return Math.max(0, Math.floor((target.getTime() - Date.now()) / 1000));
}

function formatTime(seconds: number): string {
  const h = Math.floor(seconds / 3600);
  const m = Math.floor((seconds % 3600) / 60);
  const s = seconds % 60;
  if (h > 0) return `${h}:${String(m).padStart(2, "0")}:${String(s).padStart(2, "0")}`;
  return `${String(m).padStart(2, "0")}:${String(s).padStart(2, "0")}`;
}

export function Timer({ expiresAt, onExpire, className }: TimerProps) {
  const targetDate = typeof expiresAt === "string" ? new Date(expiresAt) : expiresAt;
  const [seconds, setSeconds] = useState(() => calculateTimeLeft(targetDate));

  useEffect(() => {
    if (seconds <= 0) return;
    const interval = setInterval(() => {
      const remaining = calculateTimeLeft(targetDate);
      setSeconds(remaining);
      if (remaining <= 0) {
        clearInterval(interval);
        onExpire?.();
      }
    }, 1000);
    return () => clearInterval(interval);
  }, [targetDate, seconds, onExpire]);

  if (seconds <= 0) {
    return (
      <span className={`font-mono text-red-500 ${className ?? ""}`}>
        منقضی شد
      </span>
    );
  }

  return (
    <span className={`font-mono font-semibold text-orange-500 ${className ?? ""}`}>
      {formatTime(seconds)}
    </span>
  );
}