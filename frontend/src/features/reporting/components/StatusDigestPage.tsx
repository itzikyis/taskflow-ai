import { useState } from 'react';
import { useQuery } from '@tanstack/react-query';
import { useProjects } from '@/features/projects/hooks/useProjects';
import { aiService } from '@/services/aiService';
import type { StatusDigestDto } from '@/services/aiService';

const PERIOD_OPTIONS: { label: string; days: number }[] = [
  { label: 'Last 7 days', days: 7 },
  { label: 'Last 14 days', days: 14 },
  { label: 'Last 30 days', days: 30 },
];

function healthBadgeStyle(status: string): React.CSSProperties {
  const base: React.CSSProperties = {
    display: 'inline-block',
    padding: '2px 10px',
    borderRadius: 12,
    fontSize: 12,
    fontWeight: 600,
    marginLeft: 10,
  };
  if (status === 'Healthy') return { ...base, background: '#bbf7d0', color: '#166534' };
  if (status === 'At Risk') return { ...base, background: '#fef08a', color: '#854d0e' };
  return { ...base, background: '#fecaca', color: '#991b1b' };
}

interface TaskListColumnProps {
  icon: string;
  title: string;
  items: string[];
  emptyText: string;
}

function TaskListColumn({ icon, title, items, emptyText }: TaskListColumnProps) {
  return (
    <div style={{
      flex: 1,
      minWidth: 0,
      background: 'var(--bg-secondary)',
      border: '1px solid var(--border)',
      borderRadius: 10,
      padding: '16px 18px',
    }}>
      <div style={{ fontWeight: 600, fontSize: 14, marginBottom: 12, display: 'flex', alignItems: 'center', gap: 6 }}>
        <span>{icon}</span>
        <span style={{ color: 'var(--text-primary)' }}>{title}</span>
        <span style={{
          marginLeft: 'auto',
          background: 'var(--bg-primary)',
          border: '1px solid var(--border)',
          borderRadius: 10,
          fontSize: 11,
          padding: '1px 7px',
          color: 'var(--text-secondary)',
        }}>{items.length}</span>
      </div>
      {items.length === 0
        ? <p style={{ fontSize: 13, color: 'var(--text-secondary)', margin: 0 }}>{emptyText}</p>
        : (
          <ul style={{ listStyle: 'none', margin: 0, padding: 0 }}>
            {items.map((item, i) => (
              <li key={i} style={{
                fontSize: 13,
                color: 'var(--text-primary)',
                padding: '5px 0',
                borderBottom: i < items.length - 1 ? '1px solid var(--border)' : 'none',
              }}>
                {item}
              </li>
            ))}
          </ul>
        )
      }
    </div>
  );
}

function SkeletonCard() {
  return (
    <div style={{ background: 'var(--bg-secondary)', border: '1px solid var(--border)', borderRadius: 10, padding: 20, marginBottom: 20 }}>
      {[80, 60, 90, 50].map((w, i) => (
        <div key={i} style={{
          height: 14,
          width: `${w}%`,
          background: 'var(--border)',
          borderRadius: 4,
          marginBottom: 10,
          opacity: 0.6,
        }} />
      ))}
    </div>
  );
}

function DigestContent({ digest }: { digest: StatusDigestDto }) {
  return (
    <>
      {/* Narrative card */}
      <div style={{
        background: 'var(--bg-secondary)',
        border: '1px solid var(--border)',
        borderRadius: 10,
        padding: '18px 20px',
        marginBottom: 20,
      }}>
        <div style={{ display: 'flex', alignItems: 'center', marginBottom: 10 }}>
          <span style={{ fontWeight: 600, fontSize: 14, color: 'var(--text-primary)' }}>AI Summary</span>
          <span style={healthBadgeStyle(digest.healthStatus)}>{digest.healthStatus}</span>
        </div>
        <p style={{ margin: 0, fontSize: 14, color: 'var(--text-primary)', lineHeight: 1.6 }}>
          {digest.aiNarrative}
        </p>
      </div>

      {/* Three columns */}
      <div style={{ display: 'flex', gap: 16, flexWrap: 'wrap' }}>
        <TaskListColumn
          icon="✅"
          title="Completed"
          items={digest.completed}
          emptyText="No tasks completed in this period."
        />
        <TaskListColumn
          icon="🔄"
          title="In Progress"
          items={digest.inProgress}
          emptyText="No tasks currently in progress."
        />
        <TaskListColumn
          icon="🚨"
          title="Blockers"
          items={digest.blockers}
          emptyText="No blockers — great work!"
        />
      </div>
    </>
  );
}

export function StatusDigestPage() {
  const { data: projects = [] } = useProjects();
  const [selectedProjectId, setSelectedProjectId] = useState<string>('');
  const [periodDays, setPeriodDays] = useState(7);

  const projectId = selectedProjectId || (projects[0]?.id ?? '');

  const { data: digest, isLoading, isError } = useQuery<StatusDigestDto>({
    queryKey: ['status-digest', projectId, periodDays],
    queryFn: () => aiService.getStatusDigest(projectId, periodDays),
    enabled: !!projectId,
  });

  return (
    <div>
      <div className="page-header">
        <div>
          <h1 className="page-title">Status Digest</h1>
          <p className="page-subtitle">AI-generated project status report</p>
        </div>
        <div style={{ display: 'flex', gap: 10, alignItems: 'center' }}>
          {projects.length > 0 && (
            <select
              value={selectedProjectId}
              onChange={(e) => setSelectedProjectId(e.target.value)}
              style={{ fontSize: 13, padding: '4px 8px', borderRadius: 6, border: '1px solid var(--border)' }}
              aria-label="Select project"
            >
              {projects.map((p) => (
                <option key={p.id} value={p.id}>{p.name}</option>
              ))}
            </select>
          )}
          <select
            value={periodDays}
            onChange={(e) => setPeriodDays(Number(e.target.value))}
            style={{ fontSize: 13, padding: '4px 8px', borderRadius: 6, border: '1px solid var(--border)' }}
            aria-label="Select period"
          >
            {PERIOD_OPTIONS.map((opt) => (
              <option key={opt.days} value={opt.days}>{opt.label}</option>
            ))}
          </select>
        </div>
      </div>

      {!projectId && (
        <div className="empty-state">
          <div className="empty-state-icon">📋</div>
          <p className="empty-state-text">No projects found. Create a project first.</p>
        </div>
      )}

      {projectId && isLoading && <SkeletonCard />}

      {projectId && isError && (
        <div className="empty-state">
          <div className="empty-state-icon">⚠️</div>
          <p className="empty-state-text">Could not load the status digest. Please try again.</p>
        </div>
      )}

      {projectId && digest && <DigestContent digest={digest} />}
    </div>
  );
}
