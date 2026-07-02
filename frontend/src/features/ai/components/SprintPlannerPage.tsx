import { useState } from 'react';
import type { CSSProperties } from 'react';
import { useAllTasks } from '@/features/tasks/hooks/useTasks';
import { useSuggestSprintPlan } from '../hooks/useAi';
import { StoryPointBadge } from '@/features/tasks/components/StoryPointBadge';
import type { TaskStatus } from '@/features/tasks/types/task.types';
import { TASK_STATUSES } from '@/features/tasks/types/task.types';
import type { SprintPlan } from '@/services/aiService';

const DEFAULT_CAPACITY = 40;
const NON_DONE_STATUSES: TaskStatus[] = TASK_STATUSES.filter(s => s !== 'Done');

const STATUS_LABELS: Record<TaskStatus, string> = {
  Todo: 'Todo',
  InProgress: 'In Progress',
  Done: 'Done',
};

const styles = {
  layout: {
    display: 'grid',
    gridTemplateColumns: '280px 1fr',
    gap: 24,
    alignItems: 'start',
  } satisfies CSSProperties,

  panel: {
    background: 'var(--surface-bg)',
    border: '1px solid var(--border-color)',
    borderRadius: 'var(--radius-md)',
    padding: 20,
  } satisfies CSSProperties,

  label: {
    display: 'block',
    fontSize: 13,
    fontWeight: 600,
    color: 'var(--text-secondary)',
    marginBottom: 6,
    textTransform: 'uppercase' as const,
    letterSpacing: '0.04em',
  } satisfies CSSProperties,

  chipRow: {
    display: 'flex',
    flexWrap: 'wrap' as const,
    gap: 6,
  } satisfies CSSProperties,

  sprintGoalBox: {
    background: 'var(--color-primary-light, #ede9fe)',
    border: '1px solid var(--color-primary)',
    borderRadius: 'var(--radius-md)',
    padding: '14px 18px',
    marginBottom: 20,
  } satisfies CSSProperties,

  reasoningBox: {
    background: 'var(--surface-bg)',
    border: '1px solid var(--border-color)',
    borderRadius: 'var(--radius-md)',
    padding: '12px 16px',
    marginBottom: 20,
    color: 'var(--text-muted)',
    fontStyle: 'italic' as const,
    fontSize: 13,
    lineHeight: 1.6,
  } satisfies CSSProperties,

  table: {
    width: '100%',
    borderCollapse: 'collapse' as const,
  } satisfies CSSProperties,

  th: {
    textAlign: 'left' as const,
    fontSize: 12,
    fontWeight: 600,
    color: 'var(--text-muted)',
    textTransform: 'uppercase' as const,
    letterSpacing: '0.05em',
    padding: '8px 12px',
    borderBottom: '1px solid var(--border-color)',
  } satisfies CSSProperties,

  td: {
    padding: '10px 12px',
    borderBottom: '1px solid var(--border-color)',
    verticalAlign: 'top' as const,
    fontSize: 14,
    color: 'var(--text-primary)',
  } satisfies CSSProperties,

  totalRow: {
    display: 'flex',
    justifyContent: 'flex-end',
    alignItems: 'center',
    gap: 10,
    marginTop: 12,
    paddingTop: 12,
    borderTop: '2px solid var(--border-color)',
    fontSize: 14,
    fontWeight: 600,
    color: 'var(--text-primary)',
  } satisfies CSSProperties,

  spinnerWrap: {
    display: 'flex',
    flexDirection: 'column' as const,
    alignItems: 'center',
    gap: 12,
    padding: '48px 0',
    color: 'var(--text-muted)',
    fontSize: 14,
  } satisfies CSSProperties,
};

export function SprintPlannerPage() {
  const { data: allTasks = [] } = useAllTasks();
  const { mutate: suggestPlan, isPending, data: plan, reset } = useSuggestSprintPlan();

  const [capacity, setCapacity] = useState(DEFAULT_CAPACITY);
  const [includedStatuses, setIncludedStatuses] = useState<Set<TaskStatus>>(
    new Set(NON_DONE_STATUSES),
  );

  const backlog = allTasks.filter(t => includedStatuses.has(t.status));

  function toggleStatus(status: TaskStatus) {
    setIncludedStatuses(prev => {
      const next = new Set(prev);
      if (next.has(status)) {
        next.delete(status);
      } else {
        next.add(status);
      }
      return next;
    });
    reset();
  }

  function handleGenerate() {
    suggestPlan({
      backlog: backlog.map(t => ({
        id: t.id,
        title: t.title,
        description: t.description ?? undefined,
        priority: t.priority,
        status: t.status,
      })),
      sprintCapacity: capacity,
    });
  }

  const totalPoints =
    plan?.suggestedTasks.reduce((sum, t) => sum + t.estimatedPoints, 0) ?? 0;

  return (
    <div>
      <div className="page-header">
        <h1 className="page-title">AI Sprint Planner ✨</h1>
      </div>

      <div style={styles.layout}>
        {/* ── Controls panel ────────────────────────────────────── */}
        <aside style={styles.panel}>
          <div style={{ marginBottom: 20 }}>
            <label htmlFor="sprint-capacity" style={styles.label}>
              Sprint Capacity
            </label>
            <div style={{ display: 'flex', alignItems: 'center', gap: 8 }}>
              <input
                id="sprint-capacity"
                type="number"
                className="tf-input"
                min={1}
                max={200}
                value={capacity}
                onChange={e => {
                  setCapacity(Number(e.target.value));
                  reset();
                }}
                style={{ width: '100%' }}
              />
              <span style={{ fontSize: 12, color: 'var(--text-muted)', whiteSpace: 'nowrap' }}>
                pts
              </span>
            </div>
          </div>

          <div style={{ marginBottom: 20 }}>
            <span style={styles.label}>Include Statuses</span>
            <div style={styles.chipRow}>
              {NON_DONE_STATUSES.map(status => (
                <button
                  key={status}
                  type="button"
                  className={`tf-btn tf-btn-sm ${includedStatuses.has(status) ? 'tf-btn-primary' : 'tf-btn-ghost'}`}
                  onClick={() => toggleStatus(status)}
                >
                  {STATUS_LABELS[status]}
                </button>
              ))}
            </div>
          </div>

          <div style={{ marginBottom: 12, fontSize: 12, color: 'var(--text-muted)' }}>
            {backlog.length} task{backlog.length !== 1 ? 's' : ''} in backlog
          </div>

          <button
            type="button"
            className="tf-btn tf-btn-primary"
            style={{ width: '100%' }}
            disabled={isPending || backlog.length === 0}
            onClick={handleGenerate}
          >
            {isPending ? 'Planning...' : 'Generate Sprint Plan'}
          </button>
        </aside>

        {/* ── Results panel ─────────────────────────────────────── */}
        <section style={styles.panel}>
          {isPending && <LoadingState />}

          {!isPending && !plan && (
            <EmptyOrReadyState hasBacklog={backlog.length > 0} />
          )}

          {!isPending && plan && (
            <PlanResults plan={plan} totalPoints={totalPoints} capacity={capacity} />
          )}
        </section>
      </div>
    </div>
  );
}

function LoadingState() {
  return (
    <div style={styles.spinnerWrap}>
      <Spinner />
      <span>AI is planning your sprint...</span>
    </div>
  );
}

function EmptyOrReadyState({ hasBacklog }: { hasBacklog: boolean }) {
  if (!hasBacklog) {
    return (
      <div className="empty-state">
        <div className="empty-state-icon">📋</div>
        <div className="empty-state-text">No tasks in backlog. Create some tasks first.</div>
      </div>
    );
  }
  return (
    <div className="empty-state">
      <div className="empty-state-icon">✨</div>
      <div className="empty-state-text">
        Configure your sprint capacity and click Generate Sprint Plan to get AI-powered suggestions.
      </div>
    </div>
  );
}

function PlanResults({
  plan,
  totalPoints,
  capacity,
}: {
  plan: SprintPlan;
  totalPoints: number;
  capacity: number;
}) {
  return (
    <>
      {/* Sprint Goal */}
      <div style={styles.sprintGoalBox}>
        <div
          style={{
            fontSize: 11,
            fontWeight: 700,
            textTransform: 'uppercase',
            letterSpacing: '0.06em',
            color: 'var(--color-primary)',
            marginBottom: 6,
          }}
        >
          Sprint Goal
        </div>
        <p
          style={{
            margin: 0,
            fontSize: 15,
            fontWeight: 600,
            color: 'var(--text-primary)',
            lineHeight: 1.5,
          }}
        >
          {plan.sprintGoal}
        </p>
      </div>

      {/* Reasoning */}
      <div style={styles.reasoningBox}>
        <strong style={{ fontStyle: 'normal', color: 'var(--text-secondary)', fontSize: 12 }}>
          AI Reasoning:
        </strong>{' '}
        {plan.reasoning}
      </div>

      {/* Task table */}
      <table style={styles.table}>
        <thead>
          <tr>
            <th style={styles.th}>Task</th>
            <th style={{ ...styles.th, width: 80, textAlign: 'center' }}>Points</th>
            <th style={styles.th}>Justification</th>
          </tr>
        </thead>
        <tbody>
          {plan.suggestedTasks.map(task => (
            <tr key={task.taskId}>
              <td style={{ ...styles.td, fontWeight: 500 }}>{task.title}</td>
              <td style={{ ...styles.td, textAlign: 'center' }}>
                <StoryPointBadge points={task.estimatedPoints} />
              </td>
              <td style={{ ...styles.td, color: 'var(--text-secondary)', fontSize: 13 }}>
                {task.justification}
              </td>
            </tr>
          ))}
        </tbody>
      </table>

      {/* Total */}
      <div style={styles.totalRow}>
        <span style={{ color: 'var(--text-muted)', fontWeight: 400 }}>Total:</span>
        <span
          style={{
            color: totalPoints > capacity ? '#ef4444' : 'var(--color-success, #22c55e)',
          }}
        >
          {totalPoints} / {capacity} pts
        </span>
      </div>
    </>
  );
}

function Spinner() {
  return (
    <>
      <div
        style={{
          width: 32,
          height: 32,
          border: '3px solid var(--border-color)',
          borderTop: '3px solid var(--color-primary)',
          borderRadius: '50%',
          animation: 'spin 0.8s linear infinite',
        }}
      />
      <style>{`@keyframes spin { to { transform: rotate(360deg); } }`}</style>
    </>
  );
}
