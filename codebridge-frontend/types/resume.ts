// هم‌راستا با enum ResumeStatus در بک‌اند
export const ResumeStatus = {
  Pending: 0,
  Approved: 1,
  Rejected: 2
} as const;

export type ResumeStatus = (typeof ResumeStatus)[keyof typeof ResumeStatus];

export interface Resume {
  id: number;
  userId: number;
  title: string;
  summary: string;
  skills: string;
  attachmentId?: number | null;
  status: ResumeStatus;
  reviewedByAdminId?: number | null;
  reviewedAt?: string | null;
  createdAt: string;
}

export interface SaveResumeInput {
  title: string;
  summary: string;
  skills: string;
  attachmentId?: number | null;
}
