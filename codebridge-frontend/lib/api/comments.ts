import { api } from "@/lib/api/axios";
import type { Comment, CreateCommentInput } from "@/types/post";

export const commentsApi = {
  byPost: (postId: number) =>
    api
      .get<Comment[]>(`/comments/post/${postId}`)
      .then((response) => response.data),

  create: (input: CreateCommentInput) =>
    api.post<Comment>("/comments", input).then((response) => response.data),

  remove: (commentId: number) =>
    api.delete(`/comments/${commentId}`).then((response) => response.data)
};
