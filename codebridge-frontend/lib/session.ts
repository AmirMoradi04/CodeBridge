import type { AuthResponse, AuthUser, Role } from "@/types/auth";

const ACCESS_KEY = "cb_access_token";
const REFRESH_KEY = "cb_refresh_token";
const ROLE_KEY = "cb_role";
const USER_KEY = "cb_user";

// کوکی‌ها فقط برای گارد مسیر در middleware هستند؛ اعتبارسنجی نهایی در بک‌اند است
export const TOKEN_COOKIE = "cb_token";
export const ROLE_COOKIE = "cb_role";

const COOKIE_MAX_AGE = 60 * 60 * 24 * 7;

const ROLE_NAMES: Record<Role, string> = {
  0: "Admin",
  2: "Employer",
  3: "JobSeeker"
};

const isBrowser = () => typeof window !== "undefined";

function read(key: string): string | null {
  if (!isBrowser()) return null;

  try {
    return window.localStorage.getItem(key);
  } catch {
    return null;
  }
}

function write(key: string, value: string): void {
  if (!isBrowser()) return;

  try {
    window.localStorage.setItem(key, value);
  } catch {
    // حالت private/ blocked storage — نادیده گرفته می‌شود
  }
}

function remove(key: string): void {
  if (!isBrowser()) return;

  try {
    window.localStorage.removeItem(key);
  } catch {
    // نادیده گرفته می‌شود
  }
}

function setCookie(name: string, value: string, maxAge: number): void {
  if (!isBrowser()) return;
  document.cookie = `${name}=${encodeURIComponent(value)}; path=/; max-age=${maxAge}; samesite=lax`;
}

function deleteCookie(name: string): void {
  if (!isBrowser()) return;
  document.cookie = `${name}=; path=/; max-age=0; samesite=lax`;
}

export const session = {
  getAccessToken: (): string | null => read(ACCESS_KEY),

  getRefreshToken: (): string | null => read(REFRESH_KEY),

  getRole(): Role | null {
    const raw = read(ROLE_KEY);
    if (raw === null) return null;

    const parsed = Number(raw);

    // نقش ۱ (Developer قدیمی) دیگر وجود ندارد؛ به‌عنوان نقش نامعتبر تلقی می‌شود
    return parsed === 0 || parsed === 2 || parsed === 3 ? (parsed as Role) : null;
  },

  getUser(): AuthUser | null {
    const raw = read(USER_KEY);
    if (!raw) return null;

    try {
      const parsed = JSON.parse(raw) as AuthUser;
      return typeof parsed?.id === "number" && typeof parsed?.role === "number"
        ? parsed
        : null;
    } catch {
      return null;
    }
  },

  save(auth: AuthResponse, user: AuthUser): void {
    write(ACCESS_KEY, auth.accessToken);
    write(REFRESH_KEY, auth.refreshToken);
    write(ROLE_KEY, String(user.role));
    write(USER_KEY, JSON.stringify(user));

    setCookie(TOKEN_COOKIE, auth.accessToken, COOKIE_MAX_AGE);
    setCookie(ROLE_COOKIE, ROLE_NAMES[user.role], COOKIE_MAX_AGE);
  },

  updateTokens(accessToken: string, refreshToken: string): void {
    write(ACCESS_KEY, accessToken);
    write(REFRESH_KEY, refreshToken);
    setCookie(TOKEN_COOKIE, accessToken, COOKIE_MAX_AGE);
  },

  setUser(user: AuthUser): void {
    write(USER_KEY, JSON.stringify(user));
    write(ROLE_KEY, String(user.role));
    setCookie(ROLE_COOKIE, ROLE_NAMES[user.role], COOKIE_MAX_AGE);
  },

  clear(): void {
    remove(ACCESS_KEY);
    remove(REFRESH_KEY);
    remove(ROLE_KEY);
    remove(USER_KEY);
    deleteCookie(TOKEN_COOKIE);
    deleteCookie(ROLE_COOKIE);
  }
};
