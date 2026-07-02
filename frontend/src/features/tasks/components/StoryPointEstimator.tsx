import type { CSSProperties } from 'react';
import { useEstimateStoryPoints } from '@/features/ai/hooks/useAi';
import { StoryPointBadge } from './StoryPointBadge';

interface StoryPointEstimatorProps {
  taskTitle: string;
  taskDescription?: string;
  onEstimate?: (points: number) => void;
}

const AI_PURPLE = '#7c3aed';
const AI_PURPLE_LIGHT = '#ede9fe';
const AI_PURPLE_BORDER = '#d8b4fe';

export function StoryPointEstimator({ taskTitle, taskDescription, onEstimate }: StoryPointEstimatorProps) {
  const estimate = useEstimateStoryPoints();

  const run = () => {
    estimate.mutate(
      { title: taskTitle, description: taskDescription },
      {
        onSuccess: (data) => {
          onEstimate?.(data.points);
        },
      },
    );
  };

  const buttonStyle: CSSProperties = {
    padding: '0.3rem 0.75rem',
    fontSize: '0.8rem',
    borderRadius: 4,
    border: `1px solid ${AI_PURPLE}`,
    background: 'none',
    color: AI_PURPLE,
    cursor: 'pointer',
    display: 'inline-flex',
    alignItems: 'center',
    gap: 4,
    fontWeight: 500,
  };

  const reEstimateStyle: CSSProperties = {
    ...buttonStyle,
    fontSize: '0.75rem',
    padding: '0.2rem 0.6rem',
    marginLeft: '0.5rem',
  };

  return (
    <div style={{ marginTop: '0.5rem' }}>
      {!estimate.isPending && !estimate.isSuccess && !estimate.isError && (
        <button
          type="button"
          onClick={run}
          disabled={!taskTitle.trim()}
          style={buttonStyle}
        >
          ✨ Estimate with AI
        </button>
      )}

      {estimate.isPending && (
        <span style={{ fontSize: '0.8rem', color: AI_PURPLE, display: 'flex', alignItems: 'center', gap: 6 }}>
          <span
            style={{
              display: 'inline-block',
              width: 14,
              height: 14,
              border: `2px solid ${AI_PURPLE_BORDER}`,
              borderTopColor: AI_PURPLE,
              borderRadius: '50%',
              animation: 'spin 0.7s linear infinite',
            }}
          />
          Estimating…
        </span>
      )}

      {estimate.isSuccess && estimate.data && (
        <div
          style={{
            marginTop: '0.5rem',
            padding: '0.75rem',
            background: AI_PURPLE_LIGHT,
            borderRadius: 6,
            border: `1px solid ${AI_PURPLE_BORDER}`,
          }}
        >
          <div style={{ display: 'flex', alignItems: 'center', gap: 8, marginBottom: 6 }}>
            <StoryPointBadge points={estimate.data.points} />
            <span style={{ fontSize: '0.85rem', fontWeight: 600, color: '#4c1d95' }}>
              {estimate.data.points} {estimate.data.points === 1 ? 'point' : 'points'}
            </span>
            <button
              type="button"
              onClick={run}
              style={reEstimateStyle}
            >
              ↺ Re-estimate
            </button>
          </div>
          <p style={{ margin: 0, fontSize: '0.78rem', color: '#5b21b6', lineHeight: 1.5 }}>
            {estimate.data.reasoning}
          </p>
        </div>
      )}

      {estimate.isError && (
        <div style={{ marginTop: '0.5rem', display: 'flex', alignItems: 'center', gap: 8 }}>
          <span style={{ fontSize: '0.8rem', color: 'var(--color-danger)' }}>
            Failed to estimate.
          </span>
          <button type="button" onClick={run} style={reEstimateStyle}>
            ↺ Retry
          </button>
        </div>
      )}

      <style>{`
        @keyframes spin { to { transform: rotate(360deg); } }
      `}</style>
    </div>
  );
}
