import axios from 'axios';
import type {
  Task,
  CreateTaskPayload,
  UpdateTaskPayload,
  UpdateStatusPayload,
  MoveToColumnPayload,
} from '@/features/tasks/types/task.types';

const BASE = '/api/tasks';

export const taskService = {
  getAll: async (assignedToUserId?: string): Promise<Task[]> => {
    const params = assignedToUserId ? { assignedToUserId } : {};
    const { data } = await axios.get<Task[]>(BASE, { params });
    return data;
  },

  getById: async (id: string): Promise<Task> => {
    const { data } = await axios.get<Task>(`${BASE}/${id}`);
    return data;
  },

  create: async (payload: CreateTaskPayload): Promise<string> => {
    const { data } = await axios.post<string>(BASE, payload);
    return data;
  },

  update: async (id: string, payload: UpdateTaskPayload): Promise<void> => {
    await axios.put(`${BASE}/${id}`, payload);
  },

  updateStatus: async (id: string, payload: UpdateStatusPayload): Promise<void> => {
    await axios.patch(`${BASE}/${id}/status`, payload);
  },

  moveToColumn: async (id: string, payload: MoveToColumnPayload): Promise<void> => {
    await axios.patch(`${BASE}/${id}/column`, payload);
  },

  remove: async (id: string): Promise<void> => {
    await axios.delete(`${BASE}/${id}`);
  },

  createSubtasks: async (
    id: string,
    subtasks: Array<{ title: string; description?: string }>,
  ): Promise<string[]> => {
    const { data } = await axios.post<string[]>(`${BASE}/${id}/subtasks`, { subtasks });
    return data;
  },

  search: async (query: string): Promise<TaskSearchResult> => {
    const { data } = await axios.post<TaskSearchResult>(`${BASE}/search`, { query });
    return data;
  },

  checkDuplicates: async (
    title: string,
    description?: string,
    excludeTaskId?: string,
  ): Promise<DuplicateMatch[]> => {
    const { data } = await axios.post<DuplicateMatch[]>(`${BASE}/check-duplicates`, {
      title,
      description,
      excludeTaskId,
    });
    return data;
  },

  assignAgent: async (taskId: string): Promise<string> => {
    const { data } = await axios.post<string>(`${BASE}/${taskId}/assign-agent`);
    return data;
  },
};

/** Well-known id of the AI Agent pseudo-user (matches the backend constant). */
export const AI_AGENT_ID = 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa';

export interface TaskSearchResult {
  interpretation: string;
  results: Task[];
}

export interface DuplicateMatch {
  taskId: string;
  title: string;
  score: number;
}
