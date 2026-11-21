import type { Metadata } from "next";
import "./globals.css";
import { Providers } from "@/components/providers";

export const metadata: Metadata = {
  title: {
    default: "CodeBridge | اتصال کارفرما و برنامه‌نویس",
    template: "%s | CodeBridge"
  },
  description:
    "پلتفرم CodeBridge برای ثبت پروژه، ساخت تیم، انتشار پست و گفت‌وگوی کارفرما و برنامه‌نویسان."
};

export default function RootLayout({
  children
}: Readonly<{ children: React.ReactNode }>) {
  return (
    <html lang="fa" dir="rtl" suppressHydrationWarning>
      <body className="min-h-screen bg-background font-sans text-foreground antialiased">
        <Providers>{children}</Providers>
      </body>
    </html>
  );
}