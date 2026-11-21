export interface Post {
  id: number;
  content: string;
  authorId: number;
  authorName?: string | null;
  attachmentId?: number | null;
  isPublished: boolean;
  likesCount: number;
  commentsCount: number;
  createdAt: string;
}

export interface Comment {
  id: number;
  content: string;
  postId: number;
  authorId: number;
  authorName?: string | null;
  parentCommentId?: number | null;
  createdAt: string;
}

export interface PagedResult<T> {
  items: T[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
}

export interface CreatePostInput {
  content: string;
  attachmentId?: number | null;
}

export interface CreateCommentInput {
  postId: number;
  content: string;
  parentCommentId?: number | null;
}
