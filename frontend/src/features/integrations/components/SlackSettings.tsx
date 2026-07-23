import { useEffect, useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { slackService, type SaveSlackConfig } from '@/services/slackService';

const panel = {
  background: 'var(--surface-bg)',
  border: '1px solid var(--border-color)',
  borderRadius: 'var(--radius-md)',
  padding: 20,
  maxWidth: 720,
  marginTop: 16,
};

const CODE_STYLE: React.CSSProperties = {
  background: 'var(--surface-raised, var(--surface-bg))',
  border: '1px solid var(--border-color)',
  borderRadius: 4,
  padding: '2px 6px',
  fontFamily: 'monospace',
  fontSize: 12,
};

const STEP_STYLE: React.CSSProperties = {
  display: 'flex',
  gap: 10,
  alignItems: 'flex-start',
  fontSize: 13,
  lineHeight: 1.6,
  color: 'var(--text-primary)',
};

const STEP_NUM: React.CSSProperties = {
  flexShrink: 0,
  width: 22,
  height: 22,
  borderRadius: '50%',
  background: 'var(--color-primary, #6366f1)',
  color: '#fff',
  display: 'flex',
  alignItems: 'center',
  justifyContent: 'center',
  fontSize: 11,
  fontWeight: 700,
  marginTop: 1,
};

function StatusBadge({ configured }: { configured: boolean }) {
  return (
    <span
      style={{
        display: 'inline-flex',
        alignItems: 'center',
        gap: 4,
        fontSize: 11,
        fontWeight: 600,
        padding: '2px 8px',
        borderRadius: 12,
        background: configured ? 'var(--color-success-bg, #d1fae5)' : 'var(--surface-raised, #f3f4f6)',
        color: configured ? 'var(--color-success, #065f46)' : 'var(--text-muted, #6b7280)',
        border: `1px solid ${configured ? 'var(--color-success-border, #6ee7b7)' : 'var(--border-color)'}`,
      }}
    >
      <span style={{ fontSize: 8 }}>●</span>
      {configured ? 'Connected' : 'Not configured'}
    </span>
  );
}

export function SlackSettings() {
  const qc = useQueryClient();
  const { data } = useQuery({ queryKey: ['slack'], queryFn: () => slackService.get() });
  const { data: commandConfig } = useQuery({
    queryKey: ['slack-command-config'],
    queryFn: () => slackService.getCommandConfig(),
  });

  const [form, setForm] = useState<SaveSlackConfig>({
    webhookUrl: '', enabled: true,
    forwardCreated: true, forwardStatusChanged: true, forwardComments: true, forwardOther: false,
  });
  const [testMsg, setTestMsg] = useState('');

  useEffect(() => {
    if (data) {
      setForm(f => ({
        ...f,
        enabled: data.enabled,
        forwardCreated: data.forwardCreated,
        forwardStatusChanged: data.forwardStatusChanged,
        forwardComments: data.forwardComments,
        forwardOther: data.forwardOther,
      }));
    }
  }, [data]);

  const save = useMutation({
    mutationFn: () => slackService.save(form),
    onSuccess: () => qc.invalidateQueries({ queryKey: ['slack'] }),
  });
  const test = useMutation({
    mutationFn: () => slackService.test(),
    onSuccess: () => setTestMsg('✅ Test message sent.'),
    onError: () => setTestMsg('⚠️ Could not deliver — check the webhook URL.'),
  });
  const remove = useMutation({
    mutationFn: () => slackService.remove(),
    onSuccess: () => qc.invalidateQueries({ queryKey: ['slack'] }),
  });

  const Check = ({ k, label }: { k: keyof SaveSlackConfig; label: string }) => (
    <label style={{ display: 'flex', alignItems: 'center', gap: 6, fontSize: 13 }}>
      <input type="checkbox" checked={Boolean(form[k])} onChange={e => setForm({ ...form, [k]: e.target.checked })} />
      {label}
    </label>
  );

  return (
    <div style={panel}>
      {/* ── Header ── */}
      <div style={{ display: 'flex', alignItems: 'center', gap: 10, marginBottom: 6 }}>
        <h3 style={{ margin: 0, fontSize: 16 }}>💬 Slack Integration</h3>
        <StatusBadge configured={commandConfig?.isConfigured ?? false} />
      </div>

      {/* ── Slash command setup ── */}
      <section style={{ marginBottom: 20 }}>
        <p style={{ fontSize: 13, color: 'var(--text-secondary)', margin: '0 0 12px', lineHeight: 1.6 }}>
          Use the <code style={CODE_STYLE}>/taskflow</code> slash command to create tasks directly
          from any Slack channel. Example:
        </p>

        <div
          style={{
            background: 'var(--surface-raised, #f9fafb)',
            border: '1px solid var(--border-color)',
            borderRadius: 6,
            padding: '10px 14px',
            fontFamily: 'monospace',
            fontSize: 13,
            color: 'var(--text-primary)',
            marginBottom: 16,
          }}
        >
          /taskflow create Fix login bug
          <span
            style={{
              marginLeft: 16,
              fontSize: 11,
              fontFamily: 'sans-serif',
              color: 'var(--text-muted, #9ca3af)',
            }}
          >
            → creates a task titled "Fix login bug"
          </span>
        </div>

        <div style={{ display: 'flex', flexDirection: 'column', gap: 10 }}>
          <div style={STEP_STYLE}>
            <div style={STEP_NUM}>1</div>
            <div>
              Add the <code style={CODE_STYLE}>/taskflow</code> slash command to your Slack workspace
              via <span style={{ color: 'var(--text-muted, #6b7280)' }}>api.slack.com/apps</span>.
            </div>
          </div>
          <div style={STEP_STYLE}>
            <div style={STEP_NUM}>2</div>
            <div>
              Set <code style={CODE_STYLE}>Slack:SigningSecret</code> in your backend configuration
              (appsettings or environment variable) to the signing secret shown in the Slack app settings.
            </div>
          </div>
          <div style={STEP_STYLE}>
            <div style={STEP_NUM}>3</div>
            <div>
              Point the slash command&apos;s <strong>Request URL</strong> to{' '}
              <code style={CODE_STYLE}>POST /api/slack/command</code> on your TaskFlow backend.
            </div>
          </div>
        </div>
      </section>

      {/* ── Divider ── */}
      <hr style={{ border: 'none', borderTop: '1px solid var(--border-color)', margin: '0 0 16px' }} />

      {/* ── Webhook notifications ── */}
      <h4 style={{ margin: '0 0 6px', fontSize: 14 }}>Outbound notifications</h4>
      <p style={{ fontSize: 13, color: 'var(--text-secondary)', margin: '0 0 14px', lineHeight: 1.6 }}>
        Forward task activity to a Slack channel using an Incoming Webhook
        (<span style={{ color: 'var(--text-muted, #6b7280)' }}>api.slack.com/messaging/webhooks</span>).
        {data?.configured && <> Currently set to <code style={CODE_STYLE}>{data.webhookUrlMasked}</code>.</>}
      </p>

      <input
        className="tf-input"
        placeholder="https://hooks.slack.com/services/…"
        value={form.webhookUrl}
        onChange={e => setForm({ ...form, webhookUrl: e.target.value })}
        style={{ width: '100%', fontSize: 13, marginBottom: 12 }}
      />

      <div style={{ display: 'flex', gap: 16, flexWrap: 'wrap', marginBottom: 12 }}>
        <Check k="enabled" label="Enabled" />
        <Check k="forwardCreated" label="Task created" />
        <Check k="forwardStatusChanged" label="Status changed" />
        <Check k="forwardComments" label="Comments" />
        <Check k="forwardOther" label="Other events" />
      </div>

      <div style={{ display: 'flex', gap: 8, alignItems: 'center' }}>
        <button type="button" className="tf-btn tf-btn-primary tf-btn-sm" onClick={() => save.mutate()} disabled={save.isPending || !form.webhookUrl.trim()}>
          {save.isPending ? 'Saving…' : 'Save'}
        </button>
        <button type="button" className="tf-btn tf-btn-ghost tf-btn-sm" onClick={() => { setTestMsg(''); test.mutate(); }} disabled={test.isPending || !data?.configured}>
          {test.isPending ? 'Sending…' : 'Send test'}
        </button>
        {data?.configured && (
          <button type="button" className="tf-btn tf-btn-ghost tf-btn-sm" onClick={() => remove.mutate()} style={{ color: 'var(--color-danger)' }}>
            Remove
          </button>
        )}
        {save.isSuccess && <span style={{ fontSize: 12, color: 'var(--color-success, #10b981)' }}>Saved</span>}
        {testMsg && <span style={{ fontSize: 12, color: 'var(--text-secondary)' }}>{testMsg}</span>}
      </div>
    </div>
  );
}
