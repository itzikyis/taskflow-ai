import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { taskService } from '@/services/taskService';
import type { CreateTaskPayload, UpdateTaskPayload, TaskStatus } from '../types/task.types';

const TASKS_KEY = 'tasks' as const;

export function useTasks(assignedToUserId?: string) {
  return useQuery({
    queryKey: [TASKS_KEY, assignedToUserId],
    queryFn: () => taskService.getAll(assignedToUserId),
  });
}

export function useAllTasks() {
  return useQuery({
    queryKey: [TASKS_KEY],
    queryFn: () => taskService.getAll(),
  });
}

export function useTask(id: string) {
  return useQuery({
    queryKey: [TASKS_KEY, id],
    queryFn: () => taskService.getById(id),
    enabled: Boolean(id),
  });
}

export function useCreateTask() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (payload: CreateTaskPayload) => taskService.create(payload),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: [TASKS_KEY] }),
  });
}

export function useUpdateTask(id: string) {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (payload: UpdateTaskPayload) => taskService.update(id, payload),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: [TASKS_KEY] }),
  });
}

export function useMoveTaskToColumn() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ taskId, columnId }: { taskId: string; columnId: string | null }) =>
      taskService.moveToColumn(taskId, { columnId }),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: [TASKS_KEY] }),
  });
}

export function useUpdateTaskStatus(id: string) {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (status: TaskStatus) =>
      taskService.updateStatus(id, { status }),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: [TASKS_KEY] }),
  });
}

export function useDeleteTask() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => taskService.remove(id),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: [TASKS_KEY] }),
  });
}

export function useCreateSubtasks(parentId: string) {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (subtasks: Array<{ title: string; description?: string }>) =>
      taskService.createSubtasks(parentId, subtasks),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: [TASKS_KEY] }),
  });
}

export function useTaskSearch() {
  return useMutation({
    mutationFn: (query: string) => taskService.search(query),
  });
}
