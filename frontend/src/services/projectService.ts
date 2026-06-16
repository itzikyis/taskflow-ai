import axios from 'axios';
import type {
  Project,
  CreateProjectPayload,
  UpdateProjectPayload,
} from '@/features/projects/types/project.types';

const BASE = '/api/projects';

export const projectService = {
  getAll: async (ownerId?: string): Promise<Project[]> => {
    const params = ownerId ? { ownerId } : {};
    const { data } = await axios.get<Project[]>(BASE, { params });
    return data;
  },

  getById: async (id: string): Promise<Project> => {
    const { data } = await axios.get<Project>(`${BASE}/${id}`);
    return data;
  },

  create: async (payload: CreateProjectPayload): Promise<string> => {
    const { data } = await axios.post<string>(BASE, payload);
    return data;
  },

  update: async (id: string, payload: UpdateProjectPayload): Promise<void> => {
    await axios.put(`${BASE}/${id}`, payload);
  },

  remove: async (id: string): Promise<void> => {
    await axios.delete(`${BASE}/${id}`);
  },
};
