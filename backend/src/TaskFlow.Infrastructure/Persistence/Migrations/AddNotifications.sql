CREATE TABLE IF NOT EXISTS notifications (
    id UUID PRIMARY KEY,
    user_id UUID NOT NULL,
    title VARCHAR(200) NOT NULL,
    message VARCHAR(1000) NOT NULL,
    type INTEGER NOT NULL DEFAULT 5,
    is_read BOOLEAN NOT NULL DEFAULT FALSE,
    related_entity_id UUID NULL,
    created_at TIMESTAMP NOT NULL
);
CREATE INDEX IF NOT EXISTS ix_notifications_user_id ON notifications(user_id, created_at DESC);
