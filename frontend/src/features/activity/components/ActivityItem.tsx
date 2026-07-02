import type { ActivityLog } from '../types/activity.types';
import { ACTION_ICONS, ACTION_LABELS } from '../types/activity.types';

interface Props {
  log: ActivityLog;
}

const ACTION_COLOR: Record<string, string> = {
  Created: '#d1fae5',
  Updated: '#dbeafe',
  StatusChanged: '#dbeafe',
  MovedToColumn: '#dbeafe',
  Deleted: '#fee2e2',
  CommentDeleted: '#fee2e2',
  MemberRemoved: '#fee2e2',
  Assigned: '#ede9fe',
  MemberAdded: '#ede9fe',
  RoleChanged: '#ede9fe',
  CommentAdded: '#f1f5f9',
};

function relativeTime(iso: string): string {
  const diffMs = Date.now() - new Date(iso).getTime();
  const diffSec = Math.floor(diffMs / 1000);
  if (diffSec < 60) return 'just now';
  const diffMin = Math.floor(diffSec / 60);
  if (diffMin < 60) return `${diffMin}m ago`;
  const diffHr = Math.floor(diffMin / 60);
  if (diffHr < 24) return `${diffHr}h ago`;
  return `${Math.floor(diffHr / 24)}d ago`;
}

export function ActivityItem({ log }: Props) {
  const icon = ACTION_ICONS[log.action] ?? '•';
  const label = ACTION_LABELS[log.action] ?? log.action;
  const bgColor = ACTION_COLOR[log.action] ?? '#f1f5f9';
  const displayName = log.entityName ?? `${log.entityId.slice(0, 8)}…`;
  const actorSnippet = `${log.actorId.slice(0, 8)}…`;

  return (
    <div className="activity-item">
      <div className="activity-icon" style={{ background: bgColor }}>
        {icon}
      </div>
      <span
        style={{
          fontFamily: 'monospace',
          fontSize: 11,
          color: 'var(--text-muted)',
          flexShrink: 0,
        }}
      >
        {actorSnippet}
      </span>
      <span style={{ flex: 1, color: 'var(--text-primary)' }}>
        {label} {log.entityType}{' '}
        <span style={{ fontStyle: 'italic', color: 'var(--text-secondary)' }}>
          &quot;{displayName}&quot;
        </span>
      </span>
      <span
        style={{
          fontSize: 11,
          color: 'var(--text-muted)',
          flexShrink: 0,
          whiteSpace: 'nowrap',
        }}
      >
        {relativeTime(log.occurredAt)}
      </span>
    </div>
  );
}
