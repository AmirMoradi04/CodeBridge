// هم‌راستا با enum LiveSessionStatus در بک‌اند
export const LiveSessionStatus = {
  Scheduled: 0,
  Live: 1,
  Ended: 2,
  Cancelled: 3
} as const;

export type LiveSessionStatus =
  (typeof LiveSessionStatus)[keyof typeof LiveSessionStatus];

export interface LiveSession {
  id: number;
  projectId: number;
  projectTitle?: string | null;
  seniorId: number;
  seniorName?: string | null;
  title: string;
  description: string;
  price: number;
  scheduledAt: string;
  durationMinutes: number;
  /** فقط برای برگزارکننده، ادمین و کارجوی پرداخت‌کننده پر می‌شود. */
  meetingLink?: string | null;
  recordingLink?: string | null;
  status: LiveSessionStatus;
  participantCount: number;
  totalRevenue: number;
  isJoined: boolean;
  createdAt: string;
}

export interface CreateLiveSessionInput {
  projectId: number;
  title: string;
  description: string;
  price: number;
  scheduledAt: string;
  durationMinutes: number;
  meetingLink: string;
}

export interface JoinLiveSessionResponse {
  isSuccess: boolean;
  meetingLink: string | null;
}
