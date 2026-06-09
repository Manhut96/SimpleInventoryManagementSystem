# SimpleInventoryManagementSystem

RESTful inventory management API built with .NET 10, EF Core, PostgreSQL, and MediatR.

---

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/) (or a compatible Docker engine)

---

## Quick Start

```bash
docker-compose up --build
```

---

## Services

| Service    | URL                             |
|------------|---------------------------------|
| API        | http://localhost:8080           |
| Scalar UI  | http://localhost:8080/scalar/v1 |
| pgAdmin    | http://localhost:5050           |
| PostgreSQL | localhost:5432                  |

---

## pgAdmin Login

URL: `http://localhost:5050`

| Field    | Value           |
|----------|-----------------|
| Email    | admin@admin.com |
| Password | admin           |

After logging in, register a server manually:

| Field    | Value     |
|----------|-----------|
| Host     | postgres  |
| Port     | 5432      |
| Username | postgres  |
| Password | admin     |
| Database | inventory |

---

## Seeded Customers

Use these GUIDs as `customerId` in `POST /orders`:

| Name  | Location | GUID                                   |
|-------|----------|----------------------------------------|
| Alice | US       | `11111111-0000-0000-0000-000000000001` |
| Bob   | Europe   | `22222222-0000-0000-0000-000000000002` |
| Carol | Asia     | `33333333-0000-0000-0000-000000000003` |

Customers are seeded automatically on first startup if the database is empty.

---

## Architecture

```
Domain ← Application ← Infrastructure ← API
```

3 PostgreSQL schemas: `catalog` (products), `ordering` (customers, orders), `events` (outbox).
Domain events are written atomically with business data via the Outbox pattern; `OutboxProcessor` publishes them asynchronously every 5 seconds.

---

## Database Schema Overview

| Schema     | Tables                                       | Purpose                              |
|------------|----------------------------------------------|--------------------------------------|
| `catalog`  | `tbl_products`                               | Product catalogue with stock levels  |
| `ordering` | `tbl_customers`, `tbl_orders`, `tbl_order_items` | Customers and placed orders      |
| `events`   | `tbl_outbox_events`                          | Outbox for domain event delivery     |

---

## Tests

```bash
dotnet test
```

Unit tests run without a database. Integration tests spin up PostgreSQL via Testcontainers — **Docker must be running** when executing the integration test project.
