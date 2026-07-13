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

export function SlackSettings() {
  const qc = useQueryClient();
  const { data } = useQuery({ queryKey: ['slack'], queryFn: () => slackService.get() });

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
      <h3 style={{ margin: '0 0 6px', fontSize: 16 }}>Slack notifications 💬</h3>
      <p style={{ fontSize: 13, color: 'var(--text-secondary)', margin: '0 0 14px', lineHeight: 1.6 }}>
        Forward task activity to a Slack channel using an{' '}
        <a href="https://api.slack.com/messaging/webhooks" target="_blank" rel="noopener noreferrer">Incoming Webhook</a>.
        {data?.configured && <> Currently set to <code>{data.webhookUrlMasked}</code>.</>}
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
