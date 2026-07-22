import { useQuery } from '@tanstack/react-query';
import { aiService } from '@/services/aiService';
import type { DashboardInsightsDto } from '@/services/aiService';

interface AiInsightsPanelProps {
  projectId: string;
}

const healthColors: Record<string, { bg: string; text: string; border: string }> = {
  Healthy: { bg: '#ecfdf5', text: '#065f46', border: '#6ee7b7' },
  'At Risk': { bg: '#fffbeb', text: '#92400e', border: '#fcd34d' },
  Critical: { bg: '#fef2f2', text: '#991b1b', border: '#fca5a5' },
};

function HealthBadge({ status }: { status: string }) {
  const colors = healthColors[status] ?? healthColors['At Risk'];
  return (
    <span
      style={{
        display: 'inline-block',
        padding: '2px 10px',
        borderRadius: 12,
        fontSize: 12,
        fontWeight: 600,
        background: colors.bg,
        color: colors.text,
        border: `1px solid ${colors.border}`,
        letterSpacing: '0.02em',
      }}
    >
      {status}
    </span>
  );
}

function InsightsSkeleton() {
  return (
    <div style={{ padding: '4px 0' }}>
      {[120, 200, 160].map((w) => (
        <div
          key={w}
          style={{
            height: 14,
            width: w,
            maxWidth: '100%',
            background: 'var(--surface-bg, #f1f5f9)',
            borderRadius: 6,
            marginBottom: 10,
            animation: 'pulse 1.5s ease-in-out infinite',
          }}
        />
      ))}
    </div>
  );
}

function InsightsContent({ data }: { data: DashboardInsightsDto }) {
  return (
    <>
      <p
        style={{
          margin: '0 0 14px',
          fontSize: 14,
          color: 'var(--text-secondary)',
          lineHeight: 1.6,
        }}
      >
        {data.narrative}
      </p>
      {data.highlights.length > 0 && (
        <ul
          style={{
            margin: 0,
            paddingLeft: 18,
            fontSize: 13,
            color: 'var(--text-secondary)',
            lineHeight: 1.7,
          }}
        >
          {data.highlights.map((h, i) => (
            <li key={i}>{h}</li>
          ))}
        </ul>
      )}
    </>
  );
}

/** Displays AI-generated narrative insights for a project dashboard. */
export function AiInsightsPanel({ projectId }: AiInsightsPanelProps) {
  const { data, isLoading, isError } = useQuery({
    queryKey: ['ai-dashboard-insights', projectId],
    queryFn: () => aiService.getDashboardInsights(projectId),
    staleTime: 5 * 60 * 1000,
    retry: 1,
  });

  return (
    <div
      style={{
        background: '#ffffff',
        border: '1px solid var(--border-color)',
        borderRadius: 'var(--radius-md)',
        boxShadow: 'var(--shadow-sm)',
        padding: 18,
        marginBottom: 24,
      }}
    >
      <div
        style={{
          display: 'flex',
          alignItems: 'center',
          gap: 10,
          marginBottom: 14,
        }}
      >
        <span style={{ fontSize: 18 }}>✨</span>
        <h3 style={{ margin: 0, fontSize: 15, fontWeight: 600 }}>AI Insights</h3>
        {data && !isLoading && <HealthBadge status={data.healthStatus} />}
      </div>

      {isLoading && <InsightsSkeleton />}

      {isError && (
        <p style={{ margin: 0, fontSize: 13, color: 'var(--text-muted)' }}>
          Could not load AI insights. Please try again later.
        </p>
      )}

      {data && !isLoading && <InsightsContent data={data} />}
    </div>
  );
}
