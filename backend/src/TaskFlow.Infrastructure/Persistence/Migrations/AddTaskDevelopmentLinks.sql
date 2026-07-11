CREATE TABLE IF NOT EXISTS task_development_links (
    id UUID PRIMARY KEY,
    task_id UUID NOT NULL,
    repository TEXT NOT NULL,
    ref_type TEXT NOT NULL,
    title TEXT NOT NULL,
    url TEXT NOT NULL,
    status TEXT NOT NULL,
    external_id TEXT,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ
);

CREATE INDEX IF NOT EXISTS ix_task_development_links_task ON task_development_links(task_id);
CREATE INDEX IF NOT EXISTS ix_task_development_links_extref
    ON task_development_links(task_id, repository, external_id);
