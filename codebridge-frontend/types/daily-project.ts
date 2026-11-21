// مقادیر این enum دقیقاً مطابق وضعیت‌های DailyProject در بک‌اند است.
export const DailyProjectStatus = {
  Pending: 0,
  Approved: 1,
  Rejected: 2,
  Active: 3,
  Full: 4,
  Completed: 5,
  Cancelled: 6
} as const;
export type DailyProjectStatus = (typeof DailyProjectStatus)[keyof typeof DailyProjectStatus];

// DailyApplicationStatus enum (Pending=0, InProgress=1, Submitted=2, Approved=3, Rejected=4, Expired=5)
export const DailyApplicationStatus = {
  Pending: 0,
  InProgress: 1,
  Submitted: 2,
  Approved: 3,
  Rejected: 4,
  Expired: 5
} as const;
export type DailyApplicationStatus = (typeof DailyApplicationStatus)[keyof typeof DailyApplicationStatus];

export interface DailyProjectDto {
  id: number;
  employerId: number;
  employerName?: string | null;
  title: string;
  description?: string | null;
  totalBudget: number;
  pricePerUnit: number;
  capacity: number;
  filledSlots: number;
  deadlineSeconds: number;
  instructions?: string | null;
  proofRequired: boolean;
  status: DailyProjectStatus;
  isPaid: boolean;
  paidAmount: number;
  adminNote?: string | null;
  approvedAt?: string | null;
  createdAt: string;
}

export interface CreateDailyProjectInput {
  title: string;
  description?: string;
  totalBudget: number;
  pricePerUnit: number;
  capacity: number;
  deadlineSeconds: number;
  instructions?: string;
}

export interface DailyApplicationDto {
  id: number;
  dailyProjectId: number;
  developerId: number;
  developerName?: string | null;
  status: DailyApplicationStatus;
  startedAt?: string | null;
  submittedAt?: string | null;
  expiresAt?: string | null;
  proofAttachmentId?: number | null;
  adminNote?: string | null;
  reviewedAt?: string | null;
  createdAt: string;
}

export interface SubmitTaskInput {
  dailyProjectId: number;
  proofAttachmentId: number;
}

export interface StartTaskInput {
  dailyProjectId: number;
}