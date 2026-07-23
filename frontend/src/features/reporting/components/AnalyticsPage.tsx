import { useState } from 'react';
import { useQuery } from '@tanstack/react-query';
import { useProjects } from '@/features/projects/hooks/useProjects';
import { reportingService } from '@/services/reportingService';
import type { ProjectAnalyticsDto, WeeklyVelocityDto } from '@/services/reportingService';
import { AiInsightsPanel } from './AiInsightsPanel';

// ── Constants ────────────────────────────────────────────────────────────────

const COLOR_COMPLETED   = '#10b981';
const COLOR_IN_PROGRESS = '#2563eb';
const COLOR_OPEN        = '#94a3b8';
const COLOR_OVERDUE     = '#ef4444';

const DONUT_RADIUS        = 60;
const DONUT_CIRCUMFERENCE = 2 * Math.PI * DONUT_RADIUS;
const SVG_SIZE            = 160;
const DONUT_CENTER        = SVG_SIZE / 2;
const DONUT_STROKE        = 20;

const card: React.CSSProperties = {
  background: '#ffffff',
  border: '1px solid var(--border-color)',
  borderRadius: 'var(--radius-md)',
  boxShadow: 'var(--shadow-sm)',
  padding: 18,
};

// ── Stat tile ────────────────────────────────────────────────────────────────

interface StatTileProps {
  label: string;
  value: number;
  color: string;
}

function StatTile({ label, value, color }: StatTileProps) {
  return (
    <div style={card}>
      <div style={{ fontSize: 30, fontWeight: 800, color }}>{value}</div>
      <div style={{ fontSize: 12, color: 'var(--text-muted)', textTransform: 'uppercase', letterSpacing: '0.04em', marginTop: 4 }}>
        {label}
      </div>
    </div>
  );
}

// ── Velocity bar chart (pure CSS bars) ───────────────────────────────────────

interface VelocityChartProps {
  weeks: WeeklyVelocityDto[];
}

function VelocityChart({ weeks }: VelocityChartProps) {
  if (weeks.length === 0) {
    return <p style={{ fontSize: 13, color: 'var(--text-muted)' }}>No data yet.</p>;
  }

  const max = Math.max(...weeks.map((w) => w.completed), 1);

  return (
    <div style={{ display: 'flex', alignItems: 'flex-end', gap: 8, height: 140 }}>
      {weeks.map((w) => {
        const heightPx = Math.round((w.completed / max) * 110);
        return (
          <div
            key={w.weekLabel}
            style={{ flex: 1, display: 'flex', flexDirection: 'column', alignItems: 'center', gap: 4 }}
            title={`${w.weekLabel}: ${w.completed} completed`}
          >
            <span style={{ fontSize: 11, color: 'var(--text-muted)' }}>{w.completed}</span>
            <div
              style={{
                width: '100%',
                maxWidth: 48,
                height: `${heightPx}px`,
                background: COLOR_COMPLETED,
                borderRadius: '4px 4px 0 0',
                transition: 'height 0.3s ease',
              }}
              role="img"
              aria-label={`${w.weekLabel}: ${w.completed} tasks completed`}
            />
            <span style={{ fontSize: 10, color: 'var(--text-muted)', textAlign: 'center' }}>{w.weekLabel}</span>
          </div>
        );
      })}
    </div>
  );
}

// ── Status donut (pure SVG) ───────────────────────────────────────────────────

interface DonutSegment {
  label: string;
  value: number;
  color: string;
}

function StatusDonut({ data }: { data: ProjectAnalyticsDto }) {
  const segments: DonutSegment[] = [
    { label: 'Completed',   value: data.completedTasks,  color: COLOR_COMPLETED },
    { label: 'In Progress', value: data.inProgressTasks, color: COLOR_IN_PROGRESS },
    { label: 'Open',        value: data.openTasks,       color: COLOR_OPEN },
    { label: 'Overdue',     value: data.overdueTasks,    color: COLOR_OVERDUE },
  ];

  const total = segments.reduce((s, seg) => s + seg.value, 0);

  if (total === 0) {
    return (
      <div style={{ display: 'flex', justifyContent: 'center', alignItems: 'center', height: SVG_SIZE }}>
        <p style={{ fontSize: 13, color: 'var(--text-muted)' }}>No tasks.</p>
      </div>
    );
  }

  let offset = 0;
  const arcs = segments
    .filter((seg) => seg.value > 0)
    .map((seg) => {
      const proportion = seg.value / total;
      const dash = proportion * DONUT_CIRCUMFERENCE;
      const gap  = DONUT_CIRCUMFERENCE - dash;
      const arc  = { ...seg, dash, gap, offset };
      offset += dash;
      return arc;
    });

  return (
    <div style={{ display: 'flex', alignItems: 'center', gap: 24, flexWrap: 'wrap' }}>
      <svg
        width={SVG_SIZE}
        height={SVG_SIZE}
        viewBox={`0 0 ${SVG_SIZE} ${SVG_SIZE}`}
        aria-label="Task status donut chart"
        role="img"
        style={{ flexShrink: 0 }}
      >
        {arcs.map((arc) => (
          <circle
            key={arc.label}
            cx={DONUT_CENTER}
            cy={DONUT_CENTER}
            r={DONUT_RADIUS}
            fill="none"
            stroke={arc.color}
            strokeWidth={DONUT_STROKE}
            strokeDasharray={`${arc.dash} ${arc.gap}`}
            strokeDashoffset={-arc.offset}
            style={{ transform: `rotate(-90deg)`, transformOrigin: `${DONUT_CENTER}px ${DONUT_CENTER}px` }}
          >
            <title>{`${arc.label}: ${arc.value}`}</title>
          </circle>
        ))}
        <text
          x={DONUT_CENTER}
          y={DONUT_CENTER - 6}
          textAnchor="middle"
          dominantBaseline="middle"
          style={{ fontSize: 22, fontWeight: 800, fill: 'var(--text-primary, #0f172a)' }}
        >
          {total}
        </text>
        <text
          x={DONUT_CENTER}
          y={DONUT_CENTER + 14}
          textAnchor="middle"
          dominantBaseline="middle"
          style={{ fontSize: 11, fill: 'var(--text-muted, #94a3b8)' }}
        >
          tasks
        </text>
      </svg>

      <ul style={{ listStyle: 'none', padding: 0, margin: 0, display: 'flex', flexDirection: 'column', gap: 8 }}>
        {segments.map((seg) => (
          <li key={seg.label} style={{ display: 'flex', alignItems: 'center', gap: 8, fontSize: 13 }}>
            <span style={{ width: 12, height: 12, borderRadius: '50%', background: seg.color, display: 'inline-block', flexShrink: 0 }} />
            <span style={{ color: 'var(--text-secondary)' }}>{seg.label}</span>
            <span style={{ fontWeight: 700, color: 'var(--text-primary)', marginLeft: 'auto', paddingLeft: 12 }}>{seg.value}</span>
          </li>
        ))}
      </ul>
    </div>
  );
}

// ── Skeleton ──────────────────────────────────────────────────────────────────

function SkeletonBox({ height }: { height: number }) {
  return (
    <div
      style={{
        height,
        width: '100%',
        background: 'var(--surface-bg, #f1f5f9)',
        borderRadius: 6,
        animation: 'pulse 1.5s ease-in-out infinite',
      }}
    />
  );
}

function AnalyticsSkeleton() {
  return (
    <div>
      <div style={{ display: 'grid', gridTemplateColumns: 'repeat(5, 1fr)', gap: 20, marginBottom: 28 }}>
        {Array.from({ length: 5 }).map((_, i) => (
          <div key={i} style={card}><SkeletonBox height={52} /></div>
        ))}
      </div>
      <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: 20 }}>
        <div style={card}><SkeletonBox height={160} /></div>
        <div style={card}><SkeletonBox height={160} /></div>
      </div>
    </div>
  );
}

// ── Main page ─────────────────────────────────────────────────────────────────

export function AnalyticsPage() {
  const { data: projects = [] } = useProjects();
  const [selectedProjectId, setSelectedProjectId] = useState<string>('');

  const projectId = selectedProjectId || (projects[0]?.id ?? '');

  const { data, isLoading, isError } = useQuery({
    queryKey: ['project-analytics', projectId],
    queryFn: () => reportingService.getProjectAnalytics(projectId),
    enabled: Boolean(projectId),
    staleTime: 2 * 60 * 1000,
  });

  return (
    <div>
      <div className="page-header">
        <div>
          <h1 className="page-title">Analytics</h1>
          <p className="page-subtitle">Velocity, status breakdown, and AI insights</p>
        </div>
        {projects.length > 0 && (
          <select
            value={selectedProjectId}
            onChange={(e) => setSelectedProjectId(e.target.value)}
            aria-label="Select project"
            style={{ fontSize: 13, padding: '4px 8px', borderRadius: 6, border: '1px solid var(--border-color)' }}
          >
            {projects.map((p) => (
              <option key={p.id} value={p.id}>{p.name}</option>
            ))}
          </select>
        )}
      </div>

      {!projectId && (
        <div className="empty-state">
          <div className="empty-state-icon">📂</div>
          <p className="empty-state-text">Select a project to view analytics.</p>
        </div>
      )}

      {projectId && isLoading && <AnalyticsSkeleton />}

      {projectId && isError && (
        <div className="empty-state">
          <div className="empty-state-icon">⚠️</div>
          <p className="empty-state-text">Could not load analytics. Please try again.</p>
        </div>
      )}

      {projectId && data && !isLoading && (
        <>
          {/* Stat tiles */}
          <div style={{ display: 'grid', gridTemplateColumns: 'repeat(5, 1fr)', gap: 20, marginBottom: 28 }}>
            <StatTile label="Total"       value={data.totalTasks}      color="var(--color-primary)" />
            <StatTile label="Completed"   value={data.completedTasks}  color={COLOR_COMPLETED} />
            <StatTile label="In Progress" value={data.inProgressTasks} color={COLOR_IN_PROGRESS} />
            <StatTile label="Open"        value={data.openTasks}       color={COLOR_OPEN} />
            <StatTile label="Overdue"     value={data.overdueTasks}    color={COLOR_OVERDUE} />
          </div>

          {/* Charts */}
          <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: 20, marginBottom: 28, alignItems: 'start' }}>
            <div style={card}>
              <h3 style={{ margin: '0 0 16px', fontSize: 15 }}>Weekly velocity (last 6 weeks)</h3>
              <VelocityChart weeks={data.weeklyVelocity} />
            </div>

            <div style={card}>
              <h3 style={{ margin: '0 0 16px', fontSize: 15 }}>Status breakdown</h3>
              <StatusDonut data={data} />
            </div>
          </div>

          {/* AI Insights */}
          <AiInsightsPanel projectId={projectId} />
        </>
      )}
    </div>
  );
}
