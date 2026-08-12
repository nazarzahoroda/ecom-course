# Review Rules — EcomCourse

Concrete checklist `/pr-reviews` cites findings against. Every comment it
posts should be traceable to one of these rules — if it isn't, it's an
opinion, not a finding, and should be phrased as a question/suggestion, not
a blocking issue.

## Blocking (request changes)

1. **Layering violations** — Domain project references Application,
   Infrastructure, or any framework package (EF Core, ASP.NET, etc.).
   Application references Infrastructure or Api directly instead of via
   an abstraction defined in Application/Domain.
2. **CQRS shape violations** — a Command that returns query-shaped data
   for no reason, a Query that mutates state, a handler that does both a
   read and a write without being explicitly modeled as such.
3. **Result pattern bypassed** — throwing exceptions for expected business
   failures (not found, validation, conflict) instead of returning
   `Result.Failure(...)`. Conversely: swallowing an exception into a
   generic `Result.Failure` without any error detail.
4. **Dead/incorrect registrations** — services registered in DI that are
   never resolved, or registered with a lifetime that doesn't match how
   they're consumed (e.g. singleton depending on scoped `DbContext`).
5. **Broken run/debug config** — invalid or copy-pasted `launchSettings.json`
   that doesn't match the actual project (wrong port, wrong profile name,
   leftover template values).
6. **Leaked concerns across layers** — e.g. a stray WPF/UI namespace import,
   a UI-only package reference, or presentation-layer types appearing in
   Domain/Application.
7. **No tests for new handler logic** — new Command/Query handler with
   non-trivial branching and no xUnit test covering at least the
   success + one failure path.
8. **Logging violations** — see [logging.md](logging.md): `Console.WriteLine`/
   `Trace.WriteLine` instead of `ILogger<T>`; logging PII (passwords,
   connection strings, tokens, raw customer email/address/payment data);
   catching an exception only to log-and-swallow it instead of returning
   `Result.Failure(...)` or rethrowing.
9. **Date/timezone violations** — see [dates-timezones.md](dates-timezones.md):
   `DateTime.Now` instead of `TimeProvider`/`DateTime.UtcNow`;
   `DateTime.Parse`/`DateTimeOffset.Parse` without
   `CultureInfo.InvariantCulture`; persisting a `DateTime` with
   `Kind = Unspecified` or `Kind = Local`; comparing dates of different
   `Kind` without an explicit conversion.

## Suggestion (comment, non-blocking)

10. Naming that doesn't match the Command/Query/Handler/Result convention
    used elsewhere in the repo.
11. Missing or misleading XML doc / summary on public handler classes.
12. Magic strings/numbers that should be constants or config.
13. Overly broad `catch` blocks that could hide the specific failure mode.
14. Log message uses string interpolation (`$"..."`) instead of structured
    `{Name}` placeholders — see [logging.md](logging.md).
15. **No linked tracking issue** — the PR description doesn't close a
    GitHub Issue (`Closes #N` / `Fixes #N` / `Resolves #N`), i.e.
    `closingIssuesReferences` is empty. Every task on the
    [Project board](https://github.com/users/nazarzahoroda/projects/1) is
    a GitHub Issue; PRs are expected to link back to one so the card
    closes automatically on merge. Suggestion-tier, not Blocking — ask
    the author to add the link rather than holding up the PR for it.

## Nit (comment, clearly labeled "nit:")

16. Formatting, ordering of usings, minor naming style (casing, plurals).

## What NOT to flag

- Personal style preferences not covered by a rule above.
- Anything that's a legitimate simplification for teaching purposes (this
  is a course project, not production code — don't demand production-grade
  resilience/observability unless the lesson is specifically about that).

## Output format for `/pr-reviews`

For each finding: `severity | file:line | rule # | one-line explanation`.
Summarize at the top with a verdict: Approve / Comment / Request changes,
and *why* in one sentence, before the itemized list.
