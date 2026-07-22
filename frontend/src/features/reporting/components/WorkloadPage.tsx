import { useState } from 'react';
import { useQuery } from '@tanstack/react-query';
import { useProjects } from '@/features/projects/hooks/useProjects';
import { teamService } from '@/services/teamService';
import type { MemberWorkloadDto } from '@/services/teamService';

const CAPACITY_THRESHOLD_GREEN = 0.6;
const CAPACITY_THRESHOLD_AMBER = 0.8;

const COLOR_GREEN = '#10b981';
const COLOR_AMBER = '#f59e0b';
const COLOR_RED = '#ef4444';

const card: React.CSSProperties = {
  background: '#ffffff',
  border: '1px solid var(--border-color)',
  borderRadius: 'var(--radius-md)',
  boxShadow: 'var(--shadow-sm)',
  padding: 18,
};

function capacityColor(ratio: number): string {
  if (ratio <= CAPACITY_THRESHOLD_GREEN) return COLOR_GREEN;
  if (ratio <= CAPACITY_THRESHOLD_AMBER) return COLOR_AMBER;
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

function MemberRow({ member, max }: { member: MemberWorkloadDto; max: number }) {
  const ratio = max === 0 ? 0 : member.totalAssigned / max;
  const color = capacityColor(ratio);
  const barWidth = Math.round(ratio * 100);

  return (
    <div
      style={{
        display: 'grid',
        gridTemplateColumns: '36px 1fr auto',
        alignItems: 'center',
        gap: 12,
        padding: '12px 0',
        borderBottom: '1px solid var(--border-color)',
      }}
    >
      <AvatarInitials name={member.displayName} />

      <div>
        <div style={{ fontSize: 14, fontWeight: 600, color: 'var(--text-primary)', marginBottom: 6 }}>
          {member.displayName}
        </div>
        <div style={{ height: 8, background: 'var(--surface-bg, #f1f5f9)', borderRadius: 4, overflow: 'hidden' }}>
          <div
            style={{ width: `${barWidth}%`, height: '100%', background: color, borderRadius: 4, transition: 'width 0.3s ease' }}
            role="progressbar"
            aria-valuenow={member.totalAssigned}
            aria-valuemax={max}
            aria-label={`${member.displayName} capacity`}
          />
        </div>
      </div>

      <div style={{ display: 'flex', gap: 16, fontSize: 12, color: 'var(--text-secondary)', whiteSpace: 'nowrap' }}>
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
        padding: '12px 0',
        borderBottom: '1px solid var(--border-color)',
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

export function WorkloadPage() {
  const { data: projects = [] } = useProjects();
  const [selectedProjectId, setSelectedProjectId] = useState<string>('');

  const projectId = selectedProjectId || (projects[0]?.id ?? '');

  const { data, isLoading, isError } = useQuery({
    queryKey: ['team-workload', projectId],
    queryFn: () => teamService.getTeamWorkload(projectId),
    enabled: Boolean(projectId),
  });

  const max = data ? Math.max(...data.members.map((m) => m.totalAssigned), 1) : 1;

  return (
    <div>
      <div className="page-header">
        <div>
          <h1 className="page-title">Team Workload</h1>
          <p className="page-subtitle">Per-member task load and capacity overview</p>
        </div>
        {projects.length > 0 && (
          <select
            value={selectedProjectId}
            onChange={(e) => setSelectedProjectId(e.target.value)}
            aria-label="Select project"
            style={{
              fontSize: 13,
              padding: '4px 8px',
              borderRadius: 6,
              border: '1px solid var(--border-color)',
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
                <MemberRow key={member.userId} member={member} max={max} />
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
              }}
            >
              <span>
                <strong style={{ color: 'var(--text-primary)' }}>{data.unassignedTasks}</strong> unassigned{' '}
                {data.unassignedTasks === 1 ? 'task' : 'tasks'}
              </span>
              <span style={{ display: 'flex', gap: 12, alignItems: 'center' }}>
                <LegendDot color={COLOR_GREEN} label="Low load (≤60%)" />
                <LegendDot color={COLOR_AMBER} label="Moderate (≤80%)" />
                <LegendDot color={COLOR_RED} label="High load (>80%)" />
              </span>
            </div>
          </>
        )}
      </div>
    </div>
  );
}

function LegendDot({ color, label }: { color: string; label: string }) {
  return (
    <span style={{ display: 'flex', alignItems: 'center', gap: 4 }}>
      <span style={{ width: 10, height: 10, borderRadius: '50%', background: color, display: 'inline-block' }} />
      <span style={{ fontSize: 11, color: 'var(--text-muted)' }}>{label}</span>
    </span>
  );
}
