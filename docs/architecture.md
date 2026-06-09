# Architecture Decision Records

## ADR-001: Clean Architecture with CQRS

**Date:** 2026-06-09  
**Status:** Accepted

### Context
We need a backend architecture that is testable, maintainable, and allows the domain model to evolve independently of infrastructure concerns.

### Decision
Use Clean Architecture (Domain → Application → Infrastructure → API) combined with CQRS via MediatR. Commands mutate state; queries return DTOs only.

### Consequences
- Domain and Application layers have zero infrastructure dependencies — easy to unit test.
- MediatR pipeline behaviours (validation, logging) are applied cross-cuttingly.
- Slightly more files than a simple layered architecture, but explicit separation of intent.

---

## ADR-002: PostgreSQL via EF Core + Npgsql

**Date:** 2026-06-09  
**Status:** Accepted

### Context
We need a relational database with strong JSON/JSONB support for potential AI metadata columns.

### Decision
Use PostgreSQL 16 with EF Core 8 and the Npgsql provider.

### Consequences
- EF Core migrations manage schema evolution.
- Testcontainers.PostgreSql provides real-database integration tests in CI without external setup.

---

## ADR-003: React Query + Zustand for frontend state

**Date:** 2026-06-09  
**Status:** Accepted

### Context
We need server-state management (caching, loading, error states) and lightweight global client state (UI toggles, selected items).

### Decision
React Query handles all server-state; Zustand handles pure client-state. No Redux.

### Consequences
- Minimal boilerplate compared to Redux Toolkit.
- React Query automatically de-duplicates in-flight requests and keeps cache fresh.
