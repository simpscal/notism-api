# Notism API

A modern, clean architecture-based REST API built with .NET 9, implementing Domain-Driven Design (DDD), CQRS, and comprehensive authentication features.

## 📋 Table of Contents

- [Overview](#overview)
- [Documentation](#documentation)
- [Technologies](#technologies)
- [Prerequisites](#prerequisites)
- [Getting Started](#getting-started)
- [Running the Application](#running-the-application)
- [Docker Deployment](#docker-deployment)
- [Configuration](#configuration)

## Overview

Notism API is a backend service built following Clean Architecture principles with a focus on:

- **Domain-Driven Design**: Business logic organized around aggregates
- **CQRS Pattern**: Separation of commands and queries using MediatR
- **Secure Authentication**: JWT-based authentication with refresh tokens and password reset
- **Clean Code**: Well-structured, testable, and maintainable codebase
- **Modern .NET**: Built with .NET 9 and minimal APIs

## Documentation

Detailed documentation is available in the `docs/` directory:

| Folder | Purpose |
|--------|---------|
| `docs/rules/` | Architecture guidelines, coding standards, and implementation methodology |
| `docs/features/` | Feature-specific documentation and technical flows |

## Technologies

### Core Framework
- **.NET 9.0** - Latest .NET runtime
- **ASP.NET Core** - Web framework with minimal APIs
- **Entity Framework Core 8.0** - ORM for data access
- **PostgreSQL** - Primary database

### Key Libraries
- **MediatR 12.2.0** - CQRS implementation
- **FluentValidation 11.9.0** - Request validation
- **AutoMapper 13.0.1** - Object mapping
- **JWT Bearer Authentication** - Secure token-based auth
- **Resend 0.2.1** - Email service integration
- **AWS SDK S3** - File storage

### Testing
- **xUnit** - Testing framework
- **FluentAssertions** - Assertion library
- **NSubstitute** - Mocking framework

## Prerequisites

Before you begin, ensure you have the following installed:

- [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [PostgreSQL](https://www.postgresql.org/download/) (or Docker)
- [Docker](https://www.docker.com/get-started) (optional, for containerized deployment)
- [Git](https://git-scm.com/downloads)

## Getting Started

### 1. Clone the Repository

```bash
git clone <repository-url>
cd notism-api
```

### 2. Configure sensitive settings (.env.development)

Sensitive environment variables **must** be in an environment-specific .env file; they are loaded when you run the app. For local development, create `src/Notism.Api/.env.development` with at least:

```bash
ConnectionStrings__DefaultConnection=Host=localhost;Database=notism_db;Username=your_username;Password=your_password;Port=5432
JwtSettings__Secret=your-super-secret-jwt-key-minimum-32-characters
```

Add other secrets (e.g. `Resend__ApiKey`, AWS keys) as needed. See [Configuration](#configuration).

### 3. Run Database Migrations

```bash
cd src/Notism.Infrastructure
dotnet ef database update --project ../Notism.Infrastructure/Notism.Infrastructure.csproj --startup-project ../Notism.Api/Notism.Api.csproj
```

## Running the Application

### Development Mode

```bash
cd src/Notism.Api
dotnet run
```

The API will be available at:
- **HTTP**: `http://localhost:5000`
- **HTTPS**: `https://localhost:5001`
- **Swagger UI**: `https://localhost:5001/swagger` (Development only)

### Using Docker Compose

```bash
docker-compose up --build
```

This will start:
- API service on port 5000
- Nginx reverse proxy on port 3000

### Running the Worker Service

The worker service handles background tasks (e.g., token cleanup):

```bash
cd src/Notism.Worker
dotnet run
```

## Code Formatting

This project uses **dotnet format** to automatically fix coding errors and enforce consistent code style. The project includes:

- **StyleCop.Analyzers** - Code analysis and style enforcement
- **.editorconfig** - Editor configuration for consistent formatting
- **dotnet format** - Automatic code formatting tool

### Install dotnet format (if not already installed)

```bash
dotnet tool install -g dotnet-format
```

### Format Code

**Using the provided script (recommended):**

```bash
# Fix all code style issues (default)
./format-code.sh

# Only check if code needs formatting (doesn't modify files)
./format-code.sh --verify

# Fix specific issues
./format-code.sh --whitespace  # Fix whitespace only
./format-code.sh --style       # Fix code style only
./format-code.sh --analyzers   # Fix analyzer issues only
```

**Using dotnet format directly:**

```bash
# Fix all issues (run all three commands)
dotnet format Notism.sln whitespace
dotnet format Notism.sln style
dotnet format Notism.sln analyzers

# Only check (verify mode)
dotnet format Notism.sln whitespace --verify-no-changes
dotnet format Notism.sln style --verify-no-changes
dotnet format Notism.sln analyzers --verify-no-changes

# Fix specific issues
dotnet format Notism.sln whitespace  # Fix whitespace only
dotnet format Notism.sln style       # Fix code style only
dotnet format Notism.sln analyzers   # Fix analyzer issues only
```

### Code Style Configuration

The project uses `.editorconfig` for consistent code formatting. Key settings include:

- **Indentation**: 4 spaces
- **Line endings**: CRLF (Windows style)
- **Naming conventions**: PascalCase for types/methods, camelCase for variables
- **StyleCop rules**: Configured with sensible defaults

## Docker Deployment

### Build Docker Image

```bash
docker build -t notism-api -f Dockerfile .
```

### Run with Docker Compose

```bash
docker-compose up -d
```

### Environment Variables

Set the following environment variables for production:

- `ASPNETCORE_ENVIRONMENT=Production`
- `ConnectionStrings__DefaultConnection=<your-connection-string>`
- `JwtSettings__Secret=<your-jwt-secret>`
- `Resend__ApiKey=<your-resend-api-key>`

## Configuration

### Sensitive environment variables and .env files

- **Sensitive environment variables must be stored in environment-specific .env files** (e.g. connection strings, API keys, JWT secrets).
- **Do not** put secrets in `appsettings.json` or commit them to version control.
- **Local development:** use `src/Notism.Api/.env.development`; variable values are loaded when you run the application.
- **Production:** configure environment variables in your CI/CD pipeline (or your host’s environment); no .env file is used in production.

### Loading variables from .env files (development only)

When you run the API in development, it loads environment variables from `.env.development` **before** building configuration (when `ASPNETCORE_ENVIRONMENT=Development`). In production, variables are provided by the CI/CD pipeline or host environment—no .env file is loaded.

- Place `.env.development` in **`src/Notism.Api/`** and run the API from that directory (or the repo root with `dotnet run -p src/Notism.Api`).
- Use `KEY=value` per line. For nested config use double underscores: `Section__Key=value` (e.g. `GoogleOAuth__ClientId=...`).
- Values from the .env file override `appsettings.json` because they are loaded as environment variables.
- **Do not commit .env.development**; it is listed in `.gitignore`.
