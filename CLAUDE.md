# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Commands

### Build & Run
```bash
dotnet build Notism.sln -c Release
dotnet run --project src/Notism.Api
dotnet run --project src/Notism.Worker
```

### Testing
```bash
dotnet test                                  # All tests
dotnet test tests/Notism.Domain.Tests        # Domain tests only
dotnet test tests/Notism.Application.Tests   # Application tests only
dotnet test --filter "FullyQualifiedName~Login"  # Single test class
```

### Code Formatting
```bash
./format-code.sh             # Fix all issues
./format-code.sh --verify    # Check only (no changes)
dotnet format Notism.sln whitespace
dotnet format Notism.sln style
dotnet format Notism.sln analyzers
```

### Database Migrations
```bash
# From repo root
dotnet ef migrations add <MigrationName> \
  --project src/Notism.Infrastructure \
  --startup-project src/Notism.Api

dotnet ef database update \
  --project src/Notism.Infrastructure \
  --startup-project src/Notism.Api
```

### Docker
```bash
docker-compose up --build    # Full stack (Caddy + PostgreSQL + API)
```

## Architecture

Clean Architecture (Onion) with DDD and CQRS:

```
Notism.Api            → HTTP layer: Minimal APIs, middleware, auth
Notism.Application    → CQRS: MediatR handlers, FluentValidation, AutoMapper
Notism.Domain         → DDD: Aggregates, Value Objects, Domain Events, repository interfaces
Notism.Infrastructure → EF Core (PostgreSQL), AWS S3, MailerSend, JWT/password services
Notism.Shared         → Cross-cutting: Result<T>, Pagination, ResultFailureException
Notism.Worker         → Background service
```

**Dependency rule**: each layer only depends inward. Infrastructure and API depend on Application, Application depends on Domain, Domain depends on Shared only.

### Key Patterns

**CQRS via MediatR**: Each feature has its own folder with `XRequest`, `XRequestValidator`, `XHandler`, and `XResponse`. The handler flow is: `Request → Validator (pipeline behavior) → Handler → Domain ops → Repository → Response`.

**Result Pattern**: Handlers return `Result<T>`. Business violations throw `ResultFailureException`, which `ResultFailureMiddleware` converts to HTTP 400.

**Aggregates**: `User` (with `PasswordResetToken` entity, `Email`/`Password` value objects) and `RefreshToken`. Other aggregates (Order, Food, Cart) exist in the domain but are not fully documented in `docs/`.

**Specifications**: Query logic is encapsulated in `*Specification` classes, co-located in the Application feature folder (not in Domain).

**Repository per aggregate**: Interfaces defined in Domain (`IUserRepository`, `IRefreshTokenRepository`), implemented in Infrastructure.

### Adding a New Feature

1. **Domain**: Add business methods to aggregate root, new value objects/events as needed, repository interface if new aggregate.
2. **Application**: Create feature folder `FeatureName/ActionName/` with Request, Validator, Handler, Response. Add to `MappingProfile` if AutoMapper is needed.
3. **Infrastructure**: Implement repository, add EF entity configuration, run migration.
4. **API**: Add endpoint in `Endpoints/XEndpoints.cs` — only HTTP translation, delegate to `ISender.Send()`.

### Configuration

Local dev uses `.env.development` (loaded by DotNetEnv in `Program.cs`). Production uses CI/CD-injected environment variables. Key config sections: `JwtSettings`, `AWS` (S3 buckets), `Email` (MailerSend), `ClientApp`, `GoogleOAuth`.

## Documentation

Detailed reference docs live in `docs/`:

| Document | Path |
|---|---|
| Architecture rules & folder structure | `docs/rules/architecture.md` |
| Best practices (handlers, repos, specs) | `docs/rules/best-practices.md` |
| Image resizing flow | `docs/flows/image-resizing.md` |
| Secure authentication cookies flow | `docs/flows/secure-authentication-cookies.md` |
| AWS infrastructure architecture | `docs/infra/aws-architecture.md` |
| Terraform configuration | `docs/infra/terraform-configuration.md` |

### Code Quality

- **Warnings as errors** is enabled — all StyleCop/analyzer warnings must be resolved.
- **Centralized package versions** in `Directory.Packages.props` — do not specify versions in individual `.csproj` files.
- `.editorconfig` enforces CRLF line endings and 4-space indentation.
- Tests use xUnit + FluentAssertions + NSubstitute.
