export interface AuditLog {
  id: number;
  userId?: number | null;
  action: string;
  entityType: string;
  entityId?: number | null;
  details?: string | null;
  ipAddress?: string | null;
  createdAt: string;
}

export interface ApiErrorBody {
  isSuccess?: boolean;
  message?: string;
  errors?: string[] | null;
  title?: string;
  detail?: string;
  status?: number;
}
