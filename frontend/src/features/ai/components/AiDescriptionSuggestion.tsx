import { useState } from 'react';
import { useSuggestDescription } from '../hooks/useAi';

interface AiDescriptionSuggestionProps {
  taskTitle: string;
  onAccept: (suggestion: string) => void;
}

export function AiDescriptionSuggestion({ taskTitle, onAccept }: AiDescriptionSuggestionProps) {
  const suggest = useSuggestDescription();
  const [shown, setShown] = useState(false);

  const handleSuggest = () => {
    if (!taskTitle.trim()) return;
    setShown(true);
    suggest.mutate(taskTitle);
  };

  return (
    <div style={{ marginTop: '0.5rem' }}>
      <button
        type="button"
        onClick={handleSuggest}
        disabled={suggest.isPending || !taskTitle.trim()}
        style={{
          padding: '0.3rem 0.75rem',
          fontSize: '0.8rem',
          borderRadius: 4,
          border: '1px solid #7c3aed',
          background: 'none',
          color: '#7c3aed',
          cursor: 'pointer',
        }}
      >
        {suggest.isPending ? '✨ Thinking…' : '✨ AI: Suggest description'}
      </button>

      {shown && suggest.data && (
        <div
          style={{
            marginTop: '0.5rem',
            padding: '0.75rem',
            background: '#f5f0ff',
            borderRadius: 6,
            border: '1px solid #d8b4fe',
            fontSize: '0.875rem',
          }}
        >
          <p style={{ margin: '0 0 0.5rem', color: '#4c1d95', fontStyle: 'italic' }}>{suggest.data.suggestion}</p>
          <button
            type="button"
            onClick={() => { onAccept(suggest.data!.suggestion); setShown(false); }}
            style={{
              padding: '0.25rem 0.6rem',
              fontSize: '0.75rem',
              borderRadius: 4,
              border: 'none',
              background: '#7c3aed',
              color: '#fff',
              cursor: 'pointer',
            }}
          >
            Use this
          </button>
          <button
            type="button"
            onClick={() => setShown(false)}
            style={{
              marginLeft: '0.4rem',
              padding: '0.25rem 0.6rem',
              fontSize: '0.75rem',
              borderRadius: 4,
              border: '1px solid #ccc',
              background: 'none',
              cursor: 'pointer',
            }}
          >
            Dismiss
          </button>
        </div>
      )}
    </div>
  );
}
