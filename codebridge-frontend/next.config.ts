import type { NextConfig } from "next";

const nextConfig: NextConfig = {
  eslint: {
    // بررسی تایپ‌ها با tsc انجام می‌شود؛ lint در بیلد اجرا نمی‌شود
    ignoreDuringBuilds: true
  },
  images: {
    unoptimized: true
  }
};

export default nextConfig;
