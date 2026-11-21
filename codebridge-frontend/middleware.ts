import { NextResponse, type NextRequest } from "next/server";

const ROLE_HOME: Record<string, string> = {
  Admin: "/admin",
  Employer: "/employer",
  JobSeeker: "/jobseeker"
};

const PROTECTED: { prefix: string; role: string }[] = [
  { prefix: "/admin", role: "Admin" },
  { prefix: "/employer", role: "Employer" },
  { prefix: "/jobseeker", role: "JobSeeker" }
];

/**
 * گارد مسیر: کوکی‌های نقش/توکن فقط سمت کلاینت برای هدایت استفاده می‌شوند
 * و اعتبار واقعی دسترسی را بک‌اند در هر endpoint بررسی می‌کند.
 */
export function middleware(request: NextRequest) {
  const { pathname } = request.nextUrl;
  const token = request.cookies.get("cb_token")?.value;
  const role = request.cookies.get("cb_role")?.value;

  const target = PROTECTED.find(
    (entry) => pathname === entry.prefix || pathname.startsWith(`${entry.prefix}/`)
  );

  if (!target) return NextResponse.next();

  // بدون نشست: هدایت به ورود و بازگشت به همان مسیر پس از ورود
  if (!token || !role) {
    const url = request.nextUrl.clone();
    url.pathname = "/login";
    url.search = "";
    url.searchParams.set("next", pathname);
    return NextResponse.redirect(url);
  }

  // نقش متفاوت: هدایت به پنل خود کاربر
  if (role !== target.role) {
    const home = ROLE_HOME[role];

    if (home) {
      const url = request.nextUrl.clone();
      url.pathname = home;
      url.search = "";
      return NextResponse.redirect(url);
    }

    return NextResponse.redirect(new URL("/login", request.url));
  }

  return NextResponse.next();
}

export const config = {
  matcher: ["/admin/:path*", "/employer/:path*", "/jobseeker/:path*"]
};