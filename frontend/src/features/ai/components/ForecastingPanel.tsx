import { useState } from 'react';
import type { CSSProperties } from 'react';
import { useAllTasks } from '@/features/tasks/hooks/useTasks';
import { aiService } from '@/services/aiService';

const MS_PER_DAY = 1000 * 60 * 60 * 24;
const MS_PER_WEEK = MS_PER_DAY * 7;
const FOUR_WEEKS_MS = MS_PER_WEEK * 4;

const RISK_THRESHOLDS_DAYS = {
  onTrack: 7,
  atRisk: 14,
} as const;

type RiskStatus = 'OnTrack' | 'AtRisk' | 'OffTrack';

interface ForecastMetrics {
  totalTasks: number;
  doneTasks: number;
  inProgress: number;
  pctDone: number;
  velocity: number;
  projectedDate: Date | null;
  riskStatus: RiskStatus;
  earliestDueDate: Date | null;
}

function computeMetrics(tasks: ReturnType<typeof useAllTasks>['data']): ForecastMetrics {
  const allTasks = tasks ?? [];
  const totalTasks = allTasks.length;
  const doneTasks = allTasks.filter(t => t.status === 'Done').length;
  const inProgress = allTasks.filter(t => t.status === 'InProgress').length;
  const pctDone = totalTasks === 0 ? 0 : Math.round((doneTasks / totalTasks) * 100);

  const now = Date.now();
  const fourWeeksAgo = now - FOUR_WEEKS_MS;
  const recentlyDone = allTasks.filter(t => {
    if (t.status !== 'Done') return false;
    const updated = t.updatedAt ? new Date(t.updatedAt).getTime() : 0;
    return updated >= fourWeeksAgo;
  });
  const velocity = Math.max(recentlyDone.length / 4, 0.1);

  const remaining = totalTasks - doneTasks;
  const projectedDate =
    remaining === 0 ? new Date(now) : new Date(now + (remaining / velocity) * MS_PER_WEEK);

  const remainingTasks = allTasks.filter(t => t.status !== 'Done');
  const dueDates = remainingTasks
    .map(t => (t.dueDate ? new Date(t.dueDate).getTime() : null))
    .filter((d): d is number => d !== null);
  const earliestDueDate = dueDates.length > 0 ? new Date(Math.min(...dueDates)) : null;

  let riskStatus: RiskStatus = 'OnTrack';
  if (projectedDate && earliestDueDate) {
    const daysDiff = (projectedDate.getTime() - earliestDueDate.getTime()) / MS_PER_DAY;
    if (daysDiff > RISK_THRESHOLDS_DAYS.atRisk) {
      riskStatus = 'OffTrack';
    } else if (daysDiff > RISK_THRESHOLDS_DAYS.onTrack) {
      riskStatus = 'AtRisk';
    }
  }

  return {
    totalTasks,
    doneTasks,
    inProgress,
    pctDone,
    velocity,
    projectedDate,
    riskStatus,
    earliestDueDate,
  };
}

const RISK_CONFIG: Record<RiskStatus, { label: string; color: string; bg: string }> = {
  OnTrack: { label: 'On Track', color: '#16a34a', bg: '#dcfce7' },
  AtRisk: { label: 'At Risk', color: '#ca8a04', bg: '#fef9c3' },
  OffTrack: { label: 'Off Track', color: 'var(--color-danger)', bg: '#fee2e2' },
};

const styles = {
  section: {
    background: 'var(--surface-bg)',
    border: '1px solid var(--surface-border, var(--border-color))',
    borderRadius: 'var(--radius-md)',
    padding: 24,
    marginTop: 24,
  } satisfies CSSProperties,

  sectionTitle: {
    fontSize: 18,
    fontWeight: 700,
    color: 'var(--text-primary)',
    marginBottom: 20,
    display: 'flex',
    alignItems: 'center',
    gap: 8,
  } satisfies CSSProperties,

  statsGrid: {
    display: 'grid',
    gridTemplateColumns: 'repeat(auto-fit, minmax(140px, 1fr))',
    gap: 12,
    marginBottom: 20,
  } satisfies CSSProperties,

  statCard: {
    background: 'var(--surface-card, var(--surface-bg))',
    border: '1px solid var(--surface-border, var(--border-color))',
    borderRadius: 'var(--radius-md)',
    padding: '14px 16px',
  } satisfies CSSProperties,

  statLabel: {
    fontSize: 11,
    fontWeight: 600,
    textTransform: 'uppercase' as const,
    letterSpacing: '0.06em',
    color: 'var(--text-muted)',
    marginBottom: 6,
  } satisfies CSSProperties,

  statValue: {
    fontSize: 22,
    fontWeight: 700,
    color: 'var(--text-primary)',
  } satisfies CSSProperties,

  riskBadge: (status: RiskStatus): CSSProperties => ({
    display: 'inline-flex',
    alignItems: 'center',
    gap: 6,
    padding: '4px 12px',
    borderRadius: 9999,
    fontSize: 13,
    fontWeight: 600,
    background: RISK_CONFIG[status].bg,
    color: RISK_CONFIG[status].color,
    border: `1px solid ${RISK_CONFIG[status].color}`,
  }),

  aiBox: {
    background: 'var(--surface-card, var(--surface-bg))',
    border: '1px solid var(--surface-border, var(--border-color))',
    borderRadius: 'var(--radius-md)',
    padding: '16px 20px',
    marginTop: 16,
    fontSize: 14,
    lineHeight: 1.7,
    color: 'var(--text-secondary)',
  } satisfies CSSProperties,

  errorBox: {
    background: '#fee2e2',
    border: '1px solid var(--color-danger)',
    borderRadius: 'var(--radius-md)',
    padding: '12px 16px',
    marginTop: 16,
    fontSize: 13,
    color: 'var(--color-danger)',
  } satisfies CSSProperties,

  spinnerWrap: {
    display: 'flex',
    alignItems: 'center',
    gap: 10,
    padding: '16px 0',
    color: 'var(--text-muted)',
    fontSize: 13,
  } satisfies CSSProperties,
};

function Spinner() {
  return (
    <>
      <div
        style={{
          width: 20,
          height: 20,
          border: '2px solid var(--border-color)',
          borderTop: '2px solid var(--color-primary)',
          borderRadius: '50%',
          animation: 'spin 0.8s linear infinite',
          flexShrink: 0,
        }}
      />
      <style>{`@keyframes forecast-spin { to { transform: rotate(360deg); } }`}</style>
    </>
  );
}

function StatCard({ label, value }: { label: string; value: string }) {
  return (
    <div style={styles.statCard}>
      <div style={styles.statLabel}>{label}</div>
      <div style={styles.statValue}>{value}</div>
    </div>
  );
}

function formatDate(date: Date | null): string {
  if (!date) return 'N/A';
  return date.toLocaleDateString(undefined, { month: 'short', day: 'numeric', year: 'numeric' });
}

export function ForecastingPanel() {
  const { data: tasks } = useAllTasks();
  const [aiResponse, setAiResponse] = useState<string | null>(null);
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const metrics = computeMetrics(tasks);
  const riskConfig = RISK_CONFIG[metrics.riskStatus];

  async function handleAskAi() {
    setIsLoading(true);
    setAiResponse(null);
    setError(null);

    const velocityRounded = Math.round(metrics.velocity * 10) / 10;
    const question = `Given these sprint metrics: ${metrics.totalTasks} total tasks, ${metrics.doneTasks} completed (${metrics.pctDone}%), ${metrics.inProgress} in progress. Average weekly velocity: ${velocityRounded} tasks/week. What is the risk level for completing this sprint on time, and what can the team do to improve? Respond in 3-4 sentences.`;

    try {
      const result = await aiService.askForecast(question);
      setAiResponse(result.answer);
    } catch {
      setError('Failed to get AI forecast. Please try again.');
    } finally {
      setIsLoading(false);
    }
  }

  return (
    <div style={styles.section}>
      <div style={styles.sectionTitle}>
        <span>📈</span>
        <span>Completion Forecast</span>
      </div>

      {/* Risk indicator */}
      <div style={{ marginBottom: 20, display: 'flex', alignItems: 'center', gap: 12 }}>
        <span style={{ fontSize: 13, color: 'var(--text-secondary)', fontWeight: 500 }}>
          Sprint Risk:
        </span>
        <span style={styles.riskBadge(metrics.riskStatus)}>
          {metrics.riskStatus === 'OnTrack' && '✓ '}
          {metrics.riskStatus === 'AtRisk' && '⚠ '}
          {metrics.riskStatus === 'OffTrack' && '✗ '}
          {riskConfig.label}
        </span>
        {metrics.earliestDueDate && (
          <span style={{ fontSize: 12, color: 'var(--text-muted)' }}>
            Earliest due: {formatDate(metrics.earliestDueDate)}
          </span>
        )}
      </div>

      {/* Summary stats */}
      <div style={styles.statsGrid}>
        <StatCard label="Total Tasks" value={String(metrics.totalTasks)} />
        <StatCard label="Done" value={String(metrics.doneTasks)} />
        <StatCard label="In Progress" value={String(metrics.inProgress)} />
        <StatCard label="% Complete" value={`${metrics.pctDone}%`} />
        <StatCard label="Avg Velocity" value={`${(Math.round(metrics.velocity * 10) / 10)}/wk`} />
        <StatCard label="Projected Done" value={formatDate(metrics.projectedDate)} />
      </div>

      {/* AI forecast button */}
      <button
        type="button"
        className="tf-btn tf-btn-primary"
        onClick={handleAskAi}
        disabled={isLoading || metrics.totalTasks === 0}
      >
        {isLoading ? 'Asking AI...' : 'Ask AI to Forecast'}
      </button>

      {isLoading && (
        <div style={styles.spinnerWrap}>
          <Spinner />
          <span>Analyzing sprint metrics...</span>
        </div>
      )}

      {error && <div style={styles.errorBox}>{error}</div>}

      {aiResponse && !isLoading && (
        <div style={styles.aiBox}>
          <div
            style={{
              fontSize: 11,
              fontWeight: 700,
              textTransform: 'uppercase',
              letterSpacing: '0.06em',
              color: 'var(--color-primary)',
              marginBottom: 8,
            }}
          >
            AI Forecast
          </div>
          {aiResponse}
        </div>
      )}
    </div>
  );
}
