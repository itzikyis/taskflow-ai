# Issue #3 — Implement Boards Module

## Overview
A **Board** belongs to a Project and organises Tasks into columns (e.g. Todo → In Progress → Done). Each Project can have multiple Boards (Sprint board, Backlog board, etc.).

## Acceptance Criteria

### Domain
- [ ] `Board` aggregate root with: `Id`, `Name`, `ProjectId`, `CreatedAt`, `UpdatedAt`
- [ ] `BoardColumn` value object / child entity: `Id`, `BoardId`, `Name`, `Order` (int), `WipLimit` (optional)
- [ ] `BoardErrors` static constants: `NameRequired`, `NameTooLong`, `NotFound`, `ProjectNotFound`, `ColumnNameRequired`, `ColumnOrderConflict`
- [ ] `BoardCreatedEvent` domain event
- [ ] `Board.Create(name, projectId)` factory returning `Result<Board>`
- [ ] `Board.AddColumn(name, order)` returning `Result`
- [ ] `Board.RemoveColumn(columnId)` returning `Result`
- [ ] `Board.ReorderColumns(orderedColumnIds)` returning `Result`

### Application
- [ ] `IBoardRepository` interface in `Application/Interfaces/`
- [ ] Commands:
  - `CreateBoardCommand` (name, projectId) → `Result<Guid>`
  - `UpdateBoardCommand` (boardId, name) → `Result`
  - `DeleteBoardCommand` (boardId) → `Result`
  - `AddColumnCommand` (boardId, name, order, wipLimit?) → `Result<Guid>`
  - `RemoveColumnCommand` (boardId, columnId) → `Result`
  - `ReorderColumnsCommand` (boardId, orderedColumnIds) → `Result`
- [ ] Queries:
  - `GetBoardByIdQuery` → `BoardDto` (with columns)
  - `GetBoardsByProjectQuery` (projectId) → `IReadOnlyList<BoardSummaryDto>`
- [ ] FluentValidation validators for all commands
- [ ] `BoardDto`: `{ Id, Name, ProjectId, Columns: [{ Id, Name, Order, WipLimit }], CreatedAt }`

### Infrastructure
- [ ] `BoardRepository` + `BoardConfiguration` (tables: `boards`, `board_columns`)
- [ ] snake_case columns, proper FK `project_id`
- [ ] Register in `DependencyInjection`

### API
- [ ] `BoardsController`:
  - `GET  /api/projects/{projectId}/boards`
  - `GET  /api/boards/{id}`
  - `POST /api/boards`
  - `PUT  /api/boards/{id}`
  - `DELETE /api/boards/{id}`
  - `POST /api/boards/{id}/columns`
  - `DELETE /api/boards/{id}/columns/{columnId}`
  - `PUT  /api/boards/{id}/columns/reorder`

### Frontend
- [ ] `src/features/boards/types/board.types.ts`
- [ ] `src/services/boardService.ts`
- [ ] `src/features/boards/hooks/useBoards.ts`
- [ ] `BoardListPage` — list boards for a project
- [ ] `BoardView` — Kanban-style columns with task cards draggable between columns
- [ ] `CreateBoardForm`, `AddColumnModal`
- [ ] Wire into App.tsx (accessible from a Project card)

### Tests
- [ ] `BoardTests` — domain unit tests (Create, AddColumn, ReorderColumns, WipLimit)
- [ ] `CreateBoardCommandHandlerTests` — application unit tests

## Schema

```sql
CREATE TABLE boards (
    id UUID PRIMARY KEY,
    name VARCHAR(100) NOT NULL,
    project_id UUID NOT NULL REFERENCES projects(id) ON DELETE CASCADE,
    created_at TIMESTAMP NOT NULL,
    updated_at TIMESTAMP
);

CREATE TABLE board_columns (
    id UUID PRIMARY KEY,
    board_id UUID NOT NULL REFERENCES boards(id) ON DELETE CASCADE,
    name VARCHAR(100) NOT NULL,
    "order" INT NOT NULL,
    wip_limit INT NULL
);
```

## Labels
`feature`, `backend`, `frontend`
