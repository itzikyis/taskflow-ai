import type { ChangesMap } from '../types/audit.types';

interface AuditDiffCellProps {
  changes: string | null;
}

function formatValue(v: unknown): string {
  if (v === null || v === undefined) return '—';
  if (typeof v === 'string') return `"${v}"`;
  return String(v);
}

export function AuditDiffCell({ changes }: AuditDiffCellProps) {
  if (!changes) {
    return <span style={{ color: 'var(--text-muted)' }}>—</span>;
  }

  let map: ChangesMap;
  try {
    map = JSON.parse(changes) as ChangesMap;
  } catch {
    return <span style={{ color: 'var(--text-muted)', fontStyle: 'italic' }}>invalid diff</span>;
  }

  const fields = Object.entries(map);
  if (fields.length === 0) {
    return <span style={{ color: 'var(--text-muted)' }}>—</span>;
  }

  return (
    <div style={{ display: 'flex', flexDirection: 'column', gap: 2 }}>
      {fields.map(([field, change]) => (
        <div key={field} style={{ fontSize: 12, lineHeight: 1.4 }}>
          <span style={{ color: 'var(--text-muted)', fontWeight: 500 }}>{field}: </span>
          <span style={{ color: 'var(--text-muted)' }}>{formatValue(change.from)}</span>
          <span style={{ color: 'var(--text-muted)', margin: '0 4px' }}>→</span>
          <span style={{ color: 'var(--color-primary)', fontWeight: 500 }}>{formatValue(change.to)}</span>
        </div>
      ))}
    </div>
  );
}
