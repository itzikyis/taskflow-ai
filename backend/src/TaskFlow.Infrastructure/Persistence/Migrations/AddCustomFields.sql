CREATE TABLE IF NOT EXISTS custom_fields (
    id UUID NOT NULL PRIMARY KEY,
    project_id UUID NOT NULL,
    name VARCHAR(100) NOT NULL,
    field_type VARCHAR(20) NOT NULL DEFAULT 'Text',
    options_json VARCHAR(2000) NOT NULL DEFAULT '',
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);
CREATE INDEX IF NOT EXISTS ix_custom_fields_project_id ON custom_fields (project_id);

CREATE TABLE IF NOT EXISTS custom_field_values (
    id UUID NOT NULL PRIMARY KEY,
    task_id UUID NOT NULL,
    custom_field_id UUID NOT NULL,
    value VARCHAR(1000) NOT NULL DEFAULT '',
    UNIQUE (task_id, custom_field_id)
);
CREATE INDEX IF NOT EXISTS ix_custom_field_values_task_id ON custom_field_values (task_id);
