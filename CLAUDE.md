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
- **No anonymous tuples in public APIs** — always extract `(...)` tuples into named records in dedicated files. Tuples are allowed only as local variables inside method bodies.
- **Helper/auxiliary records** (data carriers, structured results, input/output shapes) live in a `Models/` subfolder nested under the feature folder they belong to (e.g. `Domain/Discounts/Models/OrderLineItem.cs`).
- **`var` everywhere** — use `var` for all local variable declarations where the compiler can infer the type, including simple numeric literals (`var count = 5`, `var price = 9.99m`). Only omit `var` when the type cannot be inferred or explicit declaration is required by the language.
- **Extract private methods** — split longer logic in handlers and services into small, well-named `private` methods. Each method should do one thing and read like a sentence. Prefer many short private methods over one long public method.
- **Per-feature `RegistrationExtensions.cs`** — every feature folder in Application/Infrastructure gets its own `RegistrationExtensions.cs` with a single `static` extension method on `IServiceCollection` that registers that feature's services (e.g. `AddPricingServices()`, `AddPersistence(config)`). Domain stays pure (no DI package). Top-level `ApplicationServiceExtensions` / `InfrastructureServiceExtensions` just chain-call the per-feature methods.

## Implementation Plan

`.claude/plans/task-plan.md` — 19 phases, each = one logical commit.

## Architectural Decisions & Change Log

`.claude/knowledge/` — see files there for specific decisions, gotchas, and deviations from the plan.
