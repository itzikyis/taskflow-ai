-- Automation rules: no-code if/then workflow triggers per project
CREATE TABLE IF NOT EXISTS automation_rules (
    id              UUID        NOT NULL PRIMARY KEY,
    project_id      UUID        NOT NULL,
    name            VARCHAR(200) NOT NULL,
    is_enabled      BOOLEAN     NOT NULL DEFAULT TRUE,
    trigger_type    INTEGER     NOT NULL,
    trigger_value   VARCHAR(100) NOT NULL DEFAULT '',
    action_type     INTEGER     NOT NULL,
    action_value    VARCHAR(500) NOT NULL DEFAULT '',
    created_at      TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE INDEX IF NOT EXISTS ix_automation_rules_project_id ON automation_rules (project_id);
