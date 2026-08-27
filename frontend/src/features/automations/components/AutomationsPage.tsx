import { useState, useCallback } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { useProjects } from '@/features/projects/hooks/useProjects';
import {
  automationService,
  type AutomationRuleDto,
  type AutomationTriggerType,
  type AutomationActionType,
} from '@/services/automationService';

// ── Label helpers ─────────────────────────────────────────────────────────────

const TRIGGER_LABELS: Record<AutomationTriggerType, string> = {
  TaskStatusChanged:   'Task status changes to',
  TaskCreated:         'A new task is created',
  TaskPriorityChanged: 'Task priority changes to',
};

const ACTION_LABELS: Record<AutomationActionType, string> = {
  SendNotification: 'Send in-app notification',
  PostComment:      'Add comment',
  ChangeStatus:     'Change status to',
  AssignTask:       'Assign task to',
};

const STATUS_OPTIONS = ['Todo', 'InProgress', 'Review', 'Done', 'Blocked'];
const PRIORITY_OPTIONS = ['Low', 'Medium', 'High', 'Critical'];
const TRIGGER_OPTIONS: AutomationTriggerType[] = [
  'TaskStatusChanged',
  'TaskCreated',
  'TaskPriorityChanged',
];
const ACTION_OPTIONS: AutomationActionType[] = [
  'SendNotification',
  'PostComment',
  'ChangeStatus',
  'AssignTask',
];

function triggerValueOptions(trigger: AutomationTriggerType): string[] {
  if (trigger === 'TaskStatusChanged') return STATUS_OPTIONS;
  if (trigger === 'TaskPriorityChanged') return PRIORITY_OPTIONS;
  return [];
}

function actionValuePlaceholder(action: AutomationActionType): string {
  if (action === 'SendNotification') return 'Notification message (use {task} for task title)';
  if (action === 'PostComment') return 'Comment text (use {task} for task title)';
  if (action === 'AssignTask') return 'Assignee name or user ID';
  return '';
}

function actionNeedsTextInput(action: AutomationActionType): boolean {
  return action === 'SendNotification' || action === 'PostComment' || action === 'AssignTask';
}

// ── Toast ─────────────────────────────────────────────────────────────────────

interface ToastMessage {
  id: number;
  text: string;
}

interface ToastProps {
  messages: ToastMessage[];
}

function Toast({ messages }: ToastProps) {
  if (messages.length === 0) return null;
  return (
    <div style={{
      position: 'fixed',
      bottom: 28,
      right: 28,
      display: 'flex',
      flexDirection: 'column',
      gap: 8,
      zIndex: 9999,
      pointerEvents: 'none',
    }}>
      {messages.map(m => (
        <div key={m.id} style={{
          background: 'var(--color-primary, #6366f1)',
          color: '#fff',
          borderRadius: 10,
          padding: '10px 18px',
          fontSize: 13,
          fontWeight: 500,
          boxShadow: '0 4px 16px rgba(0,0,0,0.18)',
          maxWidth: 340,
          wordBreak: 'break-word',
        }}>
          {m.text}
        </div>
      ))}
    </div>
  );
}

function useToast() {
  const [messages, setMessages] = useState<ToastMessage[]>([]);
  let nextId = 0;

  const show = useCallback((text: string, durationMs = 3000) => {
    const id = ++nextId;
    setMessages(prev => [...prev, { id, text }]);
    setTimeout(() => setMessages(prev => prev.filter(m => m.id !== id)), durationMs);
  // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  return { messages, show };
}

// ── Rule card ────────────────────────────────────────────────────────────────

interface RuleCardProps {
  rule: AutomationRuleDto;
  onToggle: (id: string, enabled: boolean) => void;
  onDelete: (id: string) => void;
  onTest: (rule: AutomationRuleDto) => void;
}

function RuleCard({ rule, onToggle, onDelete, onTest }: RuleCardProps) {
  const triggerLabel = TRIGGER_LABELS[rule.triggerType];
  const actionLabel  = ACTION_LABELS[rule.actionType];

  const actionSummary = (() => {
    if (rule.actionType === 'ChangeStatus') return `${actionLabel} "${rule.actionValue}"`;
    if (rule.actionType === 'AssignTask') return `${actionLabel} "${rule.actionValue}"`;
    if (rule.actionValue) {
      const preview = rule.actionValue.length > 50
        ? rule.actionValue.slice(0, 50) + '…'
        : rule.actionValue;
      return `${actionLabel} — "${preview}"`;
    }
    return actionLabel;
  })();

  return (
    <div style={{
      background: 'var(--surface-card, var(--bg-card))',
      border: '1px solid var(--surface-border, var(--border-default))',
      borderRadius: 10,
      padding: '16px 20px',
      display: 'flex',
      alignItems: 'center',
      gap: 16,
    }}>
      {/* Toggle */}
      <label style={{ cursor: 'pointer', flexShrink: 0 }}>
        <input
          type="checkbox"
          checked={rule.isEnabled}
          onChange={e => onToggle(rule.id, e.target.checked)}
          style={{ display: 'none' }}
        />
        <div style={{
          width: 38,
          height: 22,
          borderRadius: 11,
          background: rule.isEnabled ? 'var(--color-primary, #6366f1)' : 'var(--surface-border, var(--border-default))',
          position: 'relative',
          transition: 'background 0.2s',
        }}>
          <div style={{
            position: 'absolute',
            top: 3,
            left: rule.isEnabled ? 19 : 3,
            width: 16,
            height: 16,
            borderRadius: '50%',
            background: '#fff',
            transition: 'left 0.2s',
            boxShadow: '0 1px 3px rgba(0,0,0,0.2)',
          }} />
        </div>
      </label>

      {/* Rule description */}
      <div style={{ flex: 1 }}>
        <div style={{ fontWeight: 600, fontSize: 14, color: 'var(--text-primary)', marginBottom: 4 }}>
          {rule.name}
        </div>
        <div style={{ fontSize: 13, color: 'var(--text-secondary)', display: 'flex', alignItems: 'center', gap: 6, flexWrap: 'wrap' }}>
          <span style={{ background: '#ede9fe', color: '#7c3aed', borderRadius: 5, padding: '2px 8px', fontWeight: 500 }}>
            WHEN
          </span>
          <span>{triggerLabel}{rule.triggerValue ? ` "${rule.triggerValue}"` : ''}</span>
          <span style={{ background: '#fef3c7', color: '#92400e', borderRadius: 5, padding: '2px 8px', fontWeight: 500 }}>
            THEN
          </span>
          <span>{actionSummary}</span>
        </div>
      </div>

      {/* Test button */}
      <button
        type="button"
        onClick={() => onTest(rule)}
        title="Simulate rule trigger"
        style={{
          background: 'none',
          border: '1px solid var(--surface-border, var(--border-default))',
          cursor: 'pointer',
          color: 'var(--text-secondary)',
          fontSize: 12,
          padding: '4px 10px',
          borderRadius: 6,
          flexShrink: 0,
          fontWeight: 500,
        }}
        onMouseEnter={e => {
          e.currentTarget.style.borderColor = 'var(--color-primary, #6366f1)';
          e.currentTarget.style.color = 'var(--color-primary, #6366f1)';
        }}
        onMouseLeave={e => {
          e.currentTarget.style.borderColor = 'var(--surface-border, var(--border-default))';
          e.currentTarget.style.color = 'var(--text-secondary)';
        }}
      >
        Test
      </button>

      {/* Delete */}
      <button
        type="button"
        onClick={() => onDelete(rule.id)}
        title="Delete rule"
        style={{
          background: 'none',
          border: 'none',
          cursor: 'pointer',
          color: 'var(--text-muted)',
          fontSize: 16,
          padding: 4,
          borderRadius: 6,
          flexShrink: 0,
        }}
        onMouseEnter={e => (e.currentTarget.style.color = 'var(--color-danger, #ef4444)')}
        onMouseLeave={e => (e.currentTarget.style.color = 'var(--text-muted)')}
      >
        🗑
      </button>
    </div>
  );
}

// ── New rule form ─────────────────────────────────────────────────────────────

interface NewRuleFormProps {
  projectId: string;
  onCreated: () => void;
  onCancel: () => void;
}

function NewRuleForm({ projectId, onCreated, onCancel }: NewRuleFormProps) {
  const [name, setName]               = useState('');
  const [triggerType, setTriggerType] = useState<AutomationTriggerType>('TaskStatusChanged');
  const [triggerValue, setTriggerValue] = useState('Done');
  const [actionType, setActionType]   = useState<AutomationActionType>('SendNotification');
  const [actionValue, setActionValue] = useState('');
  const [error, setError]             = useState<string | null>(null);

  const qc = useQueryClient();
  const { mutateAsync, isPending } = useMutation({
    mutationFn: () =>
      automationService.create({ projectId, name, triggerType, triggerValue, actionType, actionValue }),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['automations', projectId] });
      onCreated();
    },
  });

  const tvOptions = triggerValueOptions(triggerType);
  const needsTriggerValue = triggerType !== 'TaskCreated';
  const needsTextInput = actionNeedsTextInput(actionType);

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    setError(null);
    if (!name.trim()) { setError('Rule name is required.'); return; }
    if (needsTriggerValue && !triggerValue) { setError('Trigger value is required.'); return; }
    if (needsTextInput && !actionValue.trim()) { setError('Action value is required.'); return; }
    if (actionType === 'ChangeStatus' && !actionValue) { setError('Target status is required.'); return; }
    try { await mutateAsync(); }
    catch { setError('Failed to create rule. Please try again.'); }
  }

  return (
    <form onSubmit={handleSubmit} style={{
      background: 'var(--surface-card, var(--bg-card))',
      border: '2px solid var(--color-primary, #6366f1)',
      borderRadius: 10,
      padding: '20px 24px',
    }}>
      <div style={{ fontWeight: 600, fontSize: 15, marginBottom: 16, color: 'var(--text-primary)' }}>
        New Automation Rule
      </div>

      <div style={{ display: 'grid', gap: 12 }}>
        {/* Name */}
        <label style={{ fontSize: 13, color: 'var(--text-secondary)', fontWeight: 500 }}>
          Rule name
          <input
            value={name}
            onChange={e => setName(e.target.value)}
            placeholder="e.g. Notify when task is done"
            style={{ display: 'block', width: '100%', marginTop: 4 }}
            className="input"
          />
        </label>

        {/* WHEN row */}
        <div style={{ display: 'flex', gap: 8, alignItems: 'flex-start', flexWrap: 'wrap' }}>
          <span style={{
            background: '#ede9fe', color: '#7c3aed', borderRadius: 6,
            padding: '6px 12px', fontWeight: 700, fontSize: 12, marginTop: 22,
          }}>WHEN</span>

          <label style={{ fontSize: 13, color: 'var(--text-secondary)', fontWeight: 500, flex: 1, minWidth: 180 }}>
            Trigger
            <select
              value={triggerType}
              onChange={e => {
                const t = e.target.value as AutomationTriggerType;
                setTriggerType(t);
                const opts = triggerValueOptions(t);
                setTriggerValue(opts[0] ?? '');
              }}
              className="input"
              style={{ display: 'block', width: '100%', marginTop: 4 }}
            >
              {TRIGGER_OPTIONS.map(t => (
                <option key={t} value={t}>{TRIGGER_LABELS[t]}</option>
              ))}
            </select>
          </label>

          {needsTriggerValue && (
            <label style={{ fontSize: 13, color: 'var(--text-secondary)', fontWeight: 500, flex: 1, minWidth: 120 }}>
              Value
              <select
                value={triggerValue}
                onChange={e => setTriggerValue(e.target.value)}
                className="input"
                style={{ display: 'block', width: '100%', marginTop: 4 }}
              >
                {tvOptions.map(v => <option key={v} value={v}>{v}</option>)}
              </select>
            </label>
          )}
        </div>

        {/* THEN row */}
        <div style={{ display: 'flex', gap: 8, alignItems: 'flex-start', flexWrap: 'wrap' }}>
          <span style={{
            background: '#fef3c7', color: '#92400e', borderRadius: 6,
            padding: '6px 12px', fontWeight: 700, fontSize: 12, marginTop: 22,
          }}>THEN</span>

          <label style={{ fontSize: 13, color: 'var(--text-secondary)', fontWeight: 500, flex: 1, minWidth: 180 }}>
            Action
            <select
              value={actionType}
              onChange={e => {
                const a = e.target.value as AutomationActionType;
                setActionType(a);
                setActionValue(a === 'ChangeStatus' ? 'Todo' : '');
              }}
              className="input"
              style={{ display: 'block', width: '100%', marginTop: 4 }}
            >
              {ACTION_OPTIONS.map(a => (
                <option key={a} value={a}>{ACTION_LABELS[a]}</option>
              ))}
            </select>
          </label>

          <label style={{ fontSize: 13, color: 'var(--text-secondary)', fontWeight: 500, flex: 2, minWidth: 200 }}>
            {actionType === 'ChangeStatus' ? 'Target status' : actionType === 'AssignTask' ? 'Assignee' : 'Message'}
            {actionType === 'ChangeStatus' ? (
              <select
                value={actionValue}
                onChange={e => setActionValue(e.target.value)}
                className="input"
                style={{ display: 'block', width: '100%', marginTop: 4 }}
              >
                {STATUS_OPTIONS.map(s => <option key={s} value={s}>{s}</option>)}
              </select>
            ) : (
              <input
                value={actionValue}
                onChange={e => setActionValue(e.target.value)}
                placeholder={actionValuePlaceholder(actionType)}
                className="input"
                style={{ display: 'block', width: '100%', marginTop: 4 }}
              />
            )}
          </label>
        </div>

        {error && (
          <div style={{ color: 'var(--color-danger, #ef4444)', fontSize: 13 }}>{error}</div>
        )}

        <div style={{ display: 'flex', gap: 8, justifyContent: 'flex-end', marginTop: 4 }}>
          <button type="button" onClick={onCancel} className="btn btn-secondary" style={{ fontSize: 13 }}>
            Cancel
          </button>
          <button type="submit" disabled={isPending} className="btn btn-primary" style={{ fontSize: 13 }}>
            {isPending ? 'Creating…' : 'Create Rule'}
          </button>
        </div>
      </div>
    </form>
  );
}

// ── Main page ─────────────────────────────────────────────────────────────────

export function AutomationsPage() {
  const { data: projects = [], isLoading: projectsLoading } = useProjects();
  const [selectedProjectId, setSelectedProjectId] = useState<string>('');
  const [showForm, setShowForm] = useState(false);
  const qc = useQueryClient();
  const { messages: toasts, show: showToast } = useToast();

  const projectId = selectedProjectId || (projects[0]?.id ?? '');

  const { data: rules = [], isLoading: rulesLoading } = useQuery({
    queryKey: ['automations', projectId],
    queryFn: () => automationService.getByProject(projectId),
    enabled: Boolean(projectId),
  });

  const toggleMutation = useMutation({
    mutationFn: ({ id, isEnabled }: { id: string; isEnabled: boolean }) =>
      automationService.toggle(id, isEnabled),
    onSuccess: () => qc.invalidateQueries({ queryKey: ['automations', projectId] }),
  });

  const deleteMutation = useMutation({
    mutationFn: (id: string) => automationService.delete(id),
    onSuccess: () => qc.invalidateQueries({ queryKey: ['automations', projectId] }),
  });

  function handleTest(rule: AutomationRuleDto) {
    const triggerLabel = TRIGGER_LABELS[rule.triggerType];
    const actionLabel  = ACTION_LABELS[rule.actionType];
    const triggerDesc  = rule.triggerValue ? `${triggerLabel} "${rule.triggerValue}"` : triggerLabel;
    showToast(`⚡ "${rule.name}" would trigger: ${triggerDesc} → ${actionLabel}`);
  }

  const isLoading     = projectsLoading || rulesLoading;
  const enabledCount  = rules.filter(r => r.isEnabled).length;
  const disabledCount = rules.length - enabledCount;

  return (
    <div style={{ padding: '24px 32px', maxWidth: 800 }}>
      {/* Header */}
      <div style={{ display: 'flex', alignItems: 'flex-start', justifyContent: 'space-between', marginBottom: 24 }}>
        <div>
          <h1 style={{ fontSize: 22, fontWeight: 700, margin: 0, color: 'var(--text-primary)' }}>
            ⚡ Automation Rules
          </h1>
          <p style={{ fontSize: 14, color: 'var(--text-secondary)', margin: '6px 0 0' }}>
            Define no-code if/then workflows that fire automatically on task events.
          </p>
        </div>
        {!showForm && (
          <button
            type="button"
            className="btn btn-primary"
            onClick={() => setShowForm(true)}
            style={{ fontSize: 13, flexShrink: 0 }}
          >
            + New Rule
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
              onChange={e => { setSelectedProjectId(e.target.value); setShowForm(false); }}
              className="input"
              style={{ display: 'inline-block', marginLeft: 10, minWidth: 200 }}
            >
              {projects.map(p => (
                <option key={p.id} value={p.id}>{p.name}</option>
              ))}
            </select>
          </label>
        </div>
      )}

      {/* Stats */}
      {rules.length > 0 && (
        <div style={{ display: 'flex', gap: 12, marginBottom: 20 }}>
          {[
            { label: 'Total Rules', value: rules.length,   bg: '#f0f4ff', color: '#3730a3' },
            { label: 'Active',      value: enabledCount,   bg: '#ecfdf5', color: '#065f46' },
            { label: 'Paused',      value: disabledCount,  bg: '#f9fafb', color: '#6b7280' },
          ].map(s => (
            <div key={s.label} style={{
              background: s.bg, borderRadius: 10,
              padding: '10px 18px', minWidth: 90,
            }}>
              <div style={{ fontSize: 22, fontWeight: 700, color: s.color }}>{s.value}</div>
              <div style={{ fontSize: 12, color: 'var(--text-secondary)' }}>{s.label}</div>
            </div>
          ))}
        </div>
      )}

      {/* New rule form */}
      {showForm && projectId && (
        <div style={{ marginBottom: 16 }}>
          <NewRuleForm
            projectId={projectId}
            onCreated={() => setShowForm(false)}
            onCancel={() => setShowForm(false)}
          />
        </div>
      )}

      {/* Rules list */}
      {isLoading ? (
        <div style={{ color: 'var(--text-secondary)', fontSize: 14 }}>Loading…</div>
      ) : !projectId ? (
        <div style={{ color: 'var(--text-secondary)', fontSize: 14 }}>
          No projects found. Create a project first.
        </div>
      ) : rules.length === 0 ? (
        <div style={{
          textAlign: 'center', padding: '48px 0',
          color: 'var(--text-secondary)', fontSize: 14,
        }}>
          <div style={{ fontSize: 40, marginBottom: 12 }}>⚡</div>
          <div style={{ fontWeight: 600, marginBottom: 6 }}>No automation rules yet</div>
          <div>Click <strong>+ New Rule</strong> to add your first workflow.</div>
        </div>
      ) : (
        <div style={{ display: 'flex', flexDirection: 'column', gap: 10 }}>
          {rules.map(rule => (
            <RuleCard
              key={rule.id}
              rule={rule}
              onToggle={(id, isEnabled) => toggleMutation.mutate({ id, isEnabled })}
              onDelete={id => deleteMutation.mutate(id)}
              onTest={handleTest}
            />
          ))}
        </div>
      )}

      {/* Toast notifications */}
      <Toast messages={toasts} />
    </div>
  );
}
