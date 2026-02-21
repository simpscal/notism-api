# Notism API - Clean Architecture Implementation

## 📋 Table of Contents

1. [Architecture Overview](#architecture-overview)
2. [Layer Definitions](#layer-definitions)
3. [Dependency Rules](#dependency-rules)
4. [Implementation Guidelines](#implementation-guidelines)
5. [Folder Structure](#folder-structure)

---

## Architecture Overview

### The Onion Architecture

```
    ┌─────────────────────────────────────────────────────────────────────┐
    │                      API & Infrastructure                           │
    │          Minimal APIs, Endpoints    │    Data Access, Services      │
    │  ┌───────────────────────────────────────────────────────────────┐  │
    │  │                      Application Layer                        │  │
    │  │              Handlers (CQRS), Request/Response                │  │
    │  │  ┌─────────────────────────────────────────────────────────┐  │  │
    │  │  │                     Domain Layer                        │  │  │
    │  │  │         Aggregates, Entities, Value Objects,            │  │  │
    │  │  │              Business Rules, Interfaces                 │  │  │
    │  │  │  ┌───────────────────────────────────────────────────┐  │  │  │
    │  │  │  │                  Shared Layer                     │  │  │  │
    │  │  │  │         Common Utilities, Extensions,             │  │  │  │
    │  │  │  │           Constants, Result Pattern               │  │  │  │
    │  │  │  └───────────────────────────────────────────────────┘  │  │  │
    │  │  └─────────────────────────────────────────────────────────┘  │  │
    │  └───────────────────────────────────────────────────────────────┘  │
    └─────────────────────────────────────────────────────────────────────┘
```

## Layer Definitions

### 🎯 Domain Layer (Core)

**Purpose**: Contains business logic organized as aggregates that represent cohesive business concepts and maintain consistency boundaries.

#### Responsibilities

- **Aggregate Roots** - Entry points to aggregate boundaries with business behavior
- **Entities** - Objects with identity within aggregates
- **Value Objects** - Immutable objects representing domain concepts
- **Domain Events** - Represent important business occurrences within aggregates
- **Domain Services** - Business logic that doesn't belong to a single aggregate
- **Business Rules** - Invariants and constraints enforced within aggregate boundaries
- **Repository Interfaces** - Contracts for persisting aggregates

#### Methodology

1. **Identify Aggregates First**: Group related entities around a root entity that owns the lifecycle
2. **Protect Invariants**: All business rules are enforced within the aggregate boundary
3. **Use Value Objects for Concepts**: Email, Password, Money - immutable and self-validating
4. **Raise Domain Events**: Communicate important state changes to other parts of the system
5. **Keep Aggregates Small**: Only include what's needed for consistency
6. **Define Repository Interfaces Here**: The domain dictates what persistence operations it needs

#### Guidelines

| Do | Don't |
|---|---|
| Encapsulate business rules in aggregate methods | Expose setters or allow direct state modification |
| Use private constructors with factory methods | Allow invalid aggregate creation |
| Validate in value object constructors | Perform validation in application layer |
| Keep aggregates focused on one concept | Create large aggregates with many entities |
| Reference other aggregates by ID only | Include references to other aggregate roots |

---

### 🔧 Application Layer (Command/Query Handlers)

**Purpose**: Orchestrates domain operations through CQRS handlers that process commands and queries using MediatR, providing a clear separation between read and write operations.

#### Responsibilities

- **Command Handlers** - Process write operations (Create, Update, Delete)
- **Query Handlers** - Process read operations (Get, Search, List)
- **Request/Response Models** - Input and output models for handlers
- **Validation** - Input validation using FluentValidation
- **Business Orchestration** - Coordinate between multiple domain aggregates
- **Exception Handling** - Throw ResultFailureException for business rule violations
- **Domain Event Handling** - React to domain events for cross-aggregate operations

#### Methodology

1. **One Handler per Use Case**: Each business operation gets its own handler
2. **Validate Early**: Use FluentValidation to check inputs before processing
3. **Orchestrate, Don't Implement**: Handlers coordinate domain objects, not contain business logic
4. **Use Result Pattern**: Return `Result<T>` for explicit success/failure handling
5. **Map at Boundaries**: Transform between domain entities and response DTOs
6. **Feature-Based Organization**: Group related handlers in feature folders (Auth/, User/, etc.)

#### Handler Flow Pattern

```
Request → Validator → Handler → Domain Operations → Repository → Response
```

1. Receive typed request via MediatR
2. Validate using FluentValidation pipeline behavior
3. Load aggregates via repository
4. Execute domain methods (business logic lives in domain)
5. Persist changes
6. Map to response DTO and return

#### Guidelines

| Do | Don't |
|---|---|
| Keep handlers thin and focused | Put business logic in handlers |
| Use specifications for queries | Write raw query logic in handlers |
| Throw `ResultFailureException` for business violations | Return error codes or null |
| Map domain entities to DTOs | Return domain entities directly |
| Use `EnumConverter` for string↔enum conversion | Hard-code enum string values |

---

### 🌐 API Layer (Minimal APIs)

**Purpose**: Handles HTTP communication using Minimal APIs with feature-based endpoint organization, delegating request processing to MediatR handlers and managing cross-cutting concerns through middleware.

#### Responsibilities

- **Minimal API Endpoints** - Feature-based HTTP endpoints using MediatR
- **Authentication & Authorization** - JWT-based auth with role-based access control
- **Error Handling** - Global exception handling via middleware
- **Request Validation** - Automatic validation through FluentValidation pipeline
- **API Documentation** - OpenAPI/Swagger integration
- **CORS Configuration** - Cross-origin resource sharing setup

#### Methodology

1. **Delegate to MediatR**: Endpoints only translate HTTP to handler requests
2. **Group by Feature**: Use `MapGroup()` for related endpoints
3. **Handle Cross-Cutting Concerns in Middleware**: Auth, logging, error handling
4. **Use Attributes for Authorization**: `[RequireAdmin]` for role-based access
5. **Consistent Response Pattern**: Success returns data, failure returns error message

#### Endpoint Flow Pattern

```
HTTP Request → Middleware → Endpoint → MediatR → Handler → Response
```

1. Request enters through middleware pipeline (auth, logging)
2. Endpoint extracts request data and creates MediatR request
3. `ISender.Send()` dispatches to appropriate handler
4. Result is translated to HTTP response

#### Guidelines

| Do | Don't |
|---|---|
| Keep endpoints thin - just HTTP translation | Put business logic in endpoints |
| Use `ISender` from MediatR | Inject repositories directly |
| Apply authorization attributes | Check roles manually in endpoint code |
| Return appropriate HTTP status codes | Always return 200 |
| Document with OpenAPI attributes | Skip API documentation |

---

### 🏗️ Infrastructure Layer (External Concerns)

**Purpose**: Implements interfaces defined by inner layers and provides aggregate persistence, handling aggregate boundaries correctly while managing external dependencies.

#### Responsibilities

- **Aggregate Repositories** - Persist and retrieve complete aggregates
- **External Services** - HTTP clients, message queues, file systems
- **Event Dispatching** - Publish domain events after aggregate persistence
- **Database Context** - Manage aggregate configurations and relationships
- **Caching** - Cache complete aggregates when appropriate
- **Configuration** - Settings, environment variables

#### Methodology

1. **Implement Domain Interfaces**: Repository implementations live here
2. **Load Complete Aggregates**: Include all entities within the aggregate boundary
3. **Dispatch Domain Events After Save**: Ensure events only fire after successful persistence
4. **Use Entity Type Configurations**: Keep DbContext clean with separate configs
5. **External Services as Interfaces**: Define in Application, implement in Infrastructure

#### Repository Pattern

```
Handler → Repository Interface (Domain) → Repository Implementation (Infrastructure) → DbContext
```

1. Handler depends on `IOrderRepository` (defined in Domain)
2. `OrderRepository` implements the interface (in Infrastructure)
3. Repository uses `DbContext` to persist/retrieve aggregates
4. Domain events are collected and dispatched after `SaveChangesAsync()`

#### Guidelines

| Do | Don't |
|---|---|
| Load aggregates with all their entities | Return partial aggregates |
| Use `Include()` for aggregate children | Lazy load across aggregate boundaries |
| Dispatch events after successful save | Fire events before persistence |
| One repository per aggregate root | Create repositories for entities |
| Use specifications for query logic | Duplicate query logic across repositories |

---

### 🔧 Shared Layer (Common Utilities)

**Purpose**: Provides common utilities, extensions, constants, and helper classes that can be used across all layers without creating dependencies between business layers.

#### Responsibilities

- **Extension Methods** - Common extensions for built-in types
- **Constants** - Application-wide constants
- **Utilities** - Helper classes and static methods
- **Common Models** - Generic result types, pagination models
- **Validation Helpers** - Reusable validation logic
- **Enumerations** - Common enums used across layers

#### Methodology

1. **No Business Logic**: Only generic utilities that could be in any project
2. **Self-Contained**: No dependencies on other project layers
3. **Static Where Appropriate**: Extension methods and utilities can be static
4. **Well-Tested**: These are used everywhere, ensure reliability

#### Guidelines

| Do | Don't |
|---|---|
| Put truly cross-cutting concerns here | Put domain-specific logic here |
| Use for Result pattern, Pagination | Put entity definitions here |
| Keep dependencies minimal | Reference Domain or Application |
| Make utilities generic and reusable | Create single-use helpers |

---

## Implementation Guidelines

### Adding a New Feature Checklist

1. **Domain Layer**
   - [ ] Define/update aggregate root with business methods
   - [ ] Create value objects if needed
   - [ ] Add domain events for important state changes
   - [ ] Define repository interface if new aggregate
   - [ ] Add specifications for query patterns

2. **Application Layer**
   - [ ] Create feature folder (e.g., `Order/CreateOrder/`)
   - [ ] Define Request with `IRequest<Result<Response>>`
   - [ ] Create Validator with FluentValidation rules
   - [ ] Implement Handler orchestrating domain operations
   - [ ] Define Response DTO
   - [ ] Add mapping profile if using AutoMapper

3. **Infrastructure Layer**
   - [ ] Implement repository if new aggregate
   - [ ] Add entity configuration for EF Core
   - [ ] Create migration if schema changed
   - [ ] Implement external service if needed

4. **API Layer**
   - [ ] Add endpoint in appropriate feature file
   - [ ] Configure route, HTTP method, authorization
   - [ ] Add OpenAPI documentation attributes

### Error Handling Strategy

| Layer | Error Type | Approach |
|-------|-----------|----------|
| Domain | Business Rule Violation | Throw domain exception or return failure |
| Application | Validation/Business | Throw `ResultFailureException` |
| Infrastructure | External Service | Wrap in domain-meaningful exception |
| API | HTTP | `ResultFailureMiddleware` converts to 400 |

### Testing Strategy

| Layer | Test Focus | Examples |
|-------|-----------|----------|
| Domain | Business rules, invariants | Aggregate creation, state transitions |
| Application | Handler orchestration | Mock repositories, verify flow |
| Infrastructure | Data access | Integration tests with test DB |
| API | HTTP contracts | Integration tests with test server |

---

## Folder Structure

### Notism API - Current Project Structure

```
notism-api/
├── .dockerignore                                   # Docker ignore patterns
├── .editorconfig                                   # Editor configuration
├── .gitignore                                      # Git ignore patterns
├── .vscode/                                        # VS Code settings
├── Directory.Build.props                           # Global build properties
├── Directory.Packages.props                        # Central package management
├── Dockerfile                                      # API container definition
├── Notism.sln                                      # Solution file
├── README.md                                       # Project documentation
├── docker-compose.yml                              # Container orchestration
├── nginx.conf                                      # Reverse proxy configuration
│
├── docs/
│   └── architecture.md                            # This document
│
├── src/
│   ├── Notism.Shared/                             # 🔧 Shared Layer
│   │   ├── Attributes/
│   │   ├── Constants/
│   │   ├── Exceptions/
│   │   │   └── ResultFailureException.cs
│   │   ├── Extensions/
│   │   ├── Models/
│   │   │   ├── Pagination.cs
│   │   │   └── Result.cs
│   │   └── Notism.Shared.csproj
│   │
│   ├── Notism.Domain/                             # 🎯 Domain Layer
│   │   ├── Common/
│   │   │   ├── AggregateRoot.cs
│   │   │   ├── DomainEvent.cs
│   │   │   ├── Entity.cs
│   │   │   ├── ValueObject.cs
│   │   │   ├── Enums/
│   │   │   ├── Interfaces/
│   │   │   │   ├── IDomainEvent.cs
│   │   │   │   ├── IRepository.cs
│   │   │   │   ├── ISpecification.cs
│   │   │   │   └── IUnitOfWork.cs
│   │   │   └── Specifications/
│   │   │
│   │   ├── User/                                  # User Aggregate
│   │   │   ├── User.cs                            # Aggregate Root
│   │   │   ├── IUserRepository.cs
│   │   │   ├── PasswordResetToken.cs              # Entity
│   │   │   ├── Enums/
│   │   │   │   └── UserRole.cs
│   │   │   ├── Events/
│   │   │   │   ├── PasswordResetCompletedEvent.cs
│   │   │   │   ├── PasswordResetRequestedEvent.cs
│   │   │   │   ├── UserCreatedEvent.cs
│   │   │   │   ├── UserPasswordChangedEvent.cs
│   │   │   │   └── UserProfileUpdatedEvent.cs
│   │   │   └── ValueObjects/
│   │   │       ├── Email.cs
│   │   │       └── Password.cs
│   │   │
│   │   ├── RefreshToken/                          # RefreshToken Aggregate
│   │   │   ├── RefreshToken.cs
│   │   │   └── IRefreshTokenRepository.cs
│   │   │
│   │   └── Notism.Domain.csproj
│   │
│   ├── Notism.Application/                        # 🔧 Application Layer
│   │   ├── Common/
│   │   │   ├── Behaviors/
│   │   │   │   └── ValidationBehavior.cs
│   │   │   ├── Constants/
│   │   │   ├── Interfaces/
│   │   │   │   ├── IEmailService.cs
│   │   │   │   ├── IPasswordService.cs
│   │   │   │   └── ITokenService.cs
│   │   │   ├── Mappings/
│   │   │   │   └── MappingProfile.cs
│   │   │   └── Utilities/
│   │   │       └── EnumConverter.cs               # Generic enum converter
│   │   │
│   │   ├── Auth/                                  # Authentication Features
│   │   │   ├── AuthMappingProfile.cs
│   │   │   ├── Login/
│   │   │   │   ├── LoginHandler.cs                # CQRS Handler
│   │   │   │   ├── LoginRequest.cs
│   │   │   │   ├── LoginRequestValidator.cs
│   │   │   │   └── LoginResponse.cs
│   │   │   ├── Register/
│   │   │   │   ├── RegisterHandler.cs
│   │   │   │   ├── RegisterRequest.cs
│   │   │   │   ├── RegisterRequestValidator.cs
│   │   │   │   └── RegisterResponse.cs
│   │   │   ├── RefreshToken/
│   │   │   │   ├── RefreshTokenHandler.cs
│   │   │   │   └── RefreshTokenRequest.cs
│   │   │   ├── RequestPasswordReset/
│   │   │   │   ├── RequestPasswordResetHandler.cs
│   │   │   │   ├── RequestPasswordResetRequest.cs
│   │   │   │   ├── RequestPasswordResetRequestValidator.cs
│   │   │   │   └── RequestPasswordResetResponse.cs
│   │   │   └── ResetPassword/
│   │   │       ├── ResetPasswordHandler.cs
│   │   │       ├── ResetPasswordRequest.cs
│   │   │       ├── ResetPasswordRequestValidator.cs
│   │   │       └── ResetPasswordResponse.cs
│   │   │
│   │   ├── User/                                  # User Management Features
│   │   │   └── UpdateProfile/
│   │   │       ├── UpdateUserProfileHandler.cs
│   │   │       ├── UpdateUserProfileRequest.cs
│   │   │       ├── UpdateUserProfileRequestValidator.cs
│   │   │       └── UpdateUserProfileResponse.cs
│   │   │
│   │   ├── Food/                                  # Food Management Features
│   │   │   ├── GetFoods/
│   │   │   │   ├── GetFoodsHandler.cs
│   │   │   │   ├── GetFoodsRequest.cs
│   │   │   │   ├── GetFoodsResponse.cs
│   │   │   │   └── GetFoodsSpecification.cs        # Specification in Application layer
│   │   │   └── ...
│   │   │
│   │   ├── Order/                                 # Order Management Features
│   │   │   ├── GetAdminOrdersForKanban/
│   │   │   │   ├── GetAdminOrdersForKanbanHandler.cs
│   │   │   │   ├── GetAdminOrdersForKanbanRequest.cs
│   │   │   │   ├── GetAdminOrdersForKanbanResponse.cs
│   │   │   │   └── GetAdminOrdersForKanbanSpecification.cs
│   │   │   └── ...
│   │   │
│   │   ├── DependencyInjection.cs
│   │   └── Notism.Application.csproj
│   │
│   ├── Notism.Infrastructure/                     # 🏗️ Infrastructure Layer
│   │   ├── Common/
│   │   │   ├── AppDbContext.cs
│   │   │   ├── Repository.cs                      # Generic repository
│   │   │   └── UnitOfWork.cs
│   │   │
│   │   ├── Migrations/                            # EF Core migrations
│   │   │   ├── 20241025123456_InitialCreate.cs
│   │   │   ├── 20241025234567_AddPasswordResetToken.cs
│   │   │   └── 20241026034538_ConvertUserRoleToString.cs
│   │   │
│   │   ├── RefreshTokens/
│   │   │   └── RefreshTokenRepository.cs
│   │   │
│   │   ├── Services/
│   │   │   ├── EmailService.cs
│   │   │   ├── PasswordService.cs
│   │   │   └── TokenService.cs                    # JWT generation with role claims
│   │   │
│   │   ├── Users/
│   │   │   └── UserRepository.cs
│   │   │
│   │   ├── DependencyInjection.cs
│   │   └── Notism.Infrastructure.csproj
│   │
│   └── Notism.Api/                                # 🌐 API Layer
│       ├── .env.development                       # Development environment variables (production: CI/CD pipeline)
│       │
│       ├── Attributes/
│       │   └── RequireAdminAttribute.cs           # Role-based authorization
│       │
│       ├── Endpoints/
│       │   ├── AuthEndpoints.cs                   # Authentication endpoints
│       │   └── UserEndpoints.cs                   # User management endpoints
│       │
│       ├── Middlewares/
│       │   └── ResultFailureMiddleware.cs         # Exception handling
│       │
│       ├── Models/
│       │   └── ErrorResponse.cs
│       │
│       ├── Properties/
│       │   └── launchSettings.json
│       │
│       ├── appsettings.json
│       ├── appsettings.Development.json
│       ├── appsettings.Production.json
│       ├── DependencyInjection.cs
│       ├── Program.cs                             # Minimal API configuration
│       └── Notism.Api.csproj
│
└── tests/                                         # 🧪 Test Projects
    ├── Notism.Application.Tests/
    │   ├── Auth/
    │   │   ├── Login/
    │   │   ├── Register/
    │   │   └── RefreshToken/
    │   ├── BaseTest.cs
    │   └── Notism.Application.Tests.csproj
    │
    └── Notism.Domain.Tests/                       # Comprehensive domain testing (122 tests)
        ├── RefreshToken/
        │   └── RefreshTokenTests.cs
        ├── User/
        │   ├── UserTests.cs                        # User aggregate tests
        │   ├── EmailTests.cs                       # Email value object tests
        │   ├── PasswordTests.cs                    # Password value object tests
        │   └── UserSpecificationTests.cs           # Specification pattern tests
        └── Notism.Domain.Tests.csproj
```

### Key Project Structure Highlights

#### 🎯 **Domain-Driven Design Organization**
- **Aggregates**: User and RefreshToken with clear boundaries
- **Value Objects**: Email and Password with business validation
- **Domain Events**: Comprehensive event handling for business actions
- **Specifications**: Encapsulated query logic located in Application layer feature folders

#### 🔧 **CQRS Implementation**
- **Handlers**: Separate command and query handlers for each feature
- **Validation**: FluentValidation validators for each request
- **Responses**: Dedicated response models for each operation
- **Feature Folders**: Authentication and User management organized by business capability

#### 🏗️ **Infrastructure Patterns**
- **Repository Pattern**: One repository per aggregate root
- **Unit of Work**: Transaction management and change tracking
- **Services**: External concerns (Email, Password hashing, JWT tokens)
- **Migrations**: Version-controlled database schema changes

#### 🌐 **API Design**
- **Minimal APIs**: Feature-based endpoint organization
- **Middleware**: Cross-cutting concerns (error handling, logging)
- **Authorization**: Role-based access control with custom attributes
- **Configuration**: Environment-specific settings management

#### 🧪 **Testing Strategy**
- **Domain Tests**: 122+ tests covering business logic and rules
- **Application Tests**: Handler validation and business flow testing
- **Separation**: Clear test organization matching source structure

This structure demonstrates clean architecture principles with proper separation of concerns, dependency inversion, and comprehensive testing coverage.
