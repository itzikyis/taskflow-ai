import { useTasks, useDeleteTask } from '../hooks/useTasks';
import { TaskCard } from './TaskCard';
import { CreateTaskForm } from './CreateTaskForm';

export function TaskListPage() {
  const { data: tasks, isLoading, isError } = useTasks();
  const deleteMutation = useDeleteTask();

  if (isLoading) return <p>Loading tasks…</p>;
  if (isError) return <p role="alert">Failed to load tasks.</p>;

  return (
    <section>
      <h2>Tasks</h2>
      <CreateTaskForm />
      {tasks?.length === 0 && <p>No tasks yet. Create one above!</p>}
      <ul style={{ listStyle: 'none', padding: 0 }}>
        {tasks?.map((task) => (
          <li key={task.id}>
            <TaskCard task={task} onDelete={() => deleteMutation.mutate(task.id)} />
          </li>
        ))}
      </ul>
    </section>
  );
}
