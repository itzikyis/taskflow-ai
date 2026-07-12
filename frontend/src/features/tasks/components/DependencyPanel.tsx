import { useState } from 'react';
import { useTaskDependencies, useAddDependency, useRemoveDependency } from '../hooks/useDependencies';
import { useAllTasks } from '../hooks/useTasks';
import type { DependencyTask } from '@/services/dependencyService';

interface DependencyPanelProps {
  taskId: string;
}

export function DependencyPanel({ taskId }: DependencyPanelProps) {
  const { data, isLoading } = useTaskDependencies(taskId);
  const { data: allTasks = [] } = useAllTasks();
  const addDep = useAddDependency(taskId);
  const removeDep = useRemoveDependency();
  const [pick, setPick] = useState('');
  const [error, setError] = useState('');

  // Tasks eligible to be a blocker: not self, not already a blocker.
  const existingBlockerIds = new Set((data?.blockedBy ?? []).map(d => d.taskId));
  const options = allTasks.filter(t => t.id !== taskId && !existingBlockerIds.has(t.id));

  const handleAdd = (e: React.FormEvent) => {
    e.preventDefault();
    if (!pick) return;
    setError('');
    addDep.mutate(pick, {
      onSuccess: () => setPick(''),
      onError: (err: unknown) => {
        const msg = (err as { response?: { data?: { description?: string } } })?.response?.data?.description;
        setError(msg ?? 'Could not add dependency.');
      },
    });
  };

  const Row = ({ d, kind }: { d: DependencyTask; kind: 'blockedBy' | 'blocks' }) => (
    <div style={{ display: 'flex', alignItems: 'center', gap: 8, fontSize: 12, padding: '3px 0' }}>
      <span style={{
        fontSize: 10, fontWeight: 700, padding: '1px 5px', borderRadius: 4, flexShrink: 0,
        color: d.status === 'Done' ? 'var(--status-done)' : 'var(--text-muted)',
        background: 'var(--surface-bg)',
      }}>
        {d.status}
      </span>
      <span style={{ flex: 1, color: 'var(--text-primary)', overflow: 'hidden', textOverflow: 'ellipsis', whiteSpace: 'nowrap' }}>
        {d.title}
      </span>
      <button
        type="button"
        onClick={() => removeDep.mutate(d.dependencyId)}
        aria-label="Remove dependency"
        style={{ background: 'none', border: 'none', cursor: 'pointer', color: 'var(--color-danger)', fontSize: 13, lineHeight: 1 }}
      >
        ×
      </button>
    </div>
  );

  return (
    <>
      <p style={{ fontSize: 12, fontWeight: 600, color: 'var(--text-secondary)', textTransform: 'uppercase', letterSpacing: '0.04em', margin: '0 0 8px' }}>
        🚧 Dependencies
      </p>

      {isLoading && <p style={{ fontSize: 12, color: 'var(--text-muted)' }}>Loading…</p>}

      <div style={{ marginBottom: 8 }}>
        <p style={{ fontSize: 11, fontWeight: 600, color: 'var(--text-muted)', margin: '0 0 2px' }}>Blocked by</p>
        {(data?.blockedBy.length ?? 0) === 0
          ? <p style={{ fontSize: 12, color: 'var(--text-muted)', margin: 0 }}>Nothing — ready to start.</p>
          : data!.blockedBy.map(d => <Row key={d.dependencyId} d={d} kind="blockedBy" />)}
      </div>

      {(data?.blocks.length ?? 0) > 0 && (
        <div style={{ marginBottom: 8 }}>
          <p style={{ fontSize: 11, fontWeight: 600, color: 'var(--text-muted)', margin: '0 0 2px' }}>Blocks</p>
          {data!.blocks.map(d => <Row key={d.dependencyId} d={d} kind="blocks" />)}
        </div>
      )}

      <form onSubmit={handleAdd} style={{ display: 'flex', gap: 6, marginTop: 6 }}>
        <select className="tf-input" value={pick} onChange={e => setPick(e.target.value)} style={{ flex: 1, fontSize: 12 }}>
          <option value="">Add a blocker…</option>
          {options.map(t => <option key={t.id} value={t.id}>{t.title}</option>)}
        </select>
        <button type="submit" className="tf-btn tf-btn-primary tf-btn-sm" disabled={!pick || addDep.isPending}>
          {addDep.isPending ? '…' : 'Add'}
        </button>
      </form>
      {error && <p style={{ fontSize: 11, color: 'var(--color-danger)', marginTop: 4 }}>{error}</p>}
    </>
  );
}
