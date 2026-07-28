CREATE TABLE IF NOT EXISTS task_templates (
    id                     UUID         NOT NULL PRIMARY KEY,
    project_id             UUID         NOT NULL,
    name                   VARCHAR(200) NOT NULL,
    default_title          VARCHAR(200) NOT NULL,
    default_description    VARCHAR(2000),
    default_priority       VARCHAR(20),
    default_estimated_hours INT,
    created_at             TIMESTAMPTZ  NOT NULL
);

CREATE INDEX IF NOT EXISTS ix_task_templates_project_id ON task_templates (project_id);
