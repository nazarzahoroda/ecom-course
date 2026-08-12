# Dates and Timezones

> **Stack:** native .NET `DateTime` / `TimeProvider` (.NET 8+). This is a
> single-server teaching app with no per-machine or per-user timezone
> model — the only zone that matters is UTC for storage/API, and whatever
> zone a customer-facing display wants to render in later (out of scope
> for now). Keep it simple: everything server-side is UTC.

> **See also:** [review-rules.md § 9 (dates)](review-rules.md) for the
> PR-review checklist that enforces the rules in this document. For date
> values in log messages, see [logging.md](logging.md).

## Current state

No entity in this repo has a date field yet (no `CreatedAt`/`UpdatedAt`,
no `DateTime` usage anywhere in `src/`). This doc is the target state to
follow from the first entity that needs one — order placement time,
promotion start/end, "cancel unpaid order after N minutes", etc. are all
coming.

## Rules

### "Now"

Prefer injecting `TimeProvider` (built into .NET 8+) over calling
`DateTime.UtcNow` directly, so handler/service logic is testable with a
fixed clock:

```csharp
public class PlaceOrderCommandHandler(TimeProvider timeProvider)
{
    public async Task<Result<OrderId>> Handle(PlaceOrderCommand command, CancellationToken ct)
    {
        var placedAt = timeProvider.GetUtcNow(); // DateTimeOffset, always UTC
        // ...
    }
}
```

If `TimeProvider` isn't wired up yet for a given call site, `DateTime.UtcNow`
is acceptable — but never `DateTime.Now`. `DateTime.Now` returns the
server's local time, which is environment-dependent and not guaranteed to
be UTC, and it can't be faked in a test without touching the system clock.

### Storage

Persisted timestamps (`CreatedAt`, `PlacedAt`, promotion windows, etc.)
should be `DateTimeOffset`, not `DateTime`. `TimeProvider.GetUtcNow()`
already returns a `DateTimeOffset`, so this is the natural fit and it
sidesteps the whole `DateTime.Kind` ambiguity problem (`Utc` /
`Unspecified` / `Local`) entirely — a `DateTimeOffset` always carries its
offset explicitly.

If a `DateTime` is used anywhere (e.g. a third-party library forces it),
every persisted value must carry `Kind = Utc`. EF Core does not validate
this for you — a `DateTime.SpecifyKind(value, DateTimeKind.Utc)` call is
only correct if you have proof the source value actually is UTC.

### Parsing

Never call `DateTime.Parse(stringInput)` or `DateTimeOffset.Parse(stringInput)`
without `CultureInfo.InvariantCulture`. The default culture is
environment-dependent and can flip day/month depending on the machine's
locale.

### API boundary

Requests and responses should use ISO 8601 with an explicit offset/UTC
marker (`...Z` or `±HH:mm`) — `DateTimeOffset`'s default
`System.Text.Json` serialization already produces this. Reject
timezone-naive date strings in DTO validation rather than silently
assuming UTC.

## Forbidden

- `DateTime.Now` anywhere in application/domain code — use `TimeProvider`
  or `DateTime.UtcNow`.
- `DateTime.Parse` / `DateTimeOffset.Parse` without
  `CultureInfo.InvariantCulture`.
- Persisting a `DateTime` with `Kind = Unspecified` or `Kind = Local`.
- Comparing two `DateTime` values with different `Kind` (or a `DateTime`
  against a `DateTimeOffset`) without an explicit conversion first.
- Hardcoded IANA timezone strings (`"Europe/Kyiv"`, etc.) outside test
  fixtures — there's no per-user/per-machine zone model yet, so a literal
  zone string in application code is almost certainly a shortcut that
  will need revisiting.

## Testing

Tests must not depend on the host machine's clock:

- Inject `TimeProvider` into services that read "now"; tests use
  `TimeProvider.System` in production wiring and a fake/fixed provider
  (e.g. `Microsoft.Extensions.Time.Testing.FakeTimeProvider`) in tests.
- Construct fixture values with an explicit offset:
  `new DateTimeOffset(2026, 5, 14, 12, 0, 0, TimeSpan.Zero)`.
