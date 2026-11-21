// هم‌راستا با DTO های بک‌اند (CodeBridge.Service.DTOs.Review)

export interface Review {
  id: number;
  projectId: number;
  projectTitle?: string | null;
  contractId: number;
  reviewerId: number;
  reviewerName?: string | null;
  targetUserId: number;
  targetUserName?: string | null;
  rating: number;
  comment?: string | null;
  createdAt: string;
}

export interface CreateReviewInput {
  projectId: number;
  targetUserId: number;
  rating: number;
  comment?: string | null;
}
