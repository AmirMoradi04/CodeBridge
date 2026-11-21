// هم‌راستا با DTO های بک‌اند (CodeBridge.Service.DTOs.Referral)

export interface ReferredUser {
  userId: number;
  firstName: string;
  lastName: string;
  email?: string | null;
  isApproved: boolean;
  joinedAt: string;
}

export interface ReferralInfo {
  referralCode?: string | null;
  rewardAmount: number;
  totalEarned: number;
  referredUsers: ReferredUser[];
}
