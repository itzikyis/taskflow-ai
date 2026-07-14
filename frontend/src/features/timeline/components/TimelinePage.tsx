import { useAllTasks } from '@/features/tasks/hooks/useTasks';
import { useAllDependencies } from '@/features/tasks/hooks/useDependencies';
import type { Task, TaskStatus } from '@/features/tasks/types/task.types';

const DEFAULT_DURATION_DAYS = 3;
const DAY = 24 * 60 * 60 * 1000;

const STATUS_BAR: Record<TaskStatus, string> = {
  Todo: '#94a3b8',
  InProgress: 'var(--status-inprogress, #2563eb)',
  InReview: 'var(--status-inreview, #8b5cf6)',
  Done: 'var(--status-done, #10b981)',
};

const STATUS_LABEL: Record<TaskStatus, string> = {
  Todo: 'To Do',
  InProgress: 'In Progress',
  InReview: 'In Review',
  Done: 'Done',
};

const AXIS_TICKS = 6;
const ROW_H = 38;
const LABEL_W = 220;

interface Bar {
  task: Task;
  start: number;
  end: number;
}

export function TimelinePage() {
  const { data: tasks = [], isLoading } = useAllTasks();
  const { data: deps = [] } = useAllDependencies();

  if (isLoading) {
    return <div className="empty-state"><div className="empty-state-icon">⌛</div><p className="empty-state-text">Loading timeline…</p></div>;
  }
  if (tasks.length === 0) {
    return <div className="empty-state"><div className="empty-state-icon">📅</div><p className="empty-state-text">No tasks to schedule yet.</p></div>;
  }

  const bars: Bar[] = tasks.map(t => {
    const start = new Date(t.createdAt).getTime();
    const end = t.dueDate ? new Date(t.dueDate).getTime() : start + DEFAULT_DURATION_DAYS * DAY;
    return { task: t, start, end: Math.max(end, start + DAY) };
  });

  const rangeStart = Math.min(...bars.map(b => b.start));
  const rangeEnd = Math.max(...bars.map(b => b.end));
  const span = Math.max(rangeEnd - rangeStart, DAY);

  const statusById = new Map(tasks.map(t => [t.id, t.status]));
  const blockerCount = new Map<string, number>();
  for (const d of deps) {
    if (statusById.get(d.blockedByTaskId) !== 'Done') {
      blockerCount.set(d.taskId, (blockerCount.get(d.taskId) ?? 0) + 1);
    }
  }

  const fmt = (ms: number) => new Date(ms).toLocaleDateString(undefined, { month: 'short', day: 'numeric' });
  const fmtFull = (ms: number) => new Date(ms).toLocaleDateString(undefined, { month: 'short', day: 'numeric', year: 'numeric' });

  const todayMs = Date.now();
  const todayPct = ((todayMs - rangeStart) / span) * 100;
  const showToday = todayPct >= 0 && todayPct <= 100;

  const ticks: number[] = Array.from({ length: AXIS_TICKS }, (_, i) =>
    rangeStart + (span / (AXIS_TICKS - 1)) * i
  );

  const sortedBars = [...bars].sort((a, b) => a.start - b.start);

  const hasDue = tasks.some(t => t.dueDate);

  return (
    <div>
      <div className="page-header">
        <div>
          <h1 className="page-title">Timeline 📅</h1>
          <p className="page-subtitle">
            Each bar spans from when a task was created to its due date.
            {!hasDue && ' Tasks without a due date show a 3-day estimate.'}
          </p>
        </div>
      </div>

      {/* Legend */}
      <div style={{
        display: 'flex', flexWrap: 'wrap', gap: '8px 20px', marginBottom: 14,
        padding: '10px 14px',
        background: '#ffffff', border: '1px solid var(--border-color)',
        borderRadius: 'var(--radius-md)', boxShadow: 'var(--shadow-sm)',
        fontSize: 12, color: 'var(--text-secondary)',
      }}>
        <span style={{ fontWeight: 600, color: 'var(--text-primary)', marginRight: 4 }}>Colors:</span>
        {(['Todo', 'InProgress', 'InReview', 'Done'] as TaskStatus[]).map(s => (
          <LegendItem key={s} color={STATUS_BAR[s]} label={STATUS_LABEL[s]} opacity={s === 'Done' ? 0.55 : 1} />
        ))}
        <LegendItem color="var(--color-danger)" label="Blocked (open blocker)" />
        {showToday && (
          <span style={{ display: 'inline-flex', alignItems: 'center', gap: 6 }}>
            <span style={{ width: 2, height: 12, background: '#f59e0b', display: 'inline-block', borderRadius: 1 }} />
            Today ({fmt(todayMs)})
          </span>
        )}
        <span style={{ marginLeft: 'auto', color: 'var(--text-muted)', fontStyle: 'italic' }}>
          Hover a bar for exact dates · 🚧 = blocked by an unfinished dependency
        </span>
      </div>

      {/* Chart */}
      <div style={{
        border: '1px solid var(--border-color)', borderRadius: 'var(--radius-md)',
        overflow: 'hidden', background: '#ffffff', boxShadow: 'var(--shadow-sm)',
      }}>
        {/* Date axis */}
        <div style={{
          display: 'flex', borderBottom: '2px solid var(--border-color)',
          background: 'var(--surface-bg)',
        }}>
          {/* Spacer for task label column */}
          <div style={{ width: LABEL_W, flexShrink: 0, borderRight: '1px solid var(--border-color)', padding: '6px 12px', fontSize: 11, fontWeight: 600, color: 'var(--text-muted)', textTransform: 'uppercase', letterSpacing: '0.04em' }}>
            Task
          </div>
          <div style={{ position: 'relative', flex: 1, height: 32 }}>
            {ticks.map((ms, i) => {
              const pct = ((ms - rangeStart) / span) * 100;
              return (
                <span
                  key={i}
                  style={{
                    position: 'absolute', left: `${pct}%`,
                    transform: i === AXIS_TICKS - 1 ? 'translateX(-100%)' : i === 0 ? 'none' : 'translateX(-50%)',
                    top: 8, fontSize: 11, color: 'var(--text-muted)', whiteSpace: 'nowrap',
                  }}
                >
                  {fmt(ms)}
                </span>
              );
            })}
            {/* Tick lines */}
            {ticks.map((ms, i) => {
              const pct = ((ms - rangeStart) / span) * 100;
              return (
                <span key={`line-${i}`} style={{
                  position: 'absolute', left: `${pct}%`,
                  top: 24, width: 1, height: 8, background: 'var(--border-color)',
                }} />
              );
            })}
          </div>
        </div>

        {/* Bars */}
        {sortedBars.map(({ task, start, end }) => {
          const left = ((start - rangeStart) / span) * 100;
          const width = Math.max(((end - start) / span) * 100, 1.5);
          const blocked = (blockerCount.get(task.id) ?? 0) > 0;
          const hasEstimate = !task.dueDate;

          return (
            <div
              key={task.id}
              style={{ display: 'flex', alignItems: 'center', borderBottom: '1px solid var(--border-color)', height: ROW_H }}
            >
              {/* Task label */}
              <div style={{
                width: LABEL_W, flexShrink: 0, padding: '0 12px', fontSize: 13,
                overflow: 'hidden', textOverflow: 'ellipsis', whiteSpace: 'nowrap',
                color: 'var(--text-primary)', borderRight: '1px solid var(--border-color)',
              }}>
                {blocked && <span title={`${blockerCount.get(task.id)} open blocker(s) — task cannot progress`}>🚧 </span>}
                {task.title}
              </div>

              {/* Bar track */}
              <div style={{ position: 'relative', flex: 1, height: '100%' }}>
                {/* Tick grid lines */}
                {ticks.map((ms, i) => {
                  const pct = ((ms - rangeStart) / span) * 100;
                  return (
                    <span key={i} style={{
                      position: 'absolute', left: `${pct}%`,
                      top: 0, bottom: 0, width: 1,
                      background: 'var(--border-color)', opacity: 0.4,
                    }} />
                  );
                })}

                {/* Today line */}
                {showToday && (
                  <span style={{
                    position: 'absolute', left: `${todayPct}%`,
                    top: 0, bottom: 0, width: 2,
                    background: '#f59e0b', opacity: 0.85, zIndex: 1,
                  }} title={`Today: ${fmtFull(todayMs)}`} />
                )}

                {/* Task bar */}
                <div
                  title={`${task.title}\nStatus: ${STATUS_LABEL[task.status]}\n${blocked ? '🚧 Blocked by unfinished task(s)\n' : ''}Created: ${fmtFull(start)}\n${task.dueDate ? `Due: ${fmtFull(end)}` : `Est. end: ${fmtFull(end)} (no due date set)`}`}
                  style={{
                    position: 'absolute',
                    top: '50%', transform: 'translateY(-50%)',
                    height: 18,
                    left: `${left}%`, width: `${width}%`,
                    background: blocked ? 'var(--color-danger)' : STATUS_BAR[task.status],
                    opacity: task.status === 'Done' ? 0.55 : 1,
                    borderRadius: 4,
                    cursor: 'default',
                    outline: hasEstimate ? '1px dashed rgba(0,0,0,0.2)' : 'none',
                    zIndex: 2,
                  }}
                />
              </div>
            </div>
          );
        })}
      </div>

      {/* Footer note */}
      <p style={{ marginTop: 8, fontSize: 11, color: 'var(--text-muted)' }}>
        Dashed border = estimated end date (no due date set). Done tasks shown at 55% opacity.
        {showToday && ` Amber line marks today (${fmt(todayMs)}).`}
      </p>
    </div>
  );
}

function LegendItem({ color, label, opacity = 1 }: { color: string; label: string; opacity?: number }) {
  return (
    <span style={{ display: 'inline-flex', alignItems: 'center', gap: 6 }}>
      <span style={{ width: 16, height: 10, borderRadius: 3, background: color, opacity, display: 'inline-block', flexShrink: 0 }} />
      {label}
    </span>
  );
}
