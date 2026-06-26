import type { CSSProperties } from 'react';
import { NotificationItem } from './NotificationItem';
import {
  useNotifications,
  useMarkAsRead,
  useMarkAllAsRead,
  useDeleteNotification,
  useDeleteAllRead,
} from '../hooks/useNotifications';

interface NotificationDrawerProps {
  onClose: () => void;
}

const drawerStyle: CSSProperties = {
  position: 'fixed',
  top: 0,
  right: 0,
  height: '100vh',
  width: 380,
  zIndex: 1000,
  background: 'var(--surface-card)',
  borderLeft: '1px solid var(--surface-border)',
  boxShadow: '-4px 0 24px rgba(0,0,0,0.12)',
  display: 'flex',
  flexDirection: 'column',
};

const backdropStyle: CSSProperties = {
  position: 'fixed',
  inset: 0,
  background: 'rgba(0,0,0,0.2)',
  zIndex: 999,
};

const headerStyle: CSSProperties = {
  padding: 16,
  display: 'flex',
  alignItems: 'center',
  justifyContent: 'space-between',
  borderBottom: '1px solid var(--surface-border)',
  flexShrink: 0,
};

const titleStyle: CSSProperties = {
  fontSize: 16,
  fontWeight: 700,
  color: 'var(--text-primary)',
  display: 'flex',
  alignItems: 'center',
  gap: 8,
};

const closeBtnStyle: CSSProperties = {
  background: 'none',
  border: 'none',
  cursor: 'pointer',
  fontSize: 18,
  color: 'var(--text-muted)',
  padding: '2px 6px',
  borderRadius: 4,
  lineHeight: 1,
};

const actionsRowStyle: CSSProperties = {
  padding: '8px 16px',
  display: 'flex',
  gap: 8,
  borderBottom: '1px solid var(--surface-border)',
  flexShrink: 0,
};

const listStyle: CSSProperties = {
  flex: 1,
  overflowY: 'auto',
};

const emptyStyle: CSSProperties = {
  display: 'flex',
  flexDirection: 'column',
  alignItems: 'center',
  justifyContent: 'center',
  height: '100%',
  color: 'var(--text-muted)',
  gap: 12,
  padding: 32,
};

export function NotificationDrawer({ onClose }: NotificationDrawerProps) {
  const { data: notifications, isLoading } = useNotifications();
  const markAsRead = useMarkAsRead();
  const markAllAsRead = useMarkAllAsRead();
  const deleteNotification = useDeleteNotification();
  const deleteAllRead = useDeleteAllRead();

  const items = notifications ?? [];
  const unreadCount = items.filter((n) => !n.isRead).length;
  const readCount = items.filter((n) => n.isRead).length;

  return (
    <>
      <div style={backdropStyle} onClick={onClose} aria-hidden="true" />
      <div style={drawerStyle} role="dialog" aria-label="Notifications">
        <div style={headerStyle}>
          <div style={titleStyle}>
            <span>🔔</span>
            <span>Notifications</span>
          </div>
          <button
            type="button"
            style={closeBtnStyle}
            onClick={onClose}
            aria-label="Close notifications"
          >
            ✕
          </button>
        </div>

        <div style={actionsRowStyle}>
          <button
            type="button"
            className="tf-btn tf-btn-ghost tf-btn-sm"
            disabled={unreadCount === 0}
            onClick={() => markAllAsRead.mutate()}
          >
            Mark all read
          </button>
          <button
            type="button"
            className="tf-btn tf-btn-danger tf-btn-sm"
            disabled={readCount === 0}
            onClick={() => deleteAllRead.mutate()}
          >
            Delete read
          </button>
        </div>

        <div style={listStyle}>
          {isLoading ? (
            <div style={emptyStyle}>
              <span style={{ fontSize: 13 }}>Loading...</span>
            </div>
          ) : items.length === 0 ? (
            <div style={emptyStyle}>
              <span style={{ fontSize: 36 }}>🔔</span>
              <span style={{ fontSize: 14 }}>You're all caught up!</span>
            </div>
          ) : (
            items.map((notification) => (
              <NotificationItem
                key={notification.id}
                notification={notification}
                onMarkRead={() => markAsRead.mutate(notification.id)}
                onDelete={() => deleteNotification.mutate(notification.id)}
              />
            ))
          )}
        </div>
      </div>
    </>
  );
}
