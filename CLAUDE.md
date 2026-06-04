# SimpleInventoryManagementSystem.CoffeeMug

Recruitment task — RESTful inventory management API in .NET 10.

## Project Layout

```
src/
  SimpleInventoryManagementSystem.Domain/
  SimpleInventoryManagementSystem.Application/
  SimpleInventoryManagementSystem.Infrastructure/
  SimpleInventoryManagementSystem.API/
tests/
  SimpleInventoryManagementSystem.Tests.Unit/
  SimpleInventoryManagementSystem.Tests.Integration/
```

References: Domain ← Application ← Infrastructure ← API; Tests.Unit → Domain+Application; Tests.Integration → API.

## Tech Stack

- .NET 10, C# 13, `[ApiController]` controllers, camelCase JSON
- EF Core + Npgsql + PostgreSQL (3 schemas: `catalog`, `ordering`, `events`)
- MediatR 12 + FluentValidation pipeline behavior
- Serilog → Console (structured)
- Scalar UI (`/scalar/v1`), RFC 9457 ProblemDetails
- xUnit + FluentAssertions + NSubstitute; integration via `WebApplicationFactory` + Testcontainers
- Docker Compose: api + postgres:17-alpine + adminer

## Key Conventions

- **Entities = classes** with `private set` + `static Create(...)` factory. No records for domain objects.
- **DTOs/requests = records** in `Application/Contracts/`.
- **No AutoMapper** — manual mapping only (constructor / record init).
- **DB tables**: `ToTable("tbl_*", "schema_name")` per entity config. No `HasDefaultSchema`.
- **UnitOfWork** is Scoped (holds per-request `_pending` event queue).
- **`public partial class Program {}`** at bottom of `Program.cs` — required for integration tests.
- Routes: no `/api` prefix — `[Route("[controller]")]` only.

## Implementation Plan

`.claude/plans/task-plan.md` — 19 phases, each = one logical commit.

## Architectural Decisions & Change Log

`.claude/knowledge/` — see files there for specific decisions, gotchas, and deviations from the plan.
