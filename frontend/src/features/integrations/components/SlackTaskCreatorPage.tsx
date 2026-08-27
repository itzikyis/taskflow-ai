import { useState, useEffect } from 'react';
import { useCreateTask } from '@/features/tasks/hooks/useTasks';
import { useProjects } from '@/features/projects/hooks/useProjects';
import { TASK_PRIORITIES } from '@/features/tasks/types/task.types';
import type { TaskPriority } from '@/features/tasks/types/task.types';

const SAMPLE_MESSAGE =
  "Hey team, we need to fix the login timeout issue. It's blocking staging deploys. High priority!";

const SAMPLE_CHANNEL = '#dev-alerts';
const SAMPLE_AUTHOR = '@john.doe';
const SAMPLE_AGO = '2m ago';

const SLACK_TASKS_KEY = 'taskflow-slack-tasks';

interface SlackTask {
  title: string;
  createdAt: string;
  channel: string;
}

function parseMessage(text: string): { title: string; priority: TaskPriority } {
  const lower = text.toLowerCase();
  let priority: TaskPriority = 'Medium';
  if (/urgent|critical|asap/.test(lower)) priority = 'Critical';
  else if (/high priority|blocking/.test(lower)) priority = 'High';
  else if (/low|minor/.test(lower)) priority = 'Low';

  const firstSentence = text.split(/[.!?]/)[0].trim();
  const title = firstSentence.length > 80 ? firstSentence.slice(0, 80) : firstSentence;

  return { title, priority };
}

function loadSlackTasks(): SlackTask[] {
  try {
    const raw = localStorage.getItem(SLACK_TASKS_KEY);
    if (!raw) return [];
    const parsed: unknown = JSON.parse(raw);
    if (!Array.isArray(parsed)) return [];
    return parsed as SlackTask[];
  } catch {
    return [];
  }
}

function saveSlackTask(task: SlackTask): void {
  const existing = loadSlackTasks();
  const updated = [task, ...existing].slice(0, 5);
  localStorage.setItem(SLACK_TASKS_KEY, JSON.stringify(updated));
}

const card: React.CSSProperties = {
  background: 'var(--surface-bg)',
  border: '1px solid var(--surface-border)',
  borderRadius: 'var(--radius-md)',
  padding: 20,
  maxWidth: 720,
  marginBottom: 16,
};

const label: React.CSSProperties = {
  display: 'block',
  fontSize: 13,
  fontWeight: 600,
  color: 'var(--text-secondary)',
  marginBottom: 6,
};

export function SlackTaskCreatorPage() {
  const createTask = useCreateTask();
  const { data: projects } = useProjects();

  const [pastedMessage, setPastedMessage] = useState('');
  const [title, setTitle] = useState('');
  const [description, setDescription] = useState('');
  const [priority, setPriority] = useState<TaskPriority>('Medium');
  const [dueDate, setDueDate] = useState('');
  const [projectId, setProjectId] = useState('');
  const [toast, setToast] = useState<{ text: string; ok: boolean } | null>(null);
  const [recentTasks, setRecentTasks] = useState<SlackTask[]>([]);
  const [autoCreate, setAutoCreate] = useState(false);

  useEffect(() => {
    setRecentTasks(loadSlackTasks());
  }, []);

  function showToast(text: string, ok: boolean) {
    setToast({ text, ok });
    setTimeout(() => setToast(null), 3000);
  }

  function applyMessage(text: string) {
    setPastedMessage(text);
    const parsed = parseMessage(text);
    setTitle(parsed.title);
    setPriority(parsed.priority);
    setDescription(text);
  }

  function handleParse() {
    if (!pastedMessage.trim()) return;
    applyMessage(pastedMessage.trim());
  }

  async function handleCreate() {
    if (!title.trim()) {
      showToast('Title is required.', false);
      return;
    }
    try {
      await createTask.mutateAsync({
        title: title.trim(),
        description: description.trim() || undefined,
        priority,
        dueDate: dueDate || undefined,
      });
      const newTask: SlackTask = {
        title: title.trim(),
        createdAt: new Date().toISOString(),
        channel: SAMPLE_CHANNEL,
      };
      saveSlackTask(newTask);
      setRecentTasks(loadSlackTasks());
      showToast('Task created successfully!', true);
      setTitle('');
      setDescription('');
      setPriority('Medium');
      setDueDate('');
      setProjectId('');
      setPastedMessage('');
    } catch {
      showToast('Failed to create task. Please try again.', false);
    }
  }

  return (
    <div style={{ position: 'relative' }}>
      {toast && (
        <div style={{
          position: 'fixed',
          bottom: 24,
          right: 24,
          background: toast.ok ? '#22c55e' : 'var(--color-danger)',
          color: '#fff',
          padding: '10px 18px',
          borderRadius: 8,
          fontSize: 14,
          fontWeight: 600,
          zIndex: 1000,
          boxShadow: '0 4px 12px rgba(0,0,0,0.25)',
        }}>
          {toast.text}
        </div>
      )}

      {/* Connection status */}
      <div style={card}>
        <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', flexWrap: 'wrap', gap: 10 }}>
          <div style={{ display: 'flex', alignItems: 'center', gap: 12 }}>
            <span style={{ fontSize: 22 }}>💬</span>
            <div>
              <div style={{ fontWeight: 700, fontSize: 15, color: 'var(--text-primary)' }}>
                Slack workspace connected: TaskFlow Workspace
              </div>
              <div style={{ display: 'flex', alignItems: 'center', gap: 6, marginTop: 4 }}>
                <span style={{
                  display: 'inline-block',
                  width: 8,
                  height: 8,
                  borderRadius: '50%',
                  background: '#22c55e',
                }} />
                <span style={{ fontSize: 12, color: '#22c55e', fontWeight: 600 }}>Connected</span>
              </div>
            </div>
          </div>
          <button
            type="button"
            className="tf-btn tf-btn-sm"
            onClick={() => showToast('Workspace disconnected.', true)}
          >
            Disconnect
          </button>
        </div>

        <div style={{ marginTop: 16, display: 'flex', alignItems: 'center', gap: 8 }}>
          <input
            id="auto-create"
            type="checkbox"
            checked={autoCreate}
            onChange={e => setAutoCreate(e.target.checked)}
            style={{ width: 15, height: 15, cursor: 'pointer' }}
          />
          <label htmlFor="auto-create" style={{ fontSize: 13, color: 'var(--text-secondary)', cursor: 'pointer' }}>
            Auto-create tasks from <strong>#dev-alerts</strong> channel mentions
          </label>
        </div>
      </div>

      {/* Slack message simulator */}
      <div style={card}>
        <h3 style={{ margin: '0 0 16px', fontSize: 16, color: 'var(--text-primary)' }}>
          Convert Slack Message to Task
        </h3>

        {/* Sample message */}
        <div style={{
          background: 'var(--surface-card)',
          border: '1px solid var(--surface-border)',
          borderRadius: 8,
          padding: 14,
          marginBottom: 16,
        }}>
          <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: 8 }}>
            <div style={{ fontSize: 12, color: 'var(--text-muted)', fontWeight: 600 }}>
              {SAMPLE_CHANNEL} &nbsp;|&nbsp; {SAMPLE_AUTHOR} &nbsp; {SAMPLE_AGO}
            </div>
            <button
              type="button"
              className="tf-btn tf-btn-sm tf-btn-primary"
              onClick={() => applyMessage(SAMPLE_MESSAGE)}
              style={{ fontSize: 11 }}
            >
              Use this message
            </button>
          </div>
          <p style={{
            margin: 0,
            fontSize: 13,
            color: 'var(--text-secondary)',
            lineHeight: 1.6,
            fontStyle: 'italic',
          }}>
            &ldquo;{SAMPLE_MESSAGE}&rdquo;
          </p>
        </div>

        {/* Paste + parse */}
        <div style={{ marginBottom: 12 }}>
          <label htmlFor="slack-paste" style={label}>Paste Slack message</label>
          <textarea
            id="slack-paste"
            className="tf-input"
            rows={3}
            placeholder="Paste any Slack message here..."
            value={pastedMessage}
            onChange={e => setPastedMessage(e.target.value)}
            style={{ width: '100%', resize: 'vertical', fontFamily: 'inherit', boxSizing: 'border-box' }}
          />
        </div>
        <button
          type="button"
          className="tf-btn tf-btn-primary tf-btn-sm"
          onClick={handleParse}
          disabled={!pastedMessage.trim()}
          style={{ marginBottom: 20 }}
        >
          Parse message
        </button>

        {/* Task form */}
        <div style={{ borderTop: '1px solid var(--surface-border)', paddingTop: 18, display: 'flex', flexDirection: 'column', gap: 14 }}>
          <div>
            <label htmlFor="task-title" style={label}>Title</label>
            <input
              id="task-title"
              className="tf-input"
              type="text"
              placeholder="Task title"
              value={title}
              onChange={e => setTitle(e.target.value)}
              style={{ width: '100%', boxSizing: 'border-box' }}
            />
          </div>

          <div>
            <label htmlFor="task-desc" style={label}>Description</label>
            <textarea
              id="task-desc"
              className="tf-input"
              rows={3}
              placeholder="Task description"
              value={description}
              onChange={e => setDescription(e.target.value)}
              style={{ width: '100%', resize: 'vertical', fontFamily: 'inherit', boxSizing: 'border-box' }}
            />
          </div>

          <div style={{ display: 'flex', gap: 12, flexWrap: 'wrap' }}>
            <div style={{ flex: 1, minWidth: 140 }}>
              <label htmlFor="task-priority" style={label}>Priority</label>
              <select
                id="task-priority"
                className="tf-input"
                value={priority}
                onChange={e => setPriority(e.target.value as TaskPriority)}
                style={{ width: '100%' }}
              >
                {TASK_PRIORITIES.map(p => (
                  <option key={p} value={p}>{p}</option>
                ))}
              </select>
            </div>

            <div style={{ flex: 1, minWidth: 140 }}>
              <label htmlFor="task-due" style={label}>Due date</label>
              <input
                id="task-due"
                className="tf-input"
                type="date"
                value={dueDate}
                onChange={e => setDueDate(e.target.value)}
                style={{ width: '100%', boxSizing: 'border-box' }}
              />
            </div>

            <div style={{ flex: 1, minWidth: 140 }}>
              <label htmlFor="task-project" style={label}>Project</label>
              <select
                id="task-project"
                className="tf-input"
                value={projectId}
                onChange={e => setProjectId(e.target.value)}
                style={{ width: '100%' }}
              >
                <option value="">No project</option>
                {projects?.map(p => (
                  <option key={p.id} value={p.id}>{p.name}</option>
                ))}
              </select>
            </div>
          </div>

          <div>
            <button
              type="button"
              className="tf-btn tf-btn-primary"
              onClick={() => void handleCreate()}
              disabled={createTask.isPending || !title.trim()}
            >
              {createTask.isPending ? 'Creating...' : 'Create Task'}
            </button>
          </div>
        </div>
      </div>

      {/* Recent Slack-sourced tasks */}
      <div style={card}>
        <h3 style={{ margin: '0 0 14px', fontSize: 16, color: 'var(--text-primary)' }}>
          Recent Slack-Sourced Tasks
        </h3>
        {recentTasks.length === 0 ? (
          <p style={{ fontSize: 13, color: 'var(--text-muted)', margin: 0 }}>
            No tasks created from Slack yet. Convert a message above to get started.
          </p>
        ) : (
          <div style={{ display: 'flex', flexDirection: 'column', gap: 8 }}>
            {recentTasks.map((t, i) => (
              <div
                key={i}
                style={{
                  display: 'flex',
                  alignItems: 'center',
                  gap: 12,
                  padding: '9px 12px',
                  background: 'var(--surface-card)',
                  border: '1px solid var(--surface-border)',
                  borderRadius: 6,
                  fontSize: 13,
                }}
              >
                <span style={{ fontSize: 16 }}>💬</span>
                <span style={{ flex: 1, color: 'var(--text-primary)', fontWeight: 500 }}>{t.title}</span>
                <span style={{ fontSize: 11, color: 'var(--text-muted)' }}>{t.channel}</span>
                <span style={{ fontSize: 11, color: 'var(--text-muted)' }}>
                  {new Date(t.createdAt).toLocaleDateString()}
                </span>
              </div>
            ))}
          </div>
        )}
      </div>
    </div>
  );
}
