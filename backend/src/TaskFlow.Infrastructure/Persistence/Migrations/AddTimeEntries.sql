CREATE TABLE IF NOT EXISTS time_entries (
    id UUID PRIMARY KEY,
    task_id UUID NOT NULL,
    user_id UUID NOT NULL,
    minutes INTEGER NOT NULL,
    note TEXT,
    logged_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE INDEX IF NOT EXISTS ix_time_entries_task ON time_entries(task_id);
CREATE INDEX IF NOT EXISTS ix_time_entries_user ON time_entries(user_id);
