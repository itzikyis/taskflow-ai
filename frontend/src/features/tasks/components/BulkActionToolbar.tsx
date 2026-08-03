import { useState } from 'react';
import { useQueryClient } from '@tanstack/react-query';
import { taskService } from '@/services/taskService';
import { TASK_STATUSES, TASK_PRIORITIES } from '../types/task.types';
import type { TaskStatus, TaskPriority } from '../types/task.types';

const STATUS_LABEL: Record<TaskStatus, string> = {
  Todo: 'To Do',
  InProgress: 'In Progress',
  InReview: 'In Review',
  Done: 'Done',
};

interface BulkActionToolbarProps {
  selectedIds: Set<string>;
  onClearSelection: () => void;
}

export function BulkActionToolbar({ selectedIds, onClearSelection }: BulkActionToolbarProps) {
  const queryClient = useQueryClient();
  const [busy, setBusy] = useState(false);
  const [showDeleteConfirm, setShowDeleteConfirm] = useState(false);
  const count = selectedIds.size;

  if (count === 0) return null;

  const invalidate = () => queryClient.invalidateQueries({ queryKey: ['tasks'] });

  const handleBulkStatus = async (status: TaskStatus) => {
    setBusy(true);
    try {
      await Promise.all([...selectedIds].map(id => taskService.updateStatus(id, { status })));
      await invalidate();
      onClearSelection();
    } finally {
      setBusy(false);
    }
  };

  const handleBulkPriority = async (priority: TaskPriority) => {
    setBusy(true);
    try {
      await Promise.all([...selectedIds].map(id => taskService.updatePriority(id, { priority })));
      await invalidate();
      onClearSelection();
    } finally {
      setBusy(false);
    }
  };

  const handleBulkDelete = async () => {
    setBusy(true);
    setShowDeleteConfirm(false);
    try {
      await Promise.all([...selectedIds].map(id => taskService.remove(id)));
      await invalidate();
      onClearSelection();
    } finally {
      setBusy(false);
    }
  };

  const toolbarStyle: React.CSSProperties = {
    position: 'fixed',
    bottom: 24,
    left: '50%',
    transform: 'translateX(-50%)',
    zIndex: 100,
    display: 'flex',
    alignItems: 'center',
    gap: 10,
    padding: '10px 16px',
    background: 'var(--surface-card)',
    border: '1px solid var(--surface-border)',
    borderRadius: 'var(--radius-md)',
    boxShadow: '0 4px 24px rgba(0,0,0,0.18)',
    flexWrap: 'wrap',
    maxWidth: '90vw',
  };

  const sepStyle: React.CSSProperties = {
    width: 1,
    alignSelf: 'stretch',
    background: 'var(--surface-border)',
    margin: '0 2px',
  };

  return (
    <>
      <div style={toolbarStyle} role="toolbar" aria-label="Bulk actions">
        {/* Count + clear */}
        <span style={{ fontSize: 13, fontWeight: 600, color: 'var(--text-primary)', whiteSpace: 'nowrap' }}>
          {count} task{count !== 1 ? 's' : ''} selected
        </span>
        <button
          type="button"
          onClick={onClearSelection}
          aria-label="Clear selection"
          style={{
            background: 'none', border: 'none', cursor: 'pointer',
            color: 'var(--text-muted)', fontSize: 16, lineHeight: 1, padding: '0 2px',
          }}
        >
          ×
        </button>

        <div style={sepStyle} />

        {/* Status */}
        <label style={{ display: 'flex', alignItems: 'center', gap: 6, fontSize: 12, color: 'var(--text-primary)' }}>
          Status
          <select
            className="tf-input"
            disabled={busy}
            defaultValue=""
            style={{ fontSize: 12, padding: '2px 6px' }}
            onChange={e => {
              const v = e.target.value as TaskStatus;
              if (v) { e.target.value = ''; handleBulkStatus(v); }
            }}
          >
            <option value="" disabled>Set status…</option>
            {TASK_STATUSES.map(s => (
              <option key={s} value={s}>{STATUS_LABEL[s]}</option>
            ))}
          </select>
        </label>

        {/* Priority */}
        <label style={{ display: 'flex', alignItems: 'center', gap: 6, fontSize: 12, color: 'var(--text-primary)' }}>
          Priority
          <select
            className="tf-input"
            disabled={busy}
            defaultValue=""
            style={{ fontSize: 12, padding: '2px 6px' }}
            onChange={e => {
              const v = e.target.value as TaskPriority;
              if (v) { e.target.value = ''; handleBulkPriority(v); }
            }}
          >
            <option value="" disabled>Set priority…</option>
            {TASK_PRIORITIES.map(p => (
              <option key={p} value={p}>{p}</option>
            ))}
          </select>
        </label>

        <div style={sepStyle} />

        {/* Delete */}
        <button
          type="button"
          className="tf-btn tf-btn-sm"
          disabled={busy}
          onClick={() => setShowDeleteConfirm(true)}
          style={{
            color: 'var(--color-danger)',
            borderColor: 'var(--color-danger)',
            background: 'none',
          }}
        >
          {busy ? 'Working…' : 'Delete'}
        </button>
      </div>

      {/* Delete confirmation dialog */}
      {showDeleteConfirm && (
        <div
          role="dialog"
          aria-modal="true"
          aria-labelledby="bulk-delete-title"
          style={{
            position: 'fixed', inset: 0, zIndex: 200,
            display: 'flex', alignItems: 'center', justifyContent: 'center',
            background: 'rgba(0,0,0,0.4)',
          }}
          onClick={e => { if (e.target === e.currentTarget) setShowDeleteConfirm(false); }}
        >
          <div style={{
            background: 'var(--surface-card)',
            border: '1px solid var(--surface-border)',
            borderRadius: 'var(--radius-md)',
            padding: '24px 28px',
            maxWidth: 380,
            width: '90vw',
          }}>
            <p id="bulk-delete-title" style={{ fontWeight: 700, fontSize: 16, marginBottom: 10, color: 'var(--text-primary)' }}>
              Delete {count} task{count !== 1 ? 's' : ''}?
            </p>
            <p style={{ fontSize: 13, color: 'var(--text-secondary)', marginBottom: 20, lineHeight: 1.5 }}>
              This action cannot be undone. All selected tasks will be permanently removed.
            </p>
            <div style={{ display: 'flex', gap: 10, justifyContent: 'flex-end' }}>
              <button
                type="button"
                className="tf-btn tf-btn-ghost tf-btn-sm"
                onClick={() => setShowDeleteConfirm(false)}
              >
                Cancel
              </button>
              <button
                type="button"
                className="tf-btn tf-btn-sm"
                onClick={handleBulkDelete}
                style={{ color: '#fff', background: 'var(--color-danger)', borderColor: 'var(--color-danger)' }}
              >
                Delete {count} task{count !== 1 ? 's' : ''}
              </button>
            </div>
          </div>
        </div>
      )}
    </>
  );
}
