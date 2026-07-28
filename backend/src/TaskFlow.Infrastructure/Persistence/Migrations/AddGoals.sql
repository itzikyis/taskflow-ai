CREATE TABLE IF NOT EXISTS goals (
    id UUID PRIMARY KEY,
    project_id UUID NOT NULL,
    owner_id UUID NOT NULL,
    title VARCHAR(200) NOT NULL,
    description TEXT,
    status VARCHAR(50) NOT NULL DEFAULT 'OnTrack',
    progress_percent INT NOT NULL DEFAULT 0,
    due_date TIMESTAMPTZ,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE TABLE IF NOT EXISTS key_results (
    id UUID PRIMARY KEY,
    goal_id UUID NOT NULL REFERENCES goals(id) ON DELETE CASCADE,
    title VARCHAR(200) NOT NULL,
    target_value NUMERIC NOT NULL DEFAULT 100,
    current_value NUMERIC NOT NULL DEFAULT 0,
    unit VARCHAR(50) NOT NULL DEFAULT '%',
    linked_task_ids TEXT
);
