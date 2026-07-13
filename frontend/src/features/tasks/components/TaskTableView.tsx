import { useState } from 'react';
import type { Task, TaskStatus } from '../types/task.types';
import { TASK_STATUSES } from '../types/task.types';
import { useUpdateTaskStatus, useUpdateTask } from '../hooks/useTasks';
import { AI_AGENT_ID } from '@/services/taskService';

export type GroupBy = 'none' | 'status' | 'priority' | 'assignee';

// Valid forward/backward transitions, mirroring the backend rules.
const ALLOWED_NEXT: Record<TaskStatus, TaskStatus[]> = {
  Todo: ['Todo', 'InProgress'],
  InProgress: ['InProgress', 'Todo', 'InReview', 'Done'],
  InReview: ['InReview', 'InProgress', 'Done'],
  Done: ['Done'],
};

const STATUS_LABEL: Record<TaskStatus, string> = {
  Todo: 'To Do', InProgress: 'In Progress', InReview: 'In Review', Done: 'Done',
};

interface TaskTableViewProps {
  tasks: Task[];
  groupBy: GroupBy;
  onDelete: (id: string) => void;
}

export function TaskTableView({ tasks, groupBy, onDelete }: TaskTableViewProps) {
  const groups = groupTasks(tasks, groupBy);

  return (
    <div style={{ border: '1px solid var(--border-color)', borderRadius: 'var(--radius-md)', overflow: 'hidden' }}>
      <table style={{ width: '100%', borderCollapse: 'collapse', fontSize: 13 }}>
        <thead>
          <tr style={{ background: 'var(--surface-bg)', textAlign: 'left', color: 'var(--text-muted)' }}>
            <Th w="40%">Title</Th>
            <Th w="130px">Status</Th>
            <Th w="90px">Priority</Th>
            <Th w="110px">Due</Th>
            <Th w="90px">Assignee</Th>
            <Th w="40px" />
          </tr>
        </thead>
        <tbody>
          {groups.map(({ key, items }) => (
            <GroupRows key={key} label={groupBy === 'none' ? null : key} items={items} onDelete={onDelete} />
          ))}
        </tbody>
      </table>
    </div>
  );
}

function GroupRows({ label, items, onDelete }: { label: string | null; items: Task[]; onDelete: (id: string) => void }) {
  return (
    <>
      {label !== null && (
        <tr>
          <td colSpan={6} style={{ background: 'var(--surface-bg)', padding: '6px 12px', fontWeight: 700, fontSize: 12, color: 'var(--text-secondary)' }}>
            {label} <span style={{ color: 'var(--text-muted)', fontWeight: 400 }}>({items.length})</span>
          </td>
        </tr>
      )}
      {items.map(t => <TaskTableRow key={t.id} task={t} onDelete={onDelete} />)}
    </>
  );
}

function TaskTableRow({ task, onDelete }: { task: Task; onDelete: (id: string) => void }) {
  const updateStatus = useUpdateTaskStatus(task.id);
  const updateTask = useUpdateTask(task.id);
  const [editing, setEditing] = useState(false);
  const [draft, setDraft] = useState(task.title);

  const commit = () => {
    const trimmed = draft.trim();
    if (trimmed && trimmed !== task.title) {
      updateTask.mutate({ title: trimmed, description: task.description ?? undefined });
    }
    setEditing(false);
  };

  return (
    <tr style={{ borderTop: '1px solid var(--border-color)' }}>
      <Td>
        {editing ? (
          <input
            className="tf-input" autoFocus value={draft}
            onChange={e => setDraft(e.target.value)} onBlur={commit}
            onKeyDown={e => { if (e.key === 'Enter') commit(); if (e.key === 'Escape') setEditing(false); }}
            style={{ fontSize: 13, padding: '2px 6px', width: '100%' }}
          />
        ) : (
          <span onDoubleClick={() => { setDraft(task.title); setEditing(true); }} title="Double-click to edit" style={{ cursor: 'text' }}>
            {task.title}
          </span>
        )}
      </Td>
      <Td>
        <select
          className="tf-input" value={task.status}
          onChange={e => updateStatus.mutate(e.target.value as TaskStatus)}
          disabled={updateStatus.isPending}
          style={{ fontSize: 12, padding: '2px 4px' }}
        >
          {TASK_STATUSES.filter(s => ALLOWED_NEXT[task.status].includes(s)).map(s => (
            <option key={s} value={s}>{STATUS_LABEL[s]}</option>
          ))}
        </select>
      </Td>
      <Td><span className="tf-badge" style={{ fontSize: 11 }}>{task.priority}</span></Td>
      <Td style={{ color: 'var(--text-muted)' }}>
        {task.dueDate ? new Date(task.dueDate).toLocaleDateString(undefined, { month: 'short', day: 'numeric' }) : '—'}
      </Td>
      <Td style={{ color: 'var(--text-muted)' }}>
        {task.assignedToUserId === AI_AGENT_ID
          ? '🤖 AI Agent'
          : task.assignedToUserId ? `${task.assignedToUserId.slice(0, 8)}…` : '—'}
      </Td>
      <Td>
        <button
          type="button" onClick={() => onDelete(task.id)} aria-label="Delete"
          style={{ background: 'none', border: 'none', cursor: 'pointer', color: 'var(--text-muted)', fontSize: 14 }}
        >×</button>
      </Td>
    </tr>
  );
}

function Th({ children, w }: { children?: React.ReactNode; w: string }) {
  return <th style={{ padding: '8px 12px', width: w, fontSize: 11, textTransform: 'uppercase', letterSpacing: '0.04em', fontWeight: 600 }}>{children}</th>;
}
function Td({ children, style }: { children?: React.ReactNode; style?: React.CSSProperties }) {
  return <td style={{ padding: '6px 12px', ...style }}>{children}</td>;
}

function groupTasks(tasks: Task[], groupBy: GroupBy): { key: string; items: Task[] }[] {
  if (groupBy === 'none') return [{ key: 'all', items: tasks }];

  const keyOf = (t: Task): string => {
    if (groupBy === 'status') return STATUS_LABEL[t.status];
    if (groupBy === 'priority') return t.priority;
    return t.assignedToUserId ? `${t.assignedToUserId.slice(0, 8)}…` : 'Unassigned';
  };

  const map = new Map<string, Task[]>();
  for (const t of tasks) {
    const k = keyOf(t);
    (map.get(k) ?? map.set(k, []).get(k)!).push(t);
  }
  return [...map.entries()].map(([key, items]) => ({ key, items }));
}
