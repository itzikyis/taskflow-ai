import { useState, useCallback, useRef } from 'react';
import { useAllTasks } from '@/features/tasks/hooks/useTasks';
import { useAllDependencies } from '@/features/tasks/hooks/useDependencies';
import type { Task, TaskPriority, TaskStatus } from '@/features/tasks/types/task.types';

// ── Constants ────────────────────────────────────────────────────────────────

const DAY_MS = 24 * 60 * 60 * 1000;

const PRIORITY_COLOR: Record<TaskPriority, string> = {
  Low: 'var(--priority-low, #22c55e)',
  Medium: 'var(--priority-medium, #f59e0b)',
  High: 'var(--priority-high, #ef4444)',
  Critical: 'var(--priority-critical, #8b5cf6)',
};

const STATUS_LABEL: Record<TaskStatus, string> = {
  Todo: 'To Do',
  InProgress: 'In Progress',
  InReview: 'In Review',
  Done: 'Done',
};

const LABEL_W = 220;
const ROW_H = 42;
const BAR_H = 22;
const HEADER_H = 52;
const PADDING_DAYS = 4;

type Zoom = 'week' | 'month';

const DAY_WIDTH_PX: Record<Zoom, number> = {
  week: 34,
  month: 10,
};

// ── Helpers ──────────────────────────────────────────────────────────────────

function startOfDay(ms: number): number {
  const d = new Date(ms);
  d.setHours(0, 0, 0, 0);
  return d.getTime();
}

function addDays(ms: number, days: number): number {
  return ms + days * DAY_MS;
}

function daysBetween(a: number, b: number): number {
  return Math.round((b - a) / DAY_MS);
}

function fmtDate(ms: number): string {
  return new Date(ms).toLocaleDateString(undefined, {
    month: 'short',
    day: 'numeric',
    year: 'numeric',
  });
}

// ── Types ────────────────────────────────────────────────────────────────────

interface GanttBar {
  task: Task;
  startDay: number;
  endDay: number;
  isEstimated: boolean;
}

interface PopoverState {
  task: Task;
  clientX: number;
  clientY: number;
}

// ── Component ────────────────────────────────────────────────────────────────

export function TimelinePage() {
  const [zoom, setZoom] = useState<Zoom>('week');
  const [popover, setPopover] = useState<PopoverState | null>(null);
  const containerRef = useRef<HTMLDivElement>(null);

  const { data: tasks = [], isLoading } = useAllTasks();
  const { data: deps = [] } = useAllDependencies();

  const handleBarClick = useCallback((task: Task, e: React.MouseEvent) => {
    e.stopPropagation();
    setPopover({ task, clientX: e.clientX, clientY: e.clientY });
  }, []);

  const closePopover = useCallback(() => setPopover(null), []);

  if (isLoading) {
    return (
      <div className="empty-state">
        <div className="empty-state-icon">⌛</div>
        <p className="empty-state-text">Loading timeline…</p>
      </div>
    );
  }

  if (tasks.length === 0) {
    return (
      <div className="empty-state">
        <div className="empty-state-icon">📅</div>
        <p className="empty-state-text">No tasks to schedule yet.</p>
      </div>
    );
  }

  const dayW = DAY_WIDTH_PX[zoom];

  // Build bars
  const bars: GanttBar[] = tasks.map(t => {
    const startDay = startOfDay(new Date(t.createdAt).getTime());
    let endDay: number;
    let isEstimated = false;
    if (t.dueDate) {
      endDay = startOfDay(new Date(t.dueDate).getTime());
      if (endDay <= startDay) endDay = addDays(startDay, 1);
    } else {
      endDay = addDays(startDay, 1);
      isEstimated = true;
    }
    return { task: t, startDay, endDay, isEstimated };
  });

  const allDayTimestamps = bars.flatMap(b => [b.startDay, b.endDay]);
  const rangeMin = startOfDay(Math.min(...allDayTimestamps));
  const rangeMax = startOfDay(Math.max(...allDayTimestamps));

  const originDay = addDays(rangeMin, -PADDING_DAYS);
  const lastDay = addDays(rangeMax, PADDING_DAYS);
  const totalDays = daysBetween(originDay, lastDay);
  const totalWidth = totalDays * dayW;

  const todayDay = startOfDay(Date.now());
  const todayX = daysBetween(originDay, todayDay) * dayW;
  const showToday = todayX >= 0 && todayX <= totalWidth;

  // Sort bars by start date
  const sortedBars = [...bars].sort((a, b) => a.startDay - b.startDay);
  const rowByTaskId = new Map(sortedBars.map((b, i) => [b.task.id, i]));

  // Build header tick marks
  const headerTicks: { x: number; label: string }[] = [];
  if (zoom === 'week') {
    let d = originDay;
    while (d <= lastDay) {
      const x = daysBetween(originDay, d) * dayW;
      headerTicks.push({
        x,
        label: new Date(d).toLocaleDateString(undefined, { month: 'short', day: 'numeric' }),
      });
      d = addDays(d, 7);
    }
  } else {
    const cursor = new Date(originDay);
    cursor.setDate(1);
    while (cursor.getTime() <= lastDay) {
      const x = daysBetween(originDay, cursor.getTime()) * dayW;
      headerTicks.push({
        x: Math.max(0, x),
        label: cursor.toLocaleDateString(undefined, { month: 'short', year: 'numeric' }),
      });
      cursor.setMonth(cursor.getMonth() + 1);
    }
  }

  // Build SVG dependency arrows
  const svgHeight = sortedBars.length * ROW_H;

  interface Arrow {
    id: string;
    x1: number;
    y1: number;
    x2: number;
    y2: number;
  }

  const arrows: Arrow[] = [];
  for (const dep of deps) {
    const srcIdx = rowByTaskId.get(dep.blockedByTaskId);
    const tgtIdx = rowByTaskId.get(dep.taskId);
    if (srcIdx === undefined || tgtIdx === undefined) continue;

    const srcBar = sortedBars[srcIdx];
    const tgtBar = sortedBars[tgtIdx];

    // Arrow: from right edge of blocker bar to left edge of blocked task bar
    const x1 = daysBetween(originDay, srcBar.endDay) * dayW;
    const y1 = srcIdx * ROW_H + ROW_H / 2;
    const x2 = daysBetween(originDay, tgtBar.startDay) * dayW;
    const y2 = tgtIdx * ROW_H + ROW_H / 2;

    arrows.push({ id: dep.id, x1, y1, x2, y2 });
  }

  return (
    // eslint-disable-next-line jsx-a11y/click-events-have-key-events, jsx-a11y/no-static-element-interactions
    <div onClick={closePopover} ref={containerRef}>
      {/* Page header */}
      <div className="page-header">
        <div>
          <h1 className="page-title">Gantt Timeline</h1>
          <p className="page-subtitle">
            Bars span from task creation to due date, colored by priority. Arrows show dependencies.
          </p>
        </div>
        <div style={{ display: 'flex', gap: 8, alignItems: 'center' }}>
          <ZoomToggle zoom={zoom} onChange={setZoom} />
        </div>
      </div>

      {/* Legend */}
      <div
        style={{
          display: 'flex',
          flexWrap: 'wrap',
          gap: '6px 18px',
          marginBottom: 12,
          padding: '8px 14px',
          background: 'var(--surface-card)',
          border: '1px solid var(--surface-border)',
          borderRadius: 'var(--radius-md)',
          fontSize: 12,
          color: 'var(--text-secondary)',
        }}
      >
        <span style={{ fontWeight: 600, color: 'var(--text-primary)', marginRight: 4 }}>Priority:</span>
        {(['Low', 'Medium', 'High', 'Critical'] as TaskPriority[]).map(p => (
          <LegendDot key={p} color={PRIORITY_COLOR[p]} label={p} />
        ))}
        {showToday && (
          <span style={{ display: 'inline-flex', alignItems: 'center', gap: 5, marginLeft: 8 }}>
            <span
              style={{
                width: 2,
                height: 12,
                background: '#ef4444',
                display: 'inline-block',
                borderRadius: 1,
              }}
            />
            Today
          </span>
        )}
        <span style={{ display: 'inline-flex', alignItems: 'center', gap: 5 }}>
          <span
            style={{
              width: 14,
              height: 10,
              borderRadius: 3,
              background: PRIORITY_COLOR.Low,
              opacity: 0.35,
              outline: '1.5px dashed rgba(0,0,0,0.3)',
              display: 'inline-block',
            }}
          />
          No due date
        </span>
        {deps.length > 0 && (
          <span style={{ display: 'inline-flex', alignItems: 'center', gap: 5 }}>
            <svg width="24" height="12" aria-hidden="true">
              <line x1="0" y1="6" x2="18" y2="6" stroke="var(--text-secondary)" strokeWidth="1.5" strokeDasharray="3 2" />
              <polygon points="18,3 24,6 18,9" fill="var(--text-secondary)" opacity="0.6" />
            </svg>
            Dependency
          </span>
        )}
      </div>

      {/* Chart */}
      <div
        style={{
          border: '1px solid var(--surface-border)',
          borderRadius: 'var(--radius-md)',
          overflow: 'auto',
          background: 'var(--surface-card)',
          position: 'relative',
        }}
      >
        <div style={{ minWidth: LABEL_W + totalWidth }}>
          {/* Header row */}
          <div
            style={{
              display: 'flex',
              borderBottom: '2px solid var(--surface-border)',
              position: 'sticky',
              top: 0,
              zIndex: 20,
              background: 'var(--surface-bg, var(--surface-card))',
            }}
          >
            {/* Label header cell */}
            <div
              style={{
                width: LABEL_W,
                flexShrink: 0,
                height: HEADER_H,
                display: 'flex',
                alignItems: 'center',
                padding: '0 12px',
                fontSize: 11,
                fontWeight: 600,
                color: 'var(--text-secondary)',
                textTransform: 'uppercase',
                letterSpacing: '0.05em',
                borderRight: '1px solid var(--surface-border)',
                position: 'sticky',
                left: 0,
                zIndex: 21,
                background: 'var(--surface-bg, var(--surface-card))',
              }}
            >
              Task
            </div>

            {/* Date axis */}
            <div style={{ position: 'relative', width: totalWidth, height: HEADER_H, flexShrink: 0 }}>
              {headerTicks.map((tick, i) => (
                <span
                  key={i}
                  style={{
                    position: 'absolute',
                    left: tick.x,
                    top: 10,
                    fontSize: 11,
                    color: 'var(--text-secondary)',
                    whiteSpace: 'nowrap',
                    paddingLeft: 4,
                  }}
                >
                  {tick.label}
                </span>
              ))}
              {headerTicks.map((tick, i) => (
                <span
                  key={`vl-${i}`}
                  style={{
                    position: 'absolute',
                    left: tick.x,
                    top: 34,
                    width: 1,
                    height: 18,
                    background: 'var(--surface-border)',
                  }}
                />
              ))}
              {/* Today marker in header */}
              {showToday && (
                <span
                  style={{
                    position: 'absolute',
                    left: todayX,
                    top: 30,
                    width: 2,
                    height: 22,
                    background: '#ef4444',
                    borderRadius: 1,
                  }}
                />
              )}
            </div>
          </div>

          {/* Body */}
          <div style={{ position: 'relative' }}>
            {/* Row label column (sticky left) */}
            <div
              style={{
                position: 'sticky',
                left: 0,
                zIndex: 10,
                width: LABEL_W,
                background: 'var(--surface-card)',
                borderRight: '1px solid var(--surface-border)',
              }}
            >
              {sortedBars.map(({ task }) => (
                <div
                  key={task.id}
                  title={task.title}
                  style={{
                    height: ROW_H,
                    display: 'flex',
                    alignItems: 'center',
                    padding: '0 12px',
                    fontSize: 13,
                    overflow: 'hidden',
                    textOverflow: 'ellipsis',
                    whiteSpace: 'nowrap',
                    color: 'var(--text-primary)',
                    borderBottom: '1px solid var(--surface-border)',
                  }}
                >
                  {task.title}
                </div>
              ))}
            </div>

            {/* Bars & arrows overlay */}
            <div
              style={{
                position: 'absolute',
                top: 0,
                left: LABEL_W,
                width: totalWidth,
                height: svgHeight,
                flexShrink: 0,
              }}
            >
              {/* Grid lines */}
              {headerTicks.map((tick, i) => (
                <span
                  key={i}
                  style={{
                    position: 'absolute',
                    left: tick.x,
                    top: 0,
                    bottom: 0,
                    width: 1,
                    background: 'var(--surface-border)',
                    opacity: 0.45,
                  }}
                />
              ))}

              {/* Today line */}
              {showToday && (
                <div
                  style={{
                    position: 'absolute',
                    left: todayX,
                    top: 0,
                    bottom: 0,
                    width: 2,
                    background: '#ef4444',
                    opacity: 0.75,
                    zIndex: 3,
                    borderRadius: 1,
                  }}
                  title={`Today: ${fmtDate(todayDay)}`}
                />
              )}

              {/* Row backgrounds */}
              {sortedBars.map((_, rowIdx) => (
                <div
                  key={rowIdx}
                  style={{
                    position: 'absolute',
                    top: rowIdx * ROW_H,
                    left: 0,
                    right: 0,
                    height: ROW_H,
                    borderBottom: '1px solid var(--surface-border)',
                    background: rowIdx % 2 === 1 ? 'rgba(0,0,0,0.015)' : 'transparent',
                  }}
                />
              ))}

              {/* Task bars */}
              {sortedBars.map(({ task, startDay, endDay: barEnd, isEstimated }, rowIdx) => {
                const barLeft = daysBetween(originDay, startDay) * dayW;
                const barWidth = Math.max(daysBetween(startDay, barEnd) * dayW, dayW);
                const color = PRIORITY_COLOR[task.priority];
                const opacity = isEstimated ? 0.4 : task.status === 'Done' ? 0.55 : 1;

                return (
                  <button
                    key={task.id}
                    type="button"
                    onClick={e => handleBarClick(task, e)}
                    title={`${task.title} · ${task.priority} · ${STATUS_LABEL[task.status]}\nCreated: ${fmtDate(startDay)}${task.dueDate ? `\nDue: ${fmtDate(barEnd)}` : '\n(No due date)'}`}
                    style={{
                      position: 'absolute',
                      top: rowIdx * ROW_H + (ROW_H - BAR_H) / 2,
                      left: barLeft,
                      width: barWidth,
                      height: BAR_H,
                      background: color,
                      opacity,
                      borderRadius: 4,
                      border: 'none',
                      cursor: 'pointer',
                      zIndex: 2,
                      outline: isEstimated ? '1.5px dashed rgba(0,0,0,0.3)' : 'none',
                      display: 'flex',
                      alignItems: 'center',
                      paddingLeft: 6,
                      overflow: 'hidden',
                      fontFamily: 'inherit',
                    }}
                  >
                    {barWidth > 48 && (
                      <span
                        style={{
                          fontSize: 11,
                          color: '#fff',
                          whiteSpace: 'nowrap',
                          overflow: 'hidden',
                          textOverflow: 'ellipsis',
                          fontWeight: 500,
                          textShadow: '0 1px 2px rgba(0,0,0,0.4)',
                          pointerEvents: 'none',
                        }}
                      >
                        {task.title}
                      </span>
                    )}
                  </button>
                );
              })}

              {/* Dependency arrows SVG */}
              {arrows.length > 0 && (
                <svg
                  style={{
                    position: 'absolute',
                    top: 0,
                    left: 0,
                    width: totalWidth,
                    height: svgHeight,
                    pointerEvents: 'none',
                    zIndex: 4,
                    overflow: 'visible',
                  }}
                  aria-hidden="true"
                >
                  <defs>
                    <marker
                      id="gantt-arrow"
                      markerWidth="8"
                      markerHeight="8"
                      refX="6"
                      refY="3"
                      orient="auto"
                    >
                      <path d="M0,0 L0,6 L8,3 z" fill="var(--text-secondary)" opacity="0.55" />
                    </marker>
                  </defs>
                  {arrows.map(({ id, x1, y1, x2, y2 }) => {
                    const midX = (x1 + x2) / 2;
                    const d = `M ${x1} ${y1} C ${midX} ${y1}, ${midX} ${y2}, ${x2} ${y2}`;
                    return (
                      <path
                        key={id}
                        d={d}
                        fill="none"
                        stroke="var(--text-secondary)"
                        strokeWidth={1.5}
                        strokeOpacity={0.5}
                        strokeDasharray="4 2"
                        markerEnd="url(#gantt-arrow)"
                      />
                    );
                  })}
                </svg>
              )}
            </div>

            {/* Spacer div so the outer container stretches to the right height */}
            <div style={{ marginLeft: LABEL_W, height: svgHeight, width: totalWidth }} />
          </div>
        </div>
      </div>

      <p style={{ marginTop: 8, fontSize: 11, color: 'var(--text-secondary)' }}>
        Faded/dashed bars = no due date set (1-day marker). Done tasks at 55% opacity. Click any bar for details.
      </p>

      {/* Popover — fixed so it escapes overflow:hidden */}
      {popover && (
        <TaskPopover
          task={popover.task}
          clientX={popover.clientX}
          clientY={popover.clientY}
          onClose={closePopover}
        />
      )}
    </div>
  );
}

// ── Sub-components ───────────────────────────────────────────────────────────

function ZoomToggle({ zoom, onChange }: { zoom: Zoom; onChange: (z: Zoom) => void }) {
  return (
    <div
      style={{
        display: 'flex',
        border: '1px solid var(--surface-border)',
        borderRadius: 'var(--radius-md)',
        overflow: 'hidden',
      }}
    >
      {(['week', 'month'] as Zoom[]).map(z => (
        <button
          key={z}
          type="button"
          onClick={() => onChange(z)}
          style={{
            padding: '6px 16px',
            fontSize: 13,
            fontFamily: 'inherit',
            cursor: 'pointer',
            border: 'none',
            background: zoom === z ? 'var(--color-primary)' : 'var(--surface-card)',
            color: zoom === z ? '#fff' : 'var(--text-secondary)',
            fontWeight: zoom === z ? 600 : 400,
            transition: 'background 0.15s, color 0.15s',
          }}
        >
          {z === 'week' ? 'Week' : 'Month'}
        </button>
      ))}
    </div>
  );
}

function LegendDot({ color, label }: { color: string; label: string }) {
  return (
    <span style={{ display: 'inline-flex', alignItems: 'center', gap: 5 }}>
      <span
        style={{
          width: 14,
          height: 10,
          borderRadius: 3,
          background: color,
          display: 'inline-block',
          flexShrink: 0,
        }}
      />
      {label}
    </span>
  );
}

function PopoverRow({ label, value, color }: { label: string; value: string; color?: string }) {
  return (
    <div style={{ display: 'flex', gap: 6, alignItems: 'center', minHeight: 20 }}>
      <span
        style={{
          minWidth: 58,
          fontSize: 10,
          textTransform: 'uppercase',
          letterSpacing: '0.05em',
          color: 'var(--text-secondary)',
          opacity: 0.8,
        }}
      >
        {label}
      </span>
      {color !== undefined && (
        <span
          style={{
            width: 10,
            height: 10,
            borderRadius: 2,
            background: color,
            display: 'inline-block',
            flexShrink: 0,
          }}
        />
      )}
      <span style={{ color: 'var(--text-primary)', fontWeight: 500, fontSize: 13 }}>{value}</span>
    </div>
  );
}

function TaskPopover({
  task,
  clientX,
  clientY,
  onClose,
}: {
  task: Task;
  clientX: number;
  clientY: number;
  onClose: () => void;
}) {
  const POPOVER_W = 250;
  const left = Math.min(clientX - POPOVER_W / 2, window.innerWidth - POPOVER_W - 12);
  const top = clientY - 10;

  return (
    // eslint-disable-next-line jsx-a11y/click-events-have-key-events, jsx-a11y/no-static-element-interactions
    <div
      onClick={e => e.stopPropagation()}
      style={{
        position: 'fixed',
        left,
        top,
        width: POPOVER_W,
        background: 'var(--surface-card)',
        border: '1px solid var(--surface-border)',
        borderRadius: 'var(--radius-md)',
        padding: '12px 14px 14px',
        boxShadow: '0 8px 24px rgba(0,0,0,0.18)',
        zIndex: 9999,
        transform: 'translateY(-100%)',
      }}
    >
      <button
        type="button"
        onClick={onClose}
        aria-label="Close task details"
        style={{
          position: 'absolute',
          top: 6,
          right: 8,
          background: 'none',
          border: 'none',
          cursor: 'pointer',
          color: 'var(--text-secondary)',
          fontSize: 18,
          lineHeight: 1,
          padding: '0 2px',
        }}
      >
        ×
      </button>

      <div
        style={{
          fontWeight: 600,
          color: 'var(--text-primary)',
          marginBottom: 10,
          lineHeight: 1.35,
          paddingRight: 20,
          fontSize: 14,
        }}
      >
        {task.title}
      </div>

      <div style={{ display: 'flex', flexDirection: 'column', gap: 5 }}>
        <PopoverRow label="Status" value={STATUS_LABEL[task.status]} />
        <PopoverRow label="Priority" value={task.priority} color={PRIORITY_COLOR[task.priority]} />
        <PopoverRow
          label="Created"
          value={fmtDate(startOfDay(new Date(task.createdAt).getTime()))}
        />
        <PopoverRow
          label="Due"
          value={task.dueDate ? fmtDate(startOfDay(new Date(task.dueDate).getTime())) : 'Not set'}
        />
        {task.assignedToUserId !== null && (
          <PopoverRow label="Assignee" value={task.assignedToUserId.slice(0, 8) + '…'} />
        )}
      </div>
    </div>
  );
}
