import { useState } from 'react';
import { useQuery } from '@tanstack/react-query';
import { useAuthStore } from '@/store/authStore';
import { useTimesheet } from '../hooks/useTimesheet';
import { timeReportService } from '@/services/timeService';
import { projectService } from '@/services/projectService';

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

/** Formats minutes as "2h 30m" / "45m" / "–". */
function fmtMin(minutes: number): string {
  if (minutes === 0) return '–';
  const h = Math.floor(minutes / 60);
  const m = minutes % 60;
  if (h === 0) return `${m}m`;
  if (m === 0) return `${h}h`;
  return `${h}h ${m}m`;
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

// ── Status badge ───────────────────────────────────────────────────────────────

type BudgetStatus = 'over' | 'on-track' | 'under';

function getBudgetStatus(estimated: number, logged: number): BudgetStatus {
  if (estimated === 0) return 'on-track';
  const ratio = logged / estimated;
  if (ratio > 1.05) return 'over';
  if (ratio < 0.95) return 'under';
  return 'on-track';
}

const BADGE_STYLES: Record<BudgetStatus, React.CSSProperties> = {
  over: {
    background: '#fee2e2',
    color: '#b91c1c',
    padding: '2px 8px',
    borderRadius: 12,
    fontSize: 11,
    fontWeight: 600,
    whiteSpace: 'nowrap',
  },
  'on-track': {
    background: '#dcfce7',
    color: '#15803d',
    padding: '2px 8px',
    borderRadius: 12,
    fontSize: 11,
    fontWeight: 600,
    whiteSpace: 'nowrap',
  },
  under: {
    background: '#e0f2fe',
    color: '#0369a1',
    padding: '2px 8px',
    borderRadius: 12,
    fontSize: 11,
    fontWeight: 600,
    whiteSpace: 'nowrap',
  },
};

const BADGE_LABELS: Record<BudgetStatus, string> = {
  over: 'Over Budget',
  'on-track': 'On Track',
  under: 'Under',
};

function StatusBadge({ estimated, logged }: { estimated: number; logged: number }) {
  const status = getBudgetStatus(estimated, logged);
  return <span style={BADGE_STYLES[status]}>{BADGE_LABELS[status]}</span>;
}

// ── Time Report tab ────────────────────────────────────────────────────────────

function TimeReportTab({ userId }: { userId: string }) {
  const [projectId, setProjectId] = useState<string>('');
  const [from, setFrom] = useState<string>('');
  const [to, setTo] = useState<string>('');

  const { data: projects, isLoading: projectsLoading } = useQuery({
    queryKey: ['projects'],
    queryFn: () => projectService.getAll(),
  });

  const { data: report, isLoading, isError, refetch } = useQuery({
    queryKey: ['time-report', projectId, from, to],
    queryFn: () => timeReportService.getTimeReport(projectId, from || undefined, to || undefined),
    enabled: !!projectId,
  });

  const cardStyle: React.CSSProperties = {
    background: '#ffffff',
    border: '1px solid var(--border-color)',
    borderRadius: 'var(--radius-md)',
    boxShadow: 'var(--shadow-sm)',
    padding: '16px 20px',
    marginBottom: 16,
  };

  const summaryGridStyle: React.CSSProperties = {
    display: 'grid',
    gridTemplateColumns: 'repeat(3, 1fr)',
    gap: 16,
    marginBottom: 24,
  };

  const statCardStyle: React.CSSProperties = {
    background: 'var(--surface-bg, #f8fafc)',
    border: '1px solid var(--border-color)',
    borderRadius: 'var(--radius-md)',
    padding: '12px 16px',
  };

  return (
    <div>
      {/* Filters */}
      <div style={cardStyle}>
        <div style={{ display: 'flex', gap: 12, flexWrap: 'wrap', alignItems: 'flex-end' }}>
          <div style={{ display: 'flex', flexDirection: 'column', gap: 4 }}>
            <label style={{ fontSize: 12, fontWeight: 600, color: 'var(--text-muted)' }}>
              PROJECT
            </label>
            {projectsLoading ? (
              <span style={{ fontSize: 13, color: 'var(--text-muted)' }}>Loading…</span>
            ) : (
              <select
                value={projectId}
                onChange={e => setProjectId(e.target.value)}
                style={{
                  padding: '6px 10px',
                  borderRadius: 6,
                  border: '1px solid var(--border-color)',
                  fontSize: 13,
                  background: '#ffffff',
                }}
              >
                <option value="">Select a project…</option>
                {projects?.map(p => (
                  <option key={p.id} value={p.id}>
                    {p.name}
                  </option>
                ))}
              </select>
            )}
          </div>

          <div style={{ display: 'flex', flexDirection: 'column', gap: 4 }}>
            <label style={{ fontSize: 12, fontWeight: 600, color: 'var(--text-muted)' }}>
              FROM
            </label>
            <input
              type="date"
              value={from}
              onChange={e => setFrom(e.target.value)}
              style={{
                padding: '6px 10px',
                borderRadius: 6,
                border: '1px solid var(--border-color)',
                fontSize: 13,
              }}
            />
          </div>

          <div style={{ display: 'flex', flexDirection: 'column', gap: 4 }}>
            <label style={{ fontSize: 12, fontWeight: 600, color: 'var(--text-muted)' }}>
              TO
            </label>
            <input
              type="date"
              value={to}
              onChange={e => setTo(e.target.value)}
              style={{
                padding: '6px 10px',
                borderRadius: 6,
                border: '1px solid var(--border-color)',
                fontSize: 13,
              }}
            />
          </div>

          <button
            type="button"
            className="btn btn-primary"
            onClick={() => refetch()}
            disabled={!projectId}
            style={{ padding: '7px 16px', fontSize: 13 }}
          >
            Run Report
          </button>
        </div>
      </div>

      {!projectId && (
        <div className="empty-state">
          <div className="empty-state-icon">📊</div>
          <p className="empty-state-text">Select a project to view the time report.</p>
        </div>
      )}

      {isLoading && projectId && (
        <div className="empty-state">
          <div className="empty-state-icon">⌛</div>
          <p className="empty-state-text">Loading report…</p>
        </div>
      )}

      {isError && (
        <div className="empty-state">
          <div className="empty-state-icon">⚠️</div>
          <p className="empty-state-text">Could not load report. Please try again.</p>
        </div>
      )}

      {report && !isLoading && (
        <>
          {/* Summary row */}
          <div style={summaryGridStyle}>
            <div style={statCardStyle}>
              <div style={{ fontSize: 11, fontWeight: 600, color: 'var(--text-muted)', marginBottom: 4 }}>
                TOTAL ESTIMATED
              </div>
              <div style={{ fontSize: 22, fontWeight: 700, color: 'var(--text-primary)' }}>
                {fmtMin(report.totalEstimatedMinutes)}
              </div>
            </div>
            <div style={statCardStyle}>
              <div style={{ fontSize: 11, fontWeight: 600, color: 'var(--text-muted)', marginBottom: 4 }}>
                TOTAL LOGGED
              </div>
              <div style={{ fontSize: 22, fontWeight: 700, color: 'var(--color-primary)' }}>
                {fmtMin(report.totalLoggedMinutes)}
              </div>
            </div>
            <div style={statCardStyle}>
              <div style={{ fontSize: 11, fontWeight: 600, color: 'var(--text-muted)', marginBottom: 4 }}>
                VARIANCE
              </div>
              <div
                style={{
                  fontSize: 22,
                  fontWeight: 700,
                  color:
                    report.varianceMinutes > 0
                      ? '#b91c1c'
                      : report.varianceMinutes < 0
                        ? '#0369a1'
                        : 'var(--text-primary)',
                }}
              >
                {report.varianceMinutes > 0 ? '+' : ''}
                {fmtMin(report.varianceMinutes)}
              </div>
            </div>
          </div>

          {/* Per-task table */}
          <div
            style={{
              background: '#ffffff',
              border: '1px solid var(--border-color)',
              borderRadius: 'var(--radius-md)',
              boxShadow: 'var(--shadow-sm)',
              overflowX: 'auto',
            }}
          >
            {report.taskBreakdown.length === 0 ? (
              <div className="empty-state" style={{ padding: 48 }}>
                <div className="empty-state-icon">⏱️</div>
                <p className="empty-state-text">No tasks found for this project.</p>
              </div>
            ) : (
              <table style={tableStyle}>
                <thead>
                  <tr>
                    <th style={{ ...thStyle, width: TASK_COL_WIDTH }}>Task</th>
                    <th style={{ ...thStyle, width: TOTAL_COL_WIDTH, textAlign: 'right' }}>
                      Estimated
                    </th>
                    <th style={{ ...thStyle, width: TOTAL_COL_WIDTH, textAlign: 'right' }}>
                      Logged
                    </th>
                    <th style={{ ...thStyle, width: TOTAL_COL_WIDTH, textAlign: 'right' }}>
                      Variance
                    </th>
                    <th style={{ ...thStyle, textAlign: 'center' }}>Status</th>
                  </tr>
                </thead>
                <tbody>
                  {report.taskBreakdown.map(row => (
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
                      <td style={{ ...tdHoursStyle }}>{fmtMin(row.estimatedMinutes)}</td>
                      <td style={{ ...tdHoursStyle, color: 'var(--text-primary)' }}>
                        {fmtMin(row.loggedMinutes)}
                      </td>
                      <td
                        style={{
                          ...tdHoursStyle,
                          color:
                            row.varianceMinutes > 0
                              ? '#b91c1c'
                              : row.varianceMinutes < 0
                                ? '#0369a1'
                                : 'var(--text-secondary)',
                        }}
                      >
                        {row.varianceMinutes > 0 ? '+' : ''}
                        {fmtMin(row.varianceMinutes)}
                      </td>
                      <td style={{ ...tdStyle, textAlign: 'center' }}>
                        <StatusBadge
                          estimated={row.estimatedMinutes}
                          logged={row.loggedMinutes}
                        />
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            )}
          </div>
        </>
      )}
    </div>
  );
}

// ── Component ──────────────────────────────────────────────────────────────────

type Tab = 'timesheet' | 'report';

export function TimesheetPage() {
  const { token } = useAuthStore();
  const userId = token?.userId ?? '';

  const [activeTab, setActiveTab] = useState<Tab>('timesheet');
  const [weekStart, setWeekStart] = useState<string>(() => getMondayOf(new Date()));

  const { data, isLoading, isError } = useTimesheet(userId, weekStart);

  const prevWeek = () => setWeekStart(prev => addDays(prev, -DAYS_IN_WEEK));
  const nextWeek = () => setWeekStart(prev => addDays(prev, DAYS_IN_WEEK));
  const goToCurrentWeek = () => setWeekStart(getMondayOf(new Date()));

  const dayHeaders = Array.from({ length: DAYS_IN_WEEK }, (_, i) =>
    addDays(weekStart, i),
  );

  const tabStyle = (tab: Tab): React.CSSProperties => ({
    padding: '8px 18px',
    fontSize: 14,
    fontWeight: activeTab === tab ? 600 : 400,
    color: activeTab === tab ? 'var(--color-primary)' : 'var(--text-secondary)',
    borderBottom: activeTab === tab ? '2px solid var(--color-primary)' : '2px solid transparent',
    background: 'none',
    border: 'none',
    borderBottomStyle: 'solid',
    borderBottomWidth: 2,
    borderBottomColor: activeTab === tab ? 'var(--color-primary)' : 'transparent',
    cursor: 'pointer',
  });

  return (
    <div>
      <div className="page-header">
        <div>
          <h1 className="page-title">Time Tracking</h1>
          <p className="page-subtitle">Weekly timesheet and estimate-vs-actual reporting</p>
        </div>

        {activeTab === 'timesheet' && (
          /* Week navigator */
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
        )}
      </div>

      {/* Tabs */}
      <div
        style={{
          display: 'flex',
          borderBottom: '1px solid var(--border-color)',
          marginBottom: 24,
        }}
      >
        <button type="button" style={tabStyle('timesheet')} onClick={() => setActiveTab('timesheet')}>
          Timesheet
        </button>
        <button type="button" style={tabStyle('report')} onClick={() => setActiveTab('report')}>
          Time Report
        </button>
      </div>

      {/* Timesheet tab */}
      {activeTab === 'timesheet' && (
        <>
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
        </>
      )}

      {/* Time Report tab */}
      {activeTab === 'report' && <TimeReportTab userId={userId} />}
    </div>
  );
}
