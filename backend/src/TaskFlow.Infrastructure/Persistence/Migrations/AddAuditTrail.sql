CREATE TABLE IF NOT EXISTS audit_entries (
    id UUID PRIMARY KEY,
    actor_id UUID NOT NULL,
    entity_type TEXT NOT NULL,
    entity_id UUID NOT NULL,
    action TEXT NOT NULL,
    changes TEXT,
    occurred_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);
CREATE INDEX IF NOT EXISTS ix_audit_entries_entity ON audit_entries(entity_type, entity_id);
CREATE INDEX IF NOT EXISTS ix_audit_entries_actor ON audit_entries(actor_id);
CREATE INDEX IF NOT EXISTS ix_audit_entries_recent ON audit_entries(occurred_at DESC);
