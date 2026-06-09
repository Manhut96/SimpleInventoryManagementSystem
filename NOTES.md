# Notes — Assumptions & Trade-offs

## 1. No Authentication

The spec does not mention authentication. `customerId` is accepted as-is from the request body. A real production system would add JWT or API keys.

## 2. Customer Entity for Location-Based Pricing

`Customer` holds `Id`, `Name`, `Email`, and `Location`. No password or auth fields. The entity exists solely to carry the customer's region for pricing calculations.

## 3. 400 for All FluentValidation Failures

All validation errors return `400 ValidationProblemDetails` per RFC 9457, with errors grouped by property name.

## 4. Domain Exceptions → Global Middleware

HTTP concerns (status codes, ProblemDetails formatting) are handled in `ExceptionHandlerMiddleware`, keeping the domain and application layers free of HTTP dependencies.

## 5. Volume Discount = Total Order Quantity Across All Items

The spec says "5 or more units" without specifying "of the same product." Standard volume-discount semantics apply to the entire order size (sum of all line item quantities).

## 6. Highest-Discount Wins via Absolute Savings Comparison

All applicable discounts are evaluated and compared by absolute savings (discount % × applicable base). The one producing the greatest savings is applied. If Holiday Sale wins, the 15% applies only to the most expensive line item.

## 7. Fixed Polish Public Holidays Only

Easter-based moveable holidays (Easter Monday, Corpus Christi, Whit Sunday) are excluded due to complexity. Only the 9 fixed-date statutory holidays are used:
Jan 1, Jan 6, May 1, May 3, Aug 15, Nov 1, Nov 11, Dec 25, Dec 26.

## 8. IDateTimeProvider for Deterministic Tests

`IDateTimeProvider` is injected into `PricingCalculatorService` and `OutboxProcessor`. Tests can substitute a fake implementation to control the current date and make discount calculations deterministic.

## 9. Location Pricing is a Multiplier, Not a Discount

Location adjustments (US: ×1.0, Europe: ×1.15, Asia: ×1.05) are applied after discount resolution and are not part of the discount-selection competition.

## 10. Outbox Pattern for Domain Events

Domain events are serialized to JSON and written to `events.tbl_outbox` in the same `SaveChangesAsync` call as the business entities, solving the dual-write problem. `OutboxProcessor` (IHostedService) polls unprocessed rows with exponential backoff (5 s when events are found, doubling up to 60 s when idle) and calls `IEventPublisher`. In production, `IEventPublisher` would publish to RabbitMQ or Azure Service Bus. This decouples event publishing from the HTTP request lifecycle.

## 11. Schema Separation Mirrors Bounded Contexts

`catalog` (products), `ordering` (customers, orders), `events` (outbox). In a microservices architecture each would be a separate database. Here they share one database for simplicity.

## 12. No Audit Service

A dedicated Audit microservice subscribing to domain events is the natural next evolution step in a full EDA setup. It is not implemented here.

## 13. Data Annotations for Documentation Only

Request records carry Data Annotations (`[Required]`, `[MaxLength]`, `[Range]`, `[MinLength]`) so that Scalar renders field-level constraints (maxLength, minimum, minItems, required) directly in the UI. `ApiBehaviorOptions.SuppressModelStateInvalidFilter = true` is set in `Program.cs` so these annotations never trigger the built-in ASP.NET Core model-state 400 filter. All actual validation remains in the FluentValidation pipeline (see note #3), keeping RFC 9457 ProblemDetails format consistent.

## 14. Contracts in Application Layer

Request/response records live in `Application/Contracts/`. No separate Contracts project is needed because all consumers (Infrastructure, API, Tests) already reference Application.

## 15. Pessimistic Locking on Stock Deduction

Products are loaded with `SELECT ... FOR UPDATE` (raw SQL via Npgsql) inside the existing EF Core transaction opened by `TransactionalHandlerBase`. This serializes concurrent stock deductions at the DB row level, preventing negative stock without application-level retry logic.

## 16. Repository Layer for ORM Replaceability

A thin repository layer (`IProductRepository`, `ICustomerRepository`, `IOrderRepository`) was introduced so that EF Core–specific constructs (LINQ, `AsNoTracking`, pessimistic-lock raw SQL) are contained entirely in Infrastructure. Application handlers depend only on the interfaces. This makes swapping the ORM a matter of replacing repository implementations, not touching handler logic.

## 17. Secret Management

Sensitive values (connection strings, passwords) are stored in `appsettings.json` / `appsettings.Development.json` for the convenience of this recruitment task only. In production these should be managed via a dedicated secret store (e.g. Azure Key Vault with Managed Identity, AWS Secrets Manager, or HashiCorp Vault) and never committed to source control.

## 18. One Line Item Per Product Per Order

`OrderItem` is modelled as a Value Object owned by `OrderEntity` and persisted via EF Core `OwnsMany`. The composite primary key `(OrderId, ProductId)` means each product can appear at most once per order.

## 19. Pagination on GET /products

The spec requires "a list of all products" without specifying pagination. A `PagedResult<ProductDto>` response with `pageNumber` / `pageSize` query parameters (defaults: 1 / 20) was added to avoid unbounded result sets in production. The response envelope includes `totalCount`, `pageNumber`, and `pageSize`.
