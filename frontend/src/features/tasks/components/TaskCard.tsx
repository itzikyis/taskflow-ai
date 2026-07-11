import { useState, useRef } from 'react';
import type { Task, TaskPriority, TaskStatus } from '../types/task.types';
import { useUpdateTaskStatus, useUpdateTask } from '../hooks/useTasks';
import { CommentThread } from '@/features/comments/components/CommentThread';
import { AttachmentList } from '@/features/attachments/components/AttachmentList';
import { AiDescriptionSuggestion } from '@/features/ai/components/AiDescriptionSuggestion';
import { StoryPointEstimator } from './StoryPointEstimator';
import { DevelopmentPanel } from '@/features/development/components/DevelopmentPanel';
import { TaskBreakdownModal } from './TaskBreakdownModal';
import { useAuthStore } from '@/store/authStore';

interface TaskCardProps {
  task: Task;
  onDelete: () => void;
}

type Panel = 'comments' | 'attachments' | 'development' | 'ai' | null;

const PRIORITY_COLOR: Record<TaskPriority, { color: string; bg: string; dot: string }> = {
  Low:      { color: 'var(--priority-low)',      bg: 'var(--priority-low-bg)',      dot: '#10b981' },
  Medium:   { color: 'var(--priority-medium)',   bg: 'var(--priority-medium-bg)',   dot: '#f59e0b' },
  High:     { color: 'var(--priority-high)',     bg: 'var(--priority-high-bg)',     dot: '#ef4444' },
  Critical: { color: 'var(--priority-critical)', bg: 'var(--priority-critical-bg)', dot: '#7c3aed' },
};

// Fallback for unrecognised/legacy priority values so a single bad row can never
// crash the whole board.
const PRIORITY_FALLBACK = { color: 'var(--text-muted)', bg: 'var(--surface-bg)', dot: '#94a3b8' };

const STATUS_COLOR: Record<TaskStatus, { color: string; bg: string }> = {
  Todo:       { color: 'var(--status-todo)',       bg: 'var(--status-todo-bg)'       },
  InProgress: { color: 'var(--status-inprogress)', bg: 'var(--status-inprogress-bg)' },
  Done:       { color: 'var(--status-done)',        bg: 'var(--status-done-bg)'       },
};

const STATUS_FALLBACK = { color: 'var(--text-muted)', bg: 'var(--surface-bg)' };

const STATUS_LABEL: Record<TaskStatus, string> = {
  Todo: 'To Do',
  InProgress: 'In Progress',
  Done: 'Done',
};

const NEXT_STATUS: Record<TaskStatus, { status: TaskStatus; label: string } | null> = {
  Todo:       { status: 'InProgress', label: 'Start'    },
  InProgress: { status: 'Done',       label: 'Mark done' },
  Done:       null,
};

function isOverdue(dueDate?: string): boolean {
  if (!dueDate) return false;
  return new Date(dueDate) < new Date(new Date().toDateString());
}

export function TaskCard({ task, onDelete }: TaskCardProps) {
  const [panel, setPanel] = useState<Panel>(null);
  const [expanded, setExpanded] = useState(false);
  const [editingTitle, setEditingTitle] = useState(false);
  const [showBreakdown, setShowBreakdown] = useState(false);
  const [titleDraft, setTitleDraft] = useState(task.title);
  const titleInputRef = useRef<HTMLInputElement>(null);
  const { token } = useAuthStore();
  const userId = token?.userId ?? '';
  const updateStatus = useUpdateTaskStatus(task.id);
  const updateTask = useUpdateTask(task.id);
  const nextStep = NEXT_STATUS[task.status];

  const startTitleEdit = () => {
    setTitleDraft(task.title);
    setEditingTitle(true);
    setTimeout(() => titleInputRef.current?.select(), 0);
  };

  const commitTitleEdit = () => {
    const trimmed = titleDraft.trim();
    if (trimmed && trimmed !== task.title) {
      updateTask.mutate({ title: trimmed, description: task.description ?? undefined });
    }
    setEditingTitle(false);
  };

  const cancelTitleEdit = () => {
    setTitleDraft(task.title);
    setEditingTitle(false);
  };

  const toggle = (p: Panel) => setPanel(prev => prev === p ? null : p);
  const pri = PRIORITY_COLOR[task.priority] ?? PRIORITY_FALLBACK;
  const st  = STATUS_COLOR[task.status] ?? STATUS_FALLBACK;
  const due = task.dueDate ? new Date(task.dueDate) : null;
  const overdue = isOverdue(task.dueDate) && task.status !== 'Done';

  return (
    <article className="tf-card" style={{ padding: '14px 16px', cursor: 'default' }}>
      {/* ── Header row ─────────────────────────────────────────── */}
      <div style={{ display: 'flex', alignItems: 'flex-start', gap: 10 }}>
        {/* Priority dot */}
        <span
          title={task.priority}
          style={{
            width: 8, height: 8, borderRadius: '50%',
            background: pri.dot, flexShrink: 0, marginTop: 6,
          }}
        />

        <div style={{ flex: 1, overflow: 'hidden' }}>
          <div style={{ display: 'flex', alignItems: 'center', gap: 8, flexWrap: 'wrap' }}>
            {editingTitle ? (
              <input
                ref={titleInputRef}
                value={titleDraft}
                onChange={e => setTitleDraft(e.target.value)}
                onBlur={commitTitleEdit}
                onKeyDown={e => {
                  if (e.key === 'Enter') { e.preventDefault(); commitTitleEdit(); }
                  if (e.key === 'Escape') cancelTitleEdit();
                }}
                className="tf-input"
                style={{ fontSize: 14, fontWeight: 600, padding: '2px 6px', height: 28, minWidth: 180 }}
              />
            ) : (
              <span
                style={{ fontSize: 14, fontWeight: 600, color: 'var(--text-primary)', lineHeight: 1.4, cursor: 'text' }}
                onDoubleClick={startTitleEdit}
                title="Double-click to edit"
              >
                {task.title}
              </span>
            )}

            {/* Status badge */}
            <span
              className="tf-badge"
              style={{ color: st.color, background: st.bg, flexShrink: 0 }}
            >
              {STATUS_LABEL[task.status]}
            </span>

            {/* Priority badge */}
            <span
              className="tf-badge"
              style={{ color: pri.color, background: pri.bg, flexShrink: 0 }}
            >
              {task.priority}
            </span>
          </div>

          {task.description && !expanded && (
            <p style={{
              fontSize: 12, color: 'var(--text-secondary)', marginTop: 4, lineHeight: 1.5,
              overflow: 'hidden', textOverflow: 'ellipsis', whiteSpace: 'nowrap',
            }}>
              {task.description}
            </p>
          )}
          {task.description && expanded && (
            <p style={{ fontSize: 13, color: 'var(--text-secondary)', marginTop: 4, lineHeight: 1.6 }}>
              {task.description}
            </p>
          )}
        </div>

        {/* Actions */}
        <div style={{ display: 'flex', gap: 4, flexShrink: 0, marginLeft: 'auto' }}>
          {task.description && (
            <button
              type="button"
              onClick={() => setExpanded(p => !p)}
              title={expanded ? 'Collapse' : 'Expand'}
              style={{
                background: 'none', border: 'none', cursor: 'pointer',
                color: 'var(--text-muted)', fontSize: 12, padding: '2px 4px',
              }}
            >
              {expanded ? '▲' : '▼'}
            </button>
          )}
          <button
            type="button"
            onClick={onDelete}
            title="Delete task"
            style={{
              background: 'none', border: 'none', cursor: 'pointer',
              color: 'var(--text-muted)', fontSize: 14, padding: '2px 6px',
              borderRadius: 4, lineHeight: 1,
            }}
            onMouseOver={e => (e.currentTarget.style.color = 'var(--color-danger)')}
            onMouseOut={e  => (e.currentTarget.style.color = 'var(--text-muted)')}
          >
            ×
          </button>
        </div>
      </div>

      {/* ── Meta row ───────────────────────────────────────────── */}
      <div style={{ display: 'flex', alignItems: 'center', gap: 12, marginTop: 10, flexWrap: 'wrap' }}>
        {due && (
          <span style={{
            fontSize: 11, fontWeight: 500,
            color: overdue ? 'var(--color-danger)' : 'var(--text-muted)',
            display: 'flex', alignItems: 'center', gap: 3,
          }}>
            {overdue ? '⚠' : '📅'} {due.toLocaleDateString(undefined, { month: 'short', day: 'numeric', year: 'numeric' })}
          </span>
        )}

        {nextStep && (
          <button
            type="button"
            onClick={() => updateStatus.mutate(nextStep.status)}
            disabled={updateStatus.isPending}
            className="tf-btn tf-btn-ghost tf-btn-sm"
            style={{
              color: nextStep.status === 'Done' ? 'var(--color-success)' : 'var(--status-inprogress)',
              borderColor: nextStep.status === 'Done' ? 'var(--color-success)' : 'var(--status-inprogress)',
              fontWeight: 600,
            }}
          >
            {updateStatus.isPending ? '…' : (nextStep.status === 'Done' ? '✓ ' : '▶ ') + nextStep.label}
          </button>
        )}

        <div style={{ marginLeft: nextStep ? 0 : 'auto', display: 'flex', gap: 4, marginLeft: 'auto' }}>
          <button
            type="button"
            onClick={() => toggle('comments')}
            className={`tf-btn tf-btn-ghost tf-btn-sm`}
            style={{
              color: panel === 'comments' ? 'var(--color-primary)' : 'var(--text-secondary)',
              borderColor: panel === 'comments' ? 'var(--color-primary)' : 'var(--surface-border)',
              background: panel === 'comments' ? 'var(--color-primary-light)' : 'none',
            }}
          >
            💬
          </button>
          <button
            type="button"
            onClick={() => toggle('attachments')}
            className="tf-btn tf-btn-ghost tf-btn-sm"
            style={{
              color: panel === 'attachments' ? 'var(--color-primary)' : 'var(--text-secondary)',
              borderColor: panel === 'attachments' ? 'var(--color-primary)' : 'var(--surface-border)',
              background: panel === 'attachments' ? 'var(--color-primary-light)' : 'none',
            }}
          >
            📎
          </button>
          <button
            type="button"
            onClick={() => toggle('development')}
            className="tf-btn tf-btn-ghost tf-btn-sm"
            title="Linked branches & pull requests"
            style={{
              color: panel === 'development' ? 'var(--color-primary)' : 'var(--text-secondary)',
              borderColor: panel === 'development' ? 'var(--color-primary)' : 'var(--surface-border)',
              background: panel === 'development' ? 'var(--color-primary-light)' : 'none',
            }}
          >
            🔗
          </button>
          <button
            type="button"
            onClick={() => toggle('ai')}
            className="tf-btn tf-btn-ghost tf-btn-sm"
            style={{
              color: panel === 'ai' ? '#7c3aed' : 'var(--text-secondary)',
              borderColor: panel === 'ai' ? '#7c3aed' : 'var(--surface-border)',
              background: panel === 'ai' ? '#ede9fe' : 'none',
            }}
          >
            ✨ AI
          </button>
        </div>
      </div>

      {/* ── Panels ─────────────────────────────────────────────── */}
      {panel && (
        <div className="task-panel">
          {panel === 'comments' && (
            <CommentThread taskId={task.id} currentUserId={userId} />
          )}
          {panel === 'attachments' && (
            <>
              <p style={{ fontSize: 12, fontWeight: 600, color: 'var(--text-secondary)', marginBottom: 8, textTransform: 'uppercase', letterSpacing: '0.04em' }}>
                Attachments
              </p>
              <AttachmentList taskId={task.id} currentUserId={userId} />
            </>
          )}
          {panel === 'development' && (
            <DevelopmentPanel taskId={task.id} />
          )}
          {panel === 'ai' && (
            <>
              <p style={{ fontSize: 12, fontWeight: 600, color: '#7c3aed', marginBottom: 8 }}>
                ✨ AI Assistant
              </p>
              <AiDescriptionSuggestion
                taskTitle={task.title}
                onAccept={(s) => { navigator.clipboard?.writeText(s); toggle('ai'); }}
              />
              <div style={{ marginTop: 12, paddingTop: 12, borderTop: '1px solid #e9d5ff' }}>
                <p style={{ fontSize: 11, fontWeight: 600, color: '#7c3aed', marginBottom: 4, textTransform: 'uppercase', letterSpacing: '0.04em' }}>
                  Story Points
                </p>
                <StoryPointEstimator
                  taskTitle={task.title}
                  taskDescription={task.description ?? undefined}
                />
              </div>
              <div style={{ marginTop: 12, paddingTop: 12, borderTop: '1px solid #e9d5ff' }}>
                <p style={{ fontSize: 11, fontWeight: 600, color: '#7c3aed', marginBottom: 6, textTransform: 'uppercase', letterSpacing: '0.04em' }}>
                  Break down
                </p>
                <button
                  type="button"
                  className="tf-btn tf-btn-sm"
                  onClick={() => setShowBreakdown(true)}
                  style={{ color: '#7c3aed', borderColor: '#c4b5fd', background: '#f5f3ff' }}
                >
                  🧩 Break into subtasks
                </button>
              </div>
            </>
          )}
        </div>
      )}

      {showBreakdown && (
        <TaskBreakdownModal
          taskId={task.id}
          taskTitle={task.title}
          taskDescription={task.description ?? undefined}
          onClose={() => setShowBreakdown(false)}
        />
      )}
    </article>
  );
}
