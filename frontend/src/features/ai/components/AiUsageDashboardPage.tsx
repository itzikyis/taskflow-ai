import { useState, useEffect, useRef } from 'react';

// ── Types ────────────────────────────────────────────────────────────────────

interface GovernanceSettings {
  monthlyBudget: number;
  alertThreshold: number;
  aiEnabled: boolean;
}

// ── Constants ────────────────────────────────────────────────────────────────

const GOVERNANCE_STORAGE_KEY = 'taskflow-ai-governance';

const DEFAULT_GOVERNANCE: GovernanceSettings = {
  monthlyBudget: 50,
  alertThreshold: 75,
  aiEnabled: true,
};

const SUMMARY_STATS = [
  { label: 'Total AI Requests This Month', value: '1,247', unit: '' },
  { label: 'Estimated Cost', value: '$18.43', unit: '' },
  { label: 'Avg Response Time', value: '1.2s', unit: '' },
  { label: 'Top Feature', value: 'Sprint Planner', unit: '34%' },
] as const;

const FEATURE_USAGE = [
  { feature: 'Sprint Planner', requests: 424 },
  { feature: 'Risk Detection', requests: 287 },
  { feature: 'Meeting Notes', requests: 198 },
  { feature: 'AI Copilot', requests: 167 },
  { feature: 'Ask TaskFlow', requests: 112 },
  { feature: 'Release Notes', requests: 59 },
] as const;

const MODEL_BREAKDOWN = [
  { label: 'Claude Sonnet', pct: 65, color: 'var(--color-primary)' },
  { label: 'Claude Haiku', pct: 25, color: '#a78bfa' },
  { label: 'GPT-4o (future)', pct: 10, color: '#34d399' },
] as const;

const CHART_WIDTH = 520;
const CHART_HEIGHT = 120;
const CHART_PAD_X = 24;
const CHART_PAD_Y = 12;

// ── Helpers ──────────────────────────────────────────────────────────────────

function generateDailyData(): number[] {
  // Sine wave base + noise, 14 values, range roughly 40–120
  return Array.from({ length: 14 }, (_, i) => {
    const base = 80 + 40 * Math.sin((i / 13) * Math.PI);
    const noise = ((i * 7919 + 3571) % 31) - 15; // deterministic pseudo-noise
    return Math.round(base + noise);
  });
}

function buildPolylinePoints(data: number[]): string {
  const max = Math.max(...data);
  const min = Math.min(...data);
  const range = max - min || 1;
  const innerW = CHART_WIDTH - CHART_PAD_X * 2;
  const innerH = CHART_HEIGHT - CHART_PAD_Y * 2;

  return data
    .map((v, i) => {
      const x = CHART_PAD_X + (i / (data.length - 1)) * innerW;
      const y = CHART_PAD_Y + innerH - ((v - min) / range) * innerH;
      return `${x.toFixed(1)},${y.toFixed(1)}`;
    })
    .join(' ');
}

function buildAreaPath(data: number[]): string {
  const max = Math.max(...data);
  const min = Math.min(...data);
  const range = max - min || 1;
  const innerW = CHART_WIDTH - CHART_PAD_X * 2;
  const innerH = CHART_HEIGHT - CHART_PAD_Y * 2;
  const bottom = CHART_PAD_Y + innerH;

  const points = data.map((v, i) => {
    const x = CHART_PAD_X + (i / (data.length - 1)) * innerW;
    const y = CHART_PAD_Y + innerH - ((v - min) / range) * innerH;
    return `${x.toFixed(1)},${y.toFixed(1)}`;
  });

  const firstX = CHART_PAD_X.toFixed(1);
  const lastX = (CHART_PAD_X + innerW).toFixed(1);

  return `M ${firstX},${bottom} L ${points.join(' L ')} L ${lastX},${bottom} Z`;
}

// Donut arc helpers
function polarToXY(cx: number, cy: number, r: number, angleDeg: number): [number, number] {
  const rad = ((angleDeg - 90) * Math.PI) / 180;
  return [cx + r * Math.cos(rad), cy + r * Math.sin(rad)];
}

function describeArc(cx: number, cy: number, r: number, startDeg: number, endDeg: number): string {
  const [sx, sy] = polarToXY(cx, cy, r, startDeg);
  const [ex, ey] = polarToXY(cx, cy, r, endDeg);
  const largeArc = endDeg - startDeg > 180 ? 1 : 0;
  return `M ${sx.toFixed(2)} ${sy.toFixed(2)} A ${r} ${r} 0 ${largeArc} 1 ${ex.toFixed(2)} ${ey.toFixed(2)}`;
}

// ── Sub-components ───────────────────────────────────────────────────────────

function SummaryCard({ label, value, unit }: { label: string; value: string; unit: string }) {
  const cardStyle: React.CSSProperties = {
    flex: '1 1 0',
    background: 'var(--surface-card)',
    border: '1px solid var(--surface-border)',
    borderRadius: 'var(--radius-md)',
    padding: '20px 24px',
    display: 'flex',
    flexDirection: 'column',
    gap: 6,
  };
  return (
    <div style={cardStyle}>
      <span style={{ fontSize: 12, color: 'var(--text-muted)', fontWeight: 500, letterSpacing: '0.04em', textTransform: 'uppercase' }}>
        {label}
      </span>
      <span style={{ fontSize: 26, fontWeight: 700, color: 'var(--text-primary)', lineHeight: 1.2 }}>
        {value}
        {unit && (
          <span style={{ fontSize: 14, fontWeight: 400, color: 'var(--text-secondary)', marginLeft: 6 }}>
            {unit}
          </span>
        )}
      </span>
    </div>
  );
}

function FeatureBarChart() {
  const max = FEATURE_USAGE[0].requests;
  return (
    <div style={{ display: 'flex', flexDirection: 'column', gap: 10 }}>
      {FEATURE_USAGE.map(({ feature, requests }) => {
        const pct = (requests / max) * 100;
        return (
          <div key={feature} style={{ display: 'grid', gridTemplateColumns: '140px 1fr 48px', alignItems: 'center', gap: 12 }}>
            <span style={{ fontSize: 13, color: 'var(--text-secondary)', whiteSpace: 'nowrap', overflow: 'hidden', textOverflow: 'ellipsis' }}>
              {feature}
            </span>
            <div style={{ background: 'var(--surface-bg)', borderRadius: 4, height: 10, overflow: 'hidden' }}>
              <div
                style={{
                  width: `${pct}%`,
                  height: '100%',
                  background: 'var(--color-primary)',
                  borderRadius: 4,
                  transition: 'width 0.4s ease',
                }}
              />
            </div>
            <span style={{ fontSize: 13, color: 'var(--text-muted)', textAlign: 'right' }}>
              {requests.toLocaleString()}
            </span>
          </div>
        );
      })}
    </div>
  );
}

function DailyLineChart({ data }: { readonly data: number[] }) {
  const points = buildPolylinePoints(data);
  const areaPath = buildAreaPath(data);
  const gradId = 'daily-area-grad';

  return (
    <svg
      viewBox={`0 0 ${CHART_WIDTH} ${CHART_HEIGHT}`}
      style={{ width: '100%', height: 'auto', display: 'block' }}
      aria-label="Daily AI request trend over last 14 days"
      role="img"
    >
      <defs>
        <linearGradient id={gradId} x1="0" y1="0" x2="0" y2="1">
          <stop offset="0%" stopColor="var(--color-primary)" stopOpacity="0.25" />
          <stop offset="100%" stopColor="var(--color-primary)" stopOpacity="0.02" />
        </linearGradient>
      </defs>
      <path d={areaPath} fill={`url(#${gradId})`} />
      <polyline
        points={points}
        fill="none"
        stroke="var(--color-primary)"
        strokeWidth="2"
        strokeLinejoin="round"
        strokeLinecap="round"
      />
    </svg>
  );
}

function DonutChart() {
  const cx = 80;
  const cy = 80;
  const r = 60;
  const stroke = 18;
  let startDeg = 0;

  return (
    <div style={{ display: 'flex', alignItems: 'center', gap: 28 }}>
      <svg viewBox="0 0 160 160" style={{ width: 160, height: 160, flexShrink: 0 }} aria-label="Model usage breakdown" role="img">
        {MODEL_BREAKDOWN.map(({ label, pct, color }) => {
          const sweep = (pct / 100) * 360;
          const endDeg = startDeg + sweep - 1.5; // gap between segments
          const d = describeArc(cx, cy, r, startDeg, endDeg);
          startDeg += sweep;
          return (
            <path
              key={label}
              d={d}
              fill="none"
              stroke={color}
              strokeWidth={stroke}
              strokeLinecap="butt"
            />
          );
        })}
        <text x={cx} y={cy - 6} textAnchor="middle" style={{ fontSize: 11, fill: 'var(--text-muted)' }}>Models</text>
        <text x={cx} y={cy + 10} textAnchor="middle" style={{ fontSize: 11, fill: 'var(--text-muted)' }}>Usage</text>
      </svg>
      <div style={{ display: 'flex', flexDirection: 'column', gap: 10 }}>
        {MODEL_BREAKDOWN.map(({ label, pct, color }) => (
          <div key={label} style={{ display: 'flex', alignItems: 'center', gap: 8 }}>
            <span style={{ width: 12, height: 12, borderRadius: 3, background: color, flexShrink: 0, display: 'inline-block' }} />
            <span style={{ fontSize: 13, color: 'var(--text-secondary)' }}>{label}</span>
            <span style={{ fontSize: 13, color: 'var(--text-muted)', marginLeft: 'auto', paddingLeft: 16 }}>{pct}%</span>
          </div>
        ))}
      </div>
    </div>
  );
}

function GovernancePanel() {
  const [settings, setSettings] = useState<GovernanceSettings>(DEFAULT_GOVERNANCE);
  const [toast, setToast] = useState(false);
  const toastTimer = useRef<ReturnType<typeof setTimeout> | null>(null);

  useEffect(() => {
    const raw = localStorage.getItem(GOVERNANCE_STORAGE_KEY);
    if (raw) {
      try {
        const parsed = JSON.parse(raw) as Partial<GovernanceSettings>;
        setSettings(prev => ({ ...prev, ...parsed }));
      } catch {
        // ignore malformed storage
      }
    }
  }, []);

  function handleSave() {
    localStorage.setItem(GOVERNANCE_STORAGE_KEY, JSON.stringify(settings));
    setToast(true);
    if (toastTimer.current) clearTimeout(toastTimer.current);
    toastTimer.current = setTimeout(() => setToast(false), 2500);
  }

  const fieldLabel: React.CSSProperties = {
    fontSize: 13,
    fontWeight: 500,
    color: 'var(--text-secondary)',
    marginBottom: 6,
    display: 'block',
  };

  const input: React.CSSProperties = {
    width: '100%',
    padding: '8px 12px',
    borderRadius: 'var(--radius-md)',
    border: '1px solid var(--surface-border)',
    background: 'var(--surface-bg)',
    color: 'var(--text-primary)',
    fontSize: 14,
    boxSizing: 'border-box',
  };

  return (
    <div style={{ position: 'relative', display: 'flex', flexDirection: 'column', gap: 20 }}>
      {/* Monthly budget */}
      <div>
        <label style={fieldLabel} htmlFor="monthly-budget">Monthly Budget Limit (USD)</label>
        <input
          id="monthly-budget"
          type="number"
          min={0}
          step={1}
          style={input}
          value={settings.monthlyBudget}
          onChange={e => setSettings(s => ({ ...s, monthlyBudget: Number(e.target.value) }))}
        />
      </div>

      {/* Alert threshold */}
      <div>
        <label style={fieldLabel} htmlFor="alert-threshold">
          Alert Threshold — {settings.alertThreshold}%
        </label>
        <input
          id="alert-threshold"
          type="range"
          min={50}
          max={90}
          step={5}
          style={{ width: '100%', accentColor: 'var(--color-primary)' }}
          value={settings.alertThreshold}
          onChange={e => setSettings(s => ({ ...s, alertThreshold: Number(e.target.value) }))}
        />
        <div style={{ display: 'flex', justifyContent: 'space-between', fontSize: 11, color: 'var(--text-muted)', marginTop: 2 }}>
          <span>50%</span><span>90%</span>
        </div>
      </div>

      {/* AI features toggle */}
      <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between' }}>
        <label style={{ ...fieldLabel, margin: 0, cursor: 'pointer' }} htmlFor="ai-enabled">
          AI Features Enabled
        </label>
        <button
          id="ai-enabled"
          role="switch"
          aria-checked={settings.aiEnabled}
          type="button"
          onClick={() => setSettings(s => ({ ...s, aiEnabled: !s.aiEnabled }))}
          style={{
            width: 44,
            height: 24,
            borderRadius: 12,
            border: 'none',
            cursor: 'pointer',
            background: settings.aiEnabled ? 'var(--color-primary)' : 'var(--surface-border)',
            position: 'relative',
            transition: 'background 0.2s',
            padding: 0,
          }}
        >
          <span
            style={{
              position: 'absolute',
              top: 3,
              left: settings.aiEnabled ? 22 : 3,
              width: 18,
              height: 18,
              borderRadius: '50%',
              background: '#fff',
              transition: 'left 0.2s',
              display: 'block',
            }}
          />
        </button>
      </div>

      {/* Save */}
      <button
        type="button"
        onClick={handleSave}
        style={{
          alignSelf: 'flex-start',
          padding: '9px 20px',
          borderRadius: 'var(--radius-md)',
          border: 'none',
          background: 'var(--color-primary)',
          color: '#fff',
          fontWeight: 600,
          fontSize: 14,
          cursor: 'pointer',
        }}
      >
        Save Settings
      </button>

      {toast && (
        <div
          role="status"
          aria-live="polite"
          style={{
            position: 'absolute',
            bottom: -40,
            left: 0,
            background: '#22c55e',
            color: '#fff',
            borderRadius: 6,
            padding: '6px 14px',
            fontSize: 13,
            fontWeight: 500,
            pointerEvents: 'none',
          }}
        >
          Settings saved
        </div>
      )}
    </div>
  );
}

// ── Page ─────────────────────────────────────────────────────────────────────

const DAILY_DATA = generateDailyData();

const card: React.CSSProperties = {
  background: 'var(--surface-card)',
  border: '1px solid var(--surface-border)',
  borderRadius: 'var(--radius-md)',
  padding: '20px 24px',
};

const sectionTitle: React.CSSProperties = {
  fontSize: 14,
  fontWeight: 600,
  color: 'var(--text-primary)',
  marginBottom: 16,
};

export function AiUsageDashboardPage() {
  return (
    <div style={{ padding: 24, display: 'flex', flexDirection: 'column', gap: 24, maxWidth: 960 }}>
      {/* Summary strip */}
      <div style={{ display: 'flex', gap: 16, flexWrap: 'wrap' }}>
        {SUMMARY_STATS.map(s => (
          <SummaryCard key={s.label} label={s.label} value={s.value} unit={s.unit} />
        ))}
      </div>

      {/* Usage by feature + Model breakdown side-by-side */}
      <div style={{ display: 'grid', gridTemplateColumns: '1fr auto', gap: 16 }}>
        <div style={card}>
          <div style={sectionTitle}>Usage by Feature</div>
          <FeatureBarChart />
        </div>
        <div style={{ ...card, minWidth: 280 }}>
          <div style={sectionTitle}>Model Breakdown</div>
          <DonutChart />
        </div>
      </div>

      {/* Daily trend */}
      <div style={card}>
        <div style={sectionTitle}>Daily Requests — Last 14 Days</div>
        <DailyLineChart data={DAILY_DATA} />
      </div>

      {/* Cost governance */}
      <div style={card}>
        <div style={sectionTitle}>Cost Governance</div>
        <GovernancePanel />
      </div>
    </div>
  );
}
