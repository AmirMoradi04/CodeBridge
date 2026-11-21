"use client";

import {
  useInfiniteQuery,
  useMutation,
  useQuery,
  useQueryClient
} from "@tanstack/react-query";
import { commentsApi } from "@/lib/api/comments";
import { likesApi } from "@/lib/api/likes";
import { postsApi } from "@/lib/api/posts";
import type { CreateCommentInput, CreatePostInput } from "@/types/post";

const FEED_PAGE_SIZE = 10;

export const postKeys = {
  all: ["posts"] as const,
  feed: () => ["posts", "feed"] as const,
  author: (authorId: number) => ["posts", "author", authorId] as const
};

export const commentKeys = {
  byPost: (postId: number) => ["comments", postId] as const
};

export const likeKeys = {
  count: (postId: number) => ["likes", "count", postId] as const,
  mine: (postId: number) => ["likes", "mine", postId] as const
};

export function useFeed() {
  return useInfiniteQuery({
    queryKey: postKeys.feed(),
    queryFn: ({ pageParam }) => postsApi.feed(pageParam, FEED_PAGE_SIZE),
    initialPageParam: 1,
    getNextPageParam: (lastPage) =>
      lastPage.page < lastPage.totalPages ? lastPage.page + 1 : undefined
  });
}

export function useAuthorPosts(authorId?: number) {
  return useQuery({
    queryKey: postKeys.author(authorId ?? 0),
    queryFn: () => postsApi.byAuthor(authorId as number),
    enabled: Boolean(authorId)
  });
}

export function useCreatePost() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (input: CreatePostInput) => postsApi.create(input),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: postKeys.all })
  });
}

export function usePublishPost() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (postId: number) => postsApi.publish(postId),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: postKeys.all })
  });
}

export function useDeletePost() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (postId: number) => postsApi.remove(postId),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: postKeys.all })
  });
}

// تعداد لایک و دیدگاه هر پست به‌صورت مستقل خوانده می‌شود
export function usePostEngagement(postId: number) {
  const likes = useQuery({
    queryKey: likeKeys.count(postId),
    queryFn: () => likesApi.count(postId)
  });

  const comments = useQuery({
    queryKey: commentKeys.byPost(postId),
    queryFn: () => commentsApi.byPost(postId)
  });

  return {
    likeCount: likes.data ?? 0,
    commentCount: comments.data?.length ?? 0,
    isLoading: likes.isLoading || comments.isLoading
  };
}

export function useComments(postId: number) {
  return useQuery({
    queryKey: commentKeys.byPost(postId),
    queryFn: () => commentsApi.byPost(postId)
  });
}

export function useAddComment(postId: number) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (input: CreateCommentInput) => commentsApi.create(input),
    onSuccess: () =>
      queryClient.invalidateQueries({ queryKey: commentKeys.byPost(postId) })
  });
}

export function useDeleteComment(postId: number) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (commentId: number) => commentsApi.remove(commentId),
    onSuccess: () =>
      queryClient.invalidateQueries({ queryKey: commentKeys.byPost(postId) })
  });
}

export function useHasLiked(postId: number, enabled: boolean) {
  return useQuery({
    queryKey: likeKeys.mine(postId),
    queryFn: () => likesApi.hasLiked(postId),
    enabled
  });
}

export function useToggleLike(postId: number) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: () => likesApi.toggle(postId),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: likeKeys.count(postId) });
      void queryClient.invalidateQueries({ queryKey: likeKeys.mine(postId) });
    }
  });
}
