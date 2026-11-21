import { api } from "@/lib/api/axios";
import type {
  Conversation,
  CreateConversationInput,
  Message,
  SendMessageInput
} from "@/types/message";

export const messagingApi = {
  conversations: () =>
    api.get<Conversation[]>("/messaging/conversations").then((response) => response.data),

  conversation: (conversationId: number) =>
    api
      .get<Conversation>(`/messaging/conversations/${conversationId}`)
      .then((response) => response.data),

  create: (input: CreateConversationInput) =>
    api
      .post<Conversation>("/messaging/conversations", input)
      .then((response) => response.data),

  messages: (conversationId: number, page = 1, pageSize = 50) =>
    api
      .get<Message[]>(`/messaging/conversations/${conversationId}/messages`, {
        params: { page, pageSize }
      })
      .then((response) => response.data),

  markRead: (conversationId: number) =>
    api
      .post(`/messaging/conversations/${conversationId}/read`)
      .then((response) => response.data),

  send: (input: SendMessageInput) =>
    api.post<Message>("/messaging/messages", input).then((response) => response.data),

  addParticipant: (conversationId: number, userId: number) =>
    api
      .post<Conversation>(`/messaging/conversations/${conversationId}/participants`, null, {
        params: { userId }
      })
      .then((response) => response.data)
};
