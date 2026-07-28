import axios from 'axios';

export const DEVELOPMENT_LINK_TYPES = ['PullRequest', 'Commit', 'Branch'] as const;
export type DevelopmentLinkType = (typeof DEVELOPMENT_LINK_TYPES)[number];

export const DEVELOPMENT_LINK_STATUSES = ['None', 'Open', 'Draft', 'Merged', 'Closed'] as const;
export type DevelopmentLinkStatus = (typeof DEVELOPMENT_LINK_STATUSES)[number];

export interface DevelopmentLink {
  id: string;
  taskId: string;
  linkType: DevelopmentLinkType;
  url: string;
  title: string;
  status: DevelopmentLinkStatus;
  createdAt: string;
}

export interface CreateDevelopmentLinkPayload {
  linkType: DevelopmentLinkType;
  url: string;
  title: string;
  status?: DevelopmentLinkStatus;
}

export const developmentLinkService = {
  getByTask: async (taskId: string): Promise<DevelopmentLink[]> => {
    const { data } = await axios.get<DevelopmentLink[]>(`/api/tasks/${taskId}/development-links`);
    return data;
  },

  create: async (taskId: string, payload: CreateDevelopmentLinkPayload): Promise<string> => {
    const { data } = await axios.post<string>(`/api/tasks/${taskId}/development-links`, payload);
    return data;
  },

  remove: async (taskId: string, linkId: string): Promise<void> => {
    await axios.delete(`/api/tasks/${taskId}/development-links/${linkId}`);
  },
};
