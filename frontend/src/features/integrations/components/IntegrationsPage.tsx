import { useState } from 'react';
import { useAuthStore } from '@/store/authStore';
import { SlackSettings } from './SlackSettings';
import { CiCdStatusPage } from './CiCdStatusPage';

const panel = {
  background: 'var(--surface-bg)',
  border: '1px solid var(--border-color)',
  borderRadius: 'var(--radius-md)',
  padding: 20,
  maxWidth: 720,
};

type PipelineMiniStatus = 'passing' | 'failing' | 'running';

interface MiniPipelineRun {
  id: string;
  name: string;
  status: PipelineMiniStatus;
  ago: string;
}

const MINI_RUNS: MiniPipelineRun[] = [
  { id: 'r1', name: 'Frontend Build',    status: 'passing', ago: '2h ago'  },
  { id: 'r2', name: 'Backend Tests',     status: 'failing', ago: '30m ago' },
  { id: 'r3', name: 'Deploy to Staging', status: 'running', ago: 'Now'     },
];

const MINI_STATUS_ICON: Record<PipelineMiniStatus, string> = {
  passing: '✅',
  failing: '❌',
  running: '🔄',
};

const MINI_STATUS_COLOR: Record<PipelineMiniStatus, string> = {
  passing: '#22c55e',
  failing: '#ef4444',
  running: '#f59e0b',
};

export function IntegrationsPage() {
  const { token } = useAuthStore();
  const [copied, setCopied] = useState(false);
  const [showCiCd, setShowCiCd] = useState(false);

  const feedUrl = token?.userId
    ? `${window.location.origin}/api/calendar/feed/${token.userId}.ics`
    : '';

  const copy = () => {
    if (!feedUrl) return;
    void navigator.clipboard.writeText(feedUrl).then(() => {
      setCopied(true);
      setTimeout(() => setCopied(false), 2000);
    });
  };

  return (
    <div>
      <div className="page-header">
        <h1 className="page-title">Integrations 📆</h1>
      </div>

      <div style={panel}>
        <h3 style={{ margin: '0 0 6px', fontSize: 16 }}>Calendar subscription</h3>
        <p style={{ fontSize: 13, color: 'var(--text-secondary)', margin: '0 0 14px', lineHeight: 1.6 }}>
          Subscribe to this private feed to see your task due dates in Google Calendar,
          Outlook, or Apple Calendar. The feed updates automatically as your tasks change.
          Keep the URL private — anyone with it can see your due dates.
        </p>

        <div style={{ display: 'flex', gap: 8, marginBottom: 16 }}>
          <input className="tf-input" readOnly value={feedUrl} style={{ flex: 1, fontSize: 12, fontFamily: 'monospace' }} />
          <button type="button" className="tf-btn tf-btn-primary tf-btn-sm" onClick={copy} disabled={!feedUrl}>
            {copied ? '✅ Copied' : '📋 Copy'}
          </button>
        </div>

        <div style={{ fontSize: 13, color: 'var(--text-secondary)', lineHeight: 1.7 }}>
          <strong>How to subscribe</strong>
          <ul style={{ margin: '6px 0 0', paddingLeft: 20 }}>
            <li><strong>Google Calendar:</strong> Other calendars → <em>From URL</em> → paste the link.</li>
            <li><strong>Outlook:</strong> Add calendar → <em>Subscribe from web</em> → paste the link.</li>
            <li><strong>Apple Calendar:</strong> File → <em>New Calendar Subscription</em> → paste the link.</li>
          </ul>
        </div>
      </div>

      <SlackSettings />

      <div style={{ ...panel, marginTop: 16, opacity: 0.75 }}>
        <h3 style={{ margin: '0 0 6px', fontSize: 16 }}>Two-way OAuth sync <span style={{ fontSize: 11, color: 'var(--text-muted)' }}>· coming soon</span></h3>
        <p style={{ fontSize: 13, color: 'var(--text-muted)', margin: 0, lineHeight: 1.6 }}>
          Direct Google/Microsoft 365 OAuth with rescheduling that flows back into TaskFlow is planned.
          The read-only feed above works with every major calendar app today, no sign-in required.
        </p>
      </div>

      {/* CI/CD Status section */}
      <div style={{ ...panel, marginTop: 16 }}>
        <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: 10 }}>
          <h3 style={{ margin: 0, fontSize: 16 }}>CI/CD Status ⚙️</h3>
          <button
            type="button"
            className="tf-btn tf-btn-sm"
            onClick={() => setShowCiCd(prev => !prev)}
            style={{ fontSize: 12 }}
          >
            {showCiCd ? 'Hide panel ▲' : 'View all CI/CD status →'}
          </button>
        </div>

        {/* Summary strip */}
        <div style={{
          display: 'flex',
          gap: 16,
          padding: '8px 12px',
          background: 'var(--surface-card)',
          border: '1px solid var(--surface-border)',
          borderRadius: 6,
          marginBottom: 12,
          fontSize: 13,
          color: 'var(--text-secondary)',
          flexWrap: 'wrap',
        }}>
          <span>🔗 3 pipelines connected</span>
          <span style={{ color: '#22c55e' }}>✅ 2 passing</span>
          <span style={{ color: '#ef4444' }}>❌ 1 failing</span>
        </div>

        {/* Mini run list */}
        <div style={{ display: 'flex', flexDirection: 'column', gap: 6 }}>
          {MINI_RUNS.map(run => (
            <div
              key={run.id}
              style={{
                display: 'flex',
                alignItems: 'center',
                gap: 10,
                padding: '7px 10px',
                background: 'var(--surface-card)',
                border: '1px solid var(--surface-border)',
                borderLeft: `3px solid ${MINI_STATUS_COLOR[run.status]}`,
                borderRadius: 6,
                fontSize: 13,
              }}
            >
              <span>{MINI_STATUS_ICON[run.status]}</span>
              <span style={{ flex: 1, color: 'var(--text-primary)', fontWeight: 500 }}>{run.name}</span>
              <span style={{ fontSize: 11, color: 'var(--text-muted)' }}>{run.ago}</span>
            </div>
          ))}
        </div>

        {showCiCd && (
          <div style={{ marginTop: 20 }}>
            <CiCdStatusPage />
          </div>
        )}
      </div>
    </div>
  );
}
