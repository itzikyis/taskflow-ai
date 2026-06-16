---
name: devops-agent
description: Use this agent for infrastructure, Docker, CI/CD, and environment tasks — docker-compose changes, Dockerfiles, GitHub Actions workflows, environment variables, and deployment concerns. Invoke when the task touches infrastructure/, .github/, or Dockerfiles.
---

# DevOps Agent — TaskFlow AI

## Stack
- Docker + Docker Compose
- GitHub Actions (CI/CD)
- Nginx (frontend static serving + reverse proxy)
- PostgreSQL 16 (managed via Docker)

## Services

| Service | Image | Port | Purpose |
|---|---|---|---|
| `db` | `postgres:16-alpine` | 5432 | PostgreSQL database |
| `backend` | local build | 5000→8080 | .NET 8 ASP.NET Core API |
| `frontend` | local build | 5173→80 | Nginx serving Vite build |

## docker-compose.yml — `infrastructure/docker-compose.yml`

Key rules:
- `backend` depends on `db` with `condition: service_healthy`
- DB health check: `pg_isready -U postgres`
- Backend environment vars use `__` for nested config: `ConnectionStrings__DefaultConnection`, `Jwt__SecretKey`
- Never hard-code secrets in compose for production — use `.env` file or secrets manager

## Backend Dockerfile — `backend/Dockerfile`

Multi-stage build pattern:

```dockerfile
# Stage 1: build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy ALL .csproj files before restore (including test projects — the .sln references them)
COPY TaskFlow.sln .
COPY src/TaskFlow.Domain/TaskFlow.Domain.csproj src/TaskFlow.Domain/
COPY src/TaskFlow.Application/TaskFlow.Application.csproj src/TaskFlow.Application/
COPY src/TaskFlow.Infrastructure/TaskFlow.Infrastructure.csproj src/TaskFlow.Infrastructure/
COPY src/TaskFlow.API/TaskFlow.API.csproj src/TaskFlow.API/
COPY tests/TaskFlow.Domain.Tests.Unit/...csproj tests/TaskFlow.Domain.Tests.Unit/
COPY tests/TaskFlow.Application.Tests.Unit/...csproj tests/TaskFlow.Application.Tests.Unit/
COPY tests/TaskFlow.Infrastructure.Tests.Integration/...csproj tests/TaskFlow.Infrastructure.Tests.Integration/

RUN dotnet restore

COPY . .
RUN dotnet publish src/TaskFlow.API/TaskFlow.API.csproj -c Release -o /app/publish --no-restore

# Stage 2: runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "TaskFlow.API.dll"]
```

**Critical:** If a new test project is added to the `.sln`, its `.csproj` MUST be added to the Dockerfile COPY block before `dotnet restore`, or the restore will fail.

## Frontend Dockerfile — `frontend/Dockerfile`

Multi-stage: Node 20 Alpine build → Nginx 1.27 Alpine runtime.

- Uses `npm ci` (not pnpm — no lockfile exists for pnpm).
- Build step: `npx vite build` (not `npm run build`, which runs tsc first and may fail on test globals).
- Nginx config at `frontend/nginx.conf` is copied into the container.

## Nginx config — `frontend/nginx.conf`

Reverse-proxies `/api/` to `http://backend:8080/api/` so the frontend can call `/api/tasks` and have it reach the .NET container.

```nginx
location /api/ {
    proxy_pass http://backend:8080/api/;
}
```

## Environment variables

| Variable | Where set | Purpose |
|---|---|---|
| `ConnectionStrings__DefaultConnection` | compose / `.env` | PostgreSQL connection string |
| `Jwt__SecretKey` | compose / `.env` | JWT signing key (min 32 chars) |
| `Jwt__Issuer` | compose / `.env` | JWT issuer claim |
| `Jwt__Audience` | compose / `.env` | JWT audience claim |
| `Jwt__ExpiryMinutes` | compose / `.env` | Token lifetime |
| `ASPNETCORE_ENVIRONMENT` | compose | `Development` or `Production` |

Secrets must never be committed. Production values go in `.env.local` (gitignored) or CI secrets.

## Rebuild commands

```bash
# Rebuild and restart a single service
docker compose -f infrastructure/docker-compose.yml up -d --build backend

# Rebuild all
docker compose -f infrastructure/docker-compose.yml up -d --build

# View logs
docker compose -f infrastructure/docker-compose.yml logs -f backend
```

## GitHub Actions — `.github/workflows/`

CI pipeline should:
1. Restore and build the .NET solution (`dotnet build`)
2. Run unit tests (`dotnet test --filter "Category!=Integration"`)
3. Run integration tests with a PostgreSQL service container
4. Build the frontend (`npm ci && npx vite build`)
5. Run frontend unit tests (`npm test`)

Branch protection: CI must pass before merging to `develop` or `master`.

## Git workflow

| Branch | Purpose |
|---|---|
| `master` | Production-ready, protected |
| `develop` | Integration branch |
| `feature/<id>-description` | New features — branch from `develop` |
| `fix/<id>-description` | Bug fixes |
| `chore/<description>` | Tooling, deps, config |

Merge strategy: squash-merge into `develop`. Release PRs merge `develop` → `master`.
