import type { CSSProperties } from 'react';
import type { Notification } from '../types/notification.types';
import { NOTIFICATION_TYPE_ICONS } from '../types/notification.types';

interface NotificationItemProps {
  notification: Notification;
  onMarkRead: () => void;
  onDelete: () => void;
}

function formatRelativeTime(isoString: string): string {
  const now = Date.now();
  const then = new Date(isoString).getTime();
  const diffMs = now - then;
  const diffMins = Math.floor(diffMs / 60_000);
  const diffHours = Math.floor(diffMs / 3_600_000);
  const diffDays = Math.floor(diffMs / 86_400_000);

  if (diffMins < 1) return 'Just now';
  if (diffMins < 60) return `${diffMins}m ago`;
  if (diffHours < 24) return `${diffHours}h ago`;
  if (diffDays === 1) return 'Yesterday';
  return new Date(isoString).toLocaleDateString();
}

const styles = {
  icon: {
    fontSize: 20,
    lineHeight: 1,
    flexShrink: 0,
    marginTop: 1,
  } satisfies CSSProperties,
  center: {
    flex: 1,
    minWidth: 0,
  } satisfies CSSProperties,
  title: {
    fontSize: 13,
    fontWeight: 600,
    color: 'var(--text-primary)',
    marginBottom: 2,
  } satisfies CSSProperties,
  message: {
    fontSize: 12,
    color: 'var(--text-secondary)',
    overflow: 'hidden',
    textOverflow: 'ellipsis',
    whiteSpace: 'nowrap' as const,
    marginBottom: 3,
  } satisfies CSSProperties,
  timestamp: {
    fontSize: 11,
    color: 'var(--text-muted)',
  } satisfies CSSProperties,
  actions: {
    display: 'flex',
    alignItems: 'center',
    gap: 4,
    flexShrink: 0,
  } satisfies CSSProperties,
  actionBtn: {
    background: 'none',
    border: 'none',
    cursor: 'pointer',
    padding: '2px 6px',
    borderRadius: 4,
    fontSize: 13,
    lineHeight: 1,
    color: 'var(--text-muted)',
    transition: 'background 0.1s, color 0.1s',
  } satisfies CSSProperties,
};

export function NotificationItem({ notification, onMarkRead, onDelete }: NotificationItemProps) {
  const icon = NOTIFICATION_TYPE_ICONS[notification.type];

  return (
    <div className={`notification-item${notification.isRead ? '' : ' unread'}`}>
      <span style={styles.icon}>{icon}</span>

      <div style={styles.center}>
        <div style={styles.title}>{notification.title}</div>
        <div style={styles.message}>{notification.message}</div>
        <div style={styles.timestamp}>{formatRelativeTime(notification.createdAt)}</div>
      </div>

      <div style={styles.actions}>
        {!notification.isRead && (
          <button
            type="button"
            style={styles.actionBtn}
            title="Mark as read"
            onClick={onMarkRead}
            aria-label="Mark as read"
          >
            ✓
          </button>
        )}
        <button
          type="button"
          style={{ ...styles.actionBtn, color: 'var(--color-danger)' }}
          title="Delete"
          onClick={onDelete}
          aria-label="Delete notification"
        >
          ×
        </button>
      </div>
    </div>
  );
}
