import { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { useProjects } from '@/features/projects/hooks/useProjects';
import {
  taskTemplateService,
  type TaskTemplate,
  type CreateTaskTemplatePayload,
} from '@/services/taskTemplateService';

const PRIORITY_OPTIONS = ['Low', 'Medium', 'High', 'Critical'];

// ── Template card ─────────────────────────────────────────────────────────────

interface TemplateCardProps {
  template: TaskTemplate;
  onDelete: (id: string) => void;
  onUse: (id: string) => void;
  isUsing: boolean;
}

function TemplateCard({ template, onDelete, onUse, isUsing }: TemplateCardProps) {
  return (
    <div style={{
      background: 'var(--bg-secondary)',
      border: '1px solid var(--border)',
      borderRadius: 8,
      padding: '14px 16px',
      display: 'flex',
      flexDirection: 'column',
      gap: 6,
    }}>
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start' }}>
        <span style={{ fontWeight: 600, fontSize: 14, color: 'var(--text-primary)' }}>
          {template.name}
        </span>
        <div style={{ display: 'flex', gap: 6 }}>
          <button
            type="button"
            onClick={() => onUse(template.id)}
            disabled={isUsing}
            style={{
              padding: '3px 10px',
              borderRadius: 5,
              border: 'none',
              background: 'var(--accent)',
              color: '#fff',
              cursor: isUsing ? 'not-allowed' : 'pointer',
              fontSize: 12,
              opacity: isUsing ? 0.6 : 1,
            }}
          >
            {isUsing ? 'Creating…' : 'Use template'}
          </button>
          <button
            type="button"
            onClick={() => onDelete(template.id)}
            style={{
              padding: '3px 8px',
              borderRadius: 5,
              border: '1px solid var(--border)',
              background: 'transparent',
              color: '#f87171',
              cursor: 'pointer',
              fontSize: 12,
            }}
          >
            Delete
          </button>
        </div>
      </div>

      <div style={{ fontSize: 13, color: 'var(--text-secondary)' }}>
        <span style={{ fontWeight: 500 }}>Default title:</span> {template.defaultTitle}
      </div>

      {template.defaultDescription && (
        <div style={{ fontSize: 12, color: 'var(--text-secondary)' }}>
          {template.defaultDescription}
        </div>
      )}

      <div style={{ display: 'flex', gap: 10, flexWrap: 'wrap', marginTop: 2 }}>
        {template.defaultPriority && (
          <span style={{
            fontSize: 11,
            padding: '2px 7px',
            borderRadius: 4,
            background: 'var(--bg-primary)',
            border: '1px solid var(--border)',
            color: 'var(--text-secondary)',
          }}>
            Priority: {template.defaultPriority}
          </span>
        )}
        {template.defaultEstimatedHours != null && (
          <span style={{
            fontSize: 11,
            padding: '2px 7px',
            borderRadius: 4,
            background: 'var(--bg-primary)',
            border: '1px solid var(--border)',
            color: 'var(--text-secondary)',
          }}>
            Est. {template.defaultEstimatedHours}h
          </span>
        )}
      </div>
    </div>
  );
}

// ── Main page ─────────────────────────────────────────────────────────────────

const EMPTY_FORM: CreateTaskTemplatePayload = {
  projectId: '',
  name: '',
  defaultTitle: '',
  defaultDescription: '',
  defaultPriority: '',
  defaultEstimatedHours: undefined,
};

export function TaskTemplatesPage() {
  const queryClient = useQueryClient();
  const { projects, isLoading: projectsLoading } = useProjects();
  const [selectedProjectId, setSelectedProjectId] = useState('');
  const [showForm, setShowForm] = useState(false);
  const [form, setForm] = useState<CreateTaskTemplatePayload>(EMPTY_FORM);
  const [usingId, setUsingId] = useState<string | null>(null);
  const [successMsg, setSuccessMsg] = useState('');

  const { data: templates = [], isLoading: templatesLoading } = useQuery({
    queryKey: ['task-templates', selectedProjectId],
    queryFn: () => taskTemplateService.getByProject(selectedProjectId),
    enabled: !!selectedProjectId,
  });

  const createMutation = useMutation({
    mutationFn: taskTemplateService.create,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['task-templates', selectedProjectId] });
      setForm({ ...EMPTY_FORM, projectId: selectedProjectId });
      setShowForm(false);
    },
  });

  const deleteMutation = useMutation({
    mutationFn: taskTemplateService.remove,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['task-templates', selectedProjectId] });
    },
  });

  const handleUse = async (templateId: string) => {
    setUsingId(templateId);
    try {
      await taskTemplateService.createTask(templateId);
      setSuccessMsg('Task created from template.');
      setTimeout(() => setSuccessMsg(''), 3000);
    } finally {
      setUsingId(null);
    }
  };

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    createMutation.mutate({
      ...form,
      projectId: selectedProjectId,
      defaultDescription: form.defaultDescription || undefined,
      defaultPriority: form.defaultPriority || undefined,
    });
  };

  return (
    <div style={{ maxWidth: 760, margin: '0 auto', padding: '24px 16px' }}>
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: 20 }}>
        <div>
          <h2 style={{ fontSize: 20, fontWeight: 700, color: 'var(--text-primary)', margin: 0 }}>
            Task Templates
          </h2>
          <p style={{ fontSize: 13, color: 'var(--text-secondary)', margin: '4px 0 0' }}>
            Create reusable task templates to speed up task creation.
          </p>
        </div>
        {selectedProjectId && (
          <button
            type="button"
            onClick={() => { setShowForm(v => !v); setForm({ ...EMPTY_FORM, projectId: selectedProjectId }); }}
            style={{
              padding: '7px 14px',
              borderRadius: 6,
              border: 'none',
              background: 'var(--accent)',
              color: '#fff',
              fontWeight: 600,
              fontSize: 13,
              cursor: 'pointer',
            }}
          >
            + New template
          </button>
        )}
      </div>

      {/* Project selector */}
      <div style={{ marginBottom: 20 }}>
        <label style={{ fontSize: 13, color: 'var(--text-secondary)', display: 'block', marginBottom: 6 }}>
          Project
        </label>
        {projectsLoading ? (
          <span style={{ fontSize: 13, color: 'var(--text-secondary)' }}>Loading projects…</span>
        ) : (
          <select
            value={selectedProjectId}
            onChange={e => setSelectedProjectId(e.target.value)}
            style={{
              padding: '7px 10px',
              borderRadius: 6,
              border: '1px solid var(--border)',
              background: 'var(--bg-secondary)',
              color: 'var(--text-primary)',
              fontSize: 13,
              minWidth: 220,
            }}
          >
            <option value="">— Select a project —</option>
            {(projects ?? []).map((p: { id: string; name: string }) => (
              <option key={p.id} value={p.id}>{p.name}</option>
            ))}
          </select>
        )}
      </div>

      {/* Create form */}
      {showForm && (
        <form
          onSubmit={handleSubmit}
          style={{
            background: 'var(--bg-secondary)',
            border: '1px solid var(--border)',
            borderRadius: 8,
            padding: '16px',
            marginBottom: 20,
            display: 'flex',
            flexDirection: 'column',
            gap: 10,
          }}
        >
          <span style={{ fontWeight: 600, fontSize: 14, color: 'var(--text-primary)' }}>New template</span>

          <input
            required
            placeholder="Template name *"
            value={form.name}
            onChange={e => setForm(f => ({ ...f, name: e.target.value }))}
            style={inputStyle}
          />

          <input
            required
            placeholder="Default task title *"
            value={form.defaultTitle}
            onChange={e => setForm(f => ({ ...f, defaultTitle: e.target.value }))}
            style={inputStyle}
          />

          <textarea
            placeholder="Default description (optional)"
            value={form.defaultDescription ?? ''}
            onChange={e => setForm(f => ({ ...f, defaultDescription: e.target.value }))}
            rows={3}
            style={{ ...inputStyle, resize: 'vertical' }}
          />

          <div style={{ display: 'flex', gap: 10 }}>
            <select
              value={form.defaultPriority ?? ''}
              onChange={e => setForm(f => ({ ...f, defaultPriority: e.target.value }))}
              style={{ ...inputStyle, flex: 1 }}
            >
              <option value="">Priority (optional)</option>
              {PRIORITY_OPTIONS.map(p => <option key={p} value={p}>{p}</option>)}
            </select>

            <input
              type="number"
              min={0}
              placeholder="Est. hours"
              value={form.defaultEstimatedHours ?? ''}
              onChange={e => setForm(f => ({
                ...f,
                defaultEstimatedHours: e.target.value ? Number(e.target.value) : undefined,
              }))}
              style={{ ...inputStyle, flex: 1 }}
            />
          </div>

          <div style={{ display: 'flex', gap: 8, justifyContent: 'flex-end' }}>
            <button
              type="button"
              onClick={() => setShowForm(false)}
              style={{
                padding: '6px 14px',
                borderRadius: 6,
                border: '1px solid var(--border)',
                background: 'transparent',
                color: 'var(--text-secondary)',
                cursor: 'pointer',
                fontSize: 13,
              }}
            >
              Cancel
            </button>
            <button
              type="submit"
              disabled={createMutation.isPending}
              style={{
                padding: '6px 16px',
                borderRadius: 6,
                border: 'none',
                background: 'var(--accent)',
                color: '#fff',
                fontWeight: 600,
                cursor: createMutation.isPending ? 'not-allowed' : 'pointer',
                fontSize: 13,
                opacity: createMutation.isPending ? 0.7 : 1,
              }}
            >
              {createMutation.isPending ? 'Saving…' : 'Save template'}
            </button>
          </div>
        </form>
      )}

      {/* Success message */}
      {successMsg && (
        <div style={{
          padding: '10px 14px',
          borderRadius: 6,
          background: '#dcfce7',
          color: '#166534',
          fontSize: 13,
          marginBottom: 16,
        }}>
          {successMsg}
        </div>
      )}

      {/* Template list */}
      {!selectedProjectId ? (
        <p style={{ color: 'var(--text-secondary)', fontSize: 13 }}>
          Select a project above to see or manage its templates.
        </p>
      ) : templatesLoading ? (
        <p style={{ color: 'var(--text-secondary)', fontSize: 13 }}>Loading templates…</p>
      ) : templates.length === 0 ? (
        <p style={{ color: 'var(--text-secondary)', fontSize: 13 }}>
          No templates yet. Click <strong>+ New template</strong> to create one.
        </p>
      ) : (
        <div style={{ display: 'flex', flexDirection: 'column', gap: 10 }}>
          {templates.map((t: TaskTemplate) => (
            <TemplateCard
              key={t.id}
              template={t}
              onDelete={id => deleteMutation.mutate(id)}
              onUse={handleUse}
              isUsing={usingId === t.id}
            />
          ))}
        </div>
      )}
    </div>
  );
}

const inputStyle: React.CSSProperties = {
  padding: '7px 10px',
  borderRadius: 6,
  border: '1px solid var(--border)',
  background: 'var(--bg-primary)',
  color: 'var(--text-primary)',
  fontSize: 13,
  width: '100%',
  boxSizing: 'border-box',
};
