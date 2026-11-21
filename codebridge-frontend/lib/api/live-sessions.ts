import { api } from "@/lib/api/axios";
import type {
  CreateLiveSessionInput,
  JoinLiveSessionResponse,
  LiveSession
} from "@/types/live-session";

export const liveSessionsApi = {
  create: (input: CreateLiveSessionInput) =>
    api.post<LiveSession>("/live-sessions", input).then((response) => response.data),

  mine: () =>
    api
      .get<LiveSession[]>("/live-sessions/mine")
      .then((response) => response.data),

  available: () =>
    api
      .get<LiveSession[]>("/live-sessions/available")
      .then((response) => response.data),

  admin: () =>
    api
      .get<LiveSession[]>("/live-sessions/admin")
      .then((response) => response.data),

  byId: (sessionId: number) =>
    api
      .get<LiveSession>(`/live-sessions/${sessionId}`)
      .then((response) => response.data),

  join: (sessionId: number) =>
    api
      .post<JoinLiveSessionResponse>(`/live-sessions/${sessionId}/join`)
      .then((response) => response.data),

  end: (sessionId: number) =>
    api
      .post(`/live-sessions/${sessionId}/end`)
      .then((response) => response.data),

  setRecording: (sessionId: number, recordingLink: string) =>
    api
      .patch(`/live-sessions/${sessionId}/recording`, { recordingLink })
      .then((response) => response.data)
};
