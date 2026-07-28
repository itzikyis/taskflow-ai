import { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { useProjects } from '@/features/projects/hooks/useProjects';
import { useAuthStore } from '@/store/authStore';
import {
  goalService,
  type GoalDto,
  type GoalStatus,
  type CreateGoalPayload,
} from '@/services/goalService';

// ── Constants ──────────────────────────────────────────────────────────────────

const STATUS_META: Record<GoalStatus, { label: string; bg: string; color: string }> = {
  OnTrack:   { label: 'On Track',  bg: '#ecfdf5', color: '#065f46' },
  AtRisk:    { label: 'At Risk',   bg: '#fffbeb', color: '#92400e' },
  OffTrack:  { label: 'Off Track', bg: '#fef2f2', color: '#991b1b' },
  Completed: { label: 'Completed', bg: '#f0f4ff', color: '#3730a3' },
};

const STATUS_ORDER: GoalStatus[] = ['OnTrack', 'AtRisk', 'OffTrack', 'Completed'];

function formatDate(iso: string | null) {
  if (!iso) return null;
  return new Date(iso).toLocaleDateString(undefined, { month: 'short', day: 'numeric', year: 'numeric' });
}

// ── Progress bar ───────────────────────────────────────────────────────────────

function ProgressBar({ percent, color }: { percent: number; color: string }) {
  return (
    <div style={{ background: 'var(--border-default)', borderRadius: 4, height: 6, overflow: 'hidden' }}>
      <div style={{
        width: `${percent}%`,
        height: 6,
        borderRadius: 4,
        background: color,
        transition: 'width 0.4s',
      }} />
    </div>
  );
}

// ── Status badge ───────────────────────────────────────────────────────────────

function StatusBadge({ status }: { status: GoalStatus }) {
  const m = STATUS_META[status];
  return (
    <span style={{
      fontSize: 11, fontWeight: 600, borderRadius: 5,
      padding: '2px 8px', background: m.bg, color: m.color,
    }}>{m.label}</span>
  );
}

// ── Goal card ──────────────────────────────────────────────────────────────────

interface GoalCardProps {
  goal: GoalDto;
  onUpdateProgress: (id: string, percent: number, status: GoalStatus) => void;
  onDelete: (id: string) => void;
}

function GoalCard({ goal, onUpdateProgress, onDelete }: GoalCardProps) {
  const [editProgress, setEditProgress] = useState(false);
  const [draftPercent, setDraftPercent] = useState(goal.progressPercent);
  const [draftStatus, setDraftStatus]   = useState<GoalStatus>(goal.status);

  const progressColor =
    goal.status === 'Completed' ? '#10b981'
    : goal.status === 'OffTrack' ? '#ef4444'
    : goal.status === 'AtRisk' ? '#f59e0b'
    : 'var(--color-primary, #6366f1)';

  const dueText = formatDate(goal.dueDate);

  return (
    <div style={{
      background: 'var(--bg-card)',
      border: '1px solid var(--border-default)',
      borderRadius: 12,
      padding: '20px 24px',
    }}>
      {/* Header */}
      <div style={{ display: 'flex', alignItems: 'flex-start', gap: 12, marginBottom: 12 }}>
        <div style={{ flex: 1 }}>
          <div style={{ display: 'flex', alignItems: 'center', gap: 8, flexWrap: 'wrap', marginBottom: 4 }}>
            <span style={{ fontWeight: 700, fontSize: 15, color: 'var(--text-primary)' }}>
              {goal.title}
            </span>
            <StatusBadge status={goal.status} />
            {dueText && (
              <span style={{ fontSize: 12, color: 'var(--text-secondary)' }}>
                Due {dueText}
              </span>
            )}
          </div>
          {goal.description && (
            <p style={{ fontSize: 13, color: 'var(--text-secondary)', margin: 0 }}>
              {goal.description}
            </p>
          )}
        </div>

        {/* Actions */}
        <div style={{ display: 'flex', gap: 6 }}>
          <button
            type="button"
            onClick={() => setEditProgress(v => !v)}
            title="Update progress"
            style={{ background: 'none', border: 'none', cursor: 'pointer', color: 'var(--text-muted)', fontSize: 14 }}
          >✏️</button>
          <button
            type="button"
            onClick={() => onDelete(goal.id)}
            title="Delete goal"
            style={{ background: 'none', border: 'none', cursor: 'pointer', color: 'var(--text-muted)', fontSize: 14 }}
            onMouseEnter={e => (e.currentTarget.style.color = '#ef4444')}
            onMouseLeave={e => (e.currentTarget.style.color = 'var(--text-muted)')}
          >🗑</button>
        </div>
      </div>

      {/* Overall progress */}
      <div style={{ marginBottom: 12 }}>
        <div style={{ display: 'flex', justifyContent: 'space-between', fontSize: 12, color: 'var(--text-secondary)', marginBottom: 4 }}>
          <span>Overall Progress</span>
          <span>{goal.progressPercent}%</span>
        </div>
        <ProgressBar percent={goal.progressPercent} color={progressColor} />
      </div>

      {/* Inline progress editor */}
      {editProgress && (
        <div style={{
          marginBottom: 12, padding: 12, borderRadius: 8,
          background: 'var(--bg-surface, #f9fafb)', border: '1px solid var(--border-default)',
        }}>
          <div style={{ display: 'flex', gap: 12, flexWrap: 'wrap', alignItems: 'flex-end' }}>
            <label style={{ fontSize: 12, color: 'var(--text-secondary)', fontWeight: 500 }}>
              Progress %
              <input
                type="number"
                min={0}
                max={100}
                value={draftPercent}
                onChange={e => setDraftPercent(Number(e.target.value))}
                className="input"
                style={{ display: 'block', width: 80, marginTop: 4 }}
              />
            </label>
            <label style={{ fontSize: 12, color: 'var(--text-secondary)', fontWeight: 500 }}>
              Status
              <select
                value={draftStatus}
                onChange={e => setDraftStatus(e.target.value as GoalStatus)}
                className="input"
                style={{ display: 'block', marginTop: 4 }}
              >
                {STATUS_ORDER.map(s => (
                  <option key={s} value={s}>{STATUS_META[s].label}</option>
                ))}
              </select>
            </label>
            <button
              type="button"
              className="btn btn-primary"
              style={{ fontSize: 12 }}
              onClick={() => {
                onUpdateProgress(goal.id, draftPercent, draftStatus);
                setEditProgress(false);
              }}
            >Save</button>
            <button
              type="button"
              className="btn btn-secondary"
              style={{ fontSize: 12 }}
              onClick={() => setEditProgress(false)}
            >Cancel</button>
          </div>
        </div>
      )}

      {/* Key results */}
      {goal.keyResults.length > 0 && (
        <div>
          <div style={{ fontSize: 12, fontWeight: 600, color: 'var(--text-secondary)', marginBottom: 8 }}>
            Key Results
          </div>
          <div style={{ display: 'flex', flexDirection: 'column', gap: 8 }}>
            {goal.keyResults.map(kr => (
              <div key={kr.id} style={{ padding: '10px 12px', borderRadius: 8, background: 'var(--bg-surface, #f9fafb)', border: '1px solid var(--border-default)' }}>
                <div style={{ display: 'flex', justifyContent: 'space-between', fontSize: 13, marginBottom: 6 }}>
                  <span style={{ fontWeight: 500, color: 'var(--text-primary)' }}>{kr.title}</span>
                  <span style={{ color: 'var(--text-secondary)', fontSize: 12 }}>
                    {kr.currentValue} / {kr.targetValue} {kr.unit} ({kr.progressPercent}%)
                  </span>
                </div>
                <ProgressBar percent={kr.progressPercent} color="var(--color-primary, #6366f1)" />
              </div>
            ))}
          </div>
        </div>
      )}
    </div>
  );
}

// ── Create goal form ───────────────────────────────────────────────────────────

interface NewGoalFormProps {
  projectId: string;
  userId: string;
  onCreated: () => void;
  onCancel: () => void;
}

function NewGoalForm({ projectId, userId, onCreated, onCancel }: NewGoalFormProps) {
  const [title, setTitle]     = useState('');
  const [desc, setDesc]       = useState('');
  const [dueDate, setDueDate] = useState('');
  const [error, setError]     = useState<string | null>(null);

  const qc = useQueryClient();
  const { mutateAsync, isPending } = useMutation({
    mutationFn: (payload: CreateGoalPayload) => goalService.create(payload),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['goals', projectId] });
      onCreated();
    },
  });

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    setError(null);
    if (!title.trim()) { setError('Title is required.'); return; }
    try {
      await mutateAsync({
        projectId,
        ownerId: userId,
        title,
        description: desc || null,
        dueDate: dueDate || null,
      });
    } catch {
      setError('Failed to create goal.');
    }
  }

  return (
    <form onSubmit={handleSubmit} style={{
      background: 'var(--bg-card)',
      border: '2px solid var(--color-primary, #6366f1)',
      borderRadius: 12, padding: '20px 24px',
    }}>
      <div style={{ fontWeight: 700, fontSize: 15, marginBottom: 16, color: 'var(--text-primary)' }}>
        New Objective
      </div>
      <div style={{ display: 'grid', gap: 12 }}>
        <label style={{ fontSize: 13, color: 'var(--text-secondary)', fontWeight: 500 }}>
          Title *
          <input value={title} onChange={e => setTitle(e.target.value)} placeholder="e.g. Grow user base by 50%" className="input" style={{ display: 'block', width: '100%', marginTop: 4 }} />
        </label>
        <label style={{ fontSize: 13, color: 'var(--text-secondary)', fontWeight: 500 }}>
          Description
          <textarea value={desc} onChange={e => setDesc(e.target.value)} placeholder="What does success look like?" rows={2} className="input" style={{ display: 'block', width: '100%', marginTop: 4, resize: 'vertical' }} />
        </label>
        <label style={{ fontSize: 13, color: 'var(--text-secondary)', fontWeight: 500 }}>
          Due date
          <input type="date" value={dueDate} onChange={e => setDueDate(e.target.value)} className="input" style={{ display: 'block', width: '100%', marginTop: 4 }} />
        </label>
        {error && <div style={{ color: '#ef4444', fontSize: 13 }}>{error}</div>}
        <div style={{ display: 'flex', gap: 8, justifyContent: 'flex-end', marginTop: 4 }}>
          <button type="button" onClick={onCancel} className="btn btn-secondary" style={{ fontSize: 13 }}>Cancel</button>
          <button type="submit" disabled={isPending} className="btn btn-primary" style={{ fontSize: 13 }}>
            {isPending ? 'Creating…' : 'Create Objective'}
          </button>
        </div>
      </div>
    </form>
  );
}

// ── Main page ──────────────────────────────────────────────────────────────────

export function GoalsPage() {
  const { token } = useAuthStore();
  const { data: projects = [] } = useProjects();
  const [selectedProjectId, setSelectedProjectId] = useState<string>('');
  const [showForm, setShowForm] = useState(false);
  const qc = useQueryClient();

  const projectId = selectedProjectId || (projects[0]?.id ?? '');

  const { data: goals = [], isLoading } = useQuery({
    queryKey: ['goals', projectId],
    queryFn: () => goalService.getByProject(projectId),
    enabled: !!projectId,
  });

  const progressMutation = useMutation({
    mutationFn: ({ id, percent, status }: { id: string; percent: number; status: GoalStatus }) =>
      goalService.updateProgress(id, { progressPercent: percent, status }),
    onSuccess: () => qc.invalidateQueries({ queryKey: ['goals', projectId] }),
  });

  const deleteMutation = useMutation({
    mutationFn: (id: string) => goalService.remove(id),
    onSuccess: () => qc.invalidateQueries({ queryKey: ['goals', projectId] }),
  });

  return (
    <div style={{ padding: '24px 32px', maxWidth: 900 }}>
      {/* Header */}
      <div style={{ display: 'flex', alignItems: 'flex-start', justifyContent: 'space-between', marginBottom: 24 }}>
        <div>
          <h1 style={{ fontSize: 22, fontWeight: 700, margin: 0, color: 'var(--text-primary)' }}>
            Goals &amp; OKRs
          </h1>
          <p style={{ fontSize: 14, color: 'var(--text-secondary)', margin: '6px 0 0' }}>
            Define objectives, link key results to tasks, and track progress automatically.
          </p>
        </div>
        {!showForm && projectId && (
          <button
            type="button"
            className="btn btn-primary"
            onClick={() => setShowForm(true)}
            style={{ fontSize: 13, flexShrink: 0 }}
          >
            + New Objective
          </button>
        )}
      </div>

      {/* Project selector */}
      {projects.length > 1 && (
        <div style={{ marginBottom: 20 }}>
          <label style={{ fontSize: 13, color: 'var(--text-secondary)', fontWeight: 500 }}>
            Project
            <select
              value={projectId}
              onChange={e => setSelectedProjectId(e.target.value)}
              className="input"
              style={{ display: 'inline-block', marginLeft: 10, width: 'auto' }}
            >
              {projects.map(p => (
                <option key={p.id} value={p.id}>{p.name}</option>
              ))}
            </select>
          </label>
        </div>
      )}

      {/* New goal form */}
      {showForm && projectId && (
        <div style={{ marginBottom: 20 }}>
          <NewGoalForm
            projectId={projectId}
            userId={token?.userId ?? ''}
            onCreated={() => setShowForm(false)}
            onCancel={() => setShowForm(false)}
          />
        </div>
      )}

      {/* Goal list */}
      {!projectId ? (
        <div style={{ textAlign: 'center', padding: '48px 0', color: 'var(--text-secondary)', fontSize: 14 }}>
          <div style={{ fontSize: 40, marginBottom: 12 }}>🎯</div>
          <div style={{ fontWeight: 600 }}>No projects yet</div>
          <div>Create a project first to start tracking goals.</div>
        </div>
      ) : isLoading ? (
        <div style={{ color: 'var(--text-secondary)', fontSize: 14 }}>Loading…</div>
      ) : goals.length === 0 ? (
        <div style={{ textAlign: 'center', padding: '48px 0', color: 'var(--text-secondary)', fontSize: 14 }}>
          <div style={{ fontSize: 40, marginBottom: 12 }}>🎯</div>
          <div style={{ fontWeight: 600, marginBottom: 6 }}>No objectives yet</div>
          <div>Click <strong>+ New Objective</strong> to define your first OKR.</div>
        </div>
      ) : (
        <div style={{ display: 'flex', flexDirection: 'column', gap: 14 }}>
          {goals.map(goal => (
            <GoalCard
              key={goal.id}
              goal={goal}
              onUpdateProgress={(id, percent, status) =>
                progressMutation.mutate({ id, percent, status })
              }
              onDelete={id => deleteMutation.mutate(id)}
            />
          ))}
        </div>
      )}
    </div>
  );
}
