---
name: database-agent
description: Use this agent for all database tasks — schema changes, EF Core configurations, migrations, queries optimisation, and PostgreSQL-specific work. Invoke when the task touches the database schema, ApplicationDbContext, entity configurations, or SQL.
---

# Database Agent — TaskFlow AI

## Stack
- PostgreSQL 16 (Docker: `postgres:16-alpine`)
- Entity Framework Core 8 + Npgsql provider
- No migration tool on host — DDL is managed manually or via `docker exec psql`

## Connection

```
Host=localhost;Port=5432;Database=taskflow;Username=postgres;Password=postgres
```

Docker internal (backend → db):
```
Host=db;Port=5432;Database=taskflow;Username=postgres;Password=postgres
```

## Schema conventions

All tables and columns use **snake_case**.

| Domain concept | Table |
|---|---|
| `TaskItem` | `tasks` |
| `Project` | `projects` |
| `User` | `users` |

Every table has:
- `id UUID PRIMARY KEY`
- `created_at TIMESTAMP NOT NULL`

Audit columns where relevant: `updated_at TIMESTAMP`.

## EF Core configuration rules

- All mappings live in `Infrastructure/Persistence/Configurations/` as `IEntityTypeConfiguration<T>`.
- `ApplyConfigurationsFromAssembly` in `OnModelCreating` picks them up automatically — never configure inline on the model builder.
- Enums are stored as strings: `builder.Property(x => x.Status).HasConversion<string>()`.
- Column names are always explicit: `HasColumnName("snake_case_name")`.
- Max lengths match the domain constraints (e.g. `HasMaxLength(200)` for task title).
- Unique indexes declared in configuration: `builder.HasIndex(u => u.Email).IsUnique()`.
- Domain events collection is always ignored: `builder.Ignore(e => e.DomainEvents)`.

## Example configuration

```csharp
internal sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");
        builder.HasKey(u => u.Id);
        builder.Property(u => u.Id).HasColumnName("id");
        builder.Property(u => u.Email).HasColumnName("email").HasMaxLength(254).IsRequired();
        builder.HasIndex(u => u.Email).IsUnique();
        builder.Ignore(u => u.DomainEvents);
    }
}
```

## Current schema

### `tasks`
| Column | Type | Constraints |
|---|---|---|
| id | UUID | PK |
| title | VARCHAR(200) | NOT NULL |
| description | TEXT | |
| status | VARCHAR(20) | NOT NULL (Todo/InProgress/Done) |
| priority | VARCHAR(20) | NOT NULL (Low/Medium/High/Critical) |
| due_date | TIMESTAMP | |
| assigned_to_user_id | UUID | |
| created_by_user_id | UUID | NOT NULL |
| created_at | TIMESTAMP | NOT NULL |
| updated_at | TIMESTAMP | |

### `projects`
| Column | Type | Constraints |
|---|---|---|
| id | UUID | PK |
| name | VARCHAR(100) | NOT NULL |
| description | TEXT | |
| owner_id | UUID | NOT NULL |
| created_at | TIMESTAMP | NOT NULL |
| updated_at | TIMESTAMP | |

### `users`
| Column | Type | Constraints |
|---|---|---|
| id | UUID | PK |
| email | VARCHAR(254) | NOT NULL, UNIQUE |
| display_name | VARCHAR(100) | NOT NULL |
| password_hash | VARCHAR(256) | NOT NULL |
| created_at | TIMESTAMP | NOT NULL |

## Running DDL manually

```bash
docker exec taskflow_db psql -U postgres -d taskflow -c "CREATE TABLE ..."
```

## Repository rules

- All repositories implement interfaces defined in `Application/Interfaces/`.
- Reads always use `.AsNoTracking()`.
- Repositories only work with aggregate roots — never with sub-entities directly.
- `SaveChangesAsync` is the only way to persist — repositories never call `context.Database.ExecuteSqlRaw`.

## Performance guidelines

- Add indexes for all FK-like columns used in WHERE clauses (e.g. `owner_id`, `created_by_user_id`).
- Use `AnyAsync` for existence checks instead of `CountAsync() > 0`.
- Order deterministically in list queries (e.g. `OrderByDescending(x => x.CreatedAt)`).
