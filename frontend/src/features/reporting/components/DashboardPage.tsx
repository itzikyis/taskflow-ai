import { useDashboardMetrics } from '../hooks/useReporting';
import type { DashboardMetrics } from '@/services/reportingService';

const card = {
  background: '#ffffff',
  border: '1px solid var(--border-color)',
  borderRadius: 'var(--radius-md)',
  boxShadow: 'var(--shadow-sm)',
  padding: 18,
};

export function DashboardPage() {
  const { data, isLoading, isError } = useDashboardMetrics();

  if (isLoading) return <div className="empty-state"><div className="empty-state-icon">⌛</div><p className="empty-state-text">Loading metrics…</p></div>;
  if (isError || !data) return <div className="empty-state"><div className="empty-state-icon">⚠️</div><p className="empty-state-text">Could not load metrics.</p></div>;

  const pct = (n: number) => (data.total === 0 ? 0 : Math.round((n / data.total) * 100));

  return (
    <div>
      <div className="page-header">
        <div>
          <h1 className="page-title">Dashboard 📊</h1>
          <p className="page-subtitle">{data.total} tasks · {pct(data.done)}% complete</p>
        </div>
      </div>

      {/* Summary cards */}
      <div style={{ display: 'grid', gridTemplateColumns: 'repeat(5, 1fr)', gap: 20, marginBottom: 28 }}>
        <Stat label="To Do" value={data.todo} color="#94a3b8" />
        <Stat label="In Progress" value={data.inProgress} color="var(--status-inprogress, #2563eb)" />
        <Stat label="In Review" value={data.inReview} color="var(--status-inreview, #8b5cf6)" />
        <Stat label="Done" value={data.done} color="var(--status-done, #10b981)" />
        <Stat label="Total" value={data.total} color="var(--color-primary)" />
      </div>

      <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: 20, alignItems: 'start' }}>
        {/* Status breakdown */}
        <div style={card}>
          <h3 style={{ margin: '0 0 14px', fontSize: 15 }}>Status breakdown</h3>
          <Bar label="To Do" value={data.todo} total={data.total} color="#94a3b8" />
          <Bar label="In Progress" value={data.inProgress} total={data.total} color="#2563eb" />
          <Bar label="In Review" value={data.inReview} total={data.total} color="#8b5cf6" />
          <Bar label="Done" value={data.done} total={data.total} color="#10b981" />
        </div>

        {/* Priority distribution */}
        <div style={card}>
          <h3 style={{ margin: '0 0 14px', fontSize: 15 }}>Priority distribution</h3>
          <Bar label="Critical" value={data.critical} total={data.total} color="#7c3aed" />
          <Bar label="High" value={data.high} total={data.total} color="#ef4444" />
          <Bar label="Medium" value={data.medium} total={data.total} color="#f59e0b" />
          <Bar label="Low" value={data.low} total={data.total} color="#10b981" />
        </div>

        {/* Workload by member */}
        <div style={card}>
          <h3 style={{ margin: '0 0 14px', fontSize: 15 }}>Workload (open tasks)</h3>
          {data.workload.length === 0
            ? <p style={{ fontSize: 13, color: 'var(--text-muted)' }}>No open tasks.</p>
            : (() => {
                const max = Math.max(...data.workload.map(w => w.openTasks), 1);
                return data.workload.map(w => (
                  <Bar
                    key={w.userId ?? 'unassigned'}
                    label={w.userId ? `${w.userId.slice(0, 8)}…` : 'Unassigned'}
                    value={w.openTasks}
                    total={max}
                    color="var(--color-primary)"
                    suffix=""
                  />
                ));
              })()}
        </div>

        {/* Completion trend (burndown proxy) */}
        <div style={card}>
          <h3 style={{ margin: '0 0 14px', fontSize: 15 }}>Completed over time</h3>
          <CompletionChart data={data} />
        </div>
      </div>
    </div>
  );
}

function Stat({ label, value, color }: { label: string; value: number; color: string }) {
  return (
    <div style={card}>
      <div style={{ fontSize: 28, fontWeight: 800, color }}>{value}</div>
      <div style={{ fontSize: 12, color: 'var(--text-muted)', textTransform: 'uppercase', letterSpacing: '0.04em' }}>{label}</div>
    </div>
  );
}

function Bar({ label, value, total, color, suffix = '' }: { label: string; value: number; total: number; color: string; suffix?: string }) {
  const width = total === 0 ? 0 : Math.round((value / total) * 100);
  return (
    <div style={{ marginBottom: 10 }}>
      <div style={{ display: 'flex', justifyContent: 'space-between', fontSize: 12, marginBottom: 3, color: 'var(--text-secondary)' }}>
        <span>{label}</span>
        <span>{value}{suffix}</span>
      </div>
      <div style={{ height: 8, background: 'var(--surface-bg)', borderRadius: 4, overflow: 'hidden' }}>
        <div style={{ width: `${width}%`, height: '100%', background: color, borderRadius: 4 }} />
      </div>
    </div>
  );
}

function CompletionChart({ data }: { data: DashboardMetrics }) {
  const points = data.completionTrend;
  if (points.length === 0) {
    return <p style={{ fontSize: 13, color: 'var(--text-muted)' }}>No completed tasks yet.</p>;
  }
  const max = Math.max(...points.map(p => p.completed), 1);
  return (
    <div style={{ display: 'flex', alignItems: 'flex-end', gap: 6, height: 140, paddingTop: 8 }}>
      {points.map(p => (
        <div key={p.date} style={{ flex: 1, display: 'flex', flexDirection: 'column', alignItems: 'center', gap: 4 }} title={`${p.date}: ${p.completed}`}>
          <div style={{
            width: '100%', maxWidth: 40,
            height: `${Math.round((p.completed / max) * 110)}px`,
            background: 'var(--status-done, #10b981)', borderRadius: '4px 4px 0 0',
          }} />
          <span style={{ fontSize: 10, color: 'var(--text-muted)' }}>{p.date.slice(5)}</span>
        </div>
      ))}
    </div>
  );
}
