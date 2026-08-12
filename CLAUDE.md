# CLAUDE.md — EcomCourse

Context for Claude Code when working in this repository.

## What this repo is

A reference / teaching project: a .NET e-commerce backend built with Clean
Architecture + DDD + CQRS, used as the base project for a 5-student .NET
course. Nazar is the instructor, sole maintainer, and PR reviewer. Tasks
are tracked as GitHub Issues on a Project board (see Task tracking below),
not JIRA/AzDO, and there's no multi-person release process — this is a
solo-maintained teaching repo, not a production team repo.

## Stack

- .NET 10
- xUnit for tests
- MediatR for CQRS (custom marker interfaces on top — see below)
- Result pattern for business/domain failures (no throwing for expected
  failure paths)
- SQL Server via Docker for local dev

## Architecture rules (short version — full detail in docs/review-rules.md)

- Clean Architecture layering: Domain → Application → Infrastructure → Api.
  Dependencies only point inward. Domain has no framework references.
- CQRS via MediatR: every write is a Command, every read is a Query. Custom
  marker interfaces (`ICommand<TResult>`, `IQuery<TResult>`, etc.) wrap
  MediatR's `IRequest<TResult>` so handlers are explicit about intent.
- Result pattern: handlers return `Result<T>` / `Result`, not exceptions,
  for anything that's a normal business outcome (validation failure, not
  found, conflict). Exceptions are reserved for actual exceptional/
  programmer-error cases.
- DI lifetimes matter and get reviewed — don't register services with the
  wrong lifetime (singleton holding scoped dependencies, etc.).

## Cross-cutting rule docs

Treat these as part of the review checklist alongside the architecture
rules above — `/pr-reviews` cites specific sections from all three:

- [`docs/review-rules.md`](docs/review-rules.md) — the full severity-rated
  checklist (layering, CQRS shape, Result pattern, logging, dates, tests).
- [`docs/logging.md`](docs/logging.md) — `ILogger<T>` usage, structured
  templates, PII rules.
- [`docs/dates-timezones.md`](docs/dates-timezones.md) — UTC discipline,
  `TimeProvider` for testability, API boundary format.

## Task tracking

Tasks live on the [Ecom Course — Sprint Board](https://github.com/users/nazarzahoroda/projects/1)
(private GitHub Project, shared with the `ecom-course-client` frontend
repo). Per the board's own README:

- Every task is a GitHub Issue in `ecom-course` or `ecom-course-client`.
- Labels: `module:catalog` / `module:cart` / `module:orders` /
  `module:payments` / `module:auth`, plus `week-1`...`week-12`.
- Cards move Backlog → Ready → In progress → In review → Done.
- **A PR links to its issue via `Closes #N` / `Fixes #N` / `Resolves #N`
  in the description** — the card closes automatically on merge.

Issue bodies often include acceptance criteria (e.g. "Application project
builds", "design-note explaining X") — `/pr-reviews` reads the linked
issue via `closingIssuesReferences` and checks the PR against those, not
just the static checklist in `docs/review-rules.md`. A PR with no linked
issue isn't blocked for it (see `docs/review-rules.md` §15), but it does
mean `/pr-reviews` has no acceptance criteria to check the PR against
beyond the standard rules.

## Roles available in this repo

User-level skills already exist for **developer** and **reviewer**.
Project-scoped roles added here (see `.claude/commands/`):

- `/architect` — for CQRS/Clean Architecture structuring decisions. Use
  this when deciding how a new feature should be sliced into
  commands/queries/handlers, where a marker interface or abstraction
  belongs, or whether something violates layering.
- `/pending-prs` — read-only status table of open PRs. Never comments,
  never reviews, just lists.
- `/pr-reviews` — does the actual review of a PR's diff against
  `docs/review-rules.md`, and posts the review via `gh`. **Manual trigger
  only.** See that command file for why.

## Explicit non-goals

- No hooks, no cron/schedule-triggered review. Every PR review is a human
  (Nazar) running `/pr-reviews` on purpose. Nothing posts to GitHub without
  Nazar having triggered it in that session.
- No AzDO/JIRA integration — this repo uses `gh` CLI against GitHub only.
