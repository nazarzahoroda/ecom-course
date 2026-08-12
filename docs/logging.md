# Logging

> **Stack:** `Microsoft.Extensions.Logging` (`ILogger<T>`) via constructor
> injection, no third-party sink for now — the default ASP.NET Core console
> logger is enough for a teaching repo. If a real sink (Serilog, etc.) gets
> added later, the rules below don't change; only the sink config does.
>
> **Rule:** Use `ILogger<T>` via constructor injection. Never
> `Console.WriteLine` in committed code.

> **See also:** [review-rules.md § 8 (logging)](review-rules.md) for the
> PR-review checklist that enforces the rules in this document. For date
> values in log messages, see [dates-timezones.md](dates-timezones.md).

## Current state

Nothing in `src/` uses `ILogger<T>` yet — there's no logging at all,
including in `EcomCourse.Api/Middleware/GlobalExceptionHandler.cs`, which
builds a `ProblemDetails` response from an unhandled exception but never
logs it. That's the first real violation of this doc once it's adopted:
an unhandled exception should be logged (with the exception object, see
below) before the `ProblemDetails` response is written, otherwise a 500
in production leaves no trace anywhere.

## Usage

Inject `ILogger<T>` at the constructor, store in a `private readonly`
field, log structured:

```csharp
public class PlaceOrderCommandHandler(ILogger<PlaceOrderCommandHandler> logger)
    : ICommandHandler<PlaceOrderCommand, Result<OrderId>>
{
    private readonly ILogger<PlaceOrderCommandHandler> _logger = logger;

    public async Task<Result<OrderId>> Handle(PlaceOrderCommand command, CancellationToken ct)
    {
        _logger.LogInformation(
            "Placing order for customer {CustomerId} with {ItemCount} items",
            command.CustomerId,
            command.Items.Count);
        // ...
    }
}
```

The message template uses `{Name}` placeholders that map positionally to
the trailing arguments. Sinks that understand structured logging index the
named values for querying — interpolated strings (`$"..."`) collapse them
into one opaque message and lose that, even with just the console sink.

## Log levels

| Level         | Method           | When                                                                  |
| ------------- | ---------------- | ---------------------------------------------------------------------|
| `Trace`       | `LogTrace`       | Method-entry / fine-grained diagnostics. Off everywhere by default.  |
| `Debug`       | `LogDebug`       | Developer diagnostics. Off in production.                            |
| `Information` | `LogInformation` | Notable lifecycle events (order placed, payment captured).           |
| `Warning`     | `LogWarning`     | Recoverable problem; falling back to a default, retry succeeded.     |
| `Error`       | `LogError`       | Feature-breaking failure; request aborted, handler threw.            |
| `Critical`    | `LogCritical`    | Process-level failure; app cannot continue, startup failed.          |

### Logging exceptions

Always pass the exception as the **first** argument so the logger
captures the stack trace:

```csharp
catch (SqlException ex)
{
    _logger.LogError(ex, "Failed to persist order {OrderId}", orderId);
    throw;
}
```

Never log only `ex.Message` — you lose the stack and inner exceptions.

## Forbidden

- `Console.WriteLine`, `Trace.WriteLine`, `Debug.WriteLine` — use
  `ILogger<T>` instead; these aren't captured by any configured sink.
- Catching an exception only to log it and swallow it. Per the Result
  pattern used in this repo (see `CLAUDE.md`), an expected failure should
  become a `Result.Failure(...)`, not a caught-and-logged exception; an
  unexpected one should be logged and rethrown, not swallowed.
- Logging the same event at multiple levels for the same fact.

## PII and sensitive data

**Never log:**

- Passwords, connection strings, API keys, JWT tokens (raw or decoded).
- Full payment details (card numbers, CVV) — if payment integration is
  added, log a transaction/reference ID, never the card data itself.

**Be careful logging:**

- **Customer email/address/phone** — log the customer ID for correlation;
  log the email itself only when specifically diagnosing an
  email-delivery issue.
- **Free-text user input** (order notes, product reviews) — log an entity
  ID by default, not the text.

There's no automatic redaction layer configured — **the rule is: do not
pass these values into `_logger.*` calls in the first place.**

## Reference

- `Microsoft.Extensions.Logging` — https://learn.microsoft.com/dotnet/core/extensions/logging
- Message templates — https://messagetemplates.org/
