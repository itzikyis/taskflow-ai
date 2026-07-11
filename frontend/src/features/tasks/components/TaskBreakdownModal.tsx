import { useEffect, useState } from 'react';
import { useTaskBreakdown } from '@/features/ai/hooks/useAi';
import { useCreateSubtasks } from '../hooks/useTasks';

interface TaskBreakdownModalProps {
  taskId: string;
  taskTitle: string;
  taskDescription?: string;
  onClose: () => void;
}

interface DraftSubtask {
  id: number;
  title: string;
  description: string;
  selected: boolean;
}

export function TaskBreakdownModal({ taskId, taskTitle, taskDescription, onClose }: TaskBreakdownModalProps) {
  const breakdown = useTaskBreakdown();
  const createSubtasks = useCreateSubtasks(taskId);
  const [drafts, setDrafts] = useState<DraftSubtask[]>([]);

  // Generate suggestions once when the modal opens.
  useEffect(() => {
    breakdown.mutate(
      { title: taskTitle, description: taskDescription },
      {
        onSuccess: (suggestions) =>
          setDrafts(
            suggestions.map((s, i) => ({
              id: i,
              title: s.title,
              description: s.description ?? '',
              selected: true,
            })),
          ),
      },
    );
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  const update = (id: number, patch: Partial<DraftSubtask>) =>
    setDrafts(prev => prev.map(d => (d.id === id ? { ...d, ...patch } : d)));

  const selectedCount = drafts.filter(d => d.selected && d.title.trim()).length;

  const handleCreate = () => {
    const chosen = drafts
      .filter(d => d.selected && d.title.trim())
      .map(d => ({ title: d.title.trim(), description: d.description.trim() || undefined }));
    if (chosen.length === 0) return;
    createSubtasks.mutate(chosen, { onSuccess: onClose });
  };

  return (
    <div className="modal-overlay" onClick={e => { if (e.target === e.currentTarget) onClose(); }}>
      <div className="modal-box" style={{ maxWidth: 640, width: '90%' }}>
        <h2 className="modal-title">🧩 Break down “{taskTitle}”</h2>

        {breakdown.isPending && (
          <p style={{ fontSize: 13, color: 'var(--text-muted)', padding: '20px 0' }}>
            AI is breaking this task down…
          </p>
        )}

        {breakdown.isError && (
          <p style={{ fontSize: 13, color: 'var(--color-danger)', padding: '12px 0' }}>
            The AI service is unavailable right now. Please try again later.
          </p>
        )}

        {!breakdown.isPending && !breakdown.isError && drafts.length === 0 && (
          <p style={{ fontSize: 13, color: 'var(--text-muted)', padding: '12px 0' }}>
            No subtasks were suggested. Try adding more detail to the task description.
          </p>
        )}

        {drafts.length > 0 && (
          <div style={{ display: 'flex', flexDirection: 'column', gap: 8, maxHeight: 380, overflowY: 'auto', marginBottom: 16 }}>
            {drafts.map(d => (
              <div
                key={d.id}
                style={{
                  display: 'flex', gap: 8, alignItems: 'flex-start',
                  padding: 8, border: '1px solid var(--border-color)', borderRadius: 6,
                  background: d.selected ? 'var(--surface-bg)' : 'transparent',
                  opacity: d.selected ? 1 : 0.55,
                }}
              >
                <input
                  type="checkbox"
                  checked={d.selected}
                  onChange={e => update(d.id, { selected: e.target.checked })}
                  style={{ marginTop: 6 }}
                />
                <div style={{ flex: 1, display: 'flex', flexDirection: 'column', gap: 4 }}>
                  <input
                    className="tf-input"
                    value={d.title}
                    onChange={e => update(d.id, { title: e.target.value })}
                    placeholder="Subtask title"
                    style={{ fontSize: 13, fontWeight: 600 }}
                  />
                  <input
                    className="tf-input"
                    value={d.description}
                    onChange={e => update(d.id, { description: e.target.value })}
                    placeholder="Description (optional)"
                    style={{ fontSize: 12 }}
                  />
                </div>
                <button
                  type="button"
                  onClick={() => setDrafts(prev => prev.filter(x => x.id !== d.id))}
                  aria-label="Remove suggestion"
                  style={{ background: 'none', border: 'none', cursor: 'pointer', color: 'var(--color-danger)', fontSize: 16 }}
                >
                  ×
                </button>
              </div>
            ))}
          </div>
        )}

        <div style={{ display: 'flex', justifyContent: 'flex-end', gap: 8 }}>
          <button type="button" className="tf-btn tf-btn-ghost" onClick={onClose}>Cancel</button>
          <button
            type="button"
            className="tf-btn tf-btn-primary"
            onClick={handleCreate}
            disabled={selectedCount === 0 || createSubtasks.isPending}
          >
            {createSubtasks.isPending ? 'Creating…' : `Create ${selectedCount} subtask${selectedCount === 1 ? '' : 's'}`}
          </button>
        </div>
      </div>
    </div>
  );
}
