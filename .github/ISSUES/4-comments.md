# Issue #4 — Implement Comments Module

## Overview
Users can add, edit, and delete **Comments** on Tasks. Comments support Markdown text. Each comment records the author and timestamps.

## Acceptance Criteria

### Domain
- [ ] `Comment` entity (child of `TaskItem` aggregate, not its own aggregate root):
  - `Id`, `TaskId`, `AuthorId`, `Body` (markdown text), `CreatedAt`, `UpdatedAt`
- [ ] `CommentErrors` constants: `BodyRequired`, `BodyTooLong`, `NotFound`, `NotAuthor` (only the author can edit/delete)
- [ ] `Comment.Create(taskId, authorId, body)` returning `Result<Comment>`
- [ ] `Comment.Update(body)` returning `Result` (validates non-empty, max 10 000 chars)
- [ ] `CommentAddedEvent` domain event raised on `TaskItem` when a comment is added

### Application
- [ ] `ICommentRepository` in `Application/Interfaces/`
- [ ] Commands:
  - `AddCommentCommand` (taskId, authorId, body) → `Result<Guid>`
  - `UpdateCommentCommand` (commentId, requestingUserId, body) → `Result`
  - `DeleteCommentCommand` (commentId, requestingUserId) → `Result`
- [ ] Queries:
  - `GetCommentsByTaskQuery` (taskId) → `IReadOnlyList<CommentDto>`
- [ ] `CommentDto`: `{ Id, TaskId, AuthorId, AuthorDisplayName, Body, CreatedAt, UpdatedAt }`
- [ ] FluentValidation validators for Add and Update

### Infrastructure
- [ ] `CommentRepository` + `CommentConfiguration` (table: `comments`)
- [ ] FK to `tasks(id)` with `ON DELETE CASCADE`
- [ ] FK to `users(id)` for `author_id`
- [ ] Register in DI

### API
- [ ] `CommentsController`:
  - `GET    /api/tasks/{taskId}/comments`
  - `POST   /api/tasks/{taskId}/comments`
  - `PUT    /api/comments/{id}`
  - `DELETE /api/comments/{id}`
- [ ] Extract `requestingUserId` from JWT claim `sub` — not from request body

### Frontend
- [ ] `src/features/comments/types/comment.types.ts`
- [ ] `src/services/commentService.ts`
- [ ] `src/features/comments/hooks/useComments.ts`
- [ ] `CommentList` component — renders comments with author name, timestamp, Markdown body
- [ ] `CommentForm` — textarea with submit; shows edit mode inline
- [ ] Visible when a Task card is expanded / task detail view is open
- [ ] Edit / Delete only shown to the comment author (compare `comment.authorId` vs `authStore.token.userId`)

### Tests
- [ ] `CommentTests` — domain unit tests
- [ ] `AddCommentCommandHandlerTests` — application unit tests (success, task not found, body too long)

## Schema

```sql
CREATE TABLE comments (
    id UUID PRIMARY KEY,
    task_id UUID NOT NULL REFERENCES tasks(id) ON DELETE CASCADE,
    author_id UUID NOT NULL REFERENCES users(id),
    body TEXT NOT NULL,
    created_at TIMESTAMP NOT NULL,
    updated_at TIMESTAMP
);

CREATE INDEX idx_comments_task_id ON comments(task_id);
```

## Labels
`feature`, `backend`, `frontend`
