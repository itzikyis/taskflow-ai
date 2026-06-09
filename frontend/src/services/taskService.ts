import axios from 'axios';
import type {
  Task,
  CreateTaskPayload,
  UpdateTaskPayload,
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

  remove: async (id: string): Promise<void> => {
    await axios.delete(`${BASE}/${id}`);
  },
};
