import { useState } from 'react';
import type { CSSProperties } from 'react';
import { useUnreadCount } from '../hooks/useNotifications';
import { NotificationDrawer } from './NotificationDrawer';

const wrapperStyle: CSSProperties = {
  position: 'relative',
  display: 'inline-flex',
  alignItems: 'center',
};

const bellBtnStyle: CSSProperties = {
  background: 'none',
  border: 'none',
  cursor: 'pointer',
  fontSize: 18,
  padding: '4px 6px',
  borderRadius: 6,
  color: 'var(--text-secondary)',
  lineHeight: 1,
  display: 'flex',
  alignItems: 'center',
  justifyContent: 'center',
  transition: 'background 0.1s',
};

const badgeStyle: CSSProperties = {
  position: 'absolute',
  top: -2,
  right: -4,
  background: 'var(--color-danger)',
  color: '#fff',
  borderRadius: '50%',
  minWidth: 18,
  height: 18,
  fontSize: 10,
  fontWeight: 700,
  display: 'flex',
  alignItems: 'center',
  justifyContent: 'center',
  lineHeight: 1,
  padding: '0 3px',
  pointerEvents: 'none',
};

export function NotificationBell() {
  const [open, setOpen] = useState(false);
  const { data: unreadCount = 0 } = useUnreadCount();

  return (
    <div style={wrapperStyle}>
      <button
        type="button"
        style={bellBtnStyle}
        onClick={() => setOpen((prev) => !prev)}
        aria-label={`Notifications${unreadCount > 0 ? ` (${unreadCount} unread)` : ''}`}
      >
        🔔
      </button>
      {unreadCount > 0 && (
        <span style={badgeStyle} aria-hidden="true">
          {unreadCount > 99 ? '99+' : unreadCount}
        </span>
      )}
      {open && <NotificationDrawer onClose={() => setOpen(false)} />}
    </div>
  );
}
