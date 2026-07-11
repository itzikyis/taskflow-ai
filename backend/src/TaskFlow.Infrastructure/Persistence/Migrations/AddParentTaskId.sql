ALTER TABLE tasks ADD COLUMN IF NOT EXISTS parent_task_id UUID;
CREATE INDEX IF NOT EXISTS ix_tasks_parent_task_id ON tasks(parent_task_id);
