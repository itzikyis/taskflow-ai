-- Cross-project initiatives for roadmap view
CREATE TABLE IF NOT EXISTS initiatives (
    id                  UUID            NOT NULL PRIMARY KEY,
    name                VARCHAR(200)    NOT NULL,
    description         VARCHAR(2000)   NOT NULL DEFAULT '',
    status              INTEGER         NOT NULL DEFAULT 0,   -- InitiativeStatus enum
    priority            INTEGER         NOT NULL DEFAULT 1,   -- InitiativePriority enum
    labels              VARCHAR(500)    NOT NULL DEFAULT '',
    project_ids         VARCHAR(4000)   NOT NULL DEFAULT '',  -- pipe-delimited GUIDs
    start_date          TIMESTAMPTZ,
    target_date         TIMESTAMPTZ,
    created_by_user_id  UUID            NOT NULL,
    created_at          TIMESTAMPTZ     NOT NULL DEFAULT NOW(),
    updated_at          TIMESTAMPTZ     NOT NULL DEFAULT NOW()
);
