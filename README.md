# Cricut Orders API

A .NET REST API that simulates a simple order management system with automatic discount logic, built following clean architecture principles.

---

## Table of Contents

- [Architecture](#architecture)
- [Design Patterns](#design-patterns)
- [State Management](#state-management)
- [API Endpoints](#api-endpoints)
- [Discount Logic & Validations](#discount-logic--validations)
- [Edge Cases](#edge-cases)
- [Unit Testing Approach](#unit-testing-approach)
- [Integration Testing Approach](#integration-testing-approach)
- [Target Environment](#target-environment)
- [How to Run](#how-to-run)
- [How to Test](#how-to-test)
- [Environment Preparation (Dev / Test / Prod)](#environment-preparation-dev--test--prod)
- [Challenge Tasks Completed](#challenge-tasks-completed)

---

## Architecture

The solution follows a **layered (clean) architecture** with clear separation of concerns across four projects:

```
┌─────────────────────────────────────────────────────┐
│                  Cricut.Orders.Api                  │  ← Presentation Layer
│  Controllers, ViewModels, Mappings, Startup         │
├─────────────────────────────────────────────────────┤
│                 Cricut.Orders.Domain                │  ← Business Logic Layer
│  OrderDomain, Models (Order, Customer, Product),    │
│  IOrderStore interface, DI extensions               │
├─────────────────────────────────────────────────────┤
│             Cricut.Orders.Infrastructure            │  ← Data Access Layer
│  OrderStore (in-memory), DI extensions              │
└─────────────────────────────────────────────────────┘
```

**Dependency flow:** Api → Domain ← Infrastructure

- The **Api** layer depends on Domain for business logic
- The **Infrastructure** layer depends on Domain for the `IOrderStore` interface
- The **Domain** layer has no dependency on Api or Infrastructure (Dependency Inversion Principle)

### Project Breakdown

| Project | Responsibility | SDK |
|---|---|---|
| `Cricut.Orders.Api` | HTTP endpoints, request/response mapping, Swagger docs | `Microsoft.NET.Sdk.Web` |
| `Cricut.Orders.Domain` | Business rules, domain models, interfaces | `Microsoft.NET.Sdk` |
| `Cricut.Orders.Infrastructure` | Data persistence (in-memory store) | `Microsoft.NET.Sdk` |
| `Cricut.Orders.Tests` | Unit tests for mappings and store | `Microsoft.NET.Sdk` |
| `Cricut.Orders.Integration.Tests` | End-to-end HTTP pipeline tests | `Microsoft.NET.Sdk` |

---

## Design Patterns

### Dependency Injection (DI)
All services are registered via **extension methods** on `IServiceCollection`, keeping registration logic co-located with each layer:

- `services.AddDomainDependencies()` → registers `IOrderDomain` → `OrderDomain` (Scoped)
- `services.AddInfrastructureDependencies()` → registers `IOrderStore` → `OrderStore` (Singleton)

Both are called from `Startup.ConfigureServices()`.

### Interface Segregation
- `IOrderDomain` — business operations (create order, get orders by customer)
- `IOrderStore` — data persistence contract (CRUD operations)

Controllers depend only on `IOrderDomain`, never on concrete implementations or the store directly.

### ViewModel ↔ Domain Model Mapping
Extension methods in `ToDomainModelMappings` and `ToViewModelMappings` handle conversion between API contracts (ViewModels) and internal domain models. This keeps the API layer decoupled from business internals.

### Record Types for ViewModels
ViewModels use C# `record` types for immutability and value-based equality, with `[Required]` and `[Range]` data annotations for input validation.

---

## State Management

The application uses an **in-memory dictionary** (`Dictionary<int, Order>`) as its data store, managed by `OrderStore`:

- **Singleton lifetime** — one instance shared across all requests, preserving state for the app's lifetime
- **Pre-seeded data** — 10 static orders are generated on startup for customer IDs `12345` and `54321` (5 orders each)
- **Auto-generated IDs** — new orders receive a random integer ID (100–1,000,000) if none is provided
- **State resets on restart** — since storage is in-memory, all data (including POSTed orders) is lost when the app stops

---

## API Endpoints

| Method | Route | Description | Response |
|---|---|---|---|
| `POST` | `/v1/orders` | Create a new order | `200 OK` with `OrderViewModel` (includes computed total) |
| `GET` | `/v1/orders/customer/{customerId}` | Get all orders for a customer | `200 OK` with `OrderViewModel[]` |

### Example: Create Order
```bash
curl -X POST http://localhost:5000/v1/orders \
  -H "Content-Type: application/json" \
  -d '{
    "customer": { "id": 1, "name": "Jane", "address": "123 Main St", "email": "jane@example.com" },
    "orderItems": [
      { "product": { "id": 1, "name": "Cricut Maker", "price": 15.00 }, "quantity": 2 }
    ]
  }'
```

### Example: Get Customer Orders
```bash
curl http://localhost:5000/v1/orders/customer/12345
```

---

## Discount Logic & Validations

### Discount Rule
Located in `Order.cs` → `Total` property (computed, not stored):

- **Threshold:** `$25.00`
- **Condition:** If the sum of all line item totals is **≥ $25**, a **10% discount** is applied
- **Formula:** `total = subtotal - (subtotal × 0.10)`

```
Line Item Total = Product.Price × Quantity
Order Subtotal  = Σ (Line Item Totals)
Order Total     = Subtotal >= 25 ? Subtotal × 0.90 : Subtotal
```

### Input Validations (Data Annotations)
| Field | Validation |
|---|---|
| `Customer` | `[Required]` |
| `Customer.Id` | `[Required]` |
| `Customer.Name` | `[Required]` |
| `Customer.Email` | `[Required]` |
| `OrderItems` | `[Required, MinLength(1)]` |
| `OrderItem.Quantity` | `[Required, Range(0, int.MaxValue)]` |
| `Product.Id` | `[Required, Range(1, int.MaxValue)]` |
| `Product.Price` | `[Required, Range(0, double.MaxValue)]` |

---

## Edge Cases

| Scenario | Behavior |
|---|---|
| Order total exactly $25.00 | Discount **is applied** (≥ threshold) → total = $22.50 |
| Order total $24.99 | Discount **not applied** → total = $24.99 |
| Order total $0.00 | No discount, total = $0.00 |
| Customer ID with no orders | GET returns empty array `[]` |
| Extremely large price values (e.g., `Double.MaxValue`) | Produces `Infinity` which causes JSON serialization error — use realistic values |
| Duplicate order creation | Each POST generates a new random ID; no uniqueness enforcement |
| Empty order items array | Rejected by `[MinLength(1)]` validation |
| Negative prices | Accepted by current `[Range(0, ...)]` — could be tightened |
| Concurrent requests | In-memory `Dictionary` is not thread-safe; acceptable for demo purposes |

---

## Unit Testing Approach

**Framework:** MSTest + FluentAssertions + AutoBogus

**Location:** `Cricut.Orders.Tests/` (4 tests)

| Test Class | What It Tests | Approach |
|---|---|---|
| `ToDomainModelMappingsTests` | ViewModel → Domain model mapping | AutoBogus generates a random `NewOrderViewModel`, maps it, asserts equivalence |
| `ToViewModelMappingsTests` | Domain model → ViewModel mapping | AutoBogus generates a random `Order`, maps it, asserts field-by-field equivalence |
| `OrderStoreTests.CanManageOrdersInTheStore` | Save → Retrieve → Delete lifecycle | Creates order, saves, retrieves by ID, deletes, verifies null after delete |
| `OrderStoreTests.CanGetStaticOrders` | Pre-seeded data integrity | Verifies 10 total orders, 5 per customer (12345 and 54321) |

**Key Libraries:**
- **AutoBogus** — auto-generates fake data for models without manual setup
- **FluentAssertions** — readable assertion syntax (`order.Should().BeEquivalentTo(...)`)
- **MSTest** — Microsoft's test framework with `[TestClass]`, `[TestMethod]`, `[DataTestMethod]`

---

## Integration Testing Approach

**Framework:** MSTest + `Microsoft.AspNetCore.Mvc.Testing` + FluentAssertions

**Location:** `Cricut.Orders.Integration.Tests/` (5 test cases)

### How It Works
`OrdersApiTestClientFactory` creates a `WebApplicationFactory<Startup>` which boots the **entire ASP.NET Core pipeline in-memory** — no real HTTP server, no ports, no network. The test `HttpClient` talks directly to the middleware pipeline.

### Test: `CreateNewOrder_Does_Apply_Discount`
Uses `[DataTestMethod]` with 5 parameterized rows:

| Line Items | Qty Each | Price Each | Total | Discount? | Expected Total |
|---|---|---|---|---|---|
| 3 | 2 | $1.50 | $9.00 | No | $9.00 |
| 3 | 2 | $1.50 | $9.00 | No | $9.00 |
| 1 | 1 | $25.00 | $25.00 | Yes | $22.50 |
| 3 | 4 | $8.00 | $96.00 | Yes | $86.40 |
| 1 | 1 | $30.00 | $30.00 | Yes | $27.00 |

Each test case:
1. Constructs a `NewOrderViewModel` with AutoBogus
2. POSTs it to `/v1/orders`
3. Asserts `200 OK`
4. Deserializes the response
5. Verifies the total matches expected discount calculation

---

## Target Environment

| Component | Version |
|---|---|
| .NET SDK | 10.0+ |
| Target Framework | `net10.0` |
| Language | C# 12 |
| OS | macOS / Windows / Linux |
| IDE | Visual Studio 2022+ / VS Code / Rider |

### NuGet Dependencies

| Package | Version | Purpose |
|---|---|---|
| `Microsoft.AspNetCore.OpenApi` | 10.0.3 | OpenAPI metadata for endpoints |
| `Swashbuckle.AspNetCore` | 10.1.4 | Swagger UI generation |
| `Microsoft.Extensions.DependencyInjection.Abstractions` | 10.0.0 | DI interfaces for Domain layer |
| `Microsoft.AspNetCore.Mvc.Testing` | 10.0.3 | In-memory test server for integration tests |
| `MSTest.TestFramework` | 3.1.1 | Test framework |
| `FluentAssertions` | 6.12.0 | Assertion library |
| `AutoBogus` | 2.13.1 | Fake data generation |
| `coverlet.collector` | 6.0.0 | Code coverage collection |

---

## How to Run

### Prerequisites
- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) installed
- Verify: `dotnet --version` should show `10.x.x`

### Run the API
```bash
cd Cricut.Orders
dotnet run --project Cricut.Orders.Api
```

The API starts at **http://localhost:5000**

### Access Swagger UI
Open **http://localhost:5000/swagger** in your browser for interactive API documentation.

### Run All Tests
```bash
dotnet test
```

### Run Specific Test Projects
```bash
# Unit tests only
dotnet test Cricut.Orders.Tests

# Integration tests only
dotnet test Cricut.Orders.Integration.Tests
```

### Run with Verbosity
```bash
dotnet test --verbosity normal
```

---

## How to Test

### Manual Testing via Swagger
1. Start the API (`dotnet run --project Cricut.Orders.Api`)
2. Go to http://localhost:5000/swagger
3. Use the **Try it out** button on each endpoint
4. Replace default extreme values with realistic ones (e.g., price: 15.00, quantity: 2)

### Manual Testing via curl
```bash
# Create an order (discount applies: $30 × 0.90 = $27)
curl -X POST http://localhost:5000/v1/orders \
  -H "Content-Type: application/json" \
  -d '{
    "customer": { "id": 1, "name": "Test User", "address": "123 St", "email": "test@test.com" },
    "orderItems": [{ "product": { "id": 1, "name": "Widget", "price": 15.00 }, "quantity": 2 }]
  }'

# Get pre-seeded orders for customer 12345
curl http://localhost:5000/v1/orders/customer/12345

# Get pre-seeded orders for customer 54321
curl http://localhost:5000/v1/orders/customer/54321
```

### Automated Testing
```bash
# All 9 tests (4 unit + 5 integration)
dotnet test --verbosity normal
```

---

## Environment Preparation (Dev / Test / Prod)

### Development
The current setup is development-ready out of the box:
```bash
dotnet run --project Cricut.Orders.Api
```

- **Swagger UI** enabled (via `env.IsDevelopment()` check in `Startup.cs`)
- **Developer Exception Page** enabled for detailed error output
- **In-memory store** with pre-seeded data — no external dependencies
- Configuration: `appsettings.Development.json`

### Test / CI
For automated testing in CI pipelines (GitHub Actions, Azure DevOps, etc.):

```yaml
# Example GitHub Actions workflow
- uses: actions/setup-dotnet@v4
  with:
    dotnet-version: '10.0.x'
- run: dotnet restore
- run: dotnet build --no-restore
- run: dotnet test --no-build --verbosity normal
```

Key considerations:
- Integration tests use `WebApplicationFactory` — no external services needed
- No database, no network calls — tests are fully self-contained
- Add `coverlet` for code coverage reporting: `dotnet test --collect:"XPlat Code Coverage"`

### Production
To prepare this for production, the following changes would be needed:

**1. Replace In-Memory Store with a Real Database**
```csharp
// Replace OrderStore registration with a real implementation
services.AddScoped<IOrderStore, SqlOrderStore>();  // e.g., EF Core, Dapper
```
No other code changes needed — the interface `IOrderStore` is already defined.

**2. Add Configuration per Environment**
```
appsettings.json               ← shared defaults
appsettings.Development.json   ← local dev overrides
appsettings.Staging.json       ← staging (add this)
appsettings.Production.json    ← production (add this)
```

Set the environment via:
```bash
export ASPNETCORE_ENVIRONMENT=Production
```

**3. Disable Swagger in Production**
Already handled — `Startup.cs` only enables Swagger when `env.IsDevelopment()`.

**4. Add Logging & Monitoring**
```csharp
// In Startup.ConfigureServices
services.AddApplicationInsightsTelemetry();  // or Serilog, etc.
```

**5. Add Authentication/Authorization**
```csharp
services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options => { /* config */ });
```

**6. Containerize**
```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY . .
RUN dotnet publish Cricut.Orders.Api -c Release -o /app/publish

FROM base AS final
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Cricut.Orders.Api.dll"]
```

**7. Add Health Checks**
```csharp
services.AddHealthChecks()
    .AddCheck<DatabaseHealthCheck>("database");

app.MapHealthChecks("/health");
```

---

## Challenge Tasks Completed

### Task 1: Fix Discount Bug
**File:** `Cricut.Orders.Domain/Models/Order.cs`

Changed the discount threshold comparison from `>` to `>=`:
```csharp
// Before (bug): discount not applied at exactly $25
if (orderItemTotal > DiscountThreshold)

// After (fix): discount applied at $25 and above
if (orderItemTotal >= DiscountThreshold)
```

### Task 2: GET Endpoint for Customer Orders
**Files:** `OrderDomain.cs`, `OrdersController.cs`

- Added `GetOrdersForCustomerAsync(int customerId)` to `IOrderDomain` interface and `OrderDomain` class
- Added `[HttpGet("customer/{customerId}")]` endpoint in `OrdersController`
- Leveraged existing `IOrderStore.GetAllOrdersForCustomerAsync()` — no store modifications needed

### Task 3: All Tests Passing
- Updated integration test `[DataRow(1, 1, 25, true)]` to expect discount at exactly $25 (was `false`)
- All **9 tests pass** (4 unit + 5 integration)

```
Test summary: total: 9, failed: 0, succeeded: 9, skipped: 0
```
