import { useState, useRef, useEffect } from 'react';
import { useQuery } from '@tanstack/react-query';
import { taskService } from '@/services/taskService';
import type { Task, TaskPriority } from '@/features/tasks/types/task.types';

// ── Constants ────────────────────────────────────────────────────────────────

const DAY_NAMES = ['Sun', 'Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat'] as const;

const MONTH_NAMES = [
  'January', 'February', 'March', 'April', 'May', 'June',
  'July', 'August', 'September', 'October', 'November', 'December',
] as const;

const PRIORITY_COLORS: Record<TaskPriority, string> = {
  Low:      'var(--priority-low)',
  Medium:   'var(--priority-medium)',
  High:     'var(--priority-high)',
  Critical: 'var(--priority-critical)',
};

const STATUS_LABELS: Record<string, string> = {
  Todo:       'To Do',
  InProgress: 'In Progress',
  InReview:   'In Review',
  Done:       'Done',
};

const MAX_PILLS_PER_CELL = 3;

// ── Helpers ──────────────────────────────────────────────────────────────────

function toLocalDateString(isoString: string): string {
  // Extract the date part from ISO string without timezone shifting
  return isoString.slice(0, 10);
}

function buildCalendarGrid(year: number, month: number): Array<Date | null> {
  const firstDay = new Date(year, month, 1);
  const lastDay  = new Date(year, month + 1, 0);
  const cells: Array<Date | null> = [];

  // Leading empty cells
  for (let i = 0; i < firstDay.getDay(); i++) {
    cells.push(null);
  }
  // Day cells
  for (let d = 1; d <= lastDay.getDate(); d++) {
    cells.push(new Date(year, month, d));
  }
  // Trailing empty cells to complete the last row
  const remainder = cells.length % 7;
  if (remainder !== 0) {
    for (let i = 0; i < 7 - remainder; i++) {
      cells.push(null);
    }
  }
  return cells;
}

function groupTasksByDate(tasks: Task[]): Map<string, Task[]> {
  const map = new Map<string, Task[]>();
  for (const task of tasks) {
    if (!task.dueDate) continue;
    const key = toLocalDateString(task.dueDate);
    if (!map.has(key)) map.set(key, []);
    map.get(key)!.push(task);
  }
  return map;
}

function dateKey(d: Date): string {
  const y = d.getFullYear();
  const m = String(d.getMonth() + 1).padStart(2, '0');
  const day = String(d.getDate()).padStart(2, '0');
  return `${y}-${m}-${day}`;
}

function todayKey(): string {
  return dateKey(new Date());
}

// ── Sub-components ───────────────────────────────────────────────────────────

interface TaskPillProps {
  task: Task;
  onOpen: (task: Task, anchor: DOMRect) => void;
}

function TaskPill({ task, onOpen }: TaskPillProps) {
  const handleClick = (e: React.MouseEvent<HTMLButtonElement>) => {
    e.stopPropagation();
    onOpen(task, e.currentTarget.getBoundingClientRect());
  };

  const pillStyle: React.CSSProperties = {
    display: 'flex',
    alignItems: 'center',
    gap: 4,
    width: '100%',
    padding: '2px 5px',
    borderRadius: 4,
    background: 'var(--surface-card)',
    border: '1px solid var(--surface-border)',
    cursor: 'pointer',
    fontSize: 11,
    color: 'var(--text-primary)',
    textAlign: 'left',
    overflow: 'hidden',
    whiteSpace: 'nowrap',
    textOverflow: 'ellipsis',
    fontFamily: 'inherit',
  };

  return (
    <button type="button" style={pillStyle} onClick={handleClick} title={task.title}>
      <span
        style={{
          flexShrink: 0,
          width: 7,
          height: 7,
          borderRadius: '50%',
          background: PRIORITY_COLORS[task.priority],
        }}
      />
      <span style={{ overflow: 'hidden', textOverflow: 'ellipsis' }}>{task.title}</span>
    </button>
  );
}

interface TaskDetailPopoverProps {
  task: Task;
  anchorRect: DOMRect;
  onClose: () => void;
}

function TaskDetailPopover({ task, anchorRect, onClose }: TaskDetailPopoverProps) {
  const ref = useRef<HTMLDivElement>(null);

  useEffect(() => {
    const handleKey = (e: KeyboardEvent) => {
      if (e.key === 'Escape') onClose();
    };
    const handleClick = (e: MouseEvent) => {
      if (ref.current && !ref.current.contains(e.target as Node)) onClose();
    };
    document.addEventListener('keydown', handleKey);
    document.addEventListener('mousedown', handleClick);
    return () => {
      document.removeEventListener('keydown', handleKey);
      document.removeEventListener('mousedown', handleClick);
    };
  }, [onClose]);

  // Position below anchor, clamp to viewport
  const POPOVER_WIDTH = 260;
  const POPOVER_OFFSET = 6;
  let left = anchorRect.left;
  let top  = anchorRect.bottom + POPOVER_OFFSET + window.scrollY;
  if (left + POPOVER_WIDTH > window.innerWidth - 8) {
    left = window.innerWidth - POPOVER_WIDTH - 8;
  }

  const popoverStyle: React.CSSProperties = {
    position: 'fixed',
    top: top - window.scrollY,
    left,
    width: POPOVER_WIDTH,
    background: 'var(--surface-card)',
    border: '1px solid var(--surface-border)',
    borderRadius: 8,
    boxShadow: '0 8px 24px rgba(0,0,0,0.25)',
    padding: '12px 14px',
    zIndex: 1000,
    color: 'var(--text-primary)',
    fontSize: 13,
  };

  const rowStyle: React.CSSProperties = {
    display: 'flex',
    justifyContent: 'space-between',
    alignItems: 'center',
    marginBottom: 6,
  };

  const labelStyle: React.CSSProperties = { color: 'var(--text-muted)', fontSize: 11 };

  return (
    <div ref={ref} style={popoverStyle} role="dialog" aria-modal="true" aria-label="Task detail">
      <div style={{ fontWeight: 600, marginBottom: 10, fontSize: 13, lineHeight: 1.4 }}>
        {task.title}
      </div>

      <div style={rowStyle}>
        <span style={labelStyle}>Status</span>
        <span>{STATUS_LABELS[task.status] ?? task.status}</span>
      </div>

      <div style={rowStyle}>
        <span style={labelStyle}>Priority</span>
        <span style={{ display: 'flex', alignItems: 'center', gap: 5 }}>
          <span
            style={{
              width: 8,
              height: 8,
              borderRadius: '50%',
              background: PRIORITY_COLORS[task.priority],
              display: 'inline-block',
            }}
          />
          {task.priority}
        </span>
      </div>

      <div style={rowStyle}>
        <span style={labelStyle}>Due date</span>
        <span>
          {task.dueDate ? new Date(task.dueDate).toLocaleDateString(undefined, { month: 'short', day: 'numeric', year: 'numeric' }) : '—'}
        </span>
      </div>

      <div style={rowStyle}>
        <span style={labelStyle}>Assignee</span>
        <span style={{ color: task.assignedToUserId ? 'var(--text-primary)' : 'var(--text-muted)' }}>
          {task.assignedToUserId ? task.assignedToUserId.slice(0, 8) + '…' : 'Unassigned'}
        </span>
      </div>

      <button
        type="button"
        onClick={onClose}
        style={{
          marginTop: 10,
          width: '100%',
          padding: '5px 0',
          background: 'transparent',
          border: '1px solid var(--surface-border)',
          borderRadius: 5,
          cursor: 'pointer',
          color: 'var(--text-secondary)',
          fontSize: 12,
          fontFamily: 'inherit',
        }}
      >
        Close
      </button>
    </div>
  );
}

// ── CalendarPage ─────────────────────────────────────────────────────────────

export function CalendarPage() {
  const today = new Date();
  const [year, setYear]   = useState(today.getFullYear());
  const [month, setMonth] = useState(today.getMonth());
  const [popover, setPopover] = useState<{ task: Task; anchor: DOMRect } | null>(null);

  const { data: tasks = [], isLoading } = useQuery({
    queryKey: ['tasks'],
    queryFn: () => taskService.getAll(),
  });

  const tasksByDate = groupTasksByDate(tasks);
  const noDueDateTasks = tasks.filter(t => !t.dueDate);
  const grid = buildCalendarGrid(year, month);
  const todayStr = todayKey();

  const prevMonth = () => {
    if (month === 0) { setYear(y => y - 1); setMonth(11); }
    else setMonth(m => m - 1);
  };

  const nextMonth = () => {
    if (month === 11) { setYear(y => y + 1); setMonth(0); }
    else setMonth(m => m + 1);
  };

  const openPopover = (task: Task, anchor: DOMRect) => {
    setPopover({ task, anchor });
  };

  const closePopover = () => setPopover(null);

  // ── Styles ────────────────────────────────────────────────────────────────

  const pageStyle: React.CSSProperties = {
    display: 'flex',
    flexDirection: 'column',
    height: '100%',
    gap: 0,
    padding: '20px 24px',
    boxSizing: 'border-box',
    overflow: 'hidden',
  };

  const headerStyle: React.CSSProperties = {
    display: 'flex',
    alignItems: 'center',
    gap: 16,
    marginBottom: 16,
    flexShrink: 0,
  };

  const navBtnStyle: React.CSSProperties = {
    background: 'var(--surface-card)',
    border: '1px solid var(--surface-border)',
    borderRadius: 6,
    padding: '5px 12px',
    cursor: 'pointer',
    color: 'var(--text-primary)',
    fontSize: 14,
    fontFamily: 'inherit',
  };

  const bodyStyle: React.CSSProperties = {
    display: 'flex',
    gap: 16,
    flex: 1,
    overflow: 'hidden',
    minHeight: 0,
  };

  const calendarContainerStyle: React.CSSProperties = {
    display: 'flex',
    flexDirection: 'column',
    flex: 1,
    overflow: 'hidden',
    minWidth: 0,
  };

  const gridStyle: React.CSSProperties = {
    display: 'grid',
    gridTemplateColumns: 'repeat(7, 1fr)',
    gap: 1,
    background: 'var(--surface-border)',
    border: '1px solid var(--surface-border)',
    borderRadius: 8,
    overflow: 'hidden',
    flex: 1,
    minHeight: 0,
  };

  const dayHeaderStyle: React.CSSProperties = {
    background: 'var(--surface-bg)',
    padding: '6px 0',
    textAlign: 'center',
    fontSize: 11,
    fontWeight: 600,
    color: 'var(--text-muted)',
    letterSpacing: '0.05em',
    textTransform: 'uppercase',
  };

  // ── Sidebar ───────────────────────────────────────────────────────────────

  const sidebarStyle: React.CSSProperties = {
    width: 220,
    flexShrink: 0,
    display: 'flex',
    flexDirection: 'column',
    background: 'var(--surface-card)',
    border: '1px solid var(--surface-border)',
    borderRadius: 8,
    overflow: 'hidden',
  };

  const sidebarHeaderStyle: React.CSSProperties = {
    padding: '10px 14px',
    fontSize: 12,
    fontWeight: 600,
    color: 'var(--text-secondary)',
    borderBottom: '1px solid var(--surface-border)',
    flexShrink: 0,
  };

  const sidebarListStyle: React.CSSProperties = {
    overflowY: 'auto',
    flex: 1,
    padding: '8px',
    display: 'flex',
    flexDirection: 'column',
    gap: 4,
  };

  const sidebarPillStyle: React.CSSProperties = {
    display: 'flex',
    alignItems: 'center',
    gap: 6,
    padding: '6px 8px',
    borderRadius: 6,
    background: 'var(--surface-bg)',
    border: '1px solid var(--surface-border)',
    fontSize: 11,
    color: 'var(--text-primary)',
    cursor: 'pointer',
    fontFamily: 'inherit',
    textAlign: 'left',
    width: '100%',
  };

  if (isLoading) {
    return (
      <div style={{ ...pageStyle, justifyContent: 'center', alignItems: 'center' }}>
        <span style={{ color: 'var(--text-muted)', fontSize: 14 }}>Loading calendar…</span>
      </div>
    );
  }

  return (
    <div style={pageStyle}>
      {/* ── Navigation header ──────────────────────────────────────────── */}
      <div style={headerStyle}>
        <button type="button" style={navBtnStyle} onClick={prevMonth} aria-label="Previous month">
          ‹
        </button>
        <h2 style={{ margin: 0, fontSize: 18, fontWeight: 700, color: 'var(--text-primary)', minWidth: 180, textAlign: 'center' }}>
          {MONTH_NAMES[month]} {year}
        </h2>
        <button type="button" style={navBtnStyle} onClick={nextMonth} aria-label="Next month">
          ›
        </button>
        <button
          type="button"
          style={{ ...navBtnStyle, marginLeft: 8 }}
          onClick={() => { setYear(today.getFullYear()); setMonth(today.getMonth()); }}
          aria-label="Go to current month"
        >
          Today
        </button>
        <span style={{ marginLeft: 'auto', fontSize: 12, color: 'var(--text-muted)' }}>
          {tasks.length} task{tasks.length !== 1 ? 's' : ''}
        </span>
      </div>

      {/* ── Body: grid + sidebar ──────────────────────────────────────── */}
      <div style={bodyStyle}>
        {/* Calendar grid */}
        <div style={calendarContainerStyle}>
          <div style={gridStyle}>
            {/* Day-of-week headers */}
            {DAY_NAMES.map(d => (
              <div key={d} style={dayHeaderStyle}>{d}</div>
            ))}

            {/* Day cells */}
            {grid.map((date, idx) => {
              if (!date) {
                return (
                  <div
                    key={`empty-${idx}`}
                    style={{ background: 'var(--surface-bg)', minHeight: 80 }}
                  />
                );
              }

              const key = dateKey(date);
              const dayTasks = tasksByDate.get(key) ?? [];
              const isToday = key === todayStr;
              const isCurrentMonth = date.getMonth() === month;
              const overflow = dayTasks.length - MAX_PILLS_PER_CELL;

              const cellStyle: React.CSSProperties = {
                background: isToday ? 'color-mix(in srgb, var(--color-primary) 10%, var(--surface-card))' : 'var(--surface-card)',
                padding: '6px 5px 5px',
                display: 'flex',
                flexDirection: 'column',
                gap: 3,
                minHeight: 80,
                overflow: 'hidden',
                opacity: isCurrentMonth ? 1 : 0.4,
              };

              const dayNumStyle: React.CSSProperties = {
                fontSize: 11,
                fontWeight: isToday ? 700 : 500,
                color: isToday ? 'var(--color-primary)' : 'var(--text-secondary)',
                marginBottom: 2,
                lineHeight: 1,
                display: 'flex',
                alignItems: 'center',
                gap: 4,
              };

              return (
                <div key={key} style={cellStyle}>
                  <div style={dayNumStyle}>
                    {isToday && (
                      <span
                        style={{
                          display: 'inline-flex',
                          alignItems: 'center',
                          justifyContent: 'center',
                          width: 18,
                          height: 18,
                          borderRadius: '50%',
                          background: 'var(--color-primary)',
                          color: '#fff',
                          fontSize: 10,
                          fontWeight: 700,
                        }}
                      >
                        {date.getDate()}
                      </span>
                    )}
                    {!isToday && date.getDate()}
                  </div>

                  {dayTasks.slice(0, MAX_PILLS_PER_CELL).map(task => (
                    <TaskPill key={task.id} task={task} onOpen={openPopover} />
                  ))}

                  {overflow > 0 && (
                    <span style={{ fontSize: 10, color: 'var(--text-muted)', paddingLeft: 2 }}>
                      +{overflow} more
                    </span>
                  )}
                </div>
              );
            })}
          </div>
        </div>

        {/* No-due-date sidebar */}
        <div style={sidebarStyle}>
          <div style={sidebarHeaderStyle}>
            No due date
            <span
              style={{
                marginLeft: 6,
                background: 'var(--surface-border)',
                borderRadius: 10,
                padding: '1px 7px',
                fontSize: 11,
                fontWeight: 500,
                color: 'var(--text-muted)',
              }}
            >
              {noDueDateTasks.length}
            </span>
          </div>
          <div style={sidebarListStyle}>
            {noDueDateTasks.length === 0 && (
              <span style={{ fontSize: 12, color: 'var(--text-muted)', padding: '4px 2px' }}>
                All tasks have due dates.
              </span>
            )}
            {noDueDateTasks.map(task => (
              <button
                key={task.id}
                type="button"
                style={sidebarPillStyle}
                onClick={e => openPopover(task, e.currentTarget.getBoundingClientRect())}
                title={task.title}
              >
                <span
                  style={{
                    flexShrink: 0,
                    width: 7,
                    height: 7,
                    borderRadius: '50%',
                    background: PRIORITY_COLORS[task.priority],
                  }}
                />
                <span style={{ overflow: 'hidden', textOverflow: 'ellipsis', whiteSpace: 'nowrap' }}>
                  {task.title}
                </span>
              </button>
            ))}
          </div>
        </div>
      </div>

      {/* ── Detail popover ─────────────────────────────────────────────── */}
      {popover && (
        <TaskDetailPopover
          task={popover.task}
          anchorRect={popover.anchor}
          onClose={closePopover}
        />
      )}
    </div>
  );
}
