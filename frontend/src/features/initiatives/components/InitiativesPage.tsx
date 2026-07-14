import { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { useProjects } from '@/features/projects/hooks/useProjects';
import { useAuthStore } from '@/store/authStore';
import {
  initiativeService,
  type InitiativeDto,
  type InitiativeStatus,
  type InitiativePriority,
  type CreateInitiativePayload,
} from '@/services/initiativeService';

// ── Constants ─────────────────────────────────────────────────────────────────

const STATUS_ORDER: InitiativeStatus[] = ['Proposed', 'Active', 'Completed', 'Canceled'];

const STATUS_STYLE: Record<InitiativeStatus, { bg: string; color: string; label: string }> = {
  Proposed:  { bg: '#f0f4ff', color: '#3730a3', label: 'Proposed'  },
  Active:    { bg: '#ecfdf5', color: '#065f46', label: 'Active'    },
  Completed: { bg: '#f0fdf4', color: '#14532d', label: 'Completed' },
  Canceled:  { bg: '#fef2f2', color: '#991b1b', label: 'Canceled'  },
};

const PRIORITY_STYLE: Record<InitiativePriority, { color: string; label: string }> = {
  Low:    { color: '#6b7280', label: '▽ Low'    },
  Medium: { color: '#f59e0b', label: '◈ Medium' },
  High:   { color: '#ef4444', label: '▲ High'   },
  Urgent: { color: '#7c3aed', label: '‼ Urgent' },
};

const PRIORITIES: InitiativePriority[] = ['Low', 'Medium', 'High', 'Urgent'];

function formatDate(iso: string | null) {
  if (!iso) return '—';
  return new Date(iso).toLocaleDateString(undefined, { month: 'short', day: 'numeric', year: 'numeric' });
}

function progressPct(done: number, total: number) {
  if (total === 0) return 0;
  return Math.round((done / total) * 100);
}

// ── Initiative card ───────────────────────────────────────────────────────────

interface CardProps {
  initiative: InitiativeDto;
  projects: { id: string; name: string }[];
  onStatusChange: (id: string, status: InitiativeStatus) => void;
  onAddProject: (id: string, projectId: string) => void;
  onDelete: (id: string) => void;
}

function InitiativeCard({ initiative, projects, onStatusChange, onAddProject, onDelete }: CardProps) {
  const [showProjectMenu, setShowProjectMenu] = useState(false);
  const s = STATUS_STYLE[initiative.status];
  const p = PRIORITY_STYLE[initiative.priority];
  const pct = progressPct(initiative.completedTasks, initiative.totalTasks);
  const labels = initiative.labels
    ? initiative.labels.split(',').map(l => l.trim()).filter(Boolean)
    : [];
  const linkedProjects = projects.filter(pr => initiative.projectIds.includes(pr.id));
  const unlinkable = projects.filter(pr => !initiative.projectIds.includes(pr.id));

  return (
    <div style={{
      background: 'var(--bg-card)',
      border: '1px solid var(--border-default)',
      borderRadius: 12,
      padding: '20px 24px',
    }}>
      {/* Header row */}
      <div style={{ display: 'flex', alignItems: 'flex-start', gap: 12, marginBottom: 10 }}>
        <div style={{ flex: 1 }}>
          <div style={{ display: 'flex', alignItems: 'center', gap: 8, flexWrap: 'wrap', marginBottom: 4 }}>
            <span style={{ fontWeight: 700, fontSize: 15, color: 'var(--text-primary)' }}>
              {initiative.name}
            </span>
            <span style={{
              fontSize: 11, fontWeight: 600, borderRadius: 5,
              padding: '2px 8px', background: s.bg, color: s.color,
            }}>{s.label}</span>
            <span style={{ fontSize: 12, color: p.color, fontWeight: 600 }}>{p.label}</span>
            {labels.map(l => (
              <span key={l} style={{
                fontSize: 11, borderRadius: 5, padding: '2px 7px',
                background: 'var(--bg-surface, #f9fafb)', color: 'var(--text-secondary)',
                border: '1px solid var(--border-default)',
              }}>{l}</span>
            ))}
          </div>
          {initiative.description && (
            <p style={{ fontSize: 13, color: 'var(--text-secondary)', margin: 0 }}>
              {initiative.description}
            </p>
          )}
        </div>

        {/* Delete */}
        <button
          type="button"
          onClick={() => onDelete(initiative.id)}
          title="Delete initiative"
          style={{ background: 'none', border: 'none', cursor: 'pointer', color: 'var(--text-muted)', fontSize: 15, padding: 2 }}
          onMouseEnter={e => (e.currentTarget.style.color = '#ef4444')}
          onMouseLeave={e => (e.currentTarget.style.color = 'var(--text-muted)')}
        >🗑</button>
      </div>

      {/* Dates + status changer */}
      <div style={{ display: 'flex', alignItems: 'center', gap: 16, flexWrap: 'wrap', marginBottom: 12, fontSize: 12, color: 'var(--text-secondary)' }}>
        <span>📅 {formatDate(initiative.startDate)} → {formatDate(initiative.targetDate)}</span>
        <select
          value={initiative.status}
          onChange={e => onStatusChange(initiative.id, e.target.value as InitiativeStatus)}
          style={{
            fontSize: 12, border: '1px solid var(--border-default)',
            borderRadius: 6, padding: '2px 8px', background: s.bg, color: s.color,
            fontWeight: 600, cursor: 'pointer',
          }}
        >
          {STATUS_ORDER.map(st => (
            <option key={st} value={st}>{STATUS_STYLE[st].label}</option>
          ))}
        </select>
      </div>

      {/* Progress bar */}
      {initiative.totalTasks > 0 && (
        <div style={{ marginBottom: 12 }}>
          <div style={{ display: 'flex', justifyContent: 'space-between', fontSize: 12, color: 'var(--text-secondary)', marginBottom: 4 }}>
            <span>Progress</span>
            <span>{initiative.completedTasks}/{initiative.totalTasks} tasks ({pct}%)</span>
          </div>
          <div style={{ background: 'var(--border-default)', borderRadius: 4, height: 6 }}>
            <div style={{
              width: `${pct}%`, height: 6, borderRadius: 4,
              background: pct === 100 ? '#10b981' : 'var(--color-primary, #6366f1)',
              transition: 'width 0.4s',
            }} />
          </div>
        </div>
      )}

      {/* Linked projects */}
      <div style={{ display: 'flex', alignItems: 'center', gap: 8, flexWrap: 'wrap' }}>
        <span style={{ fontSize: 12, color: 'var(--text-secondary)', fontWeight: 500 }}>Projects:</span>
        {linkedProjects.length === 0 ? (
          <span style={{ fontSize: 12, color: 'var(--text-muted)' }}>None linked</span>
        ) : (
          linkedProjects.map(pr => (
            <span key={pr.id} style={{
              fontSize: 12, background: '#ede9fe', color: '#7c3aed',
              borderRadius: 5, padding: '2px 8px', fontWeight: 500,
            }}>{pr.name}</span>
          ))
        )}
        {unlinkable.length > 0 && (
          <div style={{ position: 'relative' }}>
            <button
              type="button"
              onClick={() => setShowProjectMenu(v => !v)}
              style={{
                fontSize: 12, color: 'var(--color-primary, #6366f1)',
                background: 'none', border: '1px dashed var(--color-primary, #6366f1)',
                borderRadius: 5, padding: '2px 8px', cursor: 'pointer',
              }}
            >+ Link project</button>
            {showProjectMenu && (
              <div style={{
                position: 'absolute', top: 26, left: 0, zIndex: 10,
                background: 'var(--bg-card)', border: '1px solid var(--border-default)',
                borderRadius: 8, boxShadow: '0 4px 12px rgba(0,0,0,0.12)',
                minWidth: 180,
              }}>
                {unlinkable.map(pr => (
                  <button
                    key={pr.id}
                    type="button"
                    onClick={() => { onAddProject(initiative.id, pr.id); setShowProjectMenu(false); }}
                    style={{
                      display: 'block', width: '100%', textAlign: 'left',
                      padding: '8px 14px', background: 'none', border: 'none',
                      fontSize: 13, cursor: 'pointer', color: 'var(--text-primary)',
                    }}
                    onMouseEnter={e => (e.currentTarget.style.background = 'var(--bg-surface, #f9fafb)')}
                    onMouseLeave={e => (e.currentTarget.style.background = 'none')}
                  >{pr.name}</button>
                ))}
              </div>
            )}
          </div>
        )}
      </div>
    </div>
  );
}

// ── New initiative form ───────────────────────────────────────────────────────

interface NewFormProps {
  onCreated: () => void;
  onCancel: () => void;
  userId: string;
}

function NewInitiativeForm({ onCreated, onCancel, userId }: NewFormProps) {
  const [name, setName]         = useState('');
  const [desc, setDesc]         = useState('');
  const [priority, setPriority] = useState<InitiativePriority>('Medium');
  const [labels, setLabels]     = useState('');
  const [start, setStart]       = useState('');
  const [target, setTarget]     = useState('');
  const [error, setError]       = useState<string | null>(null);

  const qc = useQueryClient();
  const { mutateAsync, isPending } = useMutation({
    mutationFn: (payload: CreateInitiativePayload) => initiativeService.create(payload),
    onSuccess: () => { qc.invalidateQueries({ queryKey: ['initiatives'] }); onCreated(); },
  });

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    setError(null);
    if (!name.trim()) { setError('Name is required.'); return; }
    try {
      await mutateAsync({
        name, description: desc, priority, labels,
        startDate: start || null, targetDate: target || null,
        createdByUserId: userId,
      });
    } catch {
      setError('Failed to create initiative.');
    }
  }

  return (
    <form onSubmit={handleSubmit} style={{
      background: 'var(--bg-card)',
      border: '2px solid var(--color-primary, #6366f1)',
      borderRadius: 12, padding: '20px 24px',
    }}>
      <div style={{ fontWeight: 700, fontSize: 15, marginBottom: 16, color: 'var(--text-primary)' }}>
        New Initiative
      </div>
      <div style={{ display: 'grid', gap: 12 }}>
        <label style={{ fontSize: 13, color: 'var(--text-secondary)', fontWeight: 500 }}>
          Name *
          <input value={name} onChange={e => setName(e.target.value)} placeholder="e.g. Q3 Launch" className="input" style={{ display: 'block', width: '100%', marginTop: 4 }} />
        </label>
        <label style={{ fontSize: 13, color: 'var(--text-secondary)', fontWeight: 500 }}>
          Description
          <textarea value={desc} onChange={e => setDesc(e.target.value)} placeholder="What is this initiative about?" rows={2} className="input" style={{ display: 'block', width: '100%', marginTop: 4, resize: 'vertical' }} />
        </label>
        <div style={{ display: 'flex', gap: 12, flexWrap: 'wrap' }}>
          <label style={{ fontSize: 13, color: 'var(--text-secondary)', fontWeight: 500, flex: 1, minWidth: 140 }}>
            Priority
            <select value={priority} onChange={e => setPriority(e.target.value as InitiativePriority)} className="input" style={{ display: 'block', width: '100%', marginTop: 4 }}>
              {PRIORITIES.map(p => <option key={p} value={p}>{p}</option>)}
            </select>
          </label>
          <label style={{ fontSize: 13, color: 'var(--text-secondary)', fontWeight: 500, flex: 2, minWidth: 180 }}>
            Labels (comma-separated)
            <input value={labels} onChange={e => setLabels(e.target.value)} placeholder="e.g. Q3,Growth,Backend" className="input" style={{ display: 'block', width: '100%', marginTop: 4 }} />
          </label>
        </div>
        <div style={{ display: 'flex', gap: 12, flexWrap: 'wrap' }}>
          <label style={{ fontSize: 13, color: 'var(--text-secondary)', fontWeight: 500, flex: 1 }}>
            Start date
            <input type="date" value={start} onChange={e => setStart(e.target.value)} className="input" style={{ display: 'block', width: '100%', marginTop: 4 }} />
          </label>
          <label style={{ fontSize: 13, color: 'var(--text-secondary)', fontWeight: 500, flex: 1 }}>
            Target date
            <input type="date" value={target} onChange={e => setTarget(e.target.value)} className="input" style={{ display: 'block', width: '100%', marginTop: 4 }} />
          </label>
        </div>
        {error && <div style={{ color: '#ef4444', fontSize: 13 }}>{error}</div>}
        <div style={{ display: 'flex', gap: 8, justifyContent: 'flex-end', marginTop: 4 }}>
          <button type="button" onClick={onCancel} className="btn btn-secondary" style={{ fontSize: 13 }}>Cancel</button>
          <button type="submit" disabled={isPending} className="btn btn-primary" style={{ fontSize: 13 }}>
            {isPending ? 'Creating…' : 'Create Initiative'}
          </button>
        </div>
      </div>
    </form>
  );
}

// ── Main page ─────────────────────────────────────────────────────────────────

export function InitiativesPage() {
  const { data: initiatives = [], isLoading } = useQuery({
    queryKey: ['initiatives'],
    queryFn: initiativeService.getRoadmap,
  });
  const { data: projects = [] } = useProjects();
  const { token } = useAuthStore();
  const [showForm, setShowForm] = useState(false);
  const [filterStatus, setFilterStatus] = useState<InitiativeStatus | 'All'>('All');
  const qc = useQueryClient();

  const statusMutation = useMutation({
    mutationFn: ({ id, status }: { id: string; status: InitiativeStatus }) =>
      initiativeService.updateStatus(id, status),
    onSuccess: () => qc.invalidateQueries({ queryKey: ['initiatives'] }),
  });

  const addProjectMutation = useMutation({
    mutationFn: ({ id, projectId }: { id: string; projectId: string }) =>
      initiativeService.addProject(id, projectId),
    onSuccess: () => qc.invalidateQueries({ queryKey: ['initiatives'] }),
  });

  const deleteMutation = useMutation({
    mutationFn: (id: string) => initiativeService.delete(id),
    onSuccess: () => qc.invalidateQueries({ queryKey: ['initiatives'] }),
  });

  const visible = filterStatus === 'All'
    ? initiatives
    : initiatives.filter(i => i.status === filterStatus);

  const counts: Record<string, number> = {};
  for (const i of initiatives) counts[i.status] = (counts[i.status] ?? 0) + 1;

  return (
    <div style={{ padding: '24px 32px', maxWidth: 900 }}>
      {/* Header */}
      <div style={{ display: 'flex', alignItems: 'flex-start', justifyContent: 'space-between', marginBottom: 24 }}>
        <div>
          <h1 style={{ fontSize: 22, fontWeight: 700, margin: 0, color: 'var(--text-primary)' }}>
            🗺 Initiatives &amp; Roadmap
          </h1>
          <p style={{ fontSize: 14, color: 'var(--text-secondary)', margin: '6px 0 0' }}>
            Group related projects toward a shared goal. Track cross-project progress in one view.
          </p>
        </div>
        {!showForm && (
          <button
            type="button"
            className="btn btn-primary"
            onClick={() => setShowForm(true)}
            style={{ fontSize: 13, flexShrink: 0 }}
          >
            + New Initiative
          </button>
        )}
      </div>

      {/* Status filter pills */}
      <div style={{ display: 'flex', gap: 8, marginBottom: 20, flexWrap: 'wrap' }}>
        {(['All', ...STATUS_ORDER] as const).map(st => {
          const cnt = st === 'All' ? initiatives.length : (counts[st] ?? 0);
          const active = filterStatus === st;
          return (
            <button
              key={st}
              type="button"
              onClick={() => setFilterStatus(st)}
              style={{
                fontSize: 12, fontWeight: 600, borderRadius: 20,
                padding: '4px 14px', cursor: 'pointer', border: 'none',
                background: active ? 'var(--color-primary, #6366f1)' : 'var(--bg-card)',
                color: active ? '#fff' : 'var(--text-secondary)',
                boxShadow: active ? 'none' : '0 0 0 1px var(--border-default)',
              }}
            >
              {st} {cnt > 0 && `(${cnt})`}
            </button>
          );
        })}
      </div>

      {/* New initiative form */}
      {showForm && (
        <div style={{ marginBottom: 20 }}>
          <NewInitiativeForm
            userId={token?.userId ?? ''}
            onCreated={() => setShowForm(false)}
            onCancel={() => setShowForm(false)}
          />
        </div>
      )}

      {/* Roadmap list */}
      {isLoading ? (
        <div style={{ color: 'var(--text-secondary)', fontSize: 14 }}>Loading…</div>
      ) : visible.length === 0 ? (
        <div style={{ textAlign: 'center', padding: '48px 0', color: 'var(--text-secondary)', fontSize: 14 }}>
          <div style={{ fontSize: 40, marginBottom: 12 }}>🗺</div>
          <div style={{ fontWeight: 600, marginBottom: 6 }}>
            {filterStatus === 'All' ? 'No initiatives yet' : `No ${filterStatus} initiatives`}
          </div>
          <div>Click <strong>+ New Initiative</strong> to start your roadmap.</div>
        </div>
      ) : (
        <div style={{ display: 'flex', flexDirection: 'column', gap: 14 }}>
          {visible.map(initiative => (
            <InitiativeCard
              key={initiative.id}
              initiative={initiative}
              projects={projects}
              onStatusChange={(id, status) => statusMutation.mutate({ id, status })}
              onAddProject={(id, projectId) => addProjectMutation.mutate({ id, projectId })}
              onDelete={id => deleteMutation.mutate(id)}
            />
          ))}
        </div>
      )}
    </div>
  );
}
