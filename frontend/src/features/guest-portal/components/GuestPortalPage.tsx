import { useState } from 'react';
import { useProjects } from '@/features/projects/hooks/useProjects';
import { useAllTasks } from '@/features/tasks/hooks/useTasks';
import type { Task, TaskStatus } from '@/features/tasks/types/task.types';
import type { Project } from '@/features/projects/types/project.types';

const STATUS_COLUMNS: { status: TaskStatus; label: string; color: string }[] = [
  { status: 'Todo',       label: 'To Do',       color: '#6b7280' },
  { status: 'InProgress', label: 'In Progress',  color: '#f59e0b' },
  { status: 'InReview',   label: 'In Review',    color: '#8b5cf6' },
  { status: 'Done',       label: 'Done',         color: '#10b981' },
];

const HIGH_PRIORITY_VALUES: Task['priority'][] = ['High', 'Critical'];

function formatDate(dateStr: string | null): string {
  if (!dateStr) return 'No due date';
  return new Date(dateStr).toLocaleDateString(undefined, {
    month: 'short',
    day: 'numeric',
    year: 'numeric',
  });
}

const CARD_STYLE: React.CSSProperties = {
  background: 'var(--surface-card)',
  border: '1px solid var(--surface-border)',
  borderRadius: 'var(--radius-md)',
  padding: '12px 14px',
  marginBottom: 8,
};

const BADGE_BASE: React.CSSProperties = {
  display: 'inline-block',
  fontSize: 11,
  fontWeight: 600,
  borderRadius: 4,
  padding: '2px 8px',
};

interface TaskCardProps {
  task: Task;
}

function TaskCard({ task }: TaskCardProps) {
  return (
    <div style={CARD_STYLE}>
      <div style={{ fontSize: 13, fontWeight: 600, color: 'var(--text-primary)', marginBottom: 4 }}>
        {task.title}
      </div>
      <div style={{ fontSize: 11, color: 'var(--text-muted)' }}>
        {formatDate(task.dueDate)}
      </div>
    </div>
  );
}

interface KanbanColumnProps {
  status: TaskStatus;
  label: string;
  color: string;
  tasks: Task[];
}

function KanbanColumn({ label, color, tasks }: KanbanColumnProps) {
  return (
    <div style={{ flex: 1, minWidth: 180 }}>
      <div style={{ display: 'flex', alignItems: 'center', gap: 8, marginBottom: 12 }}>
        <span style={{ ...BADGE_BASE, background: color + '22', color }}>{label}</span>
        <span style={{ fontSize: 12, color: 'var(--text-muted)', fontWeight: 500 }}>
          {tasks.length}
        </span>
      </div>
      {tasks.length === 0 ? (
        <div style={{ fontSize: 12, color: 'var(--text-muted)', padding: '8px 0', fontStyle: 'italic' }}>
          No tasks
        </div>
      ) : (
        tasks.map(t => <TaskCard key={t.id} task={t} />)
      )}
    </div>
  );
}

interface ProjectHeaderProps {
  project: Project;
  tasks: Task[];
}

function ProjectHeader({ project, tasks }: ProjectHeaderProps) {
  const done  = tasks.filter(t => t.status === 'Done').length;
  const total = tasks.length;
  const pct   = total === 0 ? 0 : Math.round((done / total) * 100);

  return (
    <div style={{ marginBottom: 28 }}>
      <h2 style={{ fontSize: 22, fontWeight: 700, color: 'var(--text-primary)', margin: '0 0 6px' }}>
        {project.name}
      </h2>
      {project.description && (
        <p style={{ fontSize: 14, color: 'var(--text-secondary)', margin: '0 0 16px' }}>
          {project.description}
        </p>
      )}

      {/* Progress summary */}
      <div style={{
        background: 'var(--surface-card)',
        border: '1px solid var(--surface-border)',
        borderRadius: 'var(--radius-md)',
        padding: '14px 18px',
        display: 'inline-flex',
        alignItems: 'center',
        gap: 20,
        minWidth: 340,
      }}>
        <div>
          <div style={{ fontSize: 12, color: 'var(--text-muted)', fontWeight: 500, marginBottom: 2 }}>
            PROGRESS
          </div>
          <div style={{ fontSize: 15, fontWeight: 700, color: 'var(--text-primary)' }}>
            {done} of {total} tasks complete — {pct}%
          </div>
        </div>
        <div style={{ flex: 1, minWidth: 140 }}>
          <div style={{
            height: 8,
            background: 'var(--surface-border)',
            borderRadius: 999,
            overflow: 'hidden',
          }}>
            <div style={{
              height: '100%',
              width: `${pct}%`,
              background: 'var(--color-primary)',
              borderRadius: 999,
              transition: 'width 0.4s ease',
            }} />
          </div>
        </div>
      </div>
    </div>
  );
}

interface MilestonesSectionProps {
  tasks: Task[];
}

function MilestonesSection({ tasks }: MilestonesSectionProps) {
  const milestones = tasks.filter(t => HIGH_PRIORITY_VALUES.includes(t.priority));

  if (milestones.length === 0) return null;

  const priorityColor = (p: Task['priority']): string =>
    p === 'Critical' ? 'var(--color-danger)' : '#f59e0b';

  return (
    <section style={{ marginTop: 32 }}>
      <h3 style={{ fontSize: 15, fontWeight: 700, color: 'var(--text-primary)', margin: '0 0 14px' }}>
        Milestones &amp; High-Priority Items
      </h3>
      <div style={{
        background: 'var(--surface-card)',
        border: '1px solid var(--surface-border)',
        borderRadius: 'var(--radius-md)',
        overflow: 'hidden',
      }}>
        {milestones.map((task, idx) => (
          <div
            key={task.id}
            style={{
              display: 'flex',
              alignItems: 'center',
              justifyContent: 'space-between',
              padding: '12px 18px',
              borderBottom: idx < milestones.length - 1 ? '1px solid var(--surface-border)' : 'none',
              gap: 16,
            }}
          >
            <div style={{ display: 'flex', alignItems: 'center', gap: 10 }}>
              <span style={{
                ...BADGE_BASE,
                background: priorityColor(task.priority) + '22',
                color: priorityColor(task.priority),
              }}>
                {task.priority}
              </span>
              <span style={{ fontSize: 13, fontWeight: 600, color: 'var(--text-primary)' }}>
                {task.title}
              </span>
            </div>
            <div style={{ display: 'flex', alignItems: 'center', gap: 12, flexShrink: 0 }}>
              <span style={{ fontSize: 12, color: 'var(--text-muted)' }}>
                {formatDate(task.dueDate)}
              </span>
              <span style={{
                ...BADGE_BASE,
                background: STATUS_COLUMNS.find(c => c.status === task.status)?.color + '22' ?? '#6b728022',
                color: STATUS_COLUMNS.find(c => c.status === task.status)?.color ?? '#6b7280',
              }}>
                {STATUS_COLUMNS.find(c => c.status === task.status)?.label ?? task.status}
              </span>
            </div>
          </div>
        ))}
      </div>
    </section>
  );
}

export function GuestPortalPage() {
  const { data: projects, isLoading: projectsLoading } = useProjects();
  const { data: allTasks, isLoading: tasksLoading }    = useAllTasks();

  const [selectedProjectId, setSelectedProjectId] = useState<string>('');

  const selectedProject =
    projects?.find(p => p.id === selectedProjectId) ?? projects?.[0] ?? null;

  const tasks: Task[] = allTasks ?? [];

  if (projectsLoading || tasksLoading) {
    return (
      <div style={{ padding: 40, color: 'var(--text-muted)', fontSize: 14 }}>
        Loading portal…
      </div>
    );
  }

  if (!projects || projects.length === 0) {
    return (
      <div style={{ padding: 40, color: 'var(--text-muted)', fontSize: 14 }}>
        No projects available.
      </div>
    );
  }

  const currentProjectId = selectedProjectId || projects[0].id;
  const currentProject   = projects.find(p => p.id === currentProjectId) ?? projects[0];

  return (
    <div style={{ display: 'flex', height: '100%', overflow: 'hidden' }}>

      {/* ── Left panel: project selector ─────────────────────────────── */}
      <aside style={{
        width: 220,
        flexShrink: 0,
        borderRight: '1px solid var(--surface-border)',
        background: 'var(--surface-bg)',
        padding: '24px 12px',
        overflowY: 'auto',
      }}>
        <div style={{
          fontSize: 11,
          fontWeight: 700,
          letterSpacing: '0.08em',
          color: 'var(--text-muted)',
          textTransform: 'uppercase',
          marginBottom: 10,
          paddingLeft: 8,
        }}>
          Projects
        </div>
        {projects.map(project => (
          <button
            key={project.id}
            type="button"
            onClick={() => setSelectedProjectId(project.id)}
            style={{
              width: '100%',
              textAlign: 'left',
              background: currentProjectId === project.id ? 'var(--color-primary)1a' : 'transparent',
              border: 'none',
              borderRadius: 'var(--radius-md)',
              padding: '8px 10px',
              cursor: 'pointer',
              color: currentProjectId === project.id ? 'var(--color-primary)' : 'var(--text-secondary)',
              fontWeight: currentProjectId === project.id ? 600 : 400,
              fontSize: 13,
              fontFamily: 'inherit',
              marginBottom: 2,
            }}
          >
            {project.name}
          </button>
        ))}
      </aside>

      {/* ── Main area ────────────────────────────────────────────────── */}
      <div style={{ flex: 1, overflowY: 'auto', padding: '28px 32px' }}>

        {/* Project header + progress */}
        <ProjectHeader project={currentProject} tasks={tasks} />

        {/* Kanban status board */}
        <section>
          <h3 style={{ fontSize: 15, fontWeight: 700, color: 'var(--text-primary)', margin: '0 0 16px' }}>
            Status Board
          </h3>
          <div style={{ display: 'flex', gap: 16, alignItems: 'flex-start', overflowX: 'auto', paddingBottom: 8 }}>
            {STATUS_COLUMNS.map(col => (
              <KanbanColumn
                key={col.status}
                status={col.status}
                label={col.label}
                color={col.color}
                tasks={tasks.filter(t => t.status === col.status)}
              />
            ))}
          </div>
        </section>

        {/* Milestones */}
        <MilestonesSection tasks={tasks} />

        {/* Read-only notice */}
        <div style={{
          marginTop: 40,
          padding: '10px 16px',
          background: 'var(--surface-card)',
          border: '1px solid var(--surface-border)',
          borderRadius: 'var(--radius-md)',
          fontSize: 12,
          color: 'var(--text-muted)',
          display: 'flex',
          alignItems: 'center',
          gap: 8,
        }}>
          <span>🔗</span>
          <span>This is a read-only guest view. Contact your project manager to make changes.</span>
        </div>
      </div>
    </div>
  );
}
