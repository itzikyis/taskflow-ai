import { useState, useRef, ChangeEvent } from 'react';
import { useCreateTask } from '@/features/tasks/hooks/useTasks';
import type { CreateTaskPayload } from '@/features/tasks/types/task.types';

type ImportSource = 'Jira' | 'Trello' | 'Asana' | 'CSV';
type Step = 1 | 2 | 3;

interface ParsedTask {
  title: string;
  description?: string;
  priority?: string;
  dueDate?: string;
}

const SOURCES: { id: ImportSource; icon: string; label: string; hint: string }[] = [
  { id: 'Jira',   icon: '🔵', label: 'Jira',   hint: 'Paste exported CSV/JSON from Jira' },
  { id: 'Trello', icon: '📌', label: 'Trello', hint: 'Paste exported CSV/JSON from Trello' },
  { id: 'Asana',  icon: '🟢', label: 'Asana',  hint: 'Paste exported CSV/JSON from Asana' },
  { id: 'CSV',    icon: '📄', label: 'CSV',    hint: 'Paste a generic CSV with headers' },
];

const PRIORITY_MAP: Record<string, string> = {
  p1: 'Critical', critical: 'Critical',
  p2: 'High',     high: 'High',
  p3: 'Medium',   medium: 'Medium',
  p4: 'Low',      low: 'Low',
};

function normalisePriority(raw: string): string {
  return PRIORITY_MAP[raw.trim().toLowerCase()] ?? 'Medium';
}

function normaliseDueDate(raw: string): string | undefined {
  if (!raw.trim()) return undefined;
  const d = new Date(raw.trim());
  return isNaN(d.getTime()) ? undefined : d.toISOString();
}

function parseCSV(text: string): ParsedTask[] {
  const lines = text.split(/\r?\n/).filter(l => l.trim().length > 0);
  if (lines.length < 2) return [];

  const parseRow = (line: string): string[] => {
    const result: string[] = [];
    let current = '';
    let inQuotes = false;
    for (let i = 0; i < line.length; i++) {
      const ch = line[i];
      if (ch === '"') {
        if (inQuotes && line[i + 1] === '"') { current += '"'; i++; }
        else { inQuotes = !inQuotes; }
      } else if (ch === ',' && !inQuotes) {
        result.push(current); current = '';
      } else {
        current += ch;
      }
    }
    result.push(current);
    return result;
  };

  const headers = parseRow(lines[0]).map(h => h.trim().toLowerCase());

  const colIndex = (candidates: string[]): number => {
    for (const c of candidates) {
      const idx = headers.indexOf(c);
      if (idx !== -1) return idx;
    }
    return -1;
  };

  const titleIdx    = colIndex(['title', 'summary', 'name', 'task']);
  const descIdx     = colIndex(['description', 'desc', 'body', 'details']);
  const priorityIdx = colIndex(['priority']);
  const dueDateIdx  = colIndex(['duedate', 'due_date', 'due date', 'due']);

  if (titleIdx === -1) return [];

  const tasks: ParsedTask[] = [];
  for (let i = 1; i < lines.length; i++) {
    const cols = parseRow(lines[i]);
    const title = cols[titleIdx]?.trim();
    if (!title) continue;
    tasks.push({
      title,
      description: descIdx !== -1 ? cols[descIdx]?.trim() || undefined : undefined,
      priority:    priorityIdx !== -1 ? normalisePriority(cols[priorityIdx] ?? '') : undefined,
      dueDate:     dueDateIdx  !== -1 ? normaliseDueDate(cols[dueDateIdx] ?? '')   : undefined,
    });
  }
  return tasks;
}

const CARD_STYLE: React.CSSProperties = {
  background: 'var(--surface-card)',
  border: '1px solid var(--surface-border)',
  borderRadius: 'var(--radius-md)',
  padding: 24,
};

const BTN_PRIMARY: React.CSSProperties = {
  background: 'var(--color-primary)',
  color: '#fff',
  border: 'none',
  borderRadius: 'var(--radius-md)',
  padding: '8px 20px',
  cursor: 'pointer',
  fontFamily: 'inherit',
  fontSize: 14,
  fontWeight: 600,
};

const BTN_SECONDARY: React.CSSProperties = {
  background: 'transparent',
  color: 'var(--text-secondary)',
  border: '1px solid var(--surface-border)',
  borderRadius: 'var(--radius-md)',
  padding: '8px 20px',
  cursor: 'pointer',
  fontFamily: 'inherit',
  fontSize: 14,
};

export function ImportPage() {
  const [step, setStep]           = useState<Step>(1);
  const [source, setSource]       = useState<ImportSource | null>(null);
  const [rawText, setRawText]     = useState('');
  const [parseError, setParseError] = useState('');
  const [parsed, setParsed]       = useState<ParsedTask[]>([]);
  const [selected, setSelected]   = useState<boolean[]>([]);
  const [progress, setProgress]   = useState<number>(0);
  const [done, setDone]           = useState(false);
  const fileRef = useRef<HTMLInputElement>(null);

  const createTask = useCreateTask();

  const CSV_PLACEHOLDER =
    'title,description,priority,dueDate\nFix login bug,Users cannot log in on mobile,High,2026-09-01\nUpdate onboarding flow,,Medium,';

  const TOOL_PLACEHOLDER = (src: ImportSource) =>
    `Paste exported CSV or JSON from ${src} here.\n\nExpected CSV format:\ntitle,description,priority,dueDate\n"My Issue","Description here","High","2026-09-01"`;

  function handleSourceSelect(s: ImportSource) {
    setSource(s);
    setRawText('');
    setParseError('');
    setStep(2);
  }

  function handleFileChange(e: ChangeEvent<HTMLInputElement>) {
    const file = e.target.files?.[0];
    if (!file) return;
    const reader = new FileReader();
    reader.onload = ev => setRawText((ev.target?.result as string) ?? '');
    reader.readAsText(file);
  }

  function handleParse() {
    setParseError('');
    const tasks = parseCSV(rawText);
    if (tasks.length === 0) {
      setParseError('No valid tasks found. Make sure the first row is a header row containing at least a "title" column.');
      return;
    }
    setParsed(tasks);
    setSelected(tasks.map(() => true));
    setProgress(0);
    setDone(false);
    setStep(3);
  }

  function toggleRow(i: number) {
    setSelected(prev => prev.map((v, idx) => idx === i ? !v : v));
  }

  function toggleAll() {
    const allChecked = selected.every(Boolean);
    setSelected(selected.map(() => !allChecked));
  }

  async function handleImport() {
    const rows = parsed.filter((_, i) => selected[i]);
    setProgress(0);
    setDone(false);
    for (let i = 0; i < rows.length; i++) {
      const row = rows[i];
      const payload: CreateTaskPayload = {
        title: row.title,
        ...(row.description ? { description: row.description } : {}),
        ...(row.priority    ? { priority: row.priority }       : {}),
        ...(row.dueDate     ? { dueDate: row.dueDate }         : {}),
      };
      await createTask.mutateAsync(payload);
      setProgress(i + 1);
    }
    setDone(true);
  }

  const selectedCount = selected.filter(Boolean).length;

  return (
    <div style={{ maxWidth: 800, margin: '0 auto', padding: '24px 16px' }}>
      {/* Step indicators */}
      <div style={{ display: 'flex', gap: 8, marginBottom: 28, alignItems: 'center' }}>
        {([1, 2, 3] as Step[]).map(s => (
          <div key={s} style={{ display: 'flex', alignItems: 'center', gap: 8 }}>
            <div style={{
              width: 28, height: 28, borderRadius: '50%',
              background: step >= s ? 'var(--color-primary)' : 'var(--surface-border)',
              color: step >= s ? '#fff' : 'var(--text-muted)',
              display: 'flex', alignItems: 'center', justifyContent: 'center',
              fontSize: 13, fontWeight: 700,
            }}>{s}</div>
            <span style={{ fontSize: 13, color: step === s ? 'var(--text-primary)' : 'var(--text-muted)', fontWeight: step === s ? 600 : 400 }}>
              {s === 1 ? 'Choose source' : s === 2 ? 'Paste data' : 'Preview & import'}
            </span>
            {s < 3 && <span style={{ color: 'var(--text-muted)', marginLeft: 4 }}>›</span>}
          </div>
        ))}
      </div>

      {/* Step 1 — source selection */}
      {step === 1 && (
        <div>
          <h2 style={{ fontSize: 18, fontWeight: 700, color: 'var(--text-primary)', marginBottom: 6 }}>
            Choose your import source
          </h2>
          <p style={{ color: 'var(--text-secondary)', fontSize: 14, marginBottom: 20 }}>
            Select the tool you are migrating from.
          </p>
          <div style={{ display: 'grid', gridTemplateColumns: 'repeat(2, 1fr)', gap: 16 }}>
            {SOURCES.map(s => (
              <button
                key={s.id}
                type="button"
                onClick={() => handleSourceSelect(s.id)}
                style={{
                  ...CARD_STYLE,
                  cursor: 'pointer',
                  textAlign: 'left',
                  display: 'flex',
                  flexDirection: 'column',
                  gap: 8,
                  transition: 'border-color 0.15s',
                }}
                onMouseEnter={e => (e.currentTarget.style.borderColor = 'var(--color-primary)')}
                onMouseLeave={e => (e.currentTarget.style.borderColor = 'var(--surface-border)')}
              >
                <span style={{ fontSize: 32 }}>{s.icon}</span>
                <span style={{ fontSize: 16, fontWeight: 700, color: 'var(--text-primary)' }}>{s.label}</span>
                <span style={{ fontSize: 13, color: 'var(--text-secondary)' }}>{s.hint}</span>
              </button>
            ))}
          </div>
        </div>
      )}

      {/* Step 2 — paste / upload */}
      {step === 2 && source && (
        <div style={CARD_STYLE}>
          <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: 16 }}>
            <h2 style={{ fontSize: 18, fontWeight: 700, color: 'var(--text-primary)' }}>
              {source === 'CSV' ? 'Paste or upload your CSV' : `Paste exported data from ${source}`}
            </h2>
            <button type="button" style={BTN_SECONDARY} onClick={() => setStep(1)}>← Back</button>
          </div>

          {source === 'CSV' && (
            <div style={{ marginBottom: 12 }}>
              <label
                htmlFor="csv-file-upload"
                style={{ fontSize: 13, color: 'var(--text-secondary)', display: 'block', marginBottom: 6 }}
              >
                Upload a .csv file (optional):
              </label>
              <input
                id="csv-file-upload"
                ref={fileRef}
                type="file"
                accept=".csv"
                onChange={handleFileChange}
                style={{ fontSize: 13, color: 'var(--text-secondary)' }}
              />
            </div>
          )}

          <label
            htmlFor="import-textarea"
            style={{ fontSize: 13, color: 'var(--text-secondary)', display: 'block', marginBottom: 6 }}
          >
            {source === 'CSV'
              ? 'Or paste CSV content directly:'
              : `Paste your exported ${source} data (CSV format):`}
          </label>
          <textarea
            id="import-textarea"
            value={rawText}
            onChange={e => setRawText(e.target.value)}
            placeholder={source === 'CSV' ? CSV_PLACEHOLDER : TOOL_PLACEHOLDER(source)}
            rows={12}
            style={{
              width: '100%',
              boxSizing: 'border-box',
              background: 'var(--surface-bg)',
              border: '1px solid var(--surface-border)',
              borderRadius: 'var(--radius-md)',
              color: 'var(--text-primary)',
              fontFamily: 'monospace',
              fontSize: 13,
              padding: 12,
              resize: 'vertical',
            }}
          />

          {parseError && (
            <p style={{ color: 'var(--color-danger)', fontSize: 13, marginTop: 8 }}>{parseError}</p>
          )}

          <div style={{ marginTop: 16, display: 'flex', gap: 10 }}>
            <button type="button" style={BTN_PRIMARY} onClick={handleParse} disabled={!rawText.trim()}>
              Parse →
            </button>
          </div>
        </div>
      )}

      {/* Step 3 — preview & import */}
      {step === 3 && (
        <div>
          <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: 16 }}>
            <h2 style={{ fontSize: 18, fontWeight: 700, color: 'var(--text-primary)' }}>
              Preview — {parsed.length} tasks found
            </h2>
            <button type="button" style={BTN_SECONDARY} onClick={() => setStep(2)}>← Back</button>
          </div>

          {done ? (
            <div style={{ ...CARD_STYLE, textAlign: 'center', padding: 40 }}>
              <p style={{ fontSize: 18, color: 'var(--text-primary)', fontWeight: 600 }}>
                ✅ {selectedCount} task{selectedCount !== 1 ? 's' : ''} imported successfully!
              </p>
              <button
                type="button"
                style={{ ...BTN_SECONDARY, marginTop: 20 }}
                onClick={() => { setStep(1); setSource(null); setRawText(''); setParsed([]); setDone(false); }}
              >
                Import more
              </button>
            </div>
          ) : (
            <div style={CARD_STYLE}>
              <div style={{ overflowX: 'auto' }}>
                <table style={{ width: '100%', borderCollapse: 'collapse', fontSize: 13 }}>
                  <thead>
                    <tr style={{ borderBottom: '1px solid var(--surface-border)' }}>
                      <th style={{ padding: '8px 10px', textAlign: 'left', color: 'var(--text-muted)', fontWeight: 600 }}>
                        <input
                          type="checkbox"
                          checked={selected.every(Boolean)}
                          onChange={toggleAll}
                          aria-label="Select all tasks"
                        />
                      </th>
                      <th style={{ padding: '8px 10px', textAlign: 'left', color: 'var(--text-muted)', fontWeight: 600 }}>Title</th>
                      <th style={{ padding: '8px 10px', textAlign: 'left', color: 'var(--text-muted)', fontWeight: 600 }}>Description</th>
                      <th style={{ padding: '8px 10px', textAlign: 'left', color: 'var(--text-muted)', fontWeight: 600 }}>Priority</th>
                      <th style={{ padding: '8px 10px', textAlign: 'left', color: 'var(--text-muted)', fontWeight: 600 }}>Due Date</th>
                    </tr>
                  </thead>
                  <tbody>
                    {parsed.map((task, i) => (
                      <tr
                        key={i}
                        style={{
                          borderBottom: '1px solid var(--surface-border)',
                          opacity: selected[i] ? 1 : 0.4,
                        }}
                      >
                        <td style={{ padding: '8px 10px' }}>
                          <input
                            type="checkbox"
                            checked={selected[i] ?? false}
                            onChange={() => toggleRow(i)}
                            aria-label={`Select task: ${task.title}`}
                          />
                        </td>
                        <td style={{ padding: '8px 10px', color: 'var(--text-primary)', fontWeight: 500 }}>{task.title}</td>
                        <td style={{ padding: '8px 10px', color: 'var(--text-secondary)', maxWidth: 220, overflow: 'hidden', textOverflow: 'ellipsis', whiteSpace: 'nowrap' }}>
                          {task.description ?? <span style={{ color: 'var(--text-muted)' }}>—</span>}
                        </td>
                        <td style={{ padding: '8px 10px', color: 'var(--text-secondary)' }}>{task.priority ?? 'Medium'}</td>
                        <td style={{ padding: '8px 10px', color: 'var(--text-secondary)' }}>
                          {task.dueDate ? new Date(task.dueDate).toLocaleDateString() : <span style={{ color: 'var(--text-muted)' }}>—</span>}
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>

              <div style={{ marginTop: 16, display: 'flex', alignItems: 'center', gap: 16 }}>
                <button
                  type="button"
                  style={{ ...BTN_PRIMARY, opacity: selectedCount === 0 || createTask.isPending ? 0.6 : 1 }}
                  onClick={handleImport}
                  disabled={selectedCount === 0 || createTask.isPending}
                >
                  {createTask.isPending
                    ? `Importing ${progress}/${selectedCount} tasks…`
                    : `Import ${selectedCount} task${selectedCount !== 1 ? 's' : ''}`}
                </button>
                {createTask.isPending && (
                  <span style={{ fontSize: 13, color: 'var(--text-secondary)' }}>
                    Importing {progress} of {selectedCount} tasks…
                  </span>
                )}
              </div>
            </div>
          )}
        </div>
      )}
    </div>
  );
}
