"use client";

import { useQuery } from "@tanstack/react-query";
import { referralsApi } from "@/lib/api/referrals";

export const referralKeys = {
  myInfo: ["referrals", "my-info"] as const
};

/** اطلاعات پنل معرف کاربر جاری. */
export function useMyReferralInfo() {
  return useQuery({
    queryKey: referralKeys.myInfo,
    queryFn: referralsApi.myInfo
  });
}
