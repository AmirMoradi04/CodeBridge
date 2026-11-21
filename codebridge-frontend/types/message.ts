// هم‌راستا با enum ConversationType در بک‌اند
export const ConversationType = {
  AdminUser: 0,
  ProjectGroup: 1
} as const;

export type ConversationType =
  (typeof ConversationType)[keyof typeof ConversationType];

export interface ConversationParticipant {
  userId: number;
  displayName: string;
  email: string;
  joinedAt: string;
}

export interface Conversation {
  id: number;
  type: ConversationType;
  title: string;
  projectId?: number | null;
  participants: ConversationParticipant[];
  /** تعداد پیام‌های خوانده‌نشده برای کاربر جاری. */
  unreadCount: number;
  createdAt: string;
}

export interface Message {
  id: number;
  conversationId: number;
  senderId: number;
  senderName: string;
  content: string;
  createdAt: string;
}

export interface CreateConversationInput {
  type: ConversationType;
  userId?: number | null;
  projectId?: number | null;
}

export interface SendMessageInput {
  conversationId: number;
  content: string;
}
