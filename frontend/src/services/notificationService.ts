import axios from 'axios';
import type { Notification } from '@/features/notifications/types/notification.types';

const BASE = '/api/notifications';

export const notificationService = {
  getByUser: async (userId: string, page = 1, pageSize = 20): Promise<Notification[]> => {
    const { data } = await axios.get<Notification[]>(BASE, {
      params: { userId, page, pageSize },
    });
    return data;
  },

  getUnreadCount: async (userId: string): Promise<number> => {
    const { data } = await axios.get<number>(`${BASE}/unread-count`, {
      params: { userId },
    });
    return data;
  },

  markAsRead: async (id: string, userId: string): Promise<void> => {
    await axios.patch(`${BASE}/${id}/read`, null, { params: { userId } });
  },

  markAllAsRead: async (userId: string): Promise<void> => {
    await axios.patch(`${BASE}/read-all`, null, { params: { userId } });
  },

  delete: async (id: string, userId: string): Promise<void> => {
    await axios.delete(`${BASE}/${id}`, { params: { userId } });
  },

  deleteAllRead: async (userId: string): Promise<void> => {
    await axios.delete(`${BASE}/read`, { params: { userId } });
  },
};
