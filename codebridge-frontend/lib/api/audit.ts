import { api } from "@/lib/api/axios";
import type { AuditLog } from "@/types/audit";

export const auditApi = {
  recent: (count = 100) =>
    api
      .get<AuditLog[]>("/auditlogs/recent", { params: { count } })
      .then((response) => response.data),

  byUser: (userId: number) =>
    api.get<AuditLog[]>(`/auditlogs/user/${userId}`).then((response) => response.data),

  byEntity: (entityType: string, entityId: number) =>
    api
      .get<AuditLog[]>("/auditlogs/entity", { params: { entityType, entityId } })
      .then((response) => response.data)
};
