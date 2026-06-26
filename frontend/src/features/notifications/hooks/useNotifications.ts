import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { notificationService } from '@/services/notificationService';
import { useAuthStore } from '@/store/authStore';

const NOTIFICATIONS_KEY = 'notifications' as const;
const NOTIFICATIONS_COUNT_KEY = 'notifications-count' as const;

export function useNotifications() {
  const userId = useAuthStore((s) => s.token?.userId);
  return useQuery({
    queryKey: [NOTIFICATIONS_KEY, userId],
    queryFn: () => notificationService.getByUser(userId!, 1, 20),
    enabled: Boolean(userId),
    refetchInterval: 30_000,
  });
}

export function useUnreadCount() {
  const userId = useAuthStore((s) => s.token?.userId);
  return useQuery({
    queryKey: [NOTIFICATIONS_COUNT_KEY, userId],
    queryFn: () => notificationService.getUnreadCount(userId!),
    enabled: Boolean(userId),
    refetchInterval: 30_000,
  });
}

export function useMarkAsRead() {
  const queryClient = useQueryClient();
  const userId = useAuthStore((s) => s.token?.userId);
  return useMutation({
    mutationFn: (id: string) => notificationService.markAsRead(id, userId!),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: [NOTIFICATIONS_KEY, userId] });
      void queryClient.invalidateQueries({ queryKey: [NOTIFICATIONS_COUNT_KEY, userId] });
    },
  });
}

export function useMarkAllAsRead() {
  const queryClient = useQueryClient();
  const userId = useAuthStore((s) => s.token?.userId);
  return useMutation({
    mutationFn: () => notificationService.markAllAsRead(userId!),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: [NOTIFICATIONS_KEY, userId] });
      void queryClient.invalidateQueries({ queryKey: [NOTIFICATIONS_COUNT_KEY, userId] });
    },
  });
}

export function useDeleteNotification() {
  const queryClient = useQueryClient();
  const userId = useAuthStore((s) => s.token?.userId);
  return useMutation({
    mutationFn: (id: string) => notificationService.delete(id, userId!),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: [NOTIFICATIONS_KEY, userId] });
      void queryClient.invalidateQueries({ queryKey: [NOTIFICATIONS_COUNT_KEY, userId] });
    },
  });
}

export function useDeleteAllRead() {
  const queryClient = useQueryClient();
  const userId = useAuthStore((s) => s.token?.userId);
  return useMutation({
    mutationFn: () => notificationService.deleteAllRead(userId!),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: [NOTIFICATIONS_KEY, userId] });
      void queryClient.invalidateQueries({ queryKey: [NOTIFICATIONS_COUNT_KEY, userId] });
    },
  });
}
