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
- MediatR 14 + FluentValidation pipeline behavior
- Serilog → Console (structured)
- Scalar UI (`/scalar/v1`), RFC 9457 ProblemDetails
- xUnit + FluentAssertions + NSubstitute; integration via `WebApplicationFactory` + Testcontainers
- Unit handler tests use `TestSIMSDbContext : DbContext, ISIMSDbContext` with `UseInMemoryDatabase` — **never mock `DbSet<T>` with NSubstitute or custom async providers**
- Docker Compose: api + postgres:17-alpine + pgAdmin

## Key Conventions

- **Entities = classes named `*Entity`**, with a `private` parameterless constructor (EF Core) + `private set` + `static Create(...)` factory, always have `Id: Guid`. No records for domain objects.
- **ValueObjects = classes** with `private set` + `static Create(...)` factory, no `Id` property. Folder: `Domain/ValueObjects/`. EF Core maps them via `OwnsMany`/`OwnsOne`.
- **DTOs/requests = records** in `Application/Contracts/`.
- **No AutoMapper** — manual mapping only (constructor / record init).
- **DB tables**: `ToTable("tbl_*", "schema_name")` per entity config. No `HasDefaultSchema`.
- **TransactionalHandler** — abstract base class in `Application/Common/`; write handlers extend it; `Handle()` opens an explicit EF transaction, calls `HandleCoreAsync` (business logic), then `SaveChangesAsync` + commit; `protected void WriteEvent(DomainEvent)` stages an OutboxEvent row directly into the DbContext. No separate UnitOfWork.
- **`public partial class Program {}`** at bottom of `Program.cs` — required for integration tests.
- Routes: no `/api` prefix — `[Route("[controller]")]` only.
- **No anonymous tuples in public APIs** — always extract `(...)` tuples into named records in dedicated files. Tuples are allowed only as local variables inside method bodies.
- **Helper/auxiliary records** (data carriers, structured results, input/output shapes) live in a `Models/` subfolder nested under the feature folder they belong to (e.g. `Domain/Pricing/Models/OrderLineItem.cs`).
- **`var` everywhere** — use `var` for all local variable declarations where the compiler can infer the type; omit only when explicit declaration is required by the language.
- **No abbreviated names** — full descriptive names: `dbContext` not `db`, `dateTimeProvider` not `dt`, `transaction` not `tx`, `cancellationToken` not `ct`, `domainEvent` not `e`. Single-letter LINQ lambda parameters (`.Where(p => ...)`) and loop indices (`i`, `j`) are exempt.
- **Extract private methods** — split longer logic into small, well-named `private` methods; each should do one thing and read like a sentence. Do NOT extract trivial one-liners that just delegate to another method with the same arguments — extract only when the body contains real logic, multiple steps, or branching.
- **Per-feature `RegistrationExtensions.cs`** — each Infrastructure feature folder (Persistence, Pricing, Events) has its own `RegistrationExtensions.cs` with a single extension method (e.g. `AddPricingServices()`, `AddPersistence(config)`); `InfrastructureServiceExtensions` chains them. Application uses a single top-level `ApplicationServiceExtensions`. Domain stays pure (no DI package).

## Recruitment Task Requirements

All agents and all code **must unconditionally satisfy every requirement in `Requirements.md`** (project root). When in doubt about scope or business logic, check `Requirements.md` first. Requirements take priority over everything except security.

Key constraints to keep top of mind:
- `GET /products` and `POST /products` must exist with the specified fields and validation rules.
- `POST /orders` must enforce stock deduction, stock-insufficiency rejection, and the full discount + location-pricing logic.
- The discount date must flow through `IDateTimeProvider` — never `DateTime.Now` directly.
- A `NOTES.md` file covering assumptions and trade-offs must be included in the deliverable.

## Architectural Decisions & Change Log

`.claude/knowledge/` — see files there for specific decisions, gotchas, and deviations from the plan.
