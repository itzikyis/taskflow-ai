import { useTasks } from '@/features/tasks/hooks/useTasks';
import { TASK_PRIORITIES } from '@/features/tasks/types/task.types';
import type { TaskPriority } from '@/features/tasks/types/task.types';

// ── Constants ────────────────────────────────────────────────────────────────

const COLOR_BAR        = '#2563eb';
const COLOR_BURNDOWN   = '#10b981';
const COLOR_LOW        = '#94a3b8';
const COLOR_MEDIUM     = '#f59e0b';
const COLOR_HIGH       = '#f97316';
const COLOR_CRITICAL   = '#ef4444';

const PRIORITY_COLORS: Record<TaskPriority, string> = {
  Low:      COLOR_LOW,
  Medium:   COLOR_MEDIUM,
  High:     COLOR_HIGH,
  Critical: COLOR_CRITICAL,
};

const CHART_HEIGHT  = 180;
const CHART_PADDING = 40;
const WEEKS_BACK    = 8;
const DAYS_BACK     = 30;

const card: React.CSSProperties = {
  background: 'var(--surface-card, #ffffff)',
  border: '1px solid var(--surface-border, #e2e8f0)',
  borderRadius: 'var(--radius-md, 8px)',
  padding: 20,
  marginBottom: 20,
};

// ── Helpers ──────────────────────────────────────────────────────────────────

function startOfWeek(date: Date): Date {
  const d = new Date(date);
  d.setHours(0, 0, 0, 0);
  d.setDate(d.getDate() - d.getDay());
  return d;
}

function startOfDay(date: Date): Date {
  const d = new Date(date);
  d.setHours(0, 0, 0, 0);
  return d;
}

function addDays(date: Date, days: number): Date {
  const d = new Date(date);
  d.setDate(d.getDate() + days);
  return d;
}

// ── Stat strip ────────────────────────────────────────────────────────────────

interface StatProps {
  label: string;
  value: string | number;
  color?: string;
}

function Stat({ label, value, color }: StatProps) {
  return (
    <div style={card}>
      <div style={{ fontSize: 28, fontWeight: 800, color: color ?? 'var(--color-primary, #2563eb)' }}>
        {value}
      </div>
      <div style={{ fontSize: 12, color: 'var(--text-muted, #94a3b8)', textTransform: 'uppercase', letterSpacing: '0.05em', marginTop: 4 }}>
        {label}
      </div>
    </div>
  );
}

// ── Velocity chart ────────────────────────────────────────────────────────────

interface WeekBucket {
  label: string;
  count: number;
}

function VelocityChart({ weeks }: { weeks: WeekBucket[] }) {
  const max = Math.max(...weeks.map((w) => w.count), 1);
  const barAreaHeight = CHART_HEIGHT - CHART_PADDING;

  return (
    <svg
      width="100%"
      viewBox={`0 0 ${WEEKS_BACK * 60} ${CHART_HEIGHT}`}
      preserveAspectRatio="xMidYMid meet"
      aria-label="Weekly velocity bar chart"
      role="img"
    >
      {weeks.map((w, i) => {
        const barH    = Math.round((w.count / max) * barAreaHeight);
        const x       = i * 60 + 10;
        const barW    = 36;
        const barY    = barAreaHeight - barH;
        return (
          <g key={w.label}>
            <rect
              x={x}
              y={barY}
              width={barW}
              height={barH}
              rx={4}
              fill={COLOR_BAR}
              opacity={0.85}
            >
              <title>{`${w.label}: ${w.count} tasks completed`}</title>
            </rect>
            <text
              x={x + barW / 2}
              y={barY - 4}
              textAnchor="middle"
              fontSize={10}
              fill="var(--text-secondary, #475569)"
            >
              {w.count > 0 ? w.count : ''}
            </text>
            <text
              x={x + barW / 2}
              y={CHART_HEIGHT - 6}
              textAnchor="middle"
              fontSize={10}
              fill="var(--text-muted, #94a3b8)"
            >
              {w.label}
            </text>
          </g>
        );
      })}
      {/* baseline */}
      <line
        x1={0}
        y1={barAreaHeight}
        x2={WEEKS_BACK * 60}
        y2={barAreaHeight}
        stroke="var(--surface-border, #e2e8f0)"
        strokeWidth={1}
      />
    </svg>
  );
}

// ── Burndown chart ────────────────────────────────────────────────────────────

interface DayPoint {
  day: number; // 0-based index
  remaining: number;
}

function BurndownChart({ points }: { points: DayPoint[] }) {
  const maxRemaining = Math.max(...points.map((p) => p.remaining), 1);
  const svgW = DAYS_BACK * 14;
  const svgH = CHART_HEIGHT;
  const padB = CHART_PADDING;
  const plotH = svgH - padB;

  const toX = (day: number) => day * (svgW / (DAYS_BACK - 1));
  const toY = (v: number)   => plotH - Math.round((v / maxRemaining) * (plotH - 4));

  const polylinePoints = points
    .map((p) => `${toX(p.day)},${toY(p.remaining)}`)
    .join(' ');

  return (
    <svg
      width="100%"
      viewBox={`0 0 ${svgW} ${svgH}`}
      preserveAspectRatio="xMidYMid meet"
      aria-label="Burndown line chart"
      role="img"
    >
      {/* gridlines */}
      {[0, 0.25, 0.5, 0.75, 1].map((frac) => {
        const y = toY(maxRemaining * frac);
        return (
          <line
            key={frac}
            x1={0}
            y1={y}
            x2={svgW}
            y2={y}
            stroke="var(--surface-border, #e2e8f0)"
            strokeWidth={1}
            strokeDasharray="4 4"
          />
        );
      })}
      <polyline
        points={polylinePoints}
        fill="none"
        stroke={COLOR_BURNDOWN}
        strokeWidth={2.5}
        strokeLinejoin="round"
        strokeLinecap="round"
      />
      {/* area fill */}
      <polyline
        points={`0,${plotH} ${polylinePoints} ${svgW},${plotH}`}
        fill={COLOR_BURNDOWN}
        fillOpacity={0.08}
        stroke="none"
      />
      {/* x-axis labels every 5 days */}
      {points
        .filter((p) => p.day % 5 === 0)
        .map((p) => (
          <text
            key={p.day}
            x={toX(p.day)}
            y={svgH - 6}
            textAnchor="middle"
            fontSize={9}
            fill="var(--text-muted, #94a3b8)"
          >
            {`D${p.day + 1}`}
          </text>
        ))}
      {/* baseline */}
      <line
        x1={0}
        y1={plotH}
        x2={svgW}
        y2={plotH}
        stroke="var(--surface-border, #e2e8f0)"
        strokeWidth={1}
      />
    </svg>
  );
}

// ── Cycle time chart ──────────────────────────────────────────────────────────

interface CycleBar {
  priority: TaskPriority;
  avgDays: number;
  count: number;
}

function CycleTimeChart({ bars }: { bars: CycleBar[] }) {
  const max = Math.max(...bars.map((b) => b.avgDays), 1);
  const barH = 28;
  const gap  = 12;
  const labelW = 72;
  const svgH = bars.length * (barH + gap);
  const svgW = 460;

  return (
    <svg
      width="100%"
      viewBox={`0 0 ${svgW} ${svgH}`}
      preserveAspectRatio="xMidYMid meet"
      aria-label="Cycle time by priority horizontal bar chart"
      role="img"
    >
      {bars.map((b, i) => {
        const y       = i * (barH + gap);
        const barMaxW = svgW - labelW - 60;
        const barW    = Math.round((b.avgDays / max) * barMaxW);
        const color   = PRIORITY_COLORS[b.priority];
        return (
          <g key={b.priority}>
            <text
              x={labelW - 8}
              y={y + barH / 2 + 1}
              textAnchor="end"
              dominantBaseline="middle"
              fontSize={12}
              fill="var(--text-secondary, #475569)"
            >
              {b.priority}
            </text>
            <rect
              x={labelW}
              y={y}
              width={barW}
              height={barH}
              rx={4}
              fill={color}
              opacity={0.8}
            >
              <title>{`${b.priority}: ${b.avgDays.toFixed(1)} days avg (${b.count} tasks)`}</title>
            </rect>
            <text
              x={labelW + barW + 6}
              y={y + barH / 2 + 1}
              dominantBaseline="middle"
              fontSize={11}
              fill="var(--text-secondary, #475569)"
            >
              {b.count > 0 ? `${b.avgDays.toFixed(1)}d` : 'no data'}
            </text>
          </g>
        );
      })}
    </svg>
  );
}

// ── Main page ─────────────────────────────────────────────────────────────────

export function TeamAnalyticsPage() {
  const { data: tasks = [], isLoading } = useTasks();

  // ── Velocity ────────────────────────────────────────────────────────────────

  const now = new Date();
  const weekBuckets: WeekBucket[] = Array.from({ length: WEEKS_BACK }, (_, i) => {
    const weekStart = startOfWeek(addDays(now, -(WEEKS_BACK - 1 - i) * 7));
    const weekEnd   = addDays(weekStart, 7);
    const count = tasks.filter((t) => {
      if (t.status !== 'Done' || t.updatedAt == null) return false;
      const updated = new Date(t.updatedAt);
      return updated >= weekStart && updated < weekEnd;
    }).length;
    return { label: `Wk ${i + 1}`, count };
  });

  // ── Burndown ────────────────────────────────────────────────────────────────

  const burndownPoints: DayPoint[] = Array.from({ length: DAYS_BACK }, (_, i) => {
    const day = startOfDay(addDays(now, i - (DAYS_BACK - 1)));
    const remaining = tasks.filter((t) => {
      const created = new Date(t.createdAt);
      if (created > day) return false;
      if (t.status !== 'Done') return true;
      const updated = t.updatedAt ? new Date(t.updatedAt) : null;
      return updated == null || updated > day;
    }).length;
    return { day: i, remaining };
  });

  // ── Cycle time ──────────────────────────────────────────────────────────────

  const cycleBars: CycleBar[] = TASK_PRIORITIES.map((priority) => {
    const done = tasks.filter(
      (t) => t.status === 'Done' && t.priority === priority && t.updatedAt != null,
    );
    const avgDays =
      done.length === 0
        ? 0
        : done.reduce((sum, t) => {
            const ms = new Date(t.updatedAt!).getTime() - new Date(t.createdAt).getTime();
            return sum + ms / (1000 * 60 * 60 * 24);
          }, 0) / done.length;
    return { priority, avgDays, count: done.length };
  });

  // ── Summary stats ───────────────────────────────────────────────────────────

  const totalTasks     = tasks.length;
  const completedTasks = tasks.filter((t) => t.status === 'Done').length;
  const inProgress     = tasks.filter((t) => t.status === 'InProgress').length;
  const allDone        = tasks.filter((t) => t.status === 'Done' && t.updatedAt != null);
  const avgCycleAll    =
    allDone.length === 0
      ? 0
      : allDone.reduce((sum, t) => {
          const ms = new Date(t.updatedAt!).getTime() - new Date(t.createdAt).getTime();
          return sum + ms / (1000 * 60 * 60 * 24);
        }, 0) / allDone.length;

  if (isLoading) {
    return (
      <div>
        <div className="page-header">
          <h1 className="page-title">Team Analytics</h1>
        </div>
        <p style={{ color: 'var(--text-muted, #94a3b8)', fontSize: 14 }}>Loading…</p>
      </div>
    );
  }

  return (
    <div>
      <div className="page-header">
        <div>
          <h1 className="page-title">Team Analytics</h1>
          <p className="page-subtitle">Velocity, burndown, and cycle-time insights</p>
        </div>
      </div>

      {/* Summary stats */}
      <div
        style={{
          display: 'grid',
          gridTemplateColumns: 'repeat(auto-fit, minmax(140px, 1fr))',
          gap: 16,
          marginBottom: 24,
        }}
      >
        <Stat label="Total Tasks"      value={totalTasks}                 color="var(--color-primary, #2563eb)" />
        <Stat label="Completed"        value={completedTasks}             color="#10b981" />
        <Stat label="In Progress"      value={inProgress}                 color="#f59e0b" />
        <Stat label="Avg Cycle Time"   value={`${avgCycleAll.toFixed(1)}d`} color="#8b5cf6" />
      </div>

      {/* Velocity */}
      <div style={card}>
        <h3 style={{ margin: '0 0 16px', fontSize: 15, color: 'var(--text-primary, #0f172a)' }}>
          Velocity — tasks completed per week (last 8 weeks)
        </h3>
        <VelocityChart weeks={weekBuckets} />
      </div>

      {/* Burndown */}
      <div style={card}>
        <h3 style={{ margin: '0 0 16px', fontSize: 15, color: 'var(--text-primary, #0f172a)' }}>
          Burndown — open tasks remaining (last 30 days)
        </h3>
        <BurndownChart points={burndownPoints} />
      </div>

      {/* Cycle time */}
      <div style={card}>
        <h3 style={{ margin: '0 0 16px', fontSize: 15, color: 'var(--text-primary, #0f172a)' }}>
          Cycle time — avg days from creation to completion, by priority
        </h3>
        <CycleTimeChart bars={cycleBars} />
      </div>
    </div>
  );
}
