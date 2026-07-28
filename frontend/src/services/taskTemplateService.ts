import axios from 'axios';

const BASE = '/api/task-templates';

export interface TaskTemplate {
  id: string;
  projectId: string;
  name: string;
  defaultTitle: string;
  defaultDescription?: string;
  defaultPriority?: string;
  defaultEstimatedHours?: number;
}

export interface CreateTaskTemplatePayload {
  projectId: string;
  name: string;
  defaultTitle: string;
  defaultDescription?: string;
  defaultPriority?: string;
  defaultEstimatedHours?: number;
}

export const taskTemplateService = {
  getByProject: async (projectId: string): Promise<TaskTemplate[]> => {
    const { data } = await axios.get<TaskTemplate[]>(`${BASE}/project/${projectId}`);
    return data;
  },

  create: async (payload: CreateTaskTemplatePayload): Promise<string> => {
    const { data } = await axios.post<string>(BASE, payload);
    return data;
  },

  remove: async (id: string): Promise<void> => {
    await axios.delete(`${BASE}/${id}`);
  },

  createTask: async (templateId: string): Promise<string> => {
    const { data } = await axios.post<string>(`${BASE}/${templateId}/create-task`);
    return data;
  },
};

export const setTaskRecurrence = async (
  taskId: string,
  pattern: string,
  endDate?: string,
): Promise<void> => {
  await axios.patch(`/api/tasks/${taskId}/recurrence`, { pattern, endDate });
};
