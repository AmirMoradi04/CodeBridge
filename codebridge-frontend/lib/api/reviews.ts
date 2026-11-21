import { api } from "@/lib/api/axios";
import type { CreateReviewInput, Review } from "@/types/review";

export const reviewsApi = {
  byUser: (targetUserId: number) =>
    api.get<Review[]>(`/reviews/user/${targetUserId}`).then((r) => r.data),

  byProject: (projectId: number) =>
    api.get<Review[]>(`/reviews/project/${projectId}`).then((r) => r.data),

  // فهرست کامل برای پنل ادمین
  all: () => api.get<Review[]>("/reviews").then((r) => r.data),

  create: (input: CreateReviewInput) =>
    api.post("/reviews", input).then((r) => r.data)
};
