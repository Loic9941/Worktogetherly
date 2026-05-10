# WorkTogetherly

WorkTogetherly is a collaborative workspace booking platform built with .NET 10, featuring a Clean Architecture backend and a cross-platform frontend using MAUI and Blazor WebAssembly.

## Repository Layout

```
WorkTogetherly/              ← git root
├── Domain/                  ← Clean architecture: entities, interfaces, errors
├── Application/             ← Clean architecture: use-case handlers (Command pattern)
├── Infrastructure/          ← Clean architecture: EF Core, Identity, JWT, TokenService
├── Presentation/            ← Clean architecture: ASP.NET Core API (controllers)
├── TestWorkTogetherly/      ← Test project (xUnit, FluentAssertions, NSubstitute, Testcontainers)
├── WorkTogetherly/          ← Frontend sub-solution
│   ├── WorkTogetherly/             ← MAUI mobile app (Android, iOS, macOS, Windows)
│   ├── WorkTogetherly.Shared/      ← Shared Blazor components & auth — used by both frontends
│   └── WorkTogetherly.Web.Client/  ← Blazor WebAssembly standalone app
└── WorkTogetherly.slnx      ← Main solution (backend + frontend together)
```

> `WorkTogetherly.Application/` and `WorkTogetherly.Infrastructure/` at the root are **old/unused** — the active backend layers are the unprefixed `Application/` and `Infrastructure/` folders.

## Build & Run

All projects target **.NET 10**.

### Database (PostgreSQL via Docker)

A `docker-compose.yml` is provided at the root. Start the database before running the backend:

```bash
docker-compose up -d
```

This starts a PostgreSQL 16 container on port 5432 (user: `postgres`, password: `postgres`, db: `worktogetherly`). The backend applies migrations automatically on startup via `db.Database.MigrateAsync()`.

To reset the database completely (drops all data):
```bash
docker-compose down -v && docker-compose up -d
```

To generate or re-generate migrations after a schema change:
```bash
dotnet ef migrations add <MigrationName> --project Infrastructure --startup-project Presentation
```

### Backend API
```bash
# Run the backend API (https://localhost:7053)
dotnet run --project Presentation/WorkTogetherly.Presentation.csproj
```

### Web Frontend (Blazor WASM)
```bash
# Run the standalone WASM app (https://localhost:50867)
dotnet run --project WorkTogetherly/WorkTogetherly.Web.Client/WorkTogetherly.Web.Client.csproj
```

### Build Everything
```bash
dotnet build WorkTogetherly.slnx
```

## Backend Architecture

The backend follows **Clean Architecture** with unidirectional dependencies:

```
Presentation → Application → Domain
Infrastructure → Application (implements interfaces)
```

### Domain Layer
- **Entities**: Core business objects with invariants
- **Interfaces**: Repository contracts
- **Errors**: Domain-specific error types using `ErrorOr`

### Application Layer
- **Use-case handlers**: One folder per use-case (Register, Login, etc.)
- **Commands/Queries**: `IRequest<ErrorOr<T>>` with MediatR
- **Validation**: FluentValidation on commands
- **Behaviors**: `ValidationBehavior` for automatic validation
- **Ports** (`Application/Interfaces/`): `IClock`, `ITokenService`, `IFileService`, `INotificationService`, `IRefreshTokenRepository` — non-deterministic or I/O dependencies injected as interfaces, never concrete classes
- **Services** (`Application/Common/Services/`): `GeoCalculator` (Haversine distance + coordinate obfuscation)
- **Mappers** (`Application/<Aggregate>/Common/<Aggregate>Mapper.cs`): extension methods centralizing entity → Result projection; handlers never construct DTOs inline

### Infrastructure Layer
- **EF Core**: `AppDbContext` with Identity integration
- **Token Service**: JWT generation and refresh token rotation — depends on `IRefreshTokenRepository` and `IUserRepository`, not on `AppDbContext` directly
- **Repositories**: Implementation of domain and application interfaces. `UserRepository` is the **only** class that injects `UserManager<User>` — handlers never touch Identity directly

### Presentation Layer
- **ASP.NET Core API**: Controllers mapping to Application handlers
- **MediatR**: Single `IMediator` injection
- **Error handling**: `.Match(success => Ok(result), errors => Problem(errors))`

## Frontend Architecture

`WorkTogetherly.Shared` is the **single source of truth** for all UI. Both frontends only implement platform-specific services and host configuration.

### Platform Adaptations

| Interface | MAUI | Web (WASM) |
|---|---|---|
| `ITokenStorage` | `SecureTokenStorage` (MAUI SecureStorage) | `LocalStorageTokenStorage` (sessionStorage + localStorage) |
| `IFormFactor` | Returns `Mobile` / platform name | Returns `WebAssembly` |

### Authentication Flow
1. `AuthService` calls backend at `https://localhost:7053/api/auth/*`
2. Tokens stored via `ITokenStorage` — access token in memory + sessionStorage, refresh token in localStorage
3. `JwtAuthenticationStateProvider` parses JWT claims for Blazor auth state
4. `AuthTokenHandler` auto-attaches Bearer token and retries on 401 with refresh
5. On page load, `AppInitializer` restores auth state from localStorage before first render

## Key Patterns

- **ErrorOr**: All handlers return `ErrorOr<T>` for typed error results
- **MediatR**: Commands and queries with `IRequestHandler<TRequest, TResponse>`
- **FluentValidation**: Input validation on commands, not domain entities
- **IClock**: `DateTime.UtcNow` never called directly in handlers — injected via `IClock` port (`SystemClock` in production, `FakeClock` in tests)
- **Handler rules**: handlers orchestrate, never decide — all business conditions are delegated to entity methods (`slot.HasStarted(now)`, `booking.IsFull()`); all DTO construction goes through mappers (`entity.ToResult()`)
- **Mapper pattern**: each aggregate has a static mapper in `Application/<Aggregate>/Common/<Aggregate>Mapper.cs` — extension method `ToResult(this TEntity)` is the single mapping point; no handler duplicates the projection
- **MudBlazor**: Material Design UI library (v9.4.0)

## Error Architecture

Errors are split across layers with propagation inward → outward:

### Domain Errors
Tied to **entity invariants and DB lookups**:
- `*.NotFound` — entity doesn't exist
- Entity construction/mutation rules

### Application Errors
Tied to **use-case business rules**:
- Authorization checks
- Uniqueness across aggregates
- Pre-conditions

## Domain Model

### Core Entities

| Entity | Key | Notes |
|---|---|---|
| `User` | `Guid Id` | Extends `IdentityUser<Guid>` |
| `Workspace` | `int Id` | User FK → SetNull on delete |
| `Amenity` | `int Id` | Type: `Material` \| `Rule` |
| `WorkspaceAmenity` | `WorkspaceId+AmenityId` | Junction with optional `Quantity` (Material only) |
| `BookingAmenity` | `BookingId+AmenityId` | Material amenities requested by guest |
| `Slot` | `int Id` | Time slots for booking |
| `Booking` | `int Id` | User reservations |
| `Review` | `int Id` | Post-booking feedback |
| `Message` | `int Id` | In-app notifications between users |

### Relationships
- User deletion orphans records (SetNull)
- Workspace deletion cascades to WorkspaceAmenities and Slots
- Bookings restrict slot deletion; BookingAmenities cascade with Booking

## Testing

Tests are in `TestWorkTogetherly/` and cover three layers:

| Layer | Approach | Tools |
|---|---|---|
| **Domain** | Direct factory method calls, `[Theory]` for boundary cases | xUnit, FluentAssertions |
| **Application** | Mocked repositories, handler instantiated in constructor | NSubstitute |
| **Presentation** | `DefaultHttpContext` with JWT claims, mocked `IMediator` | NSubstitute |
| **Infrastructure** | Real PostgreSQL via Docker container (Testcontainers.PostgreSql), EF migrations applied on startup | Testcontainers |

### Running Tests

> **Requires Docker** for the infrastructure tests (SQL Server container).

```bash
dotnet test TestWorkTogetherly/TestWorkTogetherly.csproj
```

## Technologies

- **Backend**: .NET 10, ASP.NET Core, EF Core, Identity, JWT
- **Frontend**: Blazor WebAssembly, MudBlazor, MAUI
- **Database**: PostgreSQL 16 via Docker (with in-memory fallback for tests)
- **Authentication**: JWT with refresh tokens (Bearer header)
- **Validation**: FluentValidation
- **Mediation**: MediatR
- **Error Handling**: ErrorOr
- **Testing**: xUnit, FluentAssertions, NSubstitute, Testcontainers

## Getting Started

1. Clone the repository
2. Start the database: `docker-compose up -d`
3. Run the backend: `dotnet run --project Presentation/WorkTogetherly.Presentation.csproj`
4. Run the web frontend: `dotnet run --project WorkTogetherly/WorkTogetherly.Web.Client/WorkTogetherly.Web.Client.csproj`
5. Access the application at https://localhost:50867

### Production

Set the `DATABASE_URL` environment variable in the standard Heroku/Railway format:
```
postgres://username:password@host:port/database
```
When `DATABASE_URL` is set, it takes precedence over the `DefaultConnection` in `appsettings.json`.

## Development Notes

- CORS is open for development
- OpenAPI/Swagger enabled at https://localhost:7053
- Password requirements: 8+ chars, uppercase, lowercase, digit
