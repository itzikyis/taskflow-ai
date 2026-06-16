---
name: backend-agent
description: Use this agent for all backend .NET 8 tasks — domain logic, application commands/queries, infrastructure implementations, API controllers, and tests. Invoke when the task touches C# code under backend/.
---

# Backend Agent — TaskFlow AI

## Stack
- .NET 8, C# 12, ASP.NET Core Web API
- MediatR (CQRS), FluentValidation, Entity Framework Core 8, Npgsql

## Architecture (strict layer order — inner → outer)

```
Domain → Application → Infrastructure → API
```

- **Domain** — entities, value objects, domain events, `Result<T>`, `Error`, `AggregateRoot`. No framework references. Only `MediatR.Contracts` allowed.
- **Application** — commands, queries, handlers, validators, repository interfaces (`ITaskRepository`, `IProjectRepository`, `IUserRepository`), service interfaces (`IJwtService`, `IPasswordHasher`). No EF Core. No `DbSet<T>`.
- **Infrastructure** — EF Core `ApplicationDbContext`, repository implementations, `JwtService`, `BcryptPasswordHasher`, EF configurations (snake_case columns). References Application only.
- **API** — controllers, middleware, DI wiring in `Program.cs`. No business logic here.

## Coding rules

- All public members must have XML doc comments.
- Use C# 12 primary constructors and collection expressions where appropriate.
- Use `Result<T>` / `Result` for expected failures — never throw exceptions for control flow.
- Commands return `Result<T>` or `Result`; queries return DTOs, never domain entities.
- All I/O-bound methods must be `async Task<T>` — never `.Result` or `.Wait()`.
- Enums serialise as strings globally (`JsonStringEnumConverter` in `Program.cs`).
- `record` types for DTOs and value objects.
- PascalCase for types/members, camelCase for locals.

## Error pattern

```csharp
// Define in Domain/Common/<Feature>Errors.cs
public static class AuthErrors
{
    public static readonly Error EmailRequired = new("Auth.EmailRequired", "...");
}

// Return from handlers
return Result<Guid>.Failure(AuthErrors.EmailRequired);
```

## Validation

FluentValidation validators live next to their command in `Application/<Feature>/Commands/<Name>/`. They run automatically via `ValidationBehavior<,>` pipeline behaviour — handlers never validate manually.

## Repository pattern

- Interfaces in `Application/Interfaces/` — only work with aggregate roots.
- Implementations in `Infrastructure/Persistence/Repositories/` using EF Core.
- Reads use `.AsNoTracking()`.

## EF Core conventions

- `ApplyConfigurationsFromAssembly` picks up all `IEntityTypeConfiguration<T>` automatically.
- Column names are snake_case (`HasColumnName("created_at")`).
- Enums stored as strings via `HasConversion<string>()`.

## Test structure

| Project | Pattern |
|---|---|
| `TaskFlow.Domain.Tests.Unit` | Domain entity logic, `Result<T>`, domain events |
| `TaskFlow.Application.Tests.Unit` | Handler tests with NSubstitute mocks |
| `TaskFlow.Infrastructure.Tests.Integration` | Repository tests against real PostgreSQL via Testcontainers |

- xUnit + FluentAssertions + NSubstitute
- Minimum 80% coverage on Domain and Application projects

## File placement

```
backend/src/TaskFlow.<Layer>/<Feature>/
  Commands/<Name>/<Name>Command.cs
  Commands/<Name>/<Name>CommandHandler.cs
  Commands/<Name>/<Name>CommandValidator.cs
  Queries/<Name>/<Name>Query.cs
  Queries/<Name>/<Name>QueryHandler.cs
  Queries/<Name>/<Name>Dto.cs
```
