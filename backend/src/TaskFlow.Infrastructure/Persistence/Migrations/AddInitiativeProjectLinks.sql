-- Migrate initiatives from pipe-delimited project_ids string to a proper junction table.

-- 1. Create junction table
CREATE TABLE IF NOT EXISTS initiative_projects (
    initiative_id UUID NOT NULL,
    project_id    UUID NOT NULL,
    PRIMARY KEY (initiative_id, project_id),
    CONSTRAINT fk_initiative_projects_initiative
        FOREIGN KEY (initiative_id) REFERENCES initiatives(id) ON DELETE CASCADE,
    CONSTRAINT fk_initiative_projects_project
        FOREIGN KEY (project_id) REFERENCES projects(id) ON DELETE CASCADE
);

-- 2. Migrate existing pipe-delimited data into the junction table
INSERT INTO initiative_projects (initiative_id, project_id)
SELECT
    i.id AS initiative_id,
    CAST(TRIM(link) AS UUID) AS project_id
FROM
    initiatives i,
    LATERAL unnest(string_to_array(NULLIF(TRIM(i.project_ids), ''), '|')) AS link
WHERE
    TRIM(link) <> '';

-- 3. Drop the now-redundant column
ALTER TABLE initiatives DROP COLUMN IF EXISTS project_ids;
