# Best Practices

Rule files by category:

- [Repository Pattern (write boundary)](./repository-pattern.md)
- [Read Queries (per-handler query objects)](./read-queries.md)
- [Domain Events](./domain-events.md)
- [Handler Design](./handler-design.md)
- [Validation](./validation.md)
- [Error Handling](./error-handling.md)
- [Naming Conventions](./naming.md)
- [Code Organization](./code-organization.md)
- [Additional Practices](./additional-practices.md)

## Handler Logging

Inject `ILogger<THandler>` where a handler performs a meaningful state change or a notable read. Log at `Information` for successful outcomes and `Warning`/`Error` for handled failure branches. Keep message templates structured (named placeholders), not interpolated strings:

```csharp
// ✅ Structured template with named placeholder
_logger.LogInformation("Cleared existing items for user {UserId}", userId);

// ❌ Interpolated string
_logger.LogInformation($"Cleared existing items for user {userId}");
```
