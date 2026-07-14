-- Project documents: lightweight wiki/specs space per project
CREATE TABLE IF NOT EXISTS project_documents (
    id          UUID            NOT NULL PRIMARY KEY,
    project_id  UUID            NOT NULL,
    title       VARCHAR(300)    NOT NULL,
    body        TEXT            NOT NULL DEFAULT '',
    author_id   UUID            NOT NULL,
    created_at  TIMESTAMPTZ     NOT NULL DEFAULT NOW(),
    updated_at  TIMESTAMPTZ     NOT NULL DEFAULT NOW()
);

CREATE INDEX IF NOT EXISTS ix_project_documents_project_id ON project_documents (project_id);
