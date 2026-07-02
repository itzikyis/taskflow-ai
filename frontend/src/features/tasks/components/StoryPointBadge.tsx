import type { CSSProperties } from 'react';

interface StoryPointBadgeProps {
  points: number;
}

const LOW_POINTS = [1, 2] as const;
const MED_POINTS = [3, 5] as const;

function badgeColors(points: number): { color: string; bg: string; border: string } {
  if ((LOW_POINTS as readonly number[]).includes(points)) {
    return { color: '#065f46', bg: '#d1fae5', border: '#6ee7b7' };
  }
  if ((MED_POINTS as readonly number[]).includes(points)) {
    return { color: '#78350f', bg: '#fef3c7', border: '#fcd34d' };
  }
  // 8, 13 — red/orange
  return { color: '#7f1d1d', bg: '#fee2e2', border: '#fca5a5' };
}

export function StoryPointBadge({ points }: StoryPointBadgeProps) {
  const { color, bg, border } = badgeColors(points);

  const style: CSSProperties = {
    display: 'inline-flex',
    alignItems: 'center',
    justifyContent: 'center',
    width: 28,
    height: 28,
    borderRadius: '50%',
    background: bg,
    border: `2px solid ${border}`,
    color,
    fontSize: 12,
    fontWeight: 700,
    flexShrink: 0,
    lineHeight: 1,
  };

  return (
    <span style={style} title={`${points} story point${points === 1 ? '' : 's'}`}>
      {points}
    </span>
  );
}
