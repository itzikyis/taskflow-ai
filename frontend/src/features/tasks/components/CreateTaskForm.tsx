import { useEffect, useRef, useState } from 'react';
import { useCreateTask } from '../hooks/useTasks';
import type { TaskPriority } from '../types/task.types';
import { TASK_PRIORITIES } from '../types/task.types';
import { useAuthStore } from '@/store/authStore';
import { taskService, type DuplicateMatch } from '@/services/taskService';
import { triageTaskByContent, type TaskTriageResult } from '@/services/aiTriageService';

interface CreateTaskFormProps {
  onClose: () => void;
  /** Optional project scope for AI triage duplicate detection. Defaults to nil GUID when omitted. */
  projectId?: string;
}

const NIL_GUID = '00000000-0000-0000-0000-000000000000';

export function CreateTaskForm({ onClose, projectId = NIL_GUID }: CreateTaskFormProps) {
  const [title, setTitle]       = useState('');
  const [description, setDesc]  = useState('');
  const [priority, setPriority] = useState<TaskPriority>('Medium');
  const [dueDate, setDueDate]   = useState('');
  const [duplicates, setDuplicates] = useState<DuplicateMatch[]>([]);
  const [dupDismissed, setDupDismissed] = useState(false);
  const [triageResult, setTriageResult] = useState<TaskTriageResult | null>(null);
  const [triageLoading, setTriageLoading] = useState(false);
  const [triageError, setTriageError] = useState<string | null>(null);
  const createMutation          = useCreateTask();
  const { token }               = useAuthStore();
  const triageAbortRef          = useRef<AbortController | null>(null);

  // Debounced "possible duplicates" check as the user types, like Linear/Jira.
  useEffect(() => {
    const trimmed = title.trim();
    if (trimmed.length < 6) {
      setDuplicates([]);
      return;
    }
    const handle = setTimeout(() => {
      taskService
        .checkDuplicates(trimmed, description.trim() || undefined)
        .then(setDuplicates)
        .catch(() => setDuplicates([]));
    }, 500);
    return () => clearTimeout(handle);
  }, [title, description]);

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if (!title.trim() || !token) return;
    createMutation.mutate(
      {
        title: title.trim(),
        description: description.trim() || undefined,
        priority,
        dueDate: dueDate || undefined,
      },
      { onSuccess: onClose },
    );
  };

  const handleTriage = async () => {
    const trimmed = title.trim();
    if (!trimmed) return;

    // Cancel any in-flight triage request.
    triageAbortRef.current?.abort();
    triageAbortRef.current = new AbortController();

    setTriageLoading(true);
    setTriageError(null);
    setTriageResult(null);

    try {
      const result = await triageTaskByContent({
        title: trimmed,
        description: description.trim() || undefined,
        projectId,
      });
      setTriageResult(result);
    } catch {
      setTriageError('AI triage failed. Please try again.');
    } finally {
      setTriageLoading(false);
    }
  };

  const applyPriority = (suggested: string) => {
    if (TASK_PRIORITIES.includes(suggested as TaskPriority)) {
      setPriority(suggested as TaskPriority);
    }
    setTriageResult(null);
  };

  return (
    <div className="modal-overlay" onClick={e => { if (e.target === e.currentTarget) onClose(); }}>
      <div className="modal-box">
        <h2 className="modal-title">New task</h2>

        <form onSubmit={handleSubmit} style={{ display: 'flex', flexDirection: 'column', gap: 16 }}>
          <div className="form-group">
            <label htmlFor="task-title" className="form-label">Title *</label>
            <input
              id="task-title"
              type="text"
              className="tf-input"
              value={title}
              onChange={e => { setTitle(e.target.value); setTriageResult(null); setTriageError(null); }}
              placeholder="What needs to be done?"
              required
              autoFocus
            />
          </div>

          <div className="form-group">
            <label htmlFor="task-desc" className="form-label">Description</label>
            <textarea
              id="task-desc"
              className="tf-input"
              value={description}
              onChange={e => { setDesc(e.target.value); setTriageResult(null); setTriageError(null); }}
              placeholder="Add details, acceptance criteria…"
              rows={3}
              style={{ resize: 'vertical' }}
            />
          </div>

          {/* AI Triage button */}
          <div style={{ display: 'flex', alignItems: 'center', gap: 8 }}>
            <button
              type="button"
              className="tf-btn tf-btn-ghost"
              disabled={!title.trim() || triageLoading}
              onClick={handleTriage}
              style={{ fontSize: 13, padding: '5px 10px' }}
            >
              {triageLoading ? 'Analysing…' : '✨ AI Triage'}
            </button>
            {triageError && (
              <span style={{ fontSize: 12, color: 'var(--color-danger)' }}>{triageError}</span>
            )}
          </div>

          {/* AI Triage result panel */}
          {triageResult && (
            <div
              style={{
                border: '1px solid var(--surface-border)',
                background: 'var(--surface-card)',
                borderRadius: 'var(--radius-md)',
                padding: '12px 14px',
                display: 'flex',
                flexDirection: 'column',
                gap: 10,
              }}
            >
              {/* Priority suggestion */}
              <div style={{ display: 'flex', alignItems: 'center', gap: 8, flexWrap: 'wrap' }}>
                <span style={{ fontSize: 12, fontWeight: 700, color: 'var(--text-primary)' }}>
                  Suggested priority:
                </span>
                <span
                  style={{
                    fontSize: 12,
                    fontWeight: 700,
                    color: 'var(--color-primary)',
                    background: 'color-mix(in srgb, var(--color-primary) 12%, transparent)',
                    padding: '2px 8px',
                    borderRadius: 4,
                  }}
                >
                  {triageResult.suggestedPriority}
                </span>
                <button
                  type="button"
                  className="tf-btn tf-btn-primary"
                  onClick={() => applyPriority(triageResult.suggestedPriority)}
                  style={{ fontSize: 12, padding: '3px 10px' }}
                >
                  Apply
                </button>
                <button
                  type="button"
                  onClick={() => setTriageResult(null)}
                  aria-label="Dismiss AI triage"
                  style={{
                    marginLeft: 'auto',
                    background: 'none',
                    border: 'none',
                    cursor: 'pointer',
                    color: 'var(--text-primary)',
                    fontSize: 16,
                    lineHeight: 1,
                  }}
                >
                  ×
                </button>
              </div>

              {/* Reasoning */}
              <p style={{ fontSize: 12, color: 'var(--text-primary)', margin: 0, lineHeight: 1.5 }}>
                {triageResult.reasoning}
              </p>

              {/* Potential duplicates */}
              {triageResult.potentialDuplicates.length > 0 && (
                <div>
                  <span style={{ fontSize: 12, fontWeight: 700, color: '#b45309' }}>
                    ⚠️ Possible duplicate{triageResult.potentialDuplicates.length > 1 ? 's' : ''}
                  </span>
                  <div style={{ display: 'flex', flexDirection: 'column', gap: 4, marginTop: 6 }}>
                    {triageResult.potentialDuplicates.slice(0, 4).map(d => (
                      <div key={d.taskId} style={{ display: 'flex', alignItems: 'center', gap: 8, fontSize: 12 }}>
                        <span
                          style={{
                            fontSize: 10,
                            fontWeight: 700,
                            color: '#b45309',
                            background: '#fef3c7',
                            padding: '1px 5px',
                            borderRadius: 4,
                            flexShrink: 0,
                          }}
                        >
                          {Math.round(d.similarityScore * 100)}%
                        </span>
                        <span
                          style={{
                            color: 'var(--text-primary)',
                            overflow: 'hidden',
                            textOverflow: 'ellipsis',
                            whiteSpace: 'nowrap',
                          }}
                        >
                          {d.title}
                        </span>
                      </div>
                    ))}
                  </div>
                </div>
              )}
            </div>
          )}

          {/* Existing debounced duplicate warning (when no triage panel is shown) */}
          {duplicates.length > 0 && !dupDismissed && !triageResult && (
            <div
              style={{
                border: '1px solid #f59e0b', background: '#fffbeb', borderRadius: 8,
                padding: '10px 12px', display: 'flex', flexDirection: 'column', gap: 6,
              }}
            >
              <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between' }}>
                <span style={{ fontSize: 12, fontWeight: 700, color: '#b45309' }}>
                  ⚠️ Possible duplicate{duplicates.length > 1 ? 's' : ''}
                </span>
                <button
                  type="button"
                  onClick={() => setDupDismissed(true)}
                  aria-label="Dismiss"
                  style={{ background: 'none', border: 'none', cursor: 'pointer', color: '#b45309', fontSize: 14, lineHeight: 1 }}
                >
                  ×
                </button>
              </div>
              {duplicates.slice(0, 4).map(d => (
                <div key={d.taskId} style={{ display: 'flex', alignItems: 'center', gap: 8, fontSize: 12 }}>
                  <span style={{
                    fontSize: 10, fontWeight: 700, color: '#b45309', background: '#fef3c7',
                    padding: '1px 5px', borderRadius: 4, flexShrink: 0,
                  }}>
                    {Math.round(d.score * 100)}%
                  </span>
                  <span style={{ color: 'var(--text-secondary)', overflow: 'hidden', textOverflow: 'ellipsis', whiteSpace: 'nowrap' }}>
                    {d.title}
                  </span>
                </div>
              ))}
            </div>
          )}

          <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: 12 }}>
            <div className="form-group">
              <label htmlFor="task-priority" className="form-label">Priority</label>
              <select
                id="task-priority"
                className="tf-input"
                value={priority}
                onChange={e => setPriority(e.target.value as TaskPriority)}
              >
                {TASK_PRIORITIES.map(p => (
                  <option key={p} value={p}>{p}</option>
                ))}
              </select>
            </div>

            <div className="form-group">
              <label htmlFor="task-due" className="form-label">Due date</label>
              <input
                id="task-due"
                type="date"
                className="tf-input"
                value={dueDate}
                onChange={e => setDueDate(e.target.value)}
              />
            </div>
          </div>

          <div style={{ display: 'flex', gap: 8, justifyContent: 'flex-end', marginTop: 4 }}>
            <button type="button" className="tf-btn tf-btn-ghost" onClick={onClose}>
              Cancel
            </button>
            <button
              type="submit"
              disabled={createMutation.isPending || !title.trim()}
              className="tf-btn tf-btn-primary"
            >
              {createMutation.isPending ? 'Creating…' : 'Create task'}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
}
