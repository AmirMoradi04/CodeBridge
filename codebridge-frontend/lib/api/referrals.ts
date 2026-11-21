import { api } from "@/lib/api/axios";
import type { ReferralInfo } from "@/types/referral";

export const referralsApi = {
  myInfo: () => api.get<ReferralInfo>("/referrals/my-info").then((r) => r.data)
};
