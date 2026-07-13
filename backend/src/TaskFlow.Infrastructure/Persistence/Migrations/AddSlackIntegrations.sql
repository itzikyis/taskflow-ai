CREATE TABLE IF NOT EXISTS slack_integrations (
    id UUID PRIMARY KEY,
    webhook_url TEXT NOT NULL,
    enabled BOOLEAN NOT NULL DEFAULT TRUE,
    forward_created BOOLEAN NOT NULL DEFAULT TRUE,
    forward_status_changed BOOLEAN NOT NULL DEFAULT TRUE,
    forward_comments BOOLEAN NOT NULL DEFAULT TRUE,
    forward_other BOOLEAN NOT NULL DEFAULT FALSE,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ
);
