import axios from 'axios';
import type {
  DevelopmentLink,
  CreateDevelopmentLinkPayload,
} from '@/features/development/types/development.types';

export const developmentService = {
  getByTask: async (taskId: string): Promise<DevelopmentLink[]> => {
    const { data } = await axios.get<DevelopmentLink[]>(`/api/tasks/${taskId}/development`);
    return data;
  },

  link: async (taskId: string, payload: CreateDevelopmentLinkPayload): Promise<string> => {
    const { data } = await axios.post<string>(`/api/tasks/${taskId}/development`, payload);
    return data;
  },

  remove: async (linkId: string): Promise<void> => {
    await axios.delete(`/api/development/${linkId}`);
  },
};
