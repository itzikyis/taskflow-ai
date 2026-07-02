import { useState } from 'react';
import { useRecentAudit } from '../hooks/useAudit';
import { AuditDiffCell } from './AuditDiffCell';
import type { AuditEntry } from '../types/audit.types';

const ENTITY_TYPES = ['All', 'Task', 'Board', 'Project', 'Team'] as const;
type EntityTypeFilter = (typeof ENTITY_TYPES)[number];

const PAGE_SIZE = 50;

const ACTION_COLORS: Record<string, string> = {
  Created: 'var(--color-success, #22c55e)',
  Updated: 'var(--color-primary)',
  Deleted: '#f87171',
};

function formatDate(iso: string): string {
  return new Date(iso).toLocaleString(undefined, {
    year: 'numeric',
    month: 'short',
    day: 'numeric',
    hour: '2-digit',
    minute: '2-digit',
  });
}

function ActionBadge({ action }: { action: string }) {
  const color = ACTION_COLORS[action] ?? 'var(--text-muted)';
  return (
    <span
      style={{
        display: 'inline-block',
        padding: '1px 8px',
        borderRadius: 999,
        fontSize: 11,
        fontWeight: 600,
        letterSpacing: 0.3,
        border: `1px solid ${color}`,
        color,
      }}
    >
      {action}
    </span>
  );
}

const TABLE_STYLES: React.CSSProperties = {
  width: '100%',
  borderCollapse: 'collapse',
  fontSize: 13,
};

const TH_STYLES: React.CSSProperties = {
  textAlign: 'left',
  padding: '8px 12px',
  fontWeight: 600,
  fontSize: 11,
  textTransform: 'uppercase',
  letterSpacing: 0.5,
  color: 'var(--text-muted)',
  borderBottom: '1px solid var(--border-color)',
  whiteSpace: 'nowrap',
};

const TD_STYLES: React.CSSProperties = {
  padding: '10px 12px',
  borderBottom: '1px solid var(--border-color)',
  verticalAlign: 'top',
  color: 'var(--text-primary)',
};

export function AuditPage() {
  const [entityFilter, setEntityFilter] = useState<EntityTypeFilter>('All');
  const [page, setPage] = useState(1);

  const { data, isLoading } = useRecentAudit(page);
  const allEntries: AuditEntry[] = data ?? [];

  const filtered =
    entityFilter === 'All' ? allEntries : allEntries.filter(e => e.entityType === entityFilter);

  const canLoadMore = allEntries.length === PAGE_SIZE;

  return (
    <div>
      <div className="page-header">
        <h1 className="page-title">Audit Trail</h1>
      </div>

      {/* Filters */}
      <div style={{ display: 'flex', gap: 12, marginBottom: 20 }}>
        <select
          className="tf-input"
          style={{ width: 160 }}
          value={entityFilter}
          onChange={e => {
            setEntityFilter(e.target.value as EntityTypeFilter);
            setPage(1);
          }}
        >
          {ENTITY_TYPES.map(et => (
            <option key={et} value={et}>
              {et === 'All' ? 'All entities' : et}
            </option>
          ))}
        </select>
      </div>

      {/* Table */}
      <div className="tf-card" style={{ padding: 0, overflow: 'hidden' }}>
        {isLoading ? (
          <div style={{ padding: 16 }}>
            {[0, 1, 2, 3, 4].map(i => (
              <div
                key={i}
                className="skeleton"
                style={{ height: 36, marginBottom: 8, borderRadius: 'var(--radius-sm)' }}
              />
            ))}
          </div>
        ) : filtered.length === 0 ? (
          <div className="empty-state">
            <div className="empty-state-icon">🛡️</div>
            <div className="empty-state-text">No audit entries found</div>
          </div>
        ) : (
          <div style={{ overflowX: 'auto' }}>
            <table style={TABLE_STYLES}>
              <thead>
                <tr>
                  <th style={TH_STYLES}>Date</th>
                  <th style={TH_STYLES}>Actor</th>
                  <th style={TH_STYLES}>Entity</th>
                  <th style={TH_STYLES}>Action</th>
                  <th style={{ ...TH_STYLES, width: '40%' }}>Changes</th>
                </tr>
              </thead>
              <tbody>
                {filtered.map(entry => (
                  <tr key={entry.id}>
                    <td style={{ ...TD_STYLES, whiteSpace: 'nowrap', color: 'var(--text-secondary)' }}>
                      {formatDate(entry.occurredAt)}
                    </td>
                    <td style={{ ...TD_STYLES, fontFamily: 'var(--font-mono, monospace)', fontSize: 11, color: 'var(--text-secondary)' }}>
                      {entry.actorId}
                    </td>
                    <td style={TD_STYLES}>
                      <span style={{ fontWeight: 500 }}>{entry.entityType}</span>
                      <span style={{ color: 'var(--text-muted)', fontSize: 11, display: 'block' }}>
                        {entry.entityId}
                      </span>
                    </td>
                    <td style={TD_STYLES}>
                      <ActionBadge action={entry.action} />
                    </td>
                    <td style={TD_STYLES}>
                      <AuditDiffCell changes={entry.changes} />
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </div>

      {/* Pagination */}
      {!isLoading && (
        <div style={{ display: 'flex', gap: 8, marginTop: 16, justifyContent: 'center', alignItems: 'center' }}>
          {page > 1 && (
            <button
              type="button"
              className="tf-btn tf-btn-ghost tf-btn-sm"
              onClick={() => setPage(p => p - 1)}
            >
              ← Previous
            </button>
          )}
          <span style={{ fontSize: 12, color: 'var(--text-muted)' }}>Page {page}</span>
          {canLoadMore && (
            <button
              type="button"
              className="tf-btn tf-btn-ghost tf-btn-sm"
              onClick={() => setPage(p => p + 1)}
            >
              Next →
            </button>
          )}
        </div>
      )}
    </div>
  );
}
