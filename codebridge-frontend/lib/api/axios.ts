import axios, {
  AxiosError,
  type InternalAxiosRequestConfig
} from "axios";
import { session } from "@/lib/session";
import type { ApiErrorBody } from "@/types/audit";
import type { AuthResponse } from "@/types/auth";

export const API_URL =
  process.env.NEXT_PUBLIC_API_URL ?? "https://localhost:7220/api";

export const api = axios.create({
  baseURL: API_URL,
  headers: { "Content-Type": "application/json" }
});

// افزودن توکن دسترسی به همه‌ی درخواست‌ها
api.interceptors.request.use((config) => {
  const token = session.getAccessToken();

  if (token) config.headers.Authorization = `Bearer ${token}`;

  return config;
});

type RetriableConfig = InternalAxiosRequestConfig & { _retry?: boolean };

let refreshPromise: Promise<string | null> | null = null;

// چرخش توکن؛ چند درخواست هم‌زمان یک بار refresh می‌کنند
async function refreshAccessToken(): Promise<string | null> {
  const refreshToken = session.getRefreshToken();

  if (!refreshToken) return null;

  try {
    const { data } = await axios.post<AuthResponse>(`${API_URL}/auth/refresh`, {
      refreshToken
    });

    session.updateTokens(data.accessToken, data.refreshToken);
    return data.accessToken;
  } catch {
    session.clear();
    return null;
  }
}

api.interceptors.response.use(
  (response) => response,
  async (error: AxiosError) => {
    const original = error.config as RetriableConfig | undefined;
    const isUnauthorized = error.response?.status === 401;

    if (isUnauthorized && original && !original._retry && session.getRefreshToken()) {
      original._retry = true;

      refreshPromise ??= refreshAccessToken().finally(() => {
        refreshPromise = null;
      });

      const token = await refreshPromise;

      if (token) {
        original.headers.Authorization = `Bearer ${token}`;
        return api.request(original);
      }

      if (typeof window !== "undefined") window.location.href = "/login";
    }

    return Promise.reject(error);
  }
);

// پیام خطای بک‌اند (Result یا ProblemDetails) را به متن قابل نمایش تبدیل می‌کند
export function getErrorMessage(
  error: unknown,
  fallback = "خطایی رخ داد؛ دوباره تلاش کنید."
): string {
  if (axios.isAxiosError(error)) {
    const data = error.response?.data as ApiErrorBody | undefined;

    if (data?.errors?.length) return data.errors.join("، ");
    if (data?.message) return data.message;
    if (data?.detail) return data.detail;
    if (data?.title) return data.title;
    if (error.code === "ERR_NETWORK") return "ارتباط با سرور برقرار نشد.";
  }

  if (error instanceof Error && error.message) return error.message;

  return fallback;
}
