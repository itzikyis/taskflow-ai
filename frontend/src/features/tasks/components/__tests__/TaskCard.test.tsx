import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { TaskCard } from '../TaskCard';
import type { Task } from '../../types/task.types';

const makeTask = (overrides: Partial<Task> = {}): Task => ({
  id: 'abc-123',
  title: 'Test task',
  description: null,
  status: 'Todo',
  priority: 'Medium',
  dueDate: null,
  assignedToUserId: null,
  createdByUserId: 'user-1',
  createdAt: new Date().toISOString(),
  updatedAt: null,
  ...overrides,
});

describe('TaskCard', () => {
  it('renders the task title', () => {
    render(<TaskCard task={makeTask()} onDelete={vi.fn()} />);
    expect(screen.getByText('Test task')).toBeInTheDocument();
  });

  it('renders the description when present', () => {
    render(<TaskCard task={makeTask({ description: 'Some detail' })} onDelete={vi.fn()} />);
    expect(screen.getByText('Some detail')).toBeInTheDocument();
  });

  it('calls onDelete when the delete button is clicked', async () => {
    const onDelete = vi.fn();
    render(<TaskCard task={makeTask()} onDelete={onDelete} />);
    await userEvent.click(screen.getByRole('button', { name: /delete task test task/i }));
    expect(onDelete).toHaveBeenCalledTimes(1);
  });
});
