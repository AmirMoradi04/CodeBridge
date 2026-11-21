import { api } from "@/lib/api/axios";
import type {
  CreatePostInput,
  PagedResult,
  Post
} from "@/types/post";

export const postsApi = {
  feed: (page = 1, pageSize = 10) =>
    api
      .get<PagedResult<Post>>("/posts/feed", { params: { page, pageSize } })
      .then((response) => response.data),

  byAuthor: (authorId: number) =>
    api
      .get<Post[]>(`/posts/author/${authorId}`)
      .then((response) => response.data),

  create: (input: CreatePostInput) =>
    api.post<Post>("/posts", input).then((response) => response.data),

  publish: (postId: number) =>
    api.patch(`/posts/${postId}/publish`).then((response) => response.data),

  remove: (postId: number) =>
    api.delete(`/posts/${postId}`).then((response) => response.data)
};
