import { useState } from 'react';
import { aiService, type MeetingNotesResult, type MeetingActionItem } from '@/services/aiService';
import { useCreateTask } from '@/features/tasks/hooks/useTasks';
import type { TaskPriority } from '@/features/tasks/types/task.types';

const VALID_PRIORITIES: TaskPriority[] = ['Low', 'Medium', 'High', 'Critical'];

const PRIORITY_COLOR: Record<string, string> = {
  Critical: '#ef4444',
  High: '#f97316',
  Medium: '#f59e0b',
  Low: '#10b981',
};

interface DraftItem extends MeetingActionItem {
  selected: boolean;
  titleEdited: string;
}

export function MeetingNotesPage() {
  const [transcript, setTranscript] = useState('');
  const [participants, setParticipants] = useState('');
  const [analyzing, setAnalyzing] = useState(false);
  const [creating, setCreating] = useState(false);
  const [result, setResult] = useState<MeetingNotesResult | null>(null);
  const [drafts, setDrafts] = useState<DraftItem[]>([]);
  const [error, setError] = useState<string | null>(null);
  const [created, setCreated] = useState<number | null>(null);

  const createTask = useCreateTask();

  const analyze = async () => {
    if (!transcript.trim()) return;
    setAnalyzing(true);
    setError(null);
    setResult(null);
    setCreated(null);
    try {
      const names = participants.split(',').map(s => s.trim()).filter(Boolean);
      const res = await aiService.analyzeMeetingNotes(transcript, names);
      setResult(res);
      setDrafts(res.actionItems.map(item => ({
        ...item,
        selected: true,
        titleEdited: item.title,
      })));
    } catch {
      setError('Analysis failed. Check that the AI service is configured.');
    } finally {
      setAnalyzing(false);
    }
  };

  const toggleAll = (on: boolean) => setDrafts(d => d.map(x => ({ ...x, selected: on })));

  const createSelected = async () => {
    const toCreate = drafts.filter(d => d.selected);
    if (toCreate.length === 0) return;
    setCreating(true);
    let count = 0;
    for (const d of toCreate) {
      try {
        await createTask.mutateAsync({
          title: d.titleEdited || d.title,
          description: d.description,
          priority: (VALID_PRIORITIES.includes(d.priority as TaskPriority) ? d.priority : 'Medium') as TaskPriority,
          dueDate: d.suggestedDueDate ?? undefined,
        });
        count++;
      } catch {
        // continue with the rest
      }
    }
    setCreated(count);
    setCreating(false);
    setDrafts(prev => prev.map(d => d.selected ? { ...d, selected: false } : d));
  };

  const selectedCount = drafts.filter(d => d.selected).length;

  return (
    <div>
      <div className="page-header">
        <div>
          <h1 className="page-title">AI Meeting Notes 📝</h1>
          <p className="page-subtitle">
            Paste meeting notes or a transcript — AI extracts key decisions and creates draft tasks you can bulk-add to your board.
          </p>
        </div>
      </div>

      {/* Input panel */}
      <div style={{
        background: '#ffffff', border: '1px solid var(--border-color)',
        borderRadius: 'var(--radius-md)', boxShadow: 'var(--shadow-sm)',
        padding: 18, marginBottom: 16,
      }}>
        <label style={{ display: 'block', fontSize: 12, fontWeight: 600, color: 'var(--text-secondary)', marginBottom: 4 }}>
          PARTICIPANTS (comma-separated, optional)
        </label>
        <input
          className="tf-input"
          placeholder="Alice, Bob, Charlie…"
          value={participants}
          onChange={e => setParticipants(e.target.value)}
          style={{ width: '100%', marginBottom: 12, fontSize: 13 }}
        />

        <label style={{ display: 'block', fontSize: 12, fontWeight: 600, color: 'var(--text-secondary)', marginBottom: 4 }}>
          MEETING NOTES / TRANSCRIPT
        </label>
        <textarea
          className="tf-input"
          placeholder={"Paste meeting notes, a transcript, or bullet points here…\n\nExample:\n- Alice: we need to fix the login bug before release\n- Bob will draft the API docs by Friday\n- Decided to drop the dark mode feature from v1.2"}
          value={transcript}
          onChange={e => setTranscript(e.target.value)}
          rows={10}
          style={{ width: '100%', resize: 'vertical', fontSize: 13, fontFamily: 'inherit', marginBottom: 12 }}
        />

        <div style={{ display: 'flex', justifyContent: 'flex-end' }}>
          <button
            className="tf-btn tf-btn-primary"
            onClick={analyze}
            disabled={analyzing || !transcript.trim()}
          >
            {analyzing ? '⏳ Analyzing…' : '✨ Analyze Notes'}
          </button>
        </div>
      </div>

      {error && (
        <div style={{ padding: 14, background: '#fef2f2', border: '1px solid #fecaca', borderRadius: 'var(--radius-md)', color: '#b91c1c', marginBottom: 16 }}>
          {error}
        </div>
      )}

      {created !== null && (
        <div style={{ padding: 14, background: '#ecfdf5', border: '1px solid #a7f3d0', borderRadius: 'var(--radius-md)', color: '#065f46', marginBottom: 16 }}>
          ✓ {created} task{created !== 1 ? 's' : ''} created successfully.
        </div>
      )}

      {result && !analyzing && (
        <>
          {/* Summary */}
          <div style={{
            background: '#ffffff', border: '1px solid var(--border-color)',
            borderRadius: 'var(--radius-md)', boxShadow: 'var(--shadow-sm)',
            padding: 16, marginBottom: 16,
          }}>
            <p style={{ margin: '0 0 10px', fontWeight: 600, fontSize: 13 }}>📋 Summary</p>
            <p style={{ margin: '0 0 12px', fontSize: 13, color: 'var(--text-secondary)' }}>{result.summary}</p>

            {result.keyDecisions.length > 0 && (
              <>
                <p style={{ margin: '0 0 6px', fontWeight: 600, fontSize: 12, color: 'var(--text-muted)', textTransform: 'uppercase', letterSpacing: '0.04em' }}>
                  Key Decisions
                </p>
                <ul style={{ margin: 0, paddingLeft: 20, fontSize: 13, color: 'var(--text-secondary)', display: 'flex', flexDirection: 'column', gap: 4 }}>
                  {result.keyDecisions.map((d, i) => <li key={i}>{d}</li>)}
                </ul>
              </>
            )}
          </div>

          {/* Draft tasks */}
          {drafts.length > 0 && (
            <div style={{
              border: '1px solid var(--border-color)', borderRadius: 'var(--radius-md)',
              overflow: 'hidden', background: '#ffffff', boxShadow: 'var(--shadow-sm)',
            }}>
              {/* Toolbar */}
              <div style={{
                display: 'flex', alignItems: 'center', gap: 12, padding: '10px 14px',
                borderBottom: '1px solid var(--border-color)', background: 'var(--surface-bg)',
              }}>
                <span style={{ fontSize: 13, fontWeight: 600, color: 'var(--text-primary)' }}>
                  ✅ Draft Tasks ({drafts.length})
                </span>
                <span style={{ fontSize: 12, color: 'var(--text-muted)' }}>
                  Review and edit before creating
                </span>
                <div style={{ marginLeft: 'auto', display: 'flex', gap: 8 }}>
                  <button className="tf-btn" style={{ fontSize: 12 }} onClick={() => toggleAll(true)}>Select all</button>
                  <button className="tf-btn" style={{ fontSize: 12 }} onClick={() => toggleAll(false)}>Deselect all</button>
                  <button
                    className="tf-btn tf-btn-primary"
                    style={{ fontSize: 12 }}
                    onClick={createSelected}
                    disabled={creating || selectedCount === 0}
                  >
                    {creating ? '⏳ Creating…' : `➕ Create ${selectedCount} task${selectedCount !== 1 ? 's' : ''}`}
                  </button>
                </div>
              </div>

              {/* Draft rows */}
              {drafts.map((draft, idx) => (
                <div
                  key={idx}
                  style={{
                    display: 'flex', alignItems: 'flex-start', gap: 12, padding: '12px 14px',
                    borderBottom: idx < drafts.length - 1 ? '1px solid var(--border-color)' : 'none',
                    opacity: draft.selected ? 1 : 0.5,
                  }}
                >
                  <input
                    type="checkbox"
                    checked={draft.selected}
                    onChange={e => setDrafts(d => d.map((x, i) => i === idx ? { ...x, selected: e.target.checked } : x))}
                    style={{ marginTop: 3, flexShrink: 0 }}
                  />
                  <div style={{ flex: 1, minWidth: 0 }}>
                    <input
                      className="tf-input"
                      value={draft.titleEdited}
                      onChange={e => setDrafts(d => d.map((x, i) => i === idx ? { ...x, titleEdited: e.target.value } : x))}
                      style={{ fontSize: 13, fontWeight: 500, width: '100%', marginBottom: 4 }}
                    />
                    <p style={{ margin: 0, fontSize: 12, color: 'var(--text-muted)' }}>{draft.description}</p>
                  </div>
                  <div style={{ display: 'flex', gap: 8, flexShrink: 0, flexWrap: 'wrap', justifyContent: 'flex-end' }}>
                    <span style={{
                      padding: '2px 8px', borderRadius: 10, fontSize: 11, fontWeight: 600,
                      background: `${PRIORITY_COLOR[draft.priority] ?? '#94a3b8'}20`,
                      color: PRIORITY_COLOR[draft.priority] ?? '#64748b',
                    }}>
                      {draft.priority}
                    </span>
                    {draft.suggestedAssignee && (
                      <span style={{ fontSize: 11, color: 'var(--text-muted)', padding: '2px 6px' }}>
                        👤 {draft.suggestedAssignee}
                      </span>
                    )}
                    {draft.suggestedDueDate && (
                      <span style={{ fontSize: 11, color: 'var(--text-muted)', padding: '2px 6px' }}>
                        📅 {new Date(draft.suggestedDueDate).toLocaleDateString(undefined, { month: 'short', day: 'numeric' })}
                      </span>
                    )}
                  </div>
                </div>
              ))}
            </div>
          )}

          {drafts.length === 0 && (
            <div style={{ padding: 20, textAlign: 'center', color: 'var(--text-muted)', fontSize: 13 }}>
              No action items were found in these notes.
            </div>
          )}
        </>
      )}
    </div>
  );
}
