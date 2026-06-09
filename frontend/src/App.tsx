import { TaskListPage } from '@/features/tasks/components/TaskListPage';

export default function App() {
  return (
    <main style={{ fontFamily: 'system-ui, sans-serif', padding: '2rem' }}>
      <h1>TaskFlow AI</h1>
      <TaskListPage />
    </main>
  );
}
