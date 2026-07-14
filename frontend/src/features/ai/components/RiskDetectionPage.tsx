import { useState } from 'react';
import { useAllTasks } from '@/features/tasks/hooks/useTasks';
import { useAllDependencies } from '@/features/tasks/hooks/useDependencies';
import { aiService, type SprintRiskAssessment, type RiskLevel } from '@/services/aiService';

const RISK_COLOR: Record<RiskLevel, string> = {
  OnTrack: 'var(--status-done, #10b981)',
  AtRisk: '#f59e0b',
  Blocked: 'var(--color-danger, #ef4444)',
};

const RISK_BG: Record<RiskLevel, string> = {
  OnTrack: '#ecfdf5',
  AtRisk: '#fffbeb',
  Blocked: '#fef2f2',
};

const RISK_LABEL: Record<RiskLevel, string> = {
  OnTrack: '✓ On Track',
  AtRisk: '⚠ At Risk',
  Blocked: '🚫 Blocked',
};

export function RiskDetectionPage() {
  const { data: tasks = [], isLoading: tasksLoading } = useAllTasks();
  const { data: deps = [] } = useAllDependencies();
  const [result, setResult] = useState<SprintRiskAssessment | null>(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const statusById = new Map(tasks.map(t => [t.id, t.status]));
  const blockerCount = new Map<string, number>();
  for (const d of deps) {
    if (statusById.get(d.blockedByTaskId) !== 'Done') {
      blockerCount.set(d.taskId, (blockerCount.get(d.taskId) ?? 0) + 1);
    }
  }

  const runScan = async () => {
    setLoading(true);
    setError(null);
    try {
      const inputs = tasks.map(t => ({
        id: t.id,
        title: t.title,
        status: t.status,
        priority: t.priority,
        createdAt: t.createdAt,
        dueDate: t.dueDate ?? null,
        updatedAt: (t as any).updatedAt ?? null,
        openBlockerCount: blockerCount.get(t.id) ?? 0,
      }));
      const assessment = await aiService.assessRisk(inputs);
      setResult(assessment);
    } catch {
      setError('AI risk scan failed. Check that the AI service is configured.');
    } finally {
      setLoading(false);
    }
  };

  const nonDone = tasks.filter(t => t.status !== 'Done');

  return (
    <div>
      <div className="page-header">
        <div>
          <h1 className="page-title">AI Risk Detection 🛡️</h1>
          <p className="page-subtitle">
            AI scans all active tasks for overdue deadlines, stalled progress, and blocker chains — surfacing risk before it derails delivery.
          </p>
        </div>
        <button
          className="tf-btn tf-btn-primary"
          onClick={runScan}
          disabled={loading || tasksLoading || tasks.length === 0}
        >
          {loading ? '⏳ Scanning…' : '🔍 Run Risk Scan'}
        </button>
      </div>

      {/* Stats row */}
      <div style={{ display: 'grid', gridTemplateColumns: 'repeat(3, 1fr)', gap: 12, marginBottom: 20 }}>
        <StatCard label="Active Tasks" value={nonDone.length} color="var(--text-primary)" />
        <StatCard label="With Blockers" value={[...blockerCount.values()].filter(v => v > 0).length} color="#f59e0b" />
        <StatCard label="Overdue" value={tasks.filter(t => t.status !== 'Done' && t.dueDate && new Date(t.dueDate) < new Date()).length} color="var(--color-danger)" />
      </div>

      {error && (
        <div style={{ padding: 14, background: '#fef2f2', border: '1px solid #fecaca', borderRadius: 'var(--radius-md)', color: '#b91c1c', marginBottom: 16 }}>
          {error}
        </div>
      )}

      {!result && !loading && (
        <div style={{
          padding: 40, textAlign: 'center',
          background: '#ffffff', border: '1px solid var(--border-color)',
          borderRadius: 'var(--radius-md)', boxShadow: 'var(--shadow-sm)',
          color: 'var(--text-muted)',
        }}>
          <div style={{ fontSize: 40, marginBottom: 12 }}>🛡️</div>
          <p style={{ margin: 0, fontSize: 14 }}>Click <strong>Run Risk Scan</strong> to let AI assess the health of your {nonDone.length} active tasks.</p>
          <p style={{ margin: '8px 0 0', fontSize: 12 }}>Analysis considers due dates, last activity, priority, and dependency chains.</p>
        </div>
      )}

      {loading && (
        <div style={{
          padding: 40, textAlign: 'center',
          background: '#ffffff', border: '1px solid var(--border-color)',
          borderRadius: 'var(--radius-md)', boxShadow: 'var(--shadow-sm)',
          color: 'var(--text-muted)',
        }}>
          <div style={{ fontSize: 32, marginBottom: 12 }}>⏳</div>
          <p style={{ margin: 0 }}>AI is analyzing {tasks.length} tasks…</p>
        </div>
      )}

      {result && !loading && (
        <>
          {/* Summary banner */}
          <div style={{
            padding: '14px 18px',
            background: result.blockedCount > 0 ? '#fef2f2' : result.atRiskCount > 0 ? '#fffbeb' : '#ecfdf5',
            border: `1px solid ${result.blockedCount > 0 ? '#fecaca' : result.atRiskCount > 0 ? '#fde68a' : '#a7f3d0'}`,
            borderRadius: 'var(--radius-md)', marginBottom: 16,
          }}>
            <div style={{ display: 'flex', gap: 24, marginBottom: 8 }}>
              <Counter value={result.onTrackCount} label="On Track" color={RISK_COLOR.OnTrack} />
              <Counter value={result.atRiskCount} label="At Risk" color={RISK_COLOR.AtRisk} />
              <Counter value={result.blockedCount} label="Blocked" color={RISK_COLOR.Blocked} />
            </div>
            <p style={{ margin: 0, fontSize: 13, color: 'var(--text-secondary)' }}>{result.summary}</p>
          </div>

          {/* Recommendations */}
          {result.recommendations.length > 0 && (
            <div style={{
              padding: '12px 16px', marginBottom: 16,
              background: '#ffffff', border: '1px solid var(--border-color)',
              borderRadius: 'var(--radius-md)', boxShadow: 'var(--shadow-sm)',
            }}>
              <p style={{ margin: '0 0 8px', fontWeight: 600, fontSize: 13 }}>💡 Recommendations</p>
              <ul style={{ margin: 0, paddingLeft: 20, fontSize: 13, color: 'var(--text-secondary)', display: 'flex', flexDirection: 'column', gap: 4 }}>
                {result.recommendations.map((r, i) => <li key={i}>{r}</li>)}
              </ul>
            </div>
          )}

          {/* Task risk table */}
          <div style={{
            border: '1px solid var(--border-color)', borderRadius: 'var(--radius-md)',
            overflow: 'hidden', background: '#ffffff', boxShadow: 'var(--shadow-sm)',
          }}>
            <table style={{ width: '100%', borderCollapse: 'collapse', fontSize: 13 }}>
              <thead>
                <tr style={{ background: 'var(--surface-bg)', textAlign: 'left' }}>
                  <Th w="40%">Task</Th>
                  <Th w="130px">Risk Level</Th>
                  <Th>Reason</Th>
                </tr>
              </thead>
              <tbody>
                {[...result.tasks]
                  .sort((a, b) => {
                    const order: Record<RiskLevel, number> = { Blocked: 0, AtRisk: 1, OnTrack: 2 };
                    return order[a.level] - order[b.level];
                  })
                  .map(t => (
                    <tr key={t.taskId} style={{ borderTop: '1px solid var(--border-color)' }}>
                      <td style={{ padding: '8px 12px', color: 'var(--text-primary)' }}>{t.title}</td>
                      <td style={{ padding: '8px 12px' }}>
                        <span style={{
                          display: 'inline-block', padding: '2px 8px', borderRadius: 12, fontSize: 11, fontWeight: 600,
                          background: RISK_BG[t.level], color: RISK_COLOR[t.level],
                          border: `1px solid ${RISK_COLOR[t.level]}40`,
                        }}>
                          {RISK_LABEL[t.level]}
                        </span>
                      </td>
                      <td style={{ padding: '8px 12px', color: 'var(--text-secondary)' }}>{t.reason}</td>
                    </tr>
                  ))}
              </tbody>
            </table>
          </div>
        </>
      )}
    </div>
  );
}

function StatCard({ label, value, color }: { label: string; value: number; color: string }) {
  return (
    <div style={{
      padding: '14px 18px', background: '#ffffff',
      border: '1px solid var(--border-color)', borderRadius: 'var(--radius-md)',
      boxShadow: 'var(--shadow-sm)',
    }}>
      <div style={{ fontSize: 24, fontWeight: 700, color }}>{value}</div>
      <div style={{ fontSize: 12, color: 'var(--text-muted)', marginTop: 2 }}>{label}</div>
    </div>
  );
}

function Counter({ value, label, color }: { value: number; label: string; color: string }) {
  return (
    <span style={{ display: 'flex', alignItems: 'baseline', gap: 6 }}>
      <span style={{ fontSize: 22, fontWeight: 700, color }}>{value}</span>
      <span style={{ fontSize: 12, color: 'var(--text-muted)' }}>{label}</span>
    </span>
  );
}

function Th({ children, w }: { children?: React.ReactNode; w?: string }) {
  return (
    <th style={{ padding: '8px 12px', width: w, fontSize: 11, textTransform: 'uppercase', letterSpacing: '0.04em', fontWeight: 600, color: 'var(--text-muted)' }}>
      {children}
    </th>
  );
}
