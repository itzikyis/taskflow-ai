import type { ActivityLog } from '../types/activity.types';
import { ActivityItem } from './ActivityItem';

interface Props {
  logs: ActivityLog[];
  isLoading: boolean;
  title?: string;
}

export function ActivityFeed({ logs, isLoading, title = 'Activity' }: Props) {
  return (
    <div>
      <div style={{ display: 'flex', alignItems: 'center', gap: 8, marginBottom: 12 }}>
        <span style={{ fontWeight: 600, fontSize: 14, color: 'var(--text-primary)' }}>
          {title}
        </span>
        {!isLoading && (
          <span className="tf-badge" style={{ background: 'var(--surface-bg)', color: 'var(--text-secondary)' }}>
            {logs.length}
          </span>
        )}
      </div>

      <div style={{ maxHeight: 400, overflowY: 'auto' }}>
        {isLoading ? (
          <>
            {[0, 1, 2].map(i => (
              <div
                key={i}
                className="skeleton"
                style={{ height: 36, marginBottom: 8, borderRadius: 'var(--radius-sm)' }}
              />
            ))}
          </>
        ) : logs.length === 0 ? (
          <div className="empty-state">
            <div className="empty-state-icon">🕐</div>
            <div className="empty-state-text">No activity yet</div>
          </div>
        ) : (
          logs.map(log => <ActivityItem key={log.id} log={log} />)
        )}
      </div>
    </div>
  );
}
