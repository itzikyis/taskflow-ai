import axios from 'axios';

export interface DependencyTask {
  dependencyId: string;
  taskId: string;
  title: string;
  status: string;
}

export interface TaskDependencies {
  taskId: string;
  blockedBy: DependencyTask[];
  blocks: DependencyTask[];
}

export interface DependencyEdge {
  id: string;
  taskId: string;
  blockedByTaskId: string;
}

export const dependencyService = {
  getForTask: async (taskId: string): Promise<TaskDependencies> => {
    const { data } = await axios.get<TaskDependencies>(`/api/tasks/${taskId}/dependencies`);
    return data;
  },
  getAll: async (): Promise<DependencyEdge[]> => {
    const { data } = await axios.get<DependencyEdge[]>('/api/dependencies');
    return data;
  },
  add: async (taskId: string, blockedByTaskId: string): Promise<string> => {
    const { data } = await axios.post<string>(`/api/tasks/${taskId}/dependencies`, { blockedByTaskId });
    return data;
  },
  remove: async (dependencyId: string): Promise<void> => {
    await axios.delete(`/api/dependencies/${dependencyId}`);
  },
};
