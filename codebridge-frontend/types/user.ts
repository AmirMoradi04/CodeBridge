import type { Role } from "@/types/auth";
import type { Resume } from "@/types/resume";

export interface User {
  id: number;
  firstName: string;
  lastName: string;
  email: string;
  userRole: Role;
  isApproved: boolean;
  isActive: boolean;
  bio?: string | null;
  /** آدرس دانلود آواتار (در صورت وجود). */
  profilePictureUrl?: string | null;
  /** آدرس دانلود لوگوی شرکت (در صورت وجود). */
  companyLogoUrl?: string | null;
  /** سطح برنامه‌نویسی کارجو (از بک‌اند برای فهرست‌ها پر می‌شود). */
  level?: number | null;
  /** امتیاز برنامه‌نویسی کارجو. */
  score?: number | null;
  createdAt: string;
}

export interface DeveloperLevel {
  id: number;
  userId: number;
  level: number;
  score: number;
  projectsCompleted: number;
  averageRating?: number | null;
}

export interface UserProfile extends User {
  resume?: Resume | null;
  developerLevel?: DeveloperLevel | null;
}

export interface UpdateProfileInput {
  firstName: string;
  lastName: string;
  bio?: string | null;
  /** شناسه‌ی فایل پیوست آواتار. */
  profilePictureAttachmentId?: number;
  /** شناسه‌ی فایل پیوست لوگوی شرکت. */
  companyLogoAttachmentId?: number;
}

export interface Attachment {
  id: number;
  fileName: string;
  filePath: string;
  contentType: string;
  fileSize: number;
  ownerId: number;
  relatedEntityType: string;
  relatedEntityId?: number | null;
  createdAt: string;
}
