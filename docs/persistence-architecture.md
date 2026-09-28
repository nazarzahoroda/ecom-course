# Persistence Architecture

## Context

The application currently contains more than one persistence approach for write operations.

After the handlers orchestration refactoring, part of the application already uses an explicit Unit of Work:

- command handlers orchestrate the use case;
- repositories load and track aggregates;
- repositories do not own the commit boundary;
- `IUnitOfWork` performs `SaveChangesAsync` and manages explicit transactions when required.

The Order and Cart Checkout flows already follow this approach.

However, other write paths still persist changes directly inside infrastructure services. This currently includes parts of the Cart, Category, Product and Customer flows.

The application also uses two EF Core DbContexts:

- `EcomCourseDbContext` for domain data;
- `IdentityDbContext` for ASP.NET Core Identity and refresh tokens.

Both contexts use the same SQL Server connection string, but they have independent EF Core change trackers and persistence boundaries.

Because registration modifies both Identity and domain data, the current registration flow cannot be committed through the existing `IUnitOfWork` as one persistence operation. It therefore uses `CompensateAsync` to manually undo previously persisted changes when a later operation fails.

This results in multiple persistence idioms and makes atomic write operations harder to reason about.

## Decision

The application will use one persistence model for write use cases.

### Use-case orchestration

Command handlers own use-case orchestration.

A handler may coordinate domain aggregates, repositories and application services required by the use case.

Infrastructure services and repositories must not independently define the commit boundary for a write use case.

### Unit of Work

`IUnitOfWork` is the explicit persistence boundary for write use cases.

Repositories modify the EF Core change tracker but do not call `SaveChangesAsync` themselves.

Changes are persisted by the command handler through `IUnitOfWork.SaveChangesAsync`.

When a use case requires an explicit transaction across multiple operations, the transaction is managed through `IUnitOfWork`.

The existing Order and Cart Checkout implementation is the reference for this approach and should not be rewritten unnecessarily.

### DbContext

Domain persistence and ASP.NET Core Identity will use a single EF Core DbContext.

`EcomCourseDbContext` will become the common persistence context for:

- domain entities;
- ASP.NET Core Identity entities;
- refresh tokens.

Identity tables will continue to use the `identity` schema.

Using a single scoped DbContext allows domain and Identity changes participating in the same use case to share the same persistence and transaction boundary.

### Registration

Registration will use the common persistence boundary.

The registration use case should no longer depend on manually deleting already persisted data when a later operation fails.

After registration has been migrated to the common DbContext and Unit of Work, `CompensateAsync` will be removed.

### Cart concurrency

Cart checkout must use optimistic concurrency.

A concurrency token will be added to the Cart persistence model so that two concurrent checkout operations cannot both successfully persist changes based on the same Cart state.

A concurrency conflict must prevent the second checkout from creating another successfully committed Order.

## Consequences

Write paths that currently call `SaveChangesAsync` inside infrastructure services must be migrated to the Unit of Work persistence model.

This includes the remaining Cart, Category, Product and Customer write paths.

The separate `IdentityDbContext` and its dedicated migration configuration will be replaced by the common persistence context.

Existing Identity data and the `identity` schema must be preserved during this migration.

Dead or duplicate persistence implementations that are no longer used by dependency injection should be removed after their usages have been verified.

The existing Order and Cart Checkout Unit of Work implementation should be preserved and extended rather than replaced.

## Migration plan

1. Unify domain and Identity persistence under `EcomCourseDbContext`.
2. Update ASP.NET Core Identity registration to use the common DbContext.
3. Migrate registration to the common Unit of Work boundary and remove `CompensateAsync`.
4. Migrate the remaining Cart, Category, Product and Customer write paths to the same persistence model.
5. Remove verified dead or duplicate persistence code.
6. Add optimistic concurrency to Cart.
7. Add an integration test that performs two concurrent checkout attempts for the same Cart and verifies that exactly one Order is created.
8. Run the complete unit and integration test suites.
