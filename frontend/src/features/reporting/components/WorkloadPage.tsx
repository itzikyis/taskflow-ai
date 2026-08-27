import { useState } from 'react';
import { useQuery } from '@tanstack/react-query';
import { useProjects } from '@/features/projects/hooks/useProjects';
import { teamService } from '@/services/teamService';
import type { MemberWorkloadDto } from '@/services/teamService';

const DEFAULT_CAPACITY = 40;

// Utilisation thresholds (percent)
const THRESHOLD_AMBER = 80;
const THRESHOLD_RED = 100;

const COLOR_GREEN = '#10b981';
const COLOR_AMBER = '#f59e0b';
const COLOR_RED = '#ef4444';

const card: React.CSSProperties = {
  background: 'var(--surface-card, #ffffff)',
  border: '1px solid var(--surface-border, #e2e8f0)',
  borderRadius: 'var(--radius-md, 8px)',
  boxShadow: 'var(--shadow-sm)',
  padding: 18,
};

function utilizationColor(percent: number): string {
  if (percent <= THRESHOLD_AMBER) return COLOR_GREEN;
  if (percent <= THRESHOLD_RED) return COLOR_AMBER;
  return COLOR_RED;
}

function AvatarInitials({ name }: { name: string }) {
  const initials = name
    .split(' ')
    .map((w) => w[0] ?? '')
    .join('')
    .slice(0, 2)
    .toUpperCase();

  return (
    <div
      style={{
        width: 36,
        height: 36,
        borderRadius: '50%',
        background: 'var(--color-primary, #6366f1)',
        color: '#fff',
        display: 'flex',
        alignItems: 'center',
        justifyContent: 'center',
        fontSize: 13,
        fontWeight: 700,
        flexShrink: 0,
      }}
      aria-hidden="true"
    >
      {initials}
    </div>
  );
}

interface MemberRowProps {
  member: MemberWorkloadDto;
  /** Client-side capacity override (hours/week). When provided, recalculates utilisation locally. */
  capacityOverride: number;
}

function MemberRow({ member, capacityOverride }: MemberRowProps) {
  const loggedHours = member.loggedHoursThisWeek;
  const utilizationPct =
    capacityOverride > 0 ? Math.round((loggedHours / capacityOverride) * 100 * 10) / 10 : 0;
  const barWidth = Math.min(utilizationPct, 100);
  const color = utilizationColor(utilizationPct);
  const overCap = loggedHours > capacityOverride;

  return (
    <div
      style={{
        display: 'grid',
        gridTemplateColumns: '36px 1fr auto',
        alignItems: 'center',
        gap: 12,
        padding: '14px 0',
        borderBottom: '1px solid var(--surface-border, #e2e8f0)',
      }}
    >
      <AvatarInitials name={member.displayName} />

      <div>
        <div
          style={{
            display: 'flex',
            alignItems: 'center',
            gap: 8,
            marginBottom: 6,
          }}
        >
          <span style={{ fontSize: 14, fontWeight: 600, color: 'var(--text-primary)' }}>
            {member.displayName}
          </span>
          {overCap && (
            <span
              style={{
                fontSize: 10,
                fontWeight: 700,
                background: '#fef2f2',
                color: COLOR_RED,
                border: `1px solid ${COLOR_RED}`,
                borderRadius: 4,
                padding: '1px 5px',
                lineHeight: '16px',
              }}
              title="Member is over their weekly capacity"
            >
              OVER CAPACITY
            </span>
          )}
        </div>
        {/* Utilisation bar */}
        <div
          style={{
            height: 8,
            background: 'var(--surface-bg, #f1f5f9)',
            borderRadius: 4,
            overflow: 'hidden',
            position: 'relative',
          }}
        >
          <div
            style={{
              width: `${barWidth}%`,
              height: '100%',
              background: color,
              borderRadius: 4,
              transition: 'width 0.3s ease, background 0.3s ease',
            }}
            role="progressbar"
            aria-valuenow={utilizationPct}
            aria-valuemax={100}
            aria-label={`${member.displayName} utilisation: ${utilizationPct}%`}
          />
        </div>
        <div style={{ marginTop: 4, fontSize: 11, color: 'var(--text-secondary, #64748b)' }}>
          <span style={{ color, fontWeight: 700 }}>{utilizationPct}%</span>
          {' utilised · '}
          {loggedHours.toFixed(1)}h logged / {capacityOverride}h capacity
        </div>
      </div>

      <div style={{ display: 'flex', gap: 14, fontSize: 12, color: 'var(--text-secondary)', whiteSpace: 'nowrap' }}>
        <span title="Open (To Do)">
          <span style={{ color: '#94a3b8', fontWeight: 700 }}>{member.openTasks}</span> Open
        </span>
        <span title="In Progress / In Review">
          <span style={{ color: '#2563eb', fontWeight: 700 }}>{member.inProgressTasks}</span> Active
        </span>
        <span title="Done">
          <span style={{ color: COLOR_GREEN, fontWeight: 700 }}>{member.completedTasks}</span> Done
        </span>
        <span title="Total assigned">
          <span style={{ fontWeight: 700 }}>{member.totalAssigned}</span> Total
        </span>
      </div>
    </div>
  );
}

function SkeletonRow() {
  return (
    <div
      style={{
        display: 'grid',
        gridTemplateColumns: '36px 1fr auto',
        alignItems: 'center',
        gap: 12,
        padding: '14px 0',
        borderBottom: '1px solid var(--surface-border, #e2e8f0)',
      }}
    >
      <div style={{ width: 36, height: 36, borderRadius: '50%', background: 'var(--surface-bg, #f1f5f9)' }} />
      <div>
        <div style={{ height: 14, width: '40%', background: 'var(--surface-bg, #f1f5f9)', borderRadius: 4, marginBottom: 8 }} />
        <div style={{ height: 8, background: 'var(--surface-bg, #f1f5f9)', borderRadius: 4 }} />
      </div>
      <div style={{ width: 160, height: 14, background: 'var(--surface-bg, #f1f5f9)', borderRadius: 4 }} />
    </div>
  );
}

function LegendDot({ color, label }: { color: string; label: string }) {
  return (
    <span style={{ display: 'flex', alignItems: 'center', gap: 4 }}>
      <span
        style={{ width: 10, height: 10, borderRadius: '50%', background: color, display: 'inline-block' }}
      />
      <span style={{ fontSize: 11, color: 'var(--text-muted)' }}>{label}</span>
    </span>
  );
}

/** Team-level capacity summary bar */
function TeamSummaryBar({
  totalLogged,
  totalCapacity,
}: {
  totalLogged: number;
  totalCapacity: number;
}) {
  const pct = totalCapacity > 0 ? Math.min((totalLogged / totalCapacity) * 100, 100) : 0;
  const color = utilizationColor(totalCapacity > 0 ? (totalLogged / totalCapacity) * 100 : 0);

  return (
    <div
      style={{
        background: 'var(--surface-bg, #f8fafc)',
        border: '1px solid var(--surface-border, #e2e8f0)',
        borderRadius: 8,
        padding: '14px 16px',
        marginBottom: 16,
      }}
    >
      <div
        style={{
          display: 'flex',
          justifyContent: 'space-between',
          alignItems: 'center',
          marginBottom: 8,
        }}
      >
        <span style={{ fontSize: 13, fontWeight: 600, color: 'var(--text-primary)' }}>
          Team capacity this week
        </span>
        <span style={{ fontSize: 13, color: 'var(--text-secondary)' }}>
          <strong style={{ color }}>{totalLogged.toFixed(1)}h</strong>
          {' / '}
          {totalCapacity.toFixed(0)}h
        </span>
      </div>
      <div
        style={{
          height: 10,
          background: 'var(--surface-border, #e2e8f0)',
          borderRadius: 5,
          overflow: 'hidden',
        }}
      >
        <div
          style={{
            width: `${pct}%`,
            height: '100%',
            background: color,
            borderRadius: 5,
            transition: 'width 0.4s ease',
          }}
          role="progressbar"
          aria-valuenow={Math.round(pct)}
          aria-valuemax={100}
          aria-label={`Team utilisation ${Math.round(pct)}%`}
        />
      </div>
      <div style={{ marginTop: 6, fontSize: 11, color: 'var(--text-secondary)' }}>
        <span style={{ color, fontWeight: 700 }}>{Math.round(pct)}%</span> of total team capacity consumed
      </div>
    </div>
  );
}

export function WorkloadPage() {
  const { data: projects = [] } = useProjects();
  const [selectedProjectId, setSelectedProjectId] = useState<string>('');
  const [capacityInput, setCapacityInput] = useState<string>(String(DEFAULT_CAPACITY));

  const projectId = selectedProjectId || (projects[0]?.id ?? '');
  const capacityHours = Math.max(1, Number(capacityInput) || DEFAULT_CAPACITY);

  const { data, isLoading, isError } = useQuery({
    queryKey: ['team-workload', projectId],
    queryFn: () => teamService.getTeamWorkload(projectId),
    enabled: Boolean(projectId),
  });

  // Recompute team-level totals using the client-side capacity override
  const memberCount = data?.members.length ?? 0;
  const totalCapacity = memberCount * capacityHours;
  const totalLogged = data?.members.reduce((sum, m) => sum + m.loggedHoursThisWeek, 0) ?? 0;

  return (
    <div>
      <div className="page-header">
        <div>
          <h1 className="page-title">Team Workload</h1>
          <p className="page-subtitle">Per-member task load and capacity planning</p>
        </div>

        <div style={{ display: 'flex', gap: 12, alignItems: 'center' }}>
          {/* Capacity hours input */}
          <label
            style={{
              display: 'flex',
              alignItems: 'center',
              gap: 6,
              fontSize: 13,
              color: 'var(--text-secondary)',
            }}
          >
            Capacity h/week:
            <input
              type="number"
              min={1}
              max={168}
              step={1}
              value={capacityInput}
              onChange={(e) => setCapacityInput(e.target.value)}
              aria-label="Capacity hours per week"
              style={{
                width: 64,
                fontSize: 13,
                padding: '4px 8px',
                borderRadius: 6,
                border: '1px solid var(--surface-border, #e2e8f0)',
                textAlign: 'right',
              }}
            />
          </label>

          {projects.length > 0 && (
            <select
              value={selectedProjectId}
              onChange={(e) => setSelectedProjectId(e.target.value)}
              aria-label="Select project"
              style={{
                fontSize: 13,
                padding: '4px 8px',
                borderRadius: 6,
                border: '1px solid var(--surface-border, #e2e8f0)',
              }}
            >
              {projects.map((p) => (
                <option key={p.id} value={p.id}>
                  {p.name}
                </option>
              ))}
            </select>
          )}
        </div>
      </div>

      {/* Team-level summary — only show when we have data */}
      {!isLoading && !isError && data && data.members.length > 0 && (
        <TeamSummaryBar totalLogged={totalLogged} totalCapacity={totalCapacity} />
      )}

      <div style={card}>
        {isLoading && (
          <>
            <SkeletonRow />
            <SkeletonRow />
            <SkeletonRow />
          </>
        )}

        {isError && (
          <div className="empty-state">
            <div className="empty-state-icon">⚠️</div>
            <p className="empty-state-text">Could not load workload data.</p>
          </div>
        )}

        {!isLoading && !isError && !projectId && (
          <div className="empty-state">
            <div className="empty-state-icon">📂</div>
            <p className="empty-state-text">Select a project to view team workload.</p>
          </div>
        )}

        {!isLoading && !isError && projectId && data && data.members.length === 0 && (
          <div className="empty-state">
            <div className="empty-state-icon">👤</div>
            <p className="empty-state-text">No team members found.</p>
          </div>
        )}

        {!isLoading && !isError && data && data.members.length > 0 && (
          <>
            <div style={{ marginBottom: 4 }}>
              {data.members.map((member) => (
                <MemberRow key={member.userId} member={member} capacityOverride={capacityHours} />
              ))}
            </div>

            <div
              style={{
                marginTop: 16,
                paddingTop: 12,
                fontSize: 13,
                color: 'var(--text-secondary)',
                display: 'flex',
                gap: 20,
                alignItems: 'center',
                flexWrap: 'wrap',
              }}
            >
              <span>
                <strong style={{ color: 'var(--text-primary)' }}>{data.unassignedTasks}</strong>{' '}
                unassigned {data.unassignedTasks === 1 ? 'task' : 'tasks'}
              </span>
              <span style={{ display: 'flex', gap: 12, alignItems: 'center' }}>
                <LegendDot color={COLOR_GREEN} label="Under 80% (on track)" />
                <LegendDot color={COLOR_AMBER} label="80–100% (near capacity)" />
                <LegendDot color={COLOR_RED} label="Over 100% (over capacity)" />
              </span>
            </div>
          </>
        )}
      </div>
    </div>
  );
}
