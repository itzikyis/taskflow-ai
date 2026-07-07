import axios from 'axios';
import type { ActivityLog } from '@/features/activity/types/activity.types';
import type { ActivityAction } from '@/features/activity/types/activity.types';

const BASE = '/api/activity';

// The actor is derived server-side from the authenticated caller's JWT, so it is
// intentionally not part of this payload.
export interface LogActivityPayload {
  action: ActivityAction;
  entityType: string;
  entityId: string;
  entityName?: string;
  projectId?: string;
  metadata?: string;
}

export const activityService = {
  getRecent: async (page = 1, pageSize = 50): Promise<ActivityLog[]> => {
    const { data } = await axios.get<ActivityLog[]>(BASE, { params: { page, pageSize } });
    return data;
  },

  getByEntity: async (
    entityType: string,
    entityId: string,
    page = 1,
    pageSize = 30,
  ): Promise<ActivityLog[]> => {
    const { data } = await axios.get<ActivityLog[]>(`${BASE}/entity/${entityType}/${entityId}`, {
      params: { page, pageSize },
    });
    return data;
  },

  getByProject: async (projectId: string, page = 1, pageSize = 50): Promise<ActivityLog[]> => {
    const { data } = await axios.get<ActivityLog[]>(`${BASE}/project/${projectId}`, {
      params: { page, pageSize },
    });
    return data;
  },

  getByActor: async (actorId: string, page = 1, pageSize = 30): Promise<ActivityLog[]> => {
    const { data } = await axios.get<ActivityLog[]>(`${BASE}/actor/${actorId}`, {
      params: { page, pageSize },
    });
    return data;
  },

  log: async (payload: LogActivityPayload): Promise<string> => {
    const { data } = await axios.post<string>(BASE, payload);
    return data;
  },
};
