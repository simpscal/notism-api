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
./format-code.sh --verify  # check only; omit --verify to fix
```

## EF Core Migrations

Run from `src/Notism.Infrastructure/` (`-p Notism.Infrastructure.csproj -s ../Notism.Api/Notism.Api.csproj`):

```bash
dotnet ef migrations add <Name> -p Notism.Infrastructure.csproj -s ../Notism.Api/Notism.Api.csproj
dotnet ef database update    -p Notism.Infrastructure.csproj -s ../Notism.Api/Notism.Api.csproj
```

## Folder Structure

```
src/Notism.Api/             # Minimal APIs, middleware, JWT auth
src/Notism.Application/      # MediatR handlers (CQRS), FluentValidation
src/Notism.Domain/          # Aggregates, entities, domain events
src/Notism.Infrastructure/  # EF Core, repositories, external services
src/Notism.Shared/          # Result pattern, constants, extensions
tests/Notism.Api.Tests/         # API integration tests
tests/Notism.Application.Tests/ # Application unit tests
terraform/                  # AWS infrastructure (EC2, RDS, S3, CloudFront)
docs/                       # Architecture and feature documentation
```

## Conventions — load before editing

| When you… | Read |
|-----------|------|
| Add/edit a MediatR handler, request, response, or validator under `src/Notism.Application/**` | `docs/rules/naming.md` |
| Define cross-layer dependencies or a new aggregate/service; review `src/` project structure | `docs/rules/architecture.md` |
| Add endpoints in `src/Notism.Api/Endpoints/**`, new Application feature folders, or new interfaces | `docs/rules/code-organization.md` |
| Throw/catch exceptions in handlers or middleware | `docs/rules/error-handling.md` |
| Add/edit any `*RequestValidator.cs` | `docs/rules/validation.md` |
| Create/edit any `*Handler.cs` | `docs/rules/handler-design.md` |
| Write a read handler or any query over `IReadDbContext` | `docs/rules/read-queries.md` |
| Add methods to any `I*Repository` interface/impl under `src/Notism.Infrastructure/Repositories/**` | `docs/rules/repository-pattern.md` |
| Raise domain events / create event classes under `src/Notism.Domain/**/Events/**` | `docs/rules/domain-events.md` |
| Create aggregates/entities/value objects in `src/Notism.Domain/**` | `docs/rules/additional-practices.md` |

## Infrastructure (Terraform)

`terraform/` (`main.tf`, `variables.tf`, `outputs.tf`). Deploy: `terraform apply -var="key_name=notism-api"`.
Resources: VPC, EC2 (t4g.micro), RDS PostgreSQL (optional), ECR, S3, CloudFront.

## CI/CD

`deploy.yml` → AWS EC2 (Docker Compose) on push to `main`/`dev`. Branch → env: `main` = prod, `dev` = dev.
