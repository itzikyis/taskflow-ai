import axios from 'axios';

export type InitiativeStatus = 'Proposed' | 'Active' | 'Completed' | 'Canceled';
export type InitiativePriority = 'Low' | 'Medium' | 'High' | 'Urgent';

export interface InitiativeDto {
  id: string;
  name: string;
  description: string;
  status: InitiativeStatus;
  priority: InitiativePriority;
  labels: string;
  startDate: string | null;
  targetDate: string | null;
  createdByUserId: string;
  createdAt: string;
  projectIds: string[];
  totalTasks: number;
  completedTasks: number;
}

export interface CreateInitiativePayload {
  name: string;
  description: string;
  priority: InitiativePriority;
  labels: string;
  startDate: string | null;
  targetDate: string | null;
  createdByUserId: string;
}

const BASE = '/api/initiatives';

export const initiativeService = {
  getRoadmap: async (): Promise<InitiativeDto[]> => {
    const { data } = await axios.get<InitiativeDto[]>(BASE);
    return data;
  },

  create: async (payload: CreateInitiativePayload): Promise<string> => {
    const { data } = await axios.post<string>(BASE, payload);
    return data;
  },

  updateStatus: async (id: string, status: InitiativeStatus): Promise<void> => {
    await axios.patch(`${BASE}/${id}/status`, { status });
  },

  addProject: async (id: string, projectId: string): Promise<void> => {
    await axios.post(`${BASE}/${id}/projects/${projectId}`);
  },

  delete: async (id: string): Promise<void> => {
    await axios.delete(`${BASE}/${id}`);
  },
};
