import { useState, useRef, useEffect } from 'react';
import { useAllTasks } from '@/features/tasks/hooks/useTasks';
import { useAllDependencies } from '@/features/tasks/hooks/useDependencies';
import { aiService, type CopilotAnswer } from '@/services/aiService';

interface ChatMessage {
  role: 'user' | 'assistant';
  text: string;
  referencedTaskIds?: string[];
  error?: boolean;
}

const SUGGESTIONS = [
  'What tasks are currently blocked?',
  'Which high-priority tasks are overdue?',
  'Summarize what is in progress',
  'What is the overall project health?',
  'Which tasks have no due date?',
];

export function CopilotPage() {
  const { data: tasks = [], isLoading } = useAllTasks();
  const { data: deps = [] } = useAllDependencies();
  const [messages, setMessages] = useState<ChatMessage[]>([]);
  const [input, setInput] = useState('');
  const [thinking, setThinking] = useState(false);
  const bottomRef = useRef<HTMLDivElement>(null);

  useEffect(() => {
    bottomRef.current?.scrollIntoView({ behavior: 'smooth' });
  }, [messages, thinking]);

  const statusById = new Map(tasks.map(t => [t.id, t.status]));
  const blockerCount = new Map<string, number>();
  for (const d of deps) {
    if (statusById.get(d.blockedByTaskId) !== 'Done') {
      blockerCount.set(d.taskId, (blockerCount.get(d.taskId) ?? 0) + 1);
    }
  }

  const buildHistory = (msgs: ChatMessage[]) =>
    msgs.map(m => `${m.role === 'user' ? 'Q' : 'A'}: ${m.text}`);

  const ask = async (question: string) => {
    if (!question.trim() || thinking) return;
    const userMsg: ChatMessage = { role: 'user', text: question };
    const next = [...messages, userMsg];
    setMessages(next);
    setInput('');
    setThinking(true);

    try {
      const contexts = tasks.map(t => ({
        id: t.id,
        title: t.title,
        description: t.description ?? null,
        status: t.status,
        priority: t.priority,
        dueDate: t.dueDate ?? null,
        openBlockerCount: blockerCount.get(t.id) ?? 0,
        recentComments: [],
      }));

      const answer: CopilotAnswer = await aiService.askCopilot(
        question,
        contexts,
        buildHistory(messages),
      );

      setMessages(prev => [...prev, {
        role: 'assistant',
        text: answer.answer,
        referencedTaskIds: answer.referencedTaskIds,
      }]);
    } catch {
      setMessages(prev => [...prev, {
        role: 'assistant',
        text: 'Sorry, I could not process your question. Check that the AI service is configured.',
        error: true,
      }]);
    } finally {
      setThinking(false);
    }
  };

  const taskById = new Map(tasks.map(t => [t.id, t]));

  return (
    <div style={{ display: 'flex', flexDirection: 'column', height: '100%', maxHeight: 'calc(100vh - 120px)' }}>
      <div className="page-header" style={{ flexShrink: 0 }}>
        <div>
          <h1 className="page-title">AI Copilot 🤖</h1>
          <p className="page-subtitle">
            Ask anything about your project — status, blockers, priorities, or progress. AI answers using live task data.
          </p>
        </div>
        {messages.length > 0 && (
          <button className="tf-btn" onClick={() => setMessages([])}>
            🗑 Clear chat
          </button>
        )}
      </div>

      {/* Chat area */}
      <div style={{
        flex: 1, overflowY: 'auto', display: 'flex', flexDirection: 'column', gap: 12,
        padding: '4px 0 12px',
      }}>
        {messages.length === 0 && !isLoading && (
          <div style={{
            padding: '32px 20px', textAlign: 'center',
            background: '#ffffff', border: '1px solid var(--border-color)',
            borderRadius: 'var(--radius-md)', boxShadow: 'var(--shadow-sm)',
          }}>
            <div style={{ fontSize: 36, marginBottom: 10 }}>🤖</div>
            <p style={{ margin: '0 0 6px', fontWeight: 600, fontSize: 14 }}>Ask me anything about your project</p>
            <p style={{ margin: '0 0 20px', fontSize: 13, color: 'var(--text-muted)' }}>
              I have context over {tasks.length} tasks and can answer questions about status, blockers, priority, and progress.
            </p>
            <div style={{ display: 'flex', flexWrap: 'wrap', gap: 8, justifyContent: 'center' }}>
              {SUGGESTIONS.map(s => (
                <button
                  key={s}
                  className="tf-btn"
                  style={{ fontSize: 12 }}
                  onClick={() => ask(s)}
                >
                  {s}
                </button>
              ))}
            </div>
          </div>
        )}

        {messages.map((msg, i) => (
          <div key={i} style={{
            display: 'flex',
            flexDirection: msg.role === 'user' ? 'row-reverse' : 'row',
            gap: 10, alignItems: 'flex-start',
          }}>
            <div style={{
              width: 28, height: 28, borderRadius: '50%', flexShrink: 0,
              background: msg.role === 'user' ? 'var(--color-primary)' : '#1e293b',
              display: 'flex', alignItems: 'center', justifyContent: 'center',
              fontSize: 13, color: '#fff', fontWeight: 700,
            }}>
              {msg.role === 'user' ? 'U' : '🤖'}
            </div>
            <div style={{ maxWidth: '80%' }}>
              <div style={{
                padding: '10px 14px',
                background: msg.role === 'user' ? 'var(--color-primary)' : '#ffffff',
                color: msg.role === 'user' ? '#fff' : msg.error ? '#b91c1c' : 'var(--text-primary)',
                border: msg.role === 'user' ? 'none' : '1px solid var(--border-color)',
                borderRadius: 'var(--radius-md)',
                boxShadow: msg.role === 'assistant' ? 'var(--shadow-sm)' : 'none',
                fontSize: 13, lineHeight: 1.6, whiteSpace: 'pre-wrap',
              }}>
                {msg.text}
              </div>
              {msg.referencedTaskIds && msg.referencedTaskIds.length > 0 && (
                <div style={{ marginTop: 6, display: 'flex', flexWrap: 'wrap', gap: 6 }}>
                  {msg.referencedTaskIds.map(id => {
                    const t = taskById.get(id);
                    if (!t) return null;
                    return (
                      <span key={id} style={{
                        padding: '2px 8px', borderRadius: 10, fontSize: 11,
                        background: 'var(--surface-bg)', border: '1px solid var(--border-color)',
                        color: 'var(--text-secondary)',
                      }}>
                        📌 {t.title}
                      </span>
                    );
                  })}
                </div>
              )}
            </div>
          </div>
        ))}

        {thinking && (
          <div style={{ display: 'flex', gap: 10, alignItems: 'center' }}>
            <div style={{
              width: 28, height: 28, borderRadius: '50%',
              background: '#1e293b', display: 'flex', alignItems: 'center', justifyContent: 'center', fontSize: 13,
            }}>🤖</div>
            <div style={{
              padding: '10px 14px', background: '#ffffff',
              border: '1px solid var(--border-color)', borderRadius: 'var(--radius-md)',
              boxShadow: 'var(--shadow-sm)', fontSize: 13, color: 'var(--text-muted)',
            }}>
              Thinking…
            </div>
          </div>
        )}

        <div ref={bottomRef} />
      </div>

      {/* Input bar */}
      <div style={{
        flexShrink: 0, display: 'flex', gap: 8, paddingTop: 12,
        borderTop: '1px solid var(--border-color)',
      }}>
        <input
          className="tf-input"
          style={{ flex: 1, fontSize: 13 }}
          placeholder="Ask about your project… e.g. 'What's blocking the login redesign?'"
          value={input}
          onChange={e => setInput(e.target.value)}
          onKeyDown={e => { if (e.key === 'Enter' && !e.shiftKey) { e.preventDefault(); ask(input); } }}
          disabled={thinking || isLoading}
        />
        <button
          className="tf-btn tf-btn-primary"
          onClick={() => ask(input)}
          disabled={thinking || !input.trim() || isLoading}
        >
          {thinking ? '⏳' : '↑ Ask'}
        </button>
      </div>
    </div>
  );
}
