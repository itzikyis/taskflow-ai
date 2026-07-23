import { useState, useEffect, useRef, useCallback } from 'react';

export interface Command {
  id: string;
  icon: string;
  label: string;
  category: string;
  action: () => void;
}

interface CommandPaletteProps {
  commands: Command[];
  onClose: () => void;
}

const STYLES = `
  .cp-backdrop {
    position: fixed;
    inset: 0;
    background: rgba(0, 0, 0, 0.6);
    display: flex;
    align-items: flex-start;
    justify-content: center;
    padding-top: 15vh;
    z-index: 9999;
    backdrop-filter: blur(2px);
  }

  .cp-modal {
    background: var(--surface-card);
    border: 1px solid var(--surface-border);
    border-radius: 12px;
    width: 100%;
    max-width: 560px;
    max-height: 420px;
    display: flex;
    flex-direction: column;
    overflow: hidden;
    box-shadow: 0 24px 64px rgba(0, 0, 0, 0.5);
  }

  .cp-search-wrapper {
    display: flex;
    align-items: center;
    gap: 10px;
    padding: 14px 16px;
    border-bottom: 1px solid var(--surface-border);
  }

  .cp-search-icon {
    font-size: 16px;
    color: var(--text-secondary);
    flex-shrink: 0;
  }

  .cp-search-input {
    flex: 1;
    background: transparent;
    border: none;
    outline: none;
    font-size: 15px;
    color: var(--text-primary);
    font-family: inherit;
  }

  .cp-search-input::placeholder {
    color: var(--text-muted);
  }

  .cp-kbd {
    font-size: 11px;
    color: var(--text-secondary);
    background: var(--surface-bg);
    border: 1px solid var(--surface-border);
    border-radius: 4px;
    padding: 2px 6px;
    font-family: monospace;
    flex-shrink: 0;
  }

  .cp-results {
    overflow-y: auto;
    flex: 1;
    padding: 6px;
  }

  .cp-results::-webkit-scrollbar {
    width: 4px;
  }

  .cp-results::-webkit-scrollbar-thumb {
    background: var(--surface-border);
    border-radius: 2px;
  }

  .cp-category-label {
    font-size: 10px;
    font-weight: 700;
    letter-spacing: 0.08em;
    text-transform: uppercase;
    color: var(--text-secondary);
    padding: 8px 10px 4px;
  }

  .cp-item {
    display: flex;
    align-items: center;
    gap: 10px;
    padding: 9px 10px;
    border-radius: 7px;
    cursor: pointer;
    border: none;
    background: transparent;
    width: 100%;
    text-align: left;
    color: var(--text-primary);
    font-size: 14px;
    font-family: inherit;
    transition: background 0.08s;
  }

  .cp-item.highlighted {
    background: var(--color-primary);
    color: #fff;
  }

  .cp-item-icon {
    font-size: 16px;
    width: 22px;
    text-align: center;
    flex-shrink: 0;
  }

  .cp-item-label {
    flex: 1;
  }

  .cp-empty {
    padding: 32px 16px;
    text-align: center;
    color: var(--text-secondary);
    font-size: 14px;
  }

  .cp-footer {
    border-top: 1px solid var(--surface-border);
    padding: 8px 14px;
    display: flex;
    gap: 16px;
    align-items: center;
  }

  .cp-footer-hint {
    display: flex;
    align-items: center;
    gap: 5px;
    font-size: 11px;
    color: var(--text-secondary);
  }
`;

function fuzzyMatch(query: string, text: string): boolean {
  if (!query) return true;
  const q = query.toLowerCase();
  const t = text.toLowerCase();
  let qi = 0;
  for (let i = 0; i < t.length && qi < q.length; i++) {
    if (t[i] === q[qi]) qi++;
  }
  return qi === q.length;
}

export function CommandPalette({ commands, onClose }: CommandPaletteProps) {
  const [query, setQuery] = useState('');
  const [highlightedIndex, setHighlightedIndex] = useState(0);
  const inputRef = useRef<HTMLInputElement>(null);
  const listRef = useRef<HTMLDivElement>(null);

  const filtered = commands.filter(cmd => fuzzyMatch(query, cmd.label));

  // Reset highlight when filtered list changes
  useEffect(() => {
    setHighlightedIndex(0);
  }, [query]);

  // Focus input on mount
  useEffect(() => {
    inputRef.current?.focus();
  }, []);

  // Scroll highlighted item into view
  useEffect(() => {
    if (!listRef.current) return;
    const item = listRef.current.querySelector<HTMLElement>('.cp-item.highlighted');
    item?.scrollIntoView({ block: 'nearest' });
  }, [highlightedIndex]);

  const execute = useCallback((index: number) => {
    const cmd = filtered[index];
    if (cmd) {
      cmd.action();
      onClose();
    }
  }, [filtered, onClose]);

  const handleKeyDown = useCallback((e: React.KeyboardEvent) => {
    if (e.key === 'ArrowDown') {
      e.preventDefault();
      setHighlightedIndex(i => Math.min(i + 1, filtered.length - 1));
    } else if (e.key === 'ArrowUp') {
      e.preventDefault();
      setHighlightedIndex(i => Math.max(i - 1, 0));
    } else if (e.key === 'Enter') {
      e.preventDefault();
      execute(highlightedIndex);
    } else if (e.key === 'Escape') {
      onClose();
    }
  }, [filtered.length, highlightedIndex, execute, onClose]);

  // Group by category while preserving order
  const categories = filtered.reduce<Map<string, Command[]>>((acc, cmd) => {
    const group = acc.get(cmd.category) ?? [];
    group.push(cmd);
    acc.set(cmd.category, group);
    return acc;
  }, new Map());

  // Flat index lookup for highlighting
  const flatItems: Command[] = [];
  categories.forEach(cmds => flatItems.push(...cmds));

  return (
    <>
      <style>{STYLES}</style>
      <div
        className="cp-backdrop"
        onMouseDown={(e) => {
          if (e.target === e.currentTarget) onClose();
        }}
        role="dialog"
        aria-modal="true"
        aria-label="Command palette"
      >
        <div className="cp-modal">
          <div className="cp-search-wrapper">
            <span className="cp-search-icon" aria-hidden="true">⌕</span>
            <input
              ref={inputRef}
              className="cp-search-input"
              type="text"
              placeholder="Search commands…"
              value={query}
              onChange={e => setQuery(e.target.value)}
              onKeyDown={handleKeyDown}
              aria-label="Search commands"
              aria-autocomplete="list"
              role="combobox"
              aria-expanded="true"
            />
            <kbd className="cp-kbd">Esc</kbd>
          </div>

          <div className="cp-results" ref={listRef} role="listbox">
            {flatItems.length === 0 ? (
              <div className="cp-empty">No commands match "{query}"</div>
            ) : (
              Array.from(categories.entries()).map(([category, cmds]) => (
                <div key={category}>
                  <div className="cp-category-label">{category}</div>
                  {cmds.map(cmd => {
                    const flatIdx = flatItems.indexOf(cmd);
                    const isHighlighted = flatIdx === highlightedIndex;
                    return (
                      <button
                        key={cmd.id}
                        className={`cp-item${isHighlighted ? ' highlighted' : ''}`}
                        role="option"
                        aria-selected={isHighlighted}
                        onMouseEnter={() => setHighlightedIndex(flatIdx)}
                        onClick={() => execute(flatIdx)}
                      >
                        <span className="cp-item-icon" aria-hidden="true">{cmd.icon}</span>
                        <span className="cp-item-label">{cmd.label}</span>
                      </button>
                    );
                  })}
                </div>
              ))
            )}
          </div>

          <div className="cp-footer" aria-hidden="true">
            <span className="cp-footer-hint">
              <kbd className="cp-kbd">↑↓</kbd> navigate
            </span>
            <span className="cp-footer-hint">
              <kbd className="cp-kbd">↵</kbd> open
            </span>
            <span className="cp-footer-hint">
              <kbd className="cp-kbd">Esc</kbd> close
            </span>
          </div>
        </div>
      </div>
    </>
  );
}
