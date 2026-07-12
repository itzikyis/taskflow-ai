import { useState } from 'react';
import { useTaskTime, useLogTime, useDeleteTimeEntry } from '../hooks/useTimeTracking';
import { formatMinutes } from '@/services/timeService';
import { useAuthStore } from '@/store/authStore';

interface TimePanelProps {
  taskId: string;
}

export function TimePanel({ taskId }: TimePanelProps) {
  const { data, isLoading } = useTaskTime(taskId);
  const logTime = useLogTime(taskId);
  const removeEntry = useDeleteTimeEntry(taskId);
  const { token } = useAuthStore();

  const [hours, setHours] = useState('');
  const [mins, setMins] = useState('');
  const [note, setNote] = useState('');

  const handleLog = (e: React.FormEvent) => {
    e.preventDefault();
    const total = (parseInt(hours || '0', 10) * 60) + parseInt(mins || '0', 10);
    if (!Number.isFinite(total) || total <= 0) return;
    logTime.mutate(
      { minutes: total, note: note.trim() || undefined },
      { onSuccess: () => { setHours(''); setMins(''); setNote(''); } },
    );
  };

  return (
    <>
      <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', marginBottom: 8 }}>
        <p style={{ fontSize: 12, fontWeight: 600, color: 'var(--text-secondary)', textTransform: 'uppercase', letterSpacing: '0.04em', margin: 0 }}>
          ⏱️ Time tracking
        </p>
        <span style={{ fontSize: 13, fontWeight: 700, color: 'var(--color-primary)' }}>
          {formatMinutes(data?.totalMinutes ?? 0)} total
        </span>
      </div>

      {isLoading && <p style={{ fontSize: 12, color: 'var(--text-muted)' }}>Loading…</p>}

      <div style={{ display: 'flex', flexDirection: 'column', gap: 4, marginBottom: 10 }}>
        {data?.entries.map(entry => (
          <div key={entry.id} style={{ display: 'flex', alignItems: 'center', gap: 8, fontSize: 12 }}>
            <span style={{ fontWeight: 700, color: 'var(--text-primary)', minWidth: 56 }}>
              {formatMinutes(entry.minutes)}
            </span>
            <span style={{ flex: 1, color: 'var(--text-secondary)', overflow: 'hidden', textOverflow: 'ellipsis', whiteSpace: 'nowrap' }}>
              {entry.note || <em style={{ color: 'var(--text-muted)' }}>no note</em>}
            </span>
            <span style={{ color: 'var(--text-muted)', fontSize: 11 }}>
              {new Date(entry.loggedAt).toLocaleDateString(undefined, { month: 'short', day: 'numeric' })}
            </span>
            {token?.userId === entry.userId && (
              <button
                type="button"
                onClick={() => removeEntry.mutate(entry.id)}
                aria-label="Delete entry"
                style={{ background: 'none', border: 'none', cursor: 'pointer', color: 'var(--color-danger)', fontSize: 13, lineHeight: 1 }}
              >
                ×
              </button>
            )}
          </div>
        ))}
      </div>

      <form onSubmit={handleLog} style={{ display: 'flex', gap: 6, alignItems: 'center' }}>
        <input
          className="tf-input" type="number" min="0" value={hours}
          onChange={e => setHours(e.target.value)} placeholder="h"
          style={{ width: 52, fontSize: 13 }}
        />
        <input
          className="tf-input" type="number" min="0" max="59" value={mins}
          onChange={e => setMins(e.target.value)} placeholder="m"
          style={{ width: 52, fontSize: 13 }}
        />
        <input
          className="tf-input" value={note}
          onChange={e => setNote(e.target.value)} placeholder="Note (optional)"
          style={{ flex: 1, fontSize: 13 }}
        />
        <button type="submit" className="tf-btn tf-btn-primary tf-btn-sm" disabled={logTime.isPending}>
          {logTime.isPending ? '…' : 'Log'}
        </button>
      </form>
    </>
  );
}
