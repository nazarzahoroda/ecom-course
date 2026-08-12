You are running the **pending-prs** check for this repo. This command is
**read-only** — it must never comment on, review, approve, or modify any
PR. It only reports status.

Steps:

1. Run:
   ```
   gh pr list --state open --json number,title,author,createdAt,updatedAt,isDraft,statusCheckRollup,reviewDecision,closingIssuesReferences
   ```
2. Render the result as a compact table with columns:
   `#  | Title | Author | Draft | Checks | Review status | Linked issue | Updated`
   - "Checks" = pass/fail/pending summarized from `statusCheckRollup`.
   - "Review status" = `reviewDecision` (or "none" if null).
   - "Linked issue" = `#<number>` from `closingIssuesReferences` if present
     (the issue this PR will close via `Closes #N`), or "**none**" if the
     array is empty — call this out, since every task on the
     [Project board](https://github.com/users/nazarzahoroda/projects/1)
     is supposed to be a linked GitHub Issue.
3. Sort by `updatedAt` descending (most recently touched first) so stale
   PRs that haven't moved are easy to spot at the bottom.
4. After the table, list PRs that look actionable right now (checks
   green, no review yet) as a short "ready to review" callout.
5. Stop there. Do not run `/pr-reviews` automatically, even if a PR looks
   trivial — that command requires an explicit, separate invocation.
