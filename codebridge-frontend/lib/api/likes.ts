import { api } from "@/lib/api/axios";

export const likesApi = {
  toggle: (postId: number) =>
    api.post(`/likes/post/${postId}`).then((response) => response.data),

  count: (postId: number) =>
    api
      .get<number>(`/likes/post/${postId}/count`)
      .then((response) => response.data),

  hasLiked: (postId: number) =>
    api
      .get<boolean>(`/likes/post/${postId}/me`)
      .then((response) => response.data)
};
