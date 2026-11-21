// هم‌راستا با AchievementType بک‌اند
export const AchievementType = {
  FirstProject: 0,
  FiveStarHero: 1,
  TopEarner: 2
} as const;

export type AchievementType =
  (typeof AchievementType)[keyof typeof AchievementType];

export interface Achievement {
  id: number;
  title: string;
  description: string;
  iconUrl?: string | null;
  type: AchievementType;
  earnedAt: string;
}
