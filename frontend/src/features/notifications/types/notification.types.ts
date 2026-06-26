export const NOTIFICATION_TYPES = [
  'TaskAssigned',
  'TaskStatusChanged',
  'CommentAdded',
  'DueDateApproaching',
  'TeamMemberAdded',
  'General',
] as const;

export type NotificationType = (typeof NOTIFICATION_TYPES)[number];

export interface Notification {
  id: string;
  userId: string;
  title: string;
  message: string;
  type: NotificationType;
  isRead: boolean;
  relatedEntityId: string | null;
  createdAt: string;
}

export const NOTIFICATION_TYPE_ICONS: Record<NotificationType, string> = {
  TaskAssigned:       '👤',
  TaskStatusChanged:  '🔄',
  CommentAdded:       '💬',
  DueDateApproaching: '⚠️',
  TeamMemberAdded:    '👥',
  General:            '🔔',
};
