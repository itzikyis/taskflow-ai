import { useState } from 'react';

type PipelineStatus = 'passing' | 'failing' | 'running';

interface Pipeline {
  id: string;
  name: string;
  provider: string;
  status: PipelineStatus;
  branch: string;
  lastRunAgo: string;
  duration: string;
}

interface BuildLogEntry {
  id: string;
  time: string;
  pipeline: string;
  branch: string;
  status: PipelineStatus;
  duration: string;
  linkedTask: string;
}

interface Vulnerability {
  id: string;
  name: string;
  severity: 'High' | 'Medium' | 'Low';
  package: string;
}

const PIPELINES: Pipeline[] = [
  { id: 'p1', name: 'Frontend Build', provider: 'GitHub Actions', status: 'passing', branch: 'main', lastRunAgo: '2h ago', duration: '3m 12s' },
  { id: 'p2', name: 'Backend Tests', provider: 'GitHub Actions', status: 'failing', branch: 'feature/auth', lastRunAgo: '30m ago', duration: '6m 44s' },
  { id: 'p3', name: 'Security Scan', provider: 'Snyk', status: 'passing', branch: 'main', lastRunAgo: '1d ago', duration: '1m 08s' },
  { id: 'p4', name: 'Deploy to Staging', provider: 'GitHub Actions', status: 'running', branch: 'develop', lastRunAgo: 'Just now', duration: '—' },
];

const BUILD_LOG: BuildLogEntry[] = [
  { id: 'b1',  time: '14:32', pipeline: 'Deploy to Staging', branch: 'develop',      status: 'running', duration: '—',      linkedTask: '—' },
  { id: 'b2',  time: '14:01', pipeline: 'Backend Tests',     branch: 'feature/auth', status: 'failing', duration: '6m 44s', linkedTask: 'Fix auth bug' },
  { id: 'b3',  time: '13:50', pipeline: 'Frontend Build',    branch: 'main',         status: 'passing', duration: '3m 12s', linkedTask: 'Update UI components' },
  { id: 'b4',  time: '13:20', pipeline: 'Backend Tests',     branch: 'feature/auth', status: 'failing', duration: '6m 51s', linkedTask: 'Fix auth bug' },
  { id: 'b5',  time: '12:15', pipeline: 'Frontend Build',    branch: 'develop',      status: 'passing', duration: '3m 05s', linkedTask: '—' },
  { id: 'b6',  time: '11:00', pipeline: 'Security Scan',     branch: 'main',         status: 'passing', duration: '1m 08s', linkedTask: '—' },
  { id: 'b7',  time: '10:48', pipeline: 'Backend Tests',     branch: 'main',         status: 'passing', duration: '5m 59s', linkedTask: 'Refactor task service' },
  { id: 'b8',  time: '10:20', pipeline: 'Deploy to Staging', branch: 'develop',      status: 'passing', duration: '8m 22s', linkedTask: 'Sprint release v1.4' },
  { id: 'b9',  time: '09:55', pipeline: 'Frontend Build',    branch: 'main',         status: 'passing', duration: '3m 18s', linkedTask: '—' },
  { id: 'b10', time: '09:30', pipeline: 'Backend Tests',     branch: 'develop',      status: 'passing', duration: '6m 02s', linkedTask: '—' },
];

const VULNERABILITIES: Vulnerability[] = [
  { id: 'v1', name: 'Prototype Pollution in lodash', severity: 'High', package: 'lodash@4.17.20' },
  { id: 'v2', name: 'ReDoS in path-to-regexp', severity: 'High', package: 'path-to-regexp@0.1.7' },
  { id: 'v3', name: 'Open Redirect in express', severity: 'Medium', package: 'express@4.18.1' },
  { id: 'v4', name: 'Cross-site Scripting in marked', severity: 'Medium', package: 'marked@4.0.10' },
  { id: 'v5', name: 'Denial of Service in semver', severity: 'Medium', package: 'semver@6.3.0' },
  { id: 'v6', name: 'Information Exposure in debug', severity: 'Medium', package: 'debug@2.6.9' },
  { id: 'v7', name: 'Prototype Pollution in minimist', severity: 'Medium', package: 'minimist@1.2.5' },
];

const STATUS_ICON: Record<PipelineStatus, string> = {
  passing: '✅',
  failing: '❌',
  running: '🔄',
};

const STATUS_LABEL: Record<PipelineStatus, string> = {
  passing: 'Passed',
  failing: 'Failed',
  running: 'Running',
};

const STATUS_COLOR: Record<PipelineStatus, string> = {
  passing: '#22c55e',
  failing: '#ef4444',
  running: '#f59e0b',
};

const SEVERITY_COLOR: Record<'High' | 'Medium' | 'Low', string> = {
  High: '#ef4444',
  Medium: '#f59e0b',
  Low: '#3b82f6',
};

const panel: React.CSSProperties = {
  background: 'var(--surface-bg)',
  border: '1px solid var(--surface-border)',
  borderRadius: 'var(--radius-md)',
  padding: 20,
  marginBottom: 16,
};

const badge = (color: string): React.CSSProperties => ({
  display: 'inline-flex',
  alignItems: 'center',
  gap: 4,
  fontSize: 11,
  fontWeight: 600,
  color,
  background: `${color}18`,
  border: `1px solid ${color}44`,
  borderRadius: 4,
  padding: '2px 8px',
});

const providerBadge: React.CSSProperties = {
  fontSize: 11,
  color: 'var(--text-muted)',
  background: 'var(--surface-card)',
  border: '1px solid var(--surface-border)',
  borderRadius: 4,
  padding: '2px 7px',
};

export function CiCdStatusPage() {
  const [vulnsExpanded, setVulnsExpanded] = useState(false);

  const highCount = VULNERABILITIES.filter(v => v.severity === 'High').length;
  const medCount  = VULNERABILITIES.filter(v => v.severity === 'Medium').length;
  const lowCount  = 12;
  const total     = highCount + medCount + lowCount;

  return (
    <div style={{ maxWidth: 900 }}>
      {/* Pipeline overview */}
      <div style={panel}>
        <h3 style={{ margin: '0 0 14px', fontSize: 15, color: 'var(--text-primary)' }}>Pipeline Overview</h3>
        <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fill, minmax(200px, 1fr))', gap: 12 }}>
          {PIPELINES.map(p => (
            <div
              key={p.id}
              style={{
                background: 'var(--surface-card)',
                border: `1px solid var(--surface-border)`,
                borderLeft: `3px solid ${STATUS_COLOR[p.status]}`,
                borderRadius: 'var(--radius-md)',
                padding: 14,
              }}
            >
              <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start', marginBottom: 8 }}>
                <span style={{ fontSize: 13, fontWeight: 600, color: 'var(--text-primary)' }}>{p.name}</span>
                <span style={providerBadge}>{p.provider}</span>
              </div>
              <div style={{ display: 'flex', alignItems: 'center', gap: 6, marginBottom: 6 }}>
                <span style={badge(STATUS_COLOR[p.status])}>
                  {STATUS_ICON[p.status]} {STATUS_LABEL[p.status]}
                </span>
              </div>
              <div style={{ fontSize: 12, color: 'var(--text-secondary)' }}>
                <div>Branch: <code style={{ fontSize: 11 }}>{p.branch}</code></div>
                <div>Last run: {p.lastRunAgo}</div>
                <div>Duration: {p.duration}</div>
              </div>
            </div>
          ))}
        </div>
      </div>

      {/* Recent build log */}
      <div style={panel}>
        <h3 style={{ margin: '0 0 14px', fontSize: 15, color: 'var(--text-primary)' }}>Recent Build Log</h3>
        <div style={{ overflowX: 'auto' }}>
          <table style={{ width: '100%', borderCollapse: 'collapse', fontSize: 13 }}>
            <thead>
              <tr style={{ borderBottom: '1px solid var(--surface-border)' }}>
                {['Time', 'Pipeline', 'Branch', 'Status', 'Duration', 'Linked Task'].map(col => (
                  <th key={col} style={{ textAlign: 'left', padding: '6px 10px', color: 'var(--text-muted)', fontWeight: 600, fontSize: 11, whiteSpace: 'nowrap' }}>{col}</th>
                ))}
              </tr>
            </thead>
            <tbody>
              {BUILD_LOG.map((entry, i) => (
                <tr
                  key={entry.id}
                  style={{
                    borderBottom: '1px solid var(--surface-border)',
                    background: i % 2 === 0 ? 'transparent' : 'var(--surface-card)',
                  }}
                >
                  <td style={{ padding: '7px 10px', color: 'var(--text-muted)', fontFamily: 'monospace', fontSize: 12 }}>{entry.time}</td>
                  <td style={{ padding: '7px 10px', color: 'var(--text-primary)', fontWeight: 500 }}>{entry.pipeline}</td>
                  <td style={{ padding: '7px 10px' }}><code style={{ fontSize: 11, color: 'var(--text-secondary)' }}>{entry.branch}</code></td>
                  <td style={{ padding: '7px 10px' }}>
                    <span style={badge(STATUS_COLOR[entry.status])}>
                      {STATUS_ICON[entry.status]} {STATUS_LABEL[entry.status]}
                    </span>
                  </td>
                  <td style={{ padding: '7px 10px', color: 'var(--text-secondary)', fontFamily: 'monospace', fontSize: 12 }}>{entry.duration}</td>
                  <td style={{ padding: '7px 10px', color: entry.linkedTask === '—' ? 'var(--text-muted)' : 'var(--color-primary)', fontSize: 12 }}>{entry.linkedTask}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </div>

      {/* Security scan summary */}
      <div style={panel}>
        <h3 style={{ margin: '0 0 4px', fontSize: 15, color: 'var(--text-primary)' }}>Security Scan — Snyk</h3>
        <p style={{ margin: '0 0 14px', fontSize: 12, color: 'var(--text-muted)' }}>Last scanned: 1 day ago · Branch: main</p>

        <div style={{ display: 'flex', gap: 16, marginBottom: 16, flexWrap: 'wrap' }}>
          {([['High', highCount, '#ef4444'], ['Medium', medCount, '#f59e0b'], ['Low', lowCount, '#3b82f6']] as const).map(([label, count, color]) => (
            <div key={label} style={{ display: 'flex', alignItems: 'center', gap: 8 }}>
              <span style={{ ...badge(color), fontSize: 13, padding: '4px 12px' }}>{count} {label}</span>
            </div>
          ))}
        </div>

        <div style={{ display: 'flex', flexDirection: 'column', gap: 6, marginBottom: 16 }}>
          {([['High', highCount, '#ef4444'], ['Medium', medCount, '#f59e0b'], ['Low', lowCount, '#3b82f6']] as const).map(([label, count, color]) => (
            <div key={label} style={{ display: 'flex', alignItems: 'center', gap: 10 }}>
              <span style={{ fontSize: 11, width: 50, color: 'var(--text-secondary)', textAlign: 'right' }}>{label}</span>
              <div style={{ flex: 1, height: 8, background: 'var(--surface-card)', borderRadius: 4, overflow: 'hidden' }}>
                <div style={{ width: `${(count / total) * 100}%`, height: '100%', background: color, borderRadius: 4, transition: 'width 0.4s' }} />
              </div>
              <span style={{ fontSize: 11, width: 20, color: 'var(--text-muted)' }}>{count}</span>
            </div>
          ))}
        </div>

        <button
          type="button"
          className="tf-btn tf-btn-sm"
          onClick={() => setVulnsExpanded(prev => !prev)}
          style={{ fontSize: 12 }}
        >
          {vulnsExpanded ? 'Hide details ▲' : 'View details ▼'}
        </button>

        {vulnsExpanded && (
          <div style={{ marginTop: 14, display: 'flex', flexDirection: 'column', gap: 6 }}>
            {VULNERABILITIES.map(v => (
              <div
                key={v.id}
                style={{
                  display: 'flex',
                  alignItems: 'center',
                  gap: 10,
                  padding: '8px 12px',
                  background: 'var(--surface-card)',
                  border: '1px solid var(--surface-border)',
                  borderRadius: 6,
                }}
              >
                <span style={badge(SEVERITY_COLOR[v.severity])}>{v.severity}</span>
                <span style={{ flex: 1, fontSize: 13, color: 'var(--text-primary)' }}>{v.name}</span>
                <code style={{ fontSize: 11, color: 'var(--text-muted)' }}>{v.package}</code>
              </div>
            ))}
          </div>
        )}
      </div>
    </div>
  );
}
