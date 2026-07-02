export const ACTIVITY_ACTIONS = [
  'Created',
  'Updated',
  'Deleted',
  'StatusChanged',
  'Assigned',
  'MovedToColumn',
  'CommentAdded',
  'CommentDeleted',
  'MemberAdded',
  'MemberRemoved',
  'RoleChanged',
] as const;

export type ActivityAction = (typeof ACTIVITY_ACTIONS)[number];

export interface ActivityLog {
  id: string;
  actorId: string;
  action: ActivityAction;
  entityType: string;
  entityId: string;
  entityName: string | null;
  projectId: string | null;
  metadata: string | null;
  occurredAt: string;
}

export const ACTION_ICONS: Record<ActivityAction, string> = {
  Created: '🆕',
  Updated: '✏️',
  Deleted: '🗑️',
  StatusChanged: '🔄',
  Assigned: '👤',
  MovedToColumn: '📋',
  CommentAdded: '💬',
  CommentDeleted: '🗨️',
  MemberAdded: '👥',
  MemberRemoved: '👋',
  RoleChanged: '🔑',
};

export const ACTION_LABELS: Record<ActivityAction, string> = {
  Created: 'created',
  Updated: 'updated',
  Deleted: 'deleted',
  StatusChanged: 'changed status of',
  Assigned: 'assigned',
  MovedToColumn: 'moved',
  CommentAdded: 'commented on',
  CommentDeleted: 'deleted comment on',
  MemberAdded: 'added member to',
  MemberRemoved: 'removed member from',
  RoleChanged: 'changed role in',
};
