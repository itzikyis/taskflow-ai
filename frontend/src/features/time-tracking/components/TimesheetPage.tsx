import { useState } from 'react';
import { useAuthStore } from '@/store/authStore';
import { useTimesheet } from '../hooks/useTimesheet';

// ── Constants ──────────────────────────────────────────────────────────────────

const DAYS_IN_WEEK = 7;
const DAY_LABELS = ['Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat', 'Sun'] as const;

const CELL_WIDTH = 90;
const TASK_COL_WIDTH = 240;
const TOTAL_COL_WIDTH = 90;

// ── Helpers ────────────────────────────────────────────────────────────────────

/** Returns the ISO string (YYYY-MM-DD) of the Monday of the week containing `date`. */
function getMondayOf(date: Date): string {
  const d = new Date(date);
  const day = d.getDay(); // 0 = Sun
  const diff = day === 0 ? -6 : 1 - day; // shift so Monday = 0
  d.setDate(d.getDate() + diff);
  return d.toISOString().slice(0, 10);
}

/** Returns a Date from a YYYY-MM-DD string, interpreted as local midnight. */
function parseLocalDate(iso: string): Date {
  const [year, month, day] = iso.split('-').map(Number);
  return new Date(year, month - 1, day);
}

/** Formats a YYYY-MM-DD string as "Mon DD MMM". */
function formatDayHeader(iso: string): string {
  const d = parseLocalDate(iso);
  return d.toLocaleDateString('en-GB', { weekday: 'short', day: 'numeric', month: 'short' });
}

/** Formats a decimal hours value as "2.5h" or "" for zero. */
function formatHours(h: number): string {
  if (h === 0) return '';
  return `${h % 1 === 0 ? h : h.toFixed(2)}h`;
}

/** Adds `days` days to a YYYY-MM-DD string and returns a new YYYY-MM-DD. */
function addDays(iso: string, days: number): string {
  const d = parseLocalDate(iso);
  d.setDate(d.getDate() + days);
  return d.toISOString().slice(0, 10);
}

/** Formats the week label, e.g. "14 Jul – 20 Jul 2026". */
function formatWeekRange(weekStart: string): string {
  const start = parseLocalDate(weekStart);
  const end = parseLocalDate(addDays(weekStart, 6));
  const fmt = (d: Date) =>
    d.toLocaleDateString('en-GB', { day: 'numeric', month: 'short' });
  return `${fmt(start)} – ${fmt(end)} ${end.getFullYear()}`;
}

// ── Styles ─────────────────────────────────────────────────────────────────────

const tableStyle: React.CSSProperties = {
  width: '100%',
  borderCollapse: 'collapse',
  fontSize: 13,
};

const thStyle: React.CSSProperties = {
  padding: '8px 12px',
  textAlign: 'left',
  fontWeight: 600,
  fontSize: 12,
  color: 'var(--text-muted)',
  textTransform: 'uppercase',
  letterSpacing: '0.04em',
  borderBottom: '2px solid var(--border-color)',
  background: 'var(--surface-bg, #f8fafc)',
};

const tdStyle: React.CSSProperties = {
  padding: '8px 12px',
  borderBottom: '1px solid var(--border-color)',
  color: 'var(--text-secondary)',
};

const tdHoursStyle: React.CSSProperties = {
  ...tdStyle,
  textAlign: 'right',
  fontVariantNumeric: 'tabular-nums',
  width: CELL_WIDTH,
};

const footerTdStyle: React.CSSProperties = {
  ...tdStyle,
  fontWeight: 700,
  color: 'var(--text-primary)',
  background: 'var(--surface-bg, #f8fafc)',
  borderTop: '2px solid var(--border-color)',
  borderBottom: 'none',
};

const footerHoursStyle: React.CSSProperties = {
  ...footerTdStyle,
  textAlign: 'right',
  width: CELL_WIDTH,
};

// ── Component ──────────────────────────────────────────────────────────────────

export function TimesheetPage() {
  const { token } = useAuthStore();
  const userId = token?.userId ?? '';

  const [weekStart, setWeekStart] = useState<string>(() => getMondayOf(new Date()));

  const { data, isLoading, isError } = useTimesheet(userId, weekStart);

  const prevWeek = () => setWeekStart(prev => addDays(prev, -DAYS_IN_WEEK));
  const nextWeek = () => setWeekStart(prev => addDays(prev, DAYS_IN_WEEK));
  const goToCurrentWeek = () => setWeekStart(getMondayOf(new Date()));

  const dayHeaders = Array.from({ length: DAYS_IN_WEEK }, (_, i) =>
    addDays(weekStart, i),
  );

  return (
    <div>
      <div className="page-header">
        <div>
          <h1 className="page-title">Timesheets</h1>
          <p className="page-subtitle">Weekly summary of logged hours per task</p>
        </div>

        {/* Week navigator */}
        <div style={{ display: 'flex', alignItems: 'center', gap: 8 }}>
          <button
            type="button"
            className="btn btn-ghost"
            onClick={prevWeek}
            aria-label="Previous week"
            style={{ padding: '6px 10px', fontSize: 16 }}
          >
            ←
          </button>
          <button
            type="button"
            className="btn btn-ghost"
            onClick={goToCurrentWeek}
            style={{ padding: '6px 12px', fontSize: 13 }}
          >
            {formatWeekRange(weekStart)}
          </button>
          <button
            type="button"
            className="btn btn-ghost"
            onClick={nextWeek}
            aria-label="Next week"
            style={{ padding: '6px 10px', fontSize: 16 }}
          >
            →
          </button>
        </div>
      </div>

      {isLoading && (
        <div className="empty-state">
          <div className="empty-state-icon">⌛</div>
          <p className="empty-state-text">Loading timesheet…</p>
        </div>
      )}

      {isError && (
        <div className="empty-state">
          <div className="empty-state-icon">⚠️</div>
          <p className="empty-state-text">Could not load timesheet. Please try again.</p>
        </div>
      )}

      {!isLoading && !isError && data && (
        <div
          style={{
            background: '#ffffff',
            border: '1px solid var(--border-color)',
            borderRadius: 'var(--radius-md)',
            boxShadow: 'var(--shadow-sm)',
            overflowX: 'auto',
          }}
        >
          {data.rows.length === 0 ? (
            <div className="empty-state" style={{ padding: 48 }}>
              <div className="empty-state-icon">⏱️</div>
              <p className="empty-state-text">No time logged this week.</p>
            </div>
          ) : (
            <table style={tableStyle}>
              <thead>
                <tr>
                  <th style={{ ...thStyle, width: TASK_COL_WIDTH }}>Task</th>
                  {dayHeaders.map((day, i) => (
                    <th key={day} style={{ ...thStyle, width: CELL_WIDTH, textAlign: 'right' }}>
                      <div>{DAY_LABELS[i]}</div>
                      <div style={{ fontWeight: 400, fontSize: 11 }}>
                        {formatDayHeader(day)}
                      </div>
                    </th>
                  ))}
                  <th style={{ ...thStyle, width: TOTAL_COL_WIDTH, textAlign: 'right' }}>
                    Total
                  </th>
                </tr>
              </thead>

              <tbody>
                {data.rows.map(row => {
                  const rowTotal = row.hoursByDay.reduce((a, b) => a + b, 0);
                  return (
                    <tr key={row.taskId}>
                      <td
                        style={{
                          ...tdStyle,
                          color: 'var(--text-primary)',
                          fontWeight: 500,
                          maxWidth: TASK_COL_WIDTH,
                          overflow: 'hidden',
                          textOverflow: 'ellipsis',
                          whiteSpace: 'nowrap',
                        }}
                        title={row.taskTitle}
                      >
                        {row.taskTitle}
                      </td>
                      {row.hoursByDay.map((h, i) => (
                        <td
                          key={dayHeaders[i]}
                          style={{
                            ...tdHoursStyle,
                            color: h > 0 ? 'var(--text-primary)' : 'var(--text-muted)',
                          }}
                        >
                          {formatHours(h)}
                        </td>
                      ))}
                      <td
                        style={{
                          ...tdHoursStyle,
                          fontWeight: 600,
                          color: 'var(--color-primary)',
                        }}
                      >
                        {formatHours(rowTotal)}
                      </td>
                    </tr>
                  );
                })}
              </tbody>

              <tfoot>
                <tr>
                  <td style={{ ...footerTdStyle }}>Total</td>
                  {data.totalByDay.map((h, i) => (
                    <td key={dayHeaders[i]} style={footerHoursStyle}>
                      {formatHours(h)}
                    </td>
                  ))}
                  <td style={{ ...footerHoursStyle, color: 'var(--color-primary)' }}>
                    {formatHours(data.grandTotal)}
                  </td>
                </tr>
              </tfoot>
            </table>
          )}
        </div>
      )}
    </div>
  );
}
