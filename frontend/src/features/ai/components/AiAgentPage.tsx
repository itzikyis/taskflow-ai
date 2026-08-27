import { useState, useEffect } from 'react';
import axios from 'axios';
import { useAuthStore } from '@/store/authStore';

interface AgentTask {
  id: string;
  taskType: string;
  schedule: 'Manual' | 'Daily' | 'Weekly' | 'Monthly';
  status: 'Waiting' | 'Running' | 'Done';
  lastRun: string | null;
  lastResult: string | null;
  expanded: boolean;
}

interface ActivityEntry {
  id: string;
  timestamp: string;
  taskType: string;
  result: string;
}

const STORAGE_KEY_TASKS = 'taskflow-ai-agent-tasks';
const STORAGE_KEY_LOG = 'taskflow-ai-agent-log';
const MAX_LOG_ENTRIES = 10;

const SCHEDULE_OPTIONS: AgentTask['schedule'][] = ['Manual', 'Daily', 'Weekly', 'Monthly'];

function loadTasks(): AgentTask[] {
  try {
    const raw = localStorage.getItem(STORAGE_KEY_TASKS);
    return raw ? (JSON.parse(raw) as AgentTask[]) : [];
  } catch {
    return [];
  }
}

function saveTasks(tasks: AgentTask[]): void {
  localStorage.setItem(STORAGE_KEY_TASKS, JSON.stringify(tasks));
}

function loadLog(): ActivityEntry[] {
  try {
    const raw = localStorage.getItem(STORAGE_KEY_LOG);
    return raw ? (JSON.parse(raw) as ActivityEntry[]) : [];
  } catch {
    return [];
  }
}

function saveLog(entries: ActivityEntry[]): void {
  localStorage.setItem(STORAGE_KEY_LOG, JSON.stringify(entries));
}

const cardStyle: React.CSSProperties = {
  background: 'var(--surface-card)',
  border: '1px solid var(--surface-border)',
  borderRadius: 'var(--radius-md)',
  padding: '16px 20px',
  marginBottom: 12,
};

export function AiAgentPage() {
  const { token } = useAuthStore();

  const [tasks, setTasks] = useState<AgentTask[]>(loadTasks);
  const [log, setLog] = useState<ActivityEntry[]>(loadLog);

  const [newTaskType, setNewTaskType] = useState('');
  const [newSchedule, setNewSchedule] = useState<AgentTask['schedule']>('Manual');

  useEffect(() => {
    saveTasks(tasks);
  }, [tasks]);

  useEffect(() => {
    saveLog(log);
  }, [log]);

  function handleAssign() {
    const trimmed = newTaskType.trim();
    if (!trimmed) return;
    const entry: AgentTask = {
      id: `${Date.now()}-${Math.random()}`,
      taskType: trimmed,
      schedule: newSchedule,
      status: 'Waiting',
      lastRun: null,
      lastResult: null,
      expanded: false,
    };
    setTasks(prev => [...prev, entry]);
    setNewTaskType('');
    setNewSchedule('Manual');
  }

  async function handleRunNow(id: string) {
    const task = tasks.find(t => t.id === id);
    if (!task) return;

    setTasks(prev =>
      prev.map(t => t.id === id ? { ...t, status: 'Running' as const, expanded: false } : t)
    );

    const prompt = `You are an AI task agent. Complete this task type autonomously: ${task.taskType}. Provide a detailed completion report in 3-5 sentences.`;

    try {
      const headers: Record<string, string> = { 'Content-Type': 'application/json' };
      if (token?.token) {
        headers['Authorization'] = `Bearer ${token.token}`;
      }
      const response = await axios.post<{ answer: string }>(
        '/api/ai/ask',
        { question: prompt },
        { headers }
      );
      const answer = response.data.answer;
      const now = new Date().toISOString();

      setTasks(prev =>
        prev.map(t =>
          t.id === id
            ? { ...t, status: 'Done' as const, lastRun: now, lastResult: answer, expanded: true }
            : t
        )
      );

      const logEntry: ActivityEntry = {
        id: `${Date.now()}-${Math.random()}`,
        timestamp: now,
        taskType: task.taskType,
        result: answer,
      };
      setLog(prev => [logEntry, ...prev].slice(0, MAX_LOG_ENTRIES));
    } catch {
      setTasks(prev =>
        prev.map(t =>
          t.id === id
            ? { ...t, status: 'Waiting' as const, lastResult: 'Error: failed to get AI response.' }
            : t
        )
      );
    }
  }

  function handleDelete(id: string) {
    setTasks(prev => prev.filter(t => t.id !== id));
  }

  function toggleExpanded(id: string) {
    setTasks(prev =>
      prev.map(t => t.id === id ? { ...t, expanded: !t.expanded } : t)
    );
  }

  const statusColor: Record<AgentTask['status'], string> = {
    Waiting: 'var(--text-muted)',
    Running: 'var(--color-primary)',
    Done: '#22c55e',
  };

  return (
    <div style={{ maxWidth: 800, margin: '0 auto', padding: '24px 16px' }}>

      {/* Agent config panel */}
      <div style={{ ...cardStyle, marginBottom: 24 }}>
        <div style={{ display: 'flex', alignItems: 'center', gap: 12, marginBottom: 8 }}>
          <span style={{ fontSize: 28 }}>🤖</span>
          <div>
            <div style={{ display: 'flex', alignItems: 'center', gap: 10 }}>
              <span style={{ fontWeight: 700, fontSize: 18, color: 'var(--text-primary)' }}>
                TaskFlow AI Agent
              </span>
              <span style={{
                background: '#22c55e22',
                color: '#22c55e',
                border: '1px solid #22c55e55',
                borderRadius: 999,
                fontSize: 11,
                fontWeight: 600,
                padding: '2px 10px',
              }}>
                Active
              </span>
            </div>
            <p style={{ margin: '4px 0 0', fontSize: 13, color: 'var(--text-secondary)' }}>
              Autonomously handles assigned task types using AI
            </p>
          </div>
        </div>
      </div>

      {/* Assign task type form */}
      <div style={{ ...cardStyle, marginBottom: 24 }}>
        <h2 style={{ margin: '0 0 14px', fontSize: 15, fontWeight: 600, color: 'var(--text-primary)' }}>
          Assign Task Type
        </h2>
        <label style={{ display: 'block', marginBottom: 6, fontSize: 13, color: 'var(--text-secondary)' }}>
          Task type / description
        </label>
        <textarea
          value={newTaskType}
          onChange={e => setNewTaskType(e.target.value)}
          placeholder='e.g. "Weekly status report", "Code review summary"'
          rows={3}
          style={{
            width: '100%',
            boxSizing: 'border-box',
            resize: 'vertical',
            background: 'var(--surface-bg)',
            border: '1px solid var(--surface-border)',
            borderRadius: 'var(--radius-md)',
            color: 'var(--text-primary)',
            fontSize: 13,
            padding: '8px 10px',
            fontFamily: 'inherit',
            marginBottom: 12,
          }}
        />
        <div style={{ display: 'flex', gap: 10, alignItems: 'center', flexWrap: 'wrap' }}>
          <label style={{ fontSize: 13, color: 'var(--text-secondary)', whiteSpace: 'nowrap' }}>
            Schedule:
          </label>
          <select
            value={newSchedule}
            onChange={e => setNewSchedule(e.target.value as AgentTask['schedule'])}
            style={{
              background: 'var(--surface-bg)',
              border: '1px solid var(--surface-border)',
              borderRadius: 'var(--radius-md)',
              color: 'var(--text-primary)',
              fontSize: 13,
              padding: '6px 10px',
              fontFamily: 'inherit',
            }}
          >
            {SCHEDULE_OPTIONS.map(s => (
              <option key={s} value={s}>{s}</option>
            ))}
          </select>
          <button
            type="button"
            onClick={handleAssign}
            disabled={!newTaskType.trim()}
            style={{
              background: 'var(--color-primary)',
              color: '#fff',
              border: 'none',
              borderRadius: 'var(--radius-md)',
              padding: '7px 18px',
              fontSize: 13,
              fontWeight: 600,
              cursor: newTaskType.trim() ? 'pointer' : 'not-allowed',
              opacity: newTaskType.trim() ? 1 : 0.5,
              fontFamily: 'inherit',
            }}
          >
            Assign to AI
          </button>
        </div>
      </div>

      {/* Assigned tasks list */}
      <div style={{ marginBottom: 24 }}>
        <h2 style={{ margin: '0 0 12px', fontSize: 15, fontWeight: 600, color: 'var(--text-primary)' }}>
          Assigned Tasks
        </h2>
        {tasks.length === 0 && (
          <p style={{ color: 'var(--text-muted)', fontSize: 13 }}>
            No task types assigned yet. Use the form above to assign one.
          </p>
        )}
        {tasks.map(task => (
          <div key={task.id} style={cardStyle}>
            <div style={{ display: 'flex', alignItems: 'flex-start', gap: 10 }}>
              <div style={{ flex: 1, minWidth: 0 }}>
                <div style={{ display: 'flex', alignItems: 'center', gap: 8, flexWrap: 'wrap' }}>
                  <span style={{ fontWeight: 600, fontSize: 14, color: 'var(--text-primary)', wordBreak: 'break-word' }}>
                    {task.taskType}
                  </span>
                  <span style={{
                    fontSize: 11,
                    color: 'var(--text-muted)',
                    background: 'var(--surface-bg)',
                    border: '1px solid var(--surface-border)',
                    borderRadius: 999,
                    padding: '1px 8px',
                  }}>
                    {task.schedule}
                  </span>
                  <span style={{ fontSize: 12, fontWeight: 600, color: statusColor[task.status] }}>
                    {task.status}
                  </span>
                </div>
                {task.lastRun && (
                  <div style={{ fontSize: 11, color: 'var(--text-muted)', marginTop: 4 }}>
                    Last run: {new Date(task.lastRun).toLocaleString()}
                  </div>
                )}
              </div>
              <div style={{ display: 'flex', gap: 8, flexShrink: 0 }}>
                <button
                  type="button"
                  onClick={() => void handleRunNow(task.id)}
                  disabled={task.status === 'Running'}
                  style={{
                    background: 'var(--color-primary)',
                    color: '#fff',
                    border: 'none',
                    borderRadius: 'var(--radius-md)',
                    padding: '5px 14px',
                    fontSize: 12,
                    fontWeight: 600,
                    cursor: task.status === 'Running' ? 'not-allowed' : 'pointer',
                    opacity: task.status === 'Running' ? 0.6 : 1,
                    fontFamily: 'inherit',
                  }}
                >
                  {task.status === 'Running' ? 'Running…' : 'Run Now'}
                </button>
                <button
                  type="button"
                  onClick={() => handleDelete(task.id)}
                  aria-label="Remove task"
                  style={{
                    background: 'transparent',
                    color: 'var(--color-danger)',
                    border: '1px solid var(--color-danger)',
                    borderRadius: 'var(--radius-md)',
                    padding: '5px 10px',
                    fontSize: 13,
                    cursor: 'pointer',
                    fontFamily: 'inherit',
                  }}
                >
                  ×
                </button>
              </div>
            </div>

            {task.lastResult && task.status === 'Done' && (
              <div style={{ marginTop: 10 }}>
                <button
                  type="button"
                  onClick={() => toggleExpanded(task.id)}
                  style={{
                    background: 'transparent',
                    border: 'none',
                    color: 'var(--color-primary)',
                    fontSize: 12,
                    cursor: 'pointer',
                    fontFamily: 'inherit',
                    padding: 0,
                  }}
                >
                  {task.expanded ? '▾ Hide result' : '▸ Show result'}
                </button>
                {task.expanded && (
                  <div style={{
                    marginTop: 8,
                    padding: '10px 12px',
                    background: 'var(--surface-bg)',
                    border: '1px solid var(--surface-border)',
                    borderRadius: 'var(--radius-md)',
                    fontSize: 13,
                    color: 'var(--text-secondary)',
                    lineHeight: 1.6,
                    whiteSpace: 'pre-wrap',
                  }}>
                    {task.lastResult}
                  </div>
                )}
              </div>
            )}
          </div>
        ))}
      </div>

      {/* Activity log */}
      <div>
        <h2 style={{ margin: '0 0 12px', fontSize: 15, fontWeight: 600, color: 'var(--text-primary)' }}>
          Activity Log
        </h2>
        <div style={{
          background: 'var(--surface-card)',
          border: '1px solid var(--surface-border)',
          borderRadius: 'var(--radius-md)',
          maxHeight: 320,
          overflowY: 'auto',
        }}>
          {log.length === 0 && (
            <p style={{ padding: '16px 20px', color: 'var(--text-muted)', fontSize: 13, margin: 0 }}>
              No agent runs yet.
            </p>
          )}
          {log.map(entry => (
            <div
              key={entry.id}
              style={{
                padding: '10px 16px',
                borderBottom: '1px solid var(--surface-border)',
                display: 'flex',
                gap: 12,
                alignItems: 'flex-start',
              }}
            >
              <span style={{ fontSize: 11, color: 'var(--text-muted)', whiteSpace: 'nowrap', paddingTop: 2 }}>
                {new Date(entry.timestamp).toLocaleString()}
              </span>
              <div style={{ minWidth: 0 }}>
                <div style={{ fontSize: 12, fontWeight: 600, color: 'var(--text-primary)', marginBottom: 2 }}>
                  {entry.taskType}
                </div>
                <div style={{ fontSize: 12, color: 'var(--text-secondary)', wordBreak: 'break-word' }}>
                  {entry.result.length > 100 ? `${entry.result.slice(0, 100)}…` : entry.result}
                </div>
              </div>
            </div>
          ))}
        </div>
      </div>
    </div>
  );
}
