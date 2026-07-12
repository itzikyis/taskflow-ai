import { useAllTasks } from '@/features/tasks/hooks/useTasks';
import { useAllDependencies } from '@/features/tasks/hooks/useDependencies';
import type { Task, TaskStatus } from '@/features/tasks/types/task.types';

const DEFAULT_DURATION_DAYS = 3;
const DAY = 24 * 60 * 60 * 1000;

const STATUS_BAR: Record<TaskStatus, string> = {
  Todo: '#94a3b8',
  InProgress: 'var(--status-inprogress, #2563eb)',
  Done: 'var(--status-done, #10b981)',
};

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

  return (
    <div>
      <div className="page-header">
        <div>
          <h1 className="page-title">Timeline 📅</h1>
          <p className="page-subtitle">
            {tasks.length} tasks · {fmt(rangeStart)} – {fmt(rangeEnd)} · finish-to-start dependencies
          </p>
        </div>
      </div>

      <div style={{ border: '1px solid var(--border-color)', borderRadius: 'var(--radius-md)', overflow: 'hidden' }}>
        {bars
          .sort((a, b) => a.start - b.start)
          .map(({ task, start, end }) => {
            const left = ((start - rangeStart) / span) * 100;
            const width = Math.max(((end - start) / span) * 100, 2);
            const blocked = (blockerCount.get(task.id) ?? 0) > 0;
            return (
              <div key={task.id} style={{ display: 'flex', alignItems: 'center', borderBottom: '1px solid var(--border-color)' }}>
                <div style={{
                  width: 220, flexShrink: 0, padding: '8px 12px', fontSize: 13,
                  overflow: 'hidden', textOverflow: 'ellipsis', whiteSpace: 'nowrap',
                  color: 'var(--text-primary)', borderRight: '1px solid var(--border-color)',
                }}>
                  {blocked && <span title={`${blockerCount.get(task.id)} open blocker(s)`}>🚧 </span>}
                  {task.title}
                </div>
                <div style={{ position: 'relative', flex: 1, height: 34 }}>
                  <div
                    title={`${fmt(start)} → ${fmt(end)}`}
                    style={{
                      position: 'absolute', top: 8, height: 18,
                      left: `${left}%`, width: `${width}%`,
                      background: blocked ? 'var(--color-danger)' : STATUS_BAR[task.status],
                      opacity: task.status === 'Done' ? 0.55 : 1,
                      borderRadius: 4,
                    }}
                  />
                </div>
              </div>
            );
          })}
      </div>

      <div style={{ display: 'flex', gap: 16, marginTop: 12, fontSize: 12, color: 'var(--text-muted)' }}>
        <Legend color={STATUS_BAR.Todo} label="To Do" />
        <Legend color={STATUS_BAR.InProgress} label="In Progress" />
        <Legend color={STATUS_BAR.Done} label="Done" />
        <Legend color="var(--color-danger)" label="🚧 Blocked" />
      </div>
    </div>
  );
}

function Legend({ color, label }: { color: string; label: string }) {
  return (
    <span style={{ display: 'inline-flex', alignItems: 'center', gap: 6 }}>
      <span style={{ width: 14, height: 10, borderRadius: 3, background: color, display: 'inline-block' }} />
      {label}
    </span>
  );
}
