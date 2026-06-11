# CLAUDE.md

## Technologies

| Category | Technology |
|----------|-------------|
| Runtime | .NET 9.0 |
| Database | PostgreSQL via EF Core 8 (Npgsql) |
| CQRS | MediatR 12 |
| Validation | FluentValidation 11 |
| Mapping | AutoMapper 13 |
| Auth | JWT Bearer tokens |
| Testing | xUnit, NSubstitute, FluentAssertions |

## Commands

```bash
dotnet build Notism.sln
dotnet test Notism.sln
./format-code.sh --verify  # check only
./format-code.sh           # fix issues
```

## EF Core Migrations

Run from `src/Notism.Infrastructure/`:

```bash
dotnet ef migrations add <Name> \
  --project Notism.Infrastructure.csproj \
  --startup-project ../Notism.Api/Notism.Api.csproj

dotnet ef database update \
  --project Notism.Infrastructure.csproj \
  --startup-project ../Notism.Api/Notism.Api.csproj
```

## Folder Structure

```
src/
  Notism.Api/              # Minimal APIs, middleware, JWT auth
  Notism.Application/       # MediatR handlers (CQRS), FluentValidation
  Notism.Domain/           # Aggregates, entities, domain events
  Notism.Infrastructure/    # EF Core, repositories, external services
  Notism.Shared/            # Result pattern, constants, extensions
  Notism.Worker/            # Background services (token cleanup)
tests/
  Notism.Api.Tests/         # API integration tests
  Notism.Application.Tests/  # Application unit tests
terraform/                   # AWS infrastructure (EC2, RDS, S3, CloudFront)
docs/                       # Architecture and feature documentation
```

## Architecture

Clean Architecture (Onion) + CQRS via MediatR

| Layer | Responsibility |
|-------|----------------|
| Domain | Entities, aggregates, domain events |
| Application | MediatR handlers, FluentValidation, AutoMapper |
| Infrastructure | EF Core persistence, repository implementations |
| Api | Minimal APIs, middleware, JWT authentication |

## CQRS Naming

Feature folder: `src/Notism.Application/{Feature}/{Operation}/`

| File | Pattern |
|------|---------|
| Request | `{Operation}Request.cs` |
| Response | `{Operation}Response.cs` |
| Handler | `{Operation}Handler.cs` |
| Validator | `{Operation}RequestValidator.cs` |

Validators and AutoMapper profiles auto-register — no manual DI registration needed.

## Error Handling

| Exception | HTTP |
|-----------|------|
| `ResultFailureException` | 400 |
| `NotFoundException` | 404 |
| `UnauthorizedException` | 401 |
| `ForbiddenException` | 403 |
| `InvalidRefreshTokenException` | 401 |

Never return null/bool for failures — throw. `ValidationBehavior` throws `ResultFailureException` automatically.

## Test Naming

Method pattern: `Handle_When{Condition}_{ExpectedOutcome}`
File location: `tests/Notism.Application.Tests/{Feature}/{Operation}/{Operation}HandlerTests.cs`

## Document Navigation

| Topic | Location |
|-------|----------|
| Architecture | `docs/rules/architecture.md` |
| Best practices index | `docs/rules/best-practices.md` |
| Repository Pattern (write boundary) | `docs/rules/repository-pattern.md` |
| Read Queries (per-handler query objects) | `docs/rules/read-queries.md` |
| Domain Events | `docs/rules/domain-events.md` |
| Handler Design | `docs/rules/handler-design.md` |
| Validation | `docs/rules/validation.md` |
| Error Handling | `docs/rules/error-handling.md` |
| Naming Conventions | `docs/rules/naming.md` |
| Code Organization | `docs/rules/code-organization.md` |
| Additional Practices | `docs/rules/additional-practices.md` |
| Image resizing flow | `docs/flows/image-resizing.md` |
| Auth cookie flow | `docs/flows/secure-authentication-cookies.md` |

## Infrastructure (Terraform)

```
terraform/
  main.tf, variables.tf, outputs.tf
```

Deploy: `terraform apply -var="key_name=notism-api"`

Resources: VPC, EC2 (t4g.micro), RDS PostgreSQL (optional), ECR, S3, CloudFront

## CI/CD

| Workflow | Target | Trigger |
|----------|---------|---------|
| `deploy.yml` | AWS EC2 (Docker Compose) | Push to main/dev |
| `deploy-worker.yml` | AWS ECS | Push to main/dev when Worker/Domain/Infrastructure/Shared change |

Branch → environment: `main` = prod, `dev` = dev
