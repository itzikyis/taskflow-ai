CREATE TABLE IF NOT EXISTS task_dependencies (
    id UUID PRIMARY KEY,
    task_id UUID NOT NULL,
    blocked_by_task_id UUID NOT NULL,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE INDEX IF NOT EXISTS ix_task_dependencies_task ON task_dependencies(task_id);
CREATE INDEX IF NOT EXISTS ix_task_dependencies_blocked_by ON task_dependencies(blocked_by_task_id);
CREATE UNIQUE INDEX IF NOT EXISTS ux_task_dependencies_edge ON task_dependencies(task_id, blocked_by_task_id);
