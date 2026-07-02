CREATE TABLE IF NOT EXISTS activity_logs (
    id UUID PRIMARY KEY,
    actor_id UUID NOT NULL,
    action INTEGER NOT NULL,
    entity_type VARCHAR(100) NOT NULL,
    entity_id UUID NOT NULL,
    entity_name VARCHAR(200) NULL,
    project_id UUID NULL,
    metadata TEXT NULL,
    occurred_at TIMESTAMP NOT NULL
);
CREATE INDEX IF NOT EXISTS ix_activity_logs_entity ON activity_logs(entity_type, entity_id, occurred_at DESC);
CREATE INDEX IF NOT EXISTS ix_activity_logs_project ON activity_logs(project_id, occurred_at DESC);
CREATE INDEX IF NOT EXISTS ix_activity_logs_actor ON activity_logs(actor_id, occurred_at DESC);
CREATE INDEX IF NOT EXISTS ix_activity_logs_recent ON activity_logs(occurred_at DESC);
