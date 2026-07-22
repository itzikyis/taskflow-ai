-- Migration: AddCalendarSubscriptions
-- Adds the calendar_subscriptions table for two-way iCal sync.

CREATE TABLE IF NOT EXISTS calendar_subscriptions (
    id              UUID          NOT NULL PRIMARY KEY,
    project_id      UUID          NOT NULL,
    external_url    VARCHAR(2000) NOT NULL,
    display_name    VARCHAR(200)  NOT NULL,
    last_synced_at  TIMESTAMPTZ,
    created_at      TIMESTAMPTZ   NOT NULL DEFAULT NOW()
);

CREATE INDEX IF NOT EXISTS ix_calendar_subscriptions_project_id
    ON calendar_subscriptions (project_id);
