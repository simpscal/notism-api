# Notism API

A modern, clean architecture-based REST API built with .NET 9, implementing Domain-Driven Design (DDD), CQRS, and comprehensive authentication features.

## 📋 Table of Contents

- [Overview](#overview)
- [Architecture](#architecture)
- [Technologies](#technologies)
- [Project Structure](#project-structure)
- [Prerequisites](#prerequisites)
- [Getting Started](#getting-started)
- [Running the Application](#running-the-application)
- [Testing](#testing)
- [Docker Deployment](#docker-deployment)
- [Configuration](#configuration)

## Overview

Notism API is a backend service built following Clean Architecture principles with a focus on:

- **Domain-Driven Design**: Business logic organized around aggregates
- **CQRS Pattern**: Separation of commands and queries using MediatR
- **Secure Authentication**: JWT-based authentication with refresh tokens and password reset
- **Clean Code**: Well-structured, testable, and maintainable codebase
- **Modern .NET**: Built with .NET 9 and minimal APIs

## Architecture

This project follows **Clean Architecture** principles with clear separation of concerns across multiple layers:

```
┌─────────────────────────────────────────┐
│              API Layer                  │  ← Minimal APIs, Endpoints, Middleware
├─────────────────────────────────────────┤
│          Application Layer              │  ← CQRS Handlers, Request/Response Models
├─────────────────────────────────────────┤
│            Domain Layer                 │  ← Aggregates, Entities, Business Rules
├─────────────────────────────────────────┤
│        Infrastructure Layer             │  ← Data Access, External Services
├─────────────────────────────────────────┤
│            Shared Layer                 │  ← Common Utilities, Extensions
└─────────────────────────────────────────┘
```

For detailed architecture documentation, see [docs/architecture.md](docs/architecture.md).

### Key Principles

- **Dependency Inversion**: Inner layers define interfaces, outer layers implement them
- **Aggregate Boundaries**: Business logic encapsulated within aggregates
- **CQRS**: Commands and queries separated for better scalability
- **Domain Events**: Cross-aggregate communication through events
- **Repository Pattern**: Aggregate-focused data access

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

## Project Structure

```
notism-api/
├── src/
│   ├── Notism.Api/              # API Layer - Endpoints, Middleware
│   ├── Notism.Application/      # Application Layer - CQRS Handlers
│   ├── Notism.Domain/           # Domain Layer - Business Logic
│   ├── Notism.Infrastructure/   # Infrastructure Layer - Data Access
│   ├── Notism.Shared/           # Shared Layer - Common Utilities
│   └── Notism.Worker/           # Background Worker Service
├── tests/                        # Test Projects
├── docs/                         # Documentation
└── docker-compose.yml            # Container orchestration
```

### Layer Responsibilities

- **Notism.Api**: HTTP endpoints, authentication, middleware, error handling
- **Notism.Application**: Feature handlers (Login, Register, etc.), validation, orchestration
- **Notism.Domain**: User and RefreshToken aggregates, domain events, business rules
- **Notism.Infrastructure**: EF Core DbContext, repositories, email service, token service
- **Notism.Shared**: Result pattern, pagination models, extensions, exceptions
- **Notism.Worker**: Background services (token cleanup, scheduled tasks)

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

### 2. Configure Database

Update the connection string in `src/Notism.Api/appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=notism_db;Username=your_username;Password=your_password;Port=5432"
  }
}
```

### 3. Run Database Migrations

```bash
cd src/Notism.Infrastructure
dotnet ef database update --project ../Notism.Infrastructure/Notism.Infrastructure.csproj --startup-project ../Notism.Api/Notism.Api.csproj
```

### 4. Configure Application Settings

Update `src/Notism.Api/appsettings.Development.json` with your settings:

- **JWT Secret**: Generate a secure secret key
- **Resend API Key**: For email functionality
- **Client App URL**: Your frontend application URL

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

## Testing

### Run All Tests

```bash
dotnet test
```

### Run Specific Test Project

```bash
dotnet test tests/Notism.Domain.Tests/Notism.Domain.Tests.csproj
dotnet test tests/Notism.Application.Tests/Notism.Application.Tests.csproj
```

### Test Coverage

The project includes comprehensive test coverage:
- **Domain Tests**: 122+ tests covering business logic, value objects, and specifications
- **Application Tests**: Handler validation and business flow testing

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

### Application Settings

Key configuration sections in `appsettings.json`:

- **ConnectionStrings**: Database connection
- **JwtSettings**: JWT token configuration
- **Resend**: Email service API key
- **Email**: Email sender configuration
- **ClientApp**: Frontend application URL

### Environment-Specific Settings

- `appsettings.Development.json` - Development environment
- `appsettings.Production.json` - Production environment

### Secret Environment Variables

For security, sensitive configuration values should be stored as environment variables rather than in configuration files. The following secrets should be configured:

#### Required Secrets

- `ConnectionStrings__DefaultConnection` - PostgreSQL database connection string (includes password)
- `JwtSettings__Secret` - Secret key for JWT token signing (minimum 32 characters recommended)
- `Resend__ApiKey` - Resend email service API key

#### Optional Secrets

- `AWS__AccessKeyId` - AWS access key for S3 storage (if using AWS S3)
- `AWS__SecretAccessKey` - AWS secret key for S3 storage (if using AWS S3)

#### Setting Environment Variables

**Development (Local):**
```bash
# Using .NET User Secrets (recommended for local development)
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Database=notism_db;Username=user;Password=password;Port=5432"
dotnet user-secrets set "JwtSettings:Secret" "your-super-secret-jwt-key-minimum-32-characters"
dotnet user-secrets set "Resend:ApiKey" "re_your-resend-api-key"
```

**Production:**
```bash
# Set as environment variables in your hosting platform
export ConnectionStrings__DefaultConnection="Host=db.example.com;Database=notism_db;Username=user;Password=password;Port=5432"
export JwtSettings__Secret="your-production-jwt-secret-key"
export Resend__ApiKey="re_your-production-resend-api-key"
```

**Docker:**
```bash
# In docker-compose.yml or as environment variables
environment:
  - ConnectionStrings__DefaultConnection=Host=db;Database=notism_db;Username=user;Password=password;Port=5432
  - JwtSettings__Secret=your-jwt-secret
  - Resend__ApiKey=re_your-resend-api-key
```

> **⚠️ Security Note**: Never commit secrets to version control. Use environment variables, user secrets, or a secrets management service in production.