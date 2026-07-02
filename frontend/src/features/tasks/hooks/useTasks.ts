import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { taskService } from '@/services/taskService';
import { useAuthStore } from '@/store/authStore';
import type { CreateTaskPayload, UpdateTaskPayload, TaskStatus } from '../types/task.types';

const FALLBACK_ACTOR_ID = '00000000-0000-0000-0000-000000000000';

function getActorId(): string {
  return useAuthStore.getState().token?.userId ?? FALLBACK_ACTOR_ID;
}

const TASKS_KEY = 'tasks' as const;

export function useTasks(assignedToUserId?: string) {
  return useQuery({
    queryKey: [TASKS_KEY, assignedToUserId],
    queryFn: () => taskService.getAll(assignedToUserId),
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
    mutationFn: (payload: Omit<UpdateTaskPayload, 'actorId'>) =>
      taskService.update(id, { ...payload, actorId: getActorId() }),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: [TASKS_KEY] }),
  });
}

export function useMoveTaskToColumn() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ taskId, columnId }: { taskId: string; columnId: string | null }) =>
      taskService.moveToColumn(taskId, { columnId, actorId: getActorId() }),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: [TASKS_KEY] }),
  });
}

export function useUpdateTaskStatus(id: string) {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (status: TaskStatus) =>
      taskService.updateStatus(id, { status, actorId: getActorId() }),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: [TASKS_KEY] }),
  });
}

export function useDeleteTask() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => taskService.remove(id, { actorId: getActorId() }),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: [TASKS_KEY] }),
  });
}
