import axios from 'axios';
import type { AuditEntry } from '@/features/audit/types/audit.types';

export const auditService = {
  getRecent: async (page: number, pageSize = 50): Promise<AuditEntry[]> => {
    const { data } = await axios.get<AuditEntry[]>('/api/audit/recent', {
      params: { page, pageSize },
    });
    return data;
  },

  getByEntity: async (entityType: string, entityId: string): Promise<AuditEntry[]> => {
    const { data } = await axios.get<AuditEntry[]>(
      `/api/audit/entity/${entityType}/${entityId}`,
    );
    return data;
  },

  getByActor: async (actorId: string, page: number, pageSize = 30): Promise<AuditEntry[]> => {
    const { data } = await axios.get<AuditEntry[]>(`/api/audit/actor/${actorId}`, {
      params: { page, pageSize },
    });
    return data;
  },
};
