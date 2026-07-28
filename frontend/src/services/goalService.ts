import axios from 'axios';

const BASE = '/api/goals';

export type GoalStatus = 'OnTrack' | 'AtRisk' | 'OffTrack' | 'Completed';

export interface KeyResultDto {
  id: string;
  title: string;
  targetValue: number;
  currentValue: number;
  unit: string;
  progressPercent: number;
}

export interface GoalDto {
  id: string;
  projectId: string;
  title: string;
  description: string | null;
  status: GoalStatus;
  progressPercent: number;
  dueDate: string | null;
  keyResults: KeyResultDto[];
}

export interface CreateGoalPayload {
  projectId: string;
  ownerId: string;
  title: string;
  description?: string | null;
  dueDate?: string | null;
}

export interface UpdateGoalProgressPayload {
  progressPercent: number;
  status: GoalStatus;
}

export const goalService = {
  getByProject: async (projectId: string): Promise<GoalDto[]> => {
    const { data } = await axios.get<GoalDto[]>(`${BASE}/project/${projectId}`);
    return data;
  },

  create: async (payload: CreateGoalPayload): Promise<string> => {
    const { data } = await axios.post<string>(BASE, payload);
    return data;
  },

  updateProgress: async (id: string, payload: UpdateGoalProgressPayload): Promise<void> => {
    await axios.put(`${BASE}/${id}/progress`, payload);
  },

  remove: async (id: string): Promise<void> => {
    await axios.delete(`${BASE}/${id}`);
  },
};
