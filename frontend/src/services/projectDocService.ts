import axios from 'axios';

export interface ProjectDocumentDto {
  id: string;
  projectId: string;
  title: string;
  body: string;
  authorId: string;
  createdAt: string;
  updatedAt: string;
}

export interface CreateProjectDocumentPayload {
  projectId: string;
  title: string;
  body: string;
  authorId: string;
}

const BASE = '/api/project-docs';

export const projectDocService = {
  getByProject: async (projectId: string): Promise<ProjectDocumentDto[]> => {
    const { data } = await axios.get<ProjectDocumentDto[]>(`${BASE}/projects/${projectId}`);
    return data;
  },

  create: async (payload: CreateProjectDocumentPayload): Promise<string> => {
    const { data } = await axios.post<string>(BASE, payload);
    return data;
  },

  update: async (id: string, title: string, body: string): Promise<void> => {
    await axios.put(`${BASE}/${id}`, { title, body });
  },

  delete: async (id: string): Promise<void> => {
    await axios.delete(`${BASE}/${id}`);
  },
};
