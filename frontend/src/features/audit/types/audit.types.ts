export interface AuditEntry {
  id: string;
  actorId: string;
  entityType: string;
  entityId: string;
  action: string;
  changes: string | null;
  occurredAt: string;
}

export interface FieldChange {
  from: unknown;
  to: unknown;
}

export type ChangesMap = Record<string, FieldChange>;
