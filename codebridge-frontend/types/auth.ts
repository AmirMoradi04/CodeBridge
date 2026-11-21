// نقش‌ها دقیقاً با enum بک‌اند (عدد) هم‌خوان‌اند.
// نقش جداگانه‌ای برای برنامه‌نویس وجود ندارد؛ کارجو همان برنامه‌نویس است.
export const Role = {
  Admin: 0,
  Employer: 2,
  JobSeeker: 3
} as const;

export type Role = (typeof Role)[keyof typeof Role];

export interface AuthUser {
  id: number;
  email: string;
  role: Role;
  firstName?: string;
  lastName?: string;
}

export interface AuthResponse {
  accessToken: string;
  refreshToken: string;
  accessTokenExpiresAt: string;
  refreshTokenExpiresAt: string;
  userId: number;
  email: string;
  role: Role;
}

export interface LoginInput {
  email: string;
  password: string;
}

export interface RegisterInput {
  firstName: string;
  lastName: string;
  email: string;
  password: string;
  userRole: Role;
  /** فایل رزومه؛ برای نقش کارجو اجباری است. */
  resumeFile?: File;
  /** کد معرف دعوت‌کننده (اختیاری، از ?ref= خوانده می‌شود). */
  referralCode?: string;
}

export interface RefreshTokenInput {
  refreshToken: string;
}
