import type { CSSProperties } from 'react';
import { useAllTasks } from '@/features/tasks/hooks/useTasks';
import { useGenerateRetrospective } from '../hooks/useAi';
import type { SprintRetrospective } from '@/services/aiService';

const panel: CSSProperties = {
  background: 'var(--surface-bg)',
  border: '1px solid var(--border-color)',
  borderRadius: 'var(--radius-md)',
  padding: 20,
};

export function RetrospectivePage() {
  const { data: allTasks = [] } = useAllTasks();
  const { mutate: generate, isPending, data: retro, isError } = useGenerateRetrospective();

  const completed = allTasks.filter(t => t.status === 'Done');
  const incomplete = allTasks.filter(t => t.status !== 'Done');

  const toInput = (t: (typeof allTasks)[number]) => ({
    title: t.title,
    description: t.description ?? undefined,
    priority: t.priority,
  });

  const handleGenerate = () =>
    generate({ completed: completed.map(toInput), incomplete: incomplete.map(toInput) });

  return (
    <div>
      <div className="page-header">
        <h1 className="page-title">AI Sprint Retrospective 🔁</h1>
      </div>

      <div style={{ display: 'grid', gridTemplateColumns: '280px 1fr', gap: 24, alignItems: 'start' }}>
        <aside style={panel}>
          <div style={{ fontSize: 12, color: 'var(--text-muted)', marginBottom: 16, lineHeight: 1.6 }}>
            <div><strong>{completed.length}</strong> completed</div>
            <div><strong>{incomplete.length}</strong> still open</div>
          </div>
          <button
            type="button"
            className="tf-btn tf-btn-primary"
            style={{ width: '100%' }}
            disabled={isPending || completed.length === 0}
            onClick={handleGenerate}
          >
            {isPending ? 'Generating…' : 'Generate Retrospective'}
          </button>
        </aside>

        <section style={panel}>
          {isPending && (
            <p style={{ color: 'var(--text-muted)', fontSize: 14, padding: '32px 0', textAlign: 'center' }}>
              Reviewing the sprint…
            </p>
          )}

          {isError && (
            <p style={{ color: 'var(--color-danger)', fontSize: 14 }}>
              The AI service is unavailable right now. Please try again later.
            </p>
          )}

          {!isPending && !isError && !retro && (
            <div className="empty-state">
              <div className="empty-state-icon">🔁</div>
              <div className="empty-state-text">
                {completed.length === 0
                  ? 'No completed tasks yet. Mark tasks as Done to generate a retrospective.'
                  : 'Click Generate Retrospective to draft one from this sprint’s work.'}
              </div>
            </div>
          )}

          {!isPending && retro && <RetroResults retro={retro} />}
        </section>
      </div>
    </div>
  );
}

function RetroResults({ retro }: { retro: SprintRetrospective }) {
  return (
    <>
      <div style={{
        background: 'var(--color-primary-light, #ede9fe)', border: '1px solid var(--color-primary)',
        borderRadius: 'var(--radius-md)', padding: '14px 18px', marginBottom: 20,
      }}>
        <p style={{ margin: 0, fontSize: 14, color: 'var(--text-primary)', lineHeight: 1.6 }}>{retro.summary}</p>
      </div>

      <Section icon="✅" title="What went well" items={retro.wentWell} />
      <Section icon="⚠️" title="Issues" items={retro.issues} />
      <Section icon="🎯" title="Estimate accuracy" items={retro.estimateAccuracyNotes} />
      <Section icon="🚀" title="Action items" items={retro.actionItems} />
    </>
  );
}

function Section({ icon, title, items }: { icon: string; title: string; items: string[] }) {
  if (items.length === 0) return null;
  return (
    <div style={{ marginBottom: 20 }}>
      <p style={{ fontSize: 15, fontWeight: 700, color: 'var(--text-primary)', margin: '0 0 10px', display: 'flex', gap: 6 }}>
        <span>{icon}</span> {title}
      </p>
      <ul style={{ margin: 0, padding: 0, listStyle: 'none' }}>
        {items.map((item, i) => (
          <li key={i} style={{ fontSize: 14, color: 'var(--text-primary)', lineHeight: 1.6, padding: '6px 0', borderBottom: '1px solid var(--border-color)' }}>
            <span style={{ marginRight: 8, color: 'var(--text-muted)' }}>–</span>{item}
          </li>
        ))}
      </ul>
    </div>
  );
}
