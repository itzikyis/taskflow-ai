export const TASK_PRIORITIES = ['Low', 'Medium', 'High', 'Critical'] as const;
export const TASK_STATUSES = ['Todo', 'InProgress', 'Done'] as const;

export type TaskPriority = (typeof TASK_PRIORITIES)[number];
export type TaskStatus = (typeof TASK_STATUSES)[number];

export interface Task {
  id: string;
  title: string;
  description: string | null;
  status: TaskStatus;
  priority: TaskPriority;
  dueDate: string | null;
  assignedToUserId: string | null;
  createdByUserId: string;
  createdAt: string;
  updatedAt: string | null;
}

export interface CreateTaskPayload {
  title: string;
  description?: string;
  priority: TaskPriority;
  createdByUserId: string;
}

export interface UpdateTaskPayload {
  title: string;
  description?: string;
}
