# TaskFlow AI — CLAUDE.md

## Project Overview

TaskFlow AI is a task management application with AI-assisted features.

**Stack:**
- **Backend:** .NET 8 (C#), Clean Architecture, CQRS, Repository Pattern
- **Frontend:** React + TypeScript
- **Database:** PostgreSQL
- **Infrastructure:** Docker / Docker Compose

**Monorepo layout:**
```
taskflow-ai/
├── backend/        # .NET 8 solution
├── frontend/       # React + TypeScript app
├── infrastructure/ # Docker Compose, migrations, IaC
├── docs/           # Architecture decisions, API docs
└── .github/        # CI/CD workflows
```

---

## Architecture Rules

### Clean Architecture (backend)

Layers (inner → outer): **Domain → Application → Infrastructure → API**

- Inner layers must never reference outer layers.
- `Domain` contains entities, value objects, domain events, and interfaces — no framework dependencies.
- `Application` contains commands, queries, validators, and use-case logic. No direct database access.
- `Infrastructure` implements interfaces defined in `Application` (repositories, external services, EF Core).
- `API` contains controllers, middleware, and DI wiring only — no business logic.

### CQRS

- Commands mutate state; queries read state. Keep them in separate classes.
- Use MediatR for dispatching commands and queries.
- Command handlers return `Result<T>` (or `Unit`); never throw for expected failures.
- Query handlers return DTOs, never domain entities.

### Repository Pattern

- Define repository interfaces in `Application` (`ITaskRepository`, etc.).
- Implement them in `Infrastructure` using EF Core.
- Repositories work with aggregate roots only — no fine-grained per-table repos for sub-entities.

### Frontend

- Feature-based folder structure: `src/features/<feature-name>/`.
- Shared UI primitives live in `src/components/`.
- API calls go through typed service hooks in `src/services/` — no raw `fetch` in components.
- Use React Query for server state; Zustand for global client state.

---

## Coding Standards

### C# / .NET

- Target `.NET 8`; use C# 12 features where appropriate (primary constructors, collection expressions).
- Follow Microsoft's C# naming conventions (`PascalCase` for types and members, `camelCase` for locals).
- Prefer `record` types for DTOs and value objects.
- Use `Result<T>` / `Error` pattern for expected failures — avoid exceptions for control flow.
- All public APIs must have XML doc comments.
- Async all the way: every I/O-bound method must be `async Task<T>`, never `.Result` or `.Wait()`.

### TypeScript / React

- Strict TypeScript (`"strict": true`). No `any` — use `unknown` and narrow.
- Functional components only; no class components.
- Co-locate component styles, tests, and types with the component file.
- Export one component per file; name the file after the component.
- Prefer named exports over default exports.

### General

- No magic numbers or strings — use constants or enums.
- Remove dead code; do not comment it out.
- Keep functions small and focused (single responsibility).

---

## Testing Requirements

### Backend

- **Unit tests** for all domain logic and application handlers (xUnit + FluentAssertions + Moq/NSubstitute).
- **Integration tests** for repositories and API endpoints using `WebApplicationFactory` and a real PostgreSQL instance (via Docker).
- Minimum coverage target: **80%** on `Domain` and `Application` projects.
- Test project naming: `<ProjectName>.Tests.Unit` / `<ProjectName>.Tests.Integration`.

### Frontend

- **Unit/component tests** with Vitest + React Testing Library.
- Test behaviour, not implementation — query by role/label, not by class or test-id unless necessary.
- **E2E tests** with Playwright for critical user flows (login, create task, assign task).

### Running tests

```bash
# Backend
dotnet test

# Frontend unit
pnpm test

# Frontend E2E
pnpm test:e2e
```

---

## Git Workflow

### Branches

| Pattern | Purpose |
|---|---|
| `main` | Production-ready; protected |
| `develop` | Integration branch for active development |
| `feature/<ticket-id>-short-description` | New features |
| `fix/<ticket-id>-short-description` | Bug fixes |
| `chore/<description>` | Tooling, deps, config |

- Branch from `develop`; merge back to `develop` via PR.
- `main` is updated from `develop` via release PRs only.

### Commits

Follow [Conventional Commits](https://www.conventionalcommits.org/):

```
<type>(<scope>): <short summary>

[optional body]
[optional footer]
```

Types: `feat`, `fix`, `docs`, `style`, `refactor`, `test`, `chore`, `ci`

Examples:
```
feat(tasks): add AI-suggested due date on task creation
fix(auth): correct token refresh race condition
test(tasks): add integration tests for TaskRepository
```

- Commits must be atomic — one logical change per commit.
- Do not commit secrets, `.env` files, or build artifacts.

---

## Pull Request Requirements

1. **Title** follows Conventional Commits format.
2. **Description** must include:
   - What changed and why.
   - How to test it (manual steps or automated test reference).
   - Screenshots / recordings for any UI changes.
3. **All CI checks must pass** (build, lint, tests) before requesting review.
4. **At least one approval** required before merging.
5. **No unresolved comments** at merge time.
6. Squash-merge into `develop`; preserve the conventional commit title.
7. Delete the source branch after merge.

### PR size guideline

Keep PRs focused. If a PR exceeds ~400 lines of meaningful change (excluding generated/migration files), consider splitting it.

---

## Local Development

```bash
# Start all services (DB, backend, frontend)
docker compose up -d

# Backend only
cd backend
dotnet run --project src/TaskFlow.API

# Frontend only
cd frontend
pnpm install
pnpm dev
```

Database connection string (local): `Host=localhost;Port=5432;Database=taskflow;Username=postgres;Password=postgres`

---

## Environment Variables

Store secrets in `.env.local` (never commit). See `docs/env-vars.md` for the full list.
