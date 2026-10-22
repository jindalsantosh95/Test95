# Goal document delivery

- Status: completed
- Owner: Codex
- Created: 2026-10-06
- Scope: sequence-goal workflows, scripts, and delivery guidance

## Objective

Use the goal document on its delivery branch as the recovery record. Production
and QA publish their own entries through short, retryable Git commits.

## Acceptance Criteria

- No encoded production recovery payloads in implementation commit messages.
- Interrupted and incomplete attempts resume the existing branch and PR.
- Concurrent document updates preserve both agents' entries and requirements.
- Incomplete production remains InProgress and does not dispatch QA.
- Completed verification is reused only for the production commit and goal it verified.
- Agents publish their status directly, without a separate ledger workflow.

## Decisions

- Git commits supply history; the document supplies current state and remaining work.
- Keep existing repository/PR identity validation and QA commit binding.
- Keep delivery state and sanitized attempt costs together in the goal document.
- Restart is explicit. Remove automatic continuation chains and reservation limits;
  existing per-run resource limits still apply.

## Validation

- `scripts/verify.ps1 -ScopePath . -Base origin/main -SkipBuild -SkipDeploymentCheck`: passed.
- Production readiness exercise: 27 cases and 56 workflow PowerShell blocks passed.
- Branch registration, QA push-before-PR interruption/restart, and associated-PR recorder exercises: passed.
- Real Git exercise: interrupted restart, concurrent production/QA pushes, idempotent
  updates, stale verification rejection, authenticated direct PR recording, and QA
  sensor changes preserving newer production progress passed.

## Outcome

Removed the separate ledger workflow, encoded recovery trailers, and automatic
production continuation chain. Existing PR identity, per-run budget, and QA
commit-binding checks remain. Hosted workflow execution will be validated through
PR checks and the first delivery run after merge; no live goal was restarted here.

## Document-only follow-up

Removed placeholder registration commits and PR readiness/pending markers. Register
branches before execution and create child PRs after substantive work. Replace the
resource-retention workflow and its dispatcher with direct document updates. Retain
PR ownership validation, production commit binding, budget limits, and diagnostic
artifacts. Registration and readiness exercises now test the document contract.

Follow-up validation also covers idempotent production/QA cost recording and restored
aggregate totals from document entries. Workflow YAML and 20 embedded JavaScript
blocks parse successfully. Existing production and QA identity checks remain enforced.

## Coordinator cleanup

Removed origin records and hash/ancestry recovery, persistent goal locks and attempt
counters, coordinator JSON summaries and PR-body summary pointers, and QA continuation
summary files. Coordinator state and verification now live in the goal document;
explicit restart reads that document. The monitor locates goals through authenticated
open coordinator PRs, so closed/merged branches need no origin tombstone. Removed
old branch-name and PR-metadata compatibility paths. Review ownership checks,
per-run resource budgets, and scoped agent publication boundaries remain.

Coordinator cleanup validation: full root harness passed; actual coordinator-step
restarts ignore obsolete sidecars and preserve production/QA entries. The monitor
exercise runs workflow discovery against mocked GitHub responses and covers trusted
open PRs, closed/untrusted/abandoned exclusion, and Complete publication recovery.
Both comparison diagrams render with the pinned Mermaid CLI 11.12.0.

## Final repository cleanup

Removed the 13 tracked obsolete lock and run-summary files. Their history remains
in Git; the document-token goal now links to its historical verification record
at the prior commit. Removed dead harness assertions for the deleted retention
workflow and origin cleanup, corrected repair guidance, and renamed monitor and
coordinator variables to describe the goal document and discovered branches.
Reset guidance now identifies delivery through document entries and Git history,
and clears Delivery progress when explicitly resetting a goal to Draft.

Historical execution plans, the original-flow comparison, regression fixtures,
and supported review/diagnostic evidence remain intentionally. This cleanup does
not migrate or restart remote goal branches.

## Review corrections

QA goal edits now use the trusted pre-agent commit as the merge base, even when
the publication checkout advances. Only sensor and acceptance evidence rows may
change; task state, requirements, resource records, and the index are protected.
Production completion compares the verified requirements digest inside each
optimistic edit, including retries after a concurrent push. Readiness derives the
current digest from the fetched goal and checks it again before QA dispatch.

Regression coverage includes a real concurrent requirements change during the
completion push, a publication checkout newer than the QA baseline, rejected
QA state/requirements forgery, and production remote-head and PR convergence
checks. The removed bootstrap implementation remains removed. Retry exhaustion
reports the configured attempt count, and branch-registration guidance now
consistently describes PR creation after substantive work.

Additional review corrections publish a base branch with an expected-ref lease
before recording ownership, reject closed/merged QA branch history as missing-PR
recovery, and record QA completion only after generated restore/build succeed.
Real Git registration tests cover collision rejection without document mutation
and a closed QA PR starting a clean retry branch. The QA completion gate is tested
with successful, failed, and skipped restore/build outcomes.

## QA reuse and completion bindings

Separated QA readiness from the production identity schema. QA now records its
verified PR head and requirements digest alongside the tested production head;
restore/build use a pinned published QA checkout, and every document-update retry
revalidates the QA PR head and goal digest. The monitor checks production and QA
bindings before the open/merged split. Evidence rows are excluded from requirement
identity so ordinary QA evidence publication does not invalidate production.
Merged historical QA PRs are always cleared before selecting a new registration,
including attestation-only attempts.

Regression coverage exercises the real dispatcher, monitor binding gates for open
and merged PRs, stale QA completion rejection, evidence-only digest stability, and
attestation publication from a fresh registered retry branch.

## Finalization freshness

Monitor readiness and merged-coordinator finalization now validate both task
digests against the current goal, along with the live production/QA commits.
Finalization refuses a moved main ref instead of rebasing stale completion
evidence. Regression cases execute both workflow gates for changed requirements,
missing/stale digests, and changed child commits, with valid evidence-only edits
still accepted. Digest parity is checked against the PowerShell production helper
for LF/CRLF documents and lifecycle/evidence sections.

Complete-goal publication recovery now passes through the same child metadata and
binding checks as in-progress monitoring. The goal is read at an immutable
coordinator commit, and readiness requires the live coordinator head to remain
unchanged. Full monitor exercises cover valid recovery, changed requirements,
changed production/QA heads, stale QA bindings, missing state, and coordinator
movement; only valid recovery marks the PR ready.

Draft coordinator publication retains the completion compatibility marker required
by completion, finalization, and review workflows. Reusing an authenticated draft
repairs a missing marker without replacing its existing body. This marker identifies
the PR to lifecycle consumers; the goal document remains authoritative for status.

## Hosted authentication correction

Darwinium run `37466454443` passed coordinator setup but failed production
registration because the document updater appended a second Authorization header
to the checkout's existing Git configuration. Each updater network command now
resets inherited headers before selecting its token. Inspection also found an
unauthenticated QA verification fetch after publisher credentials were removed;
that fetch now supplies temporary authentication. Native Git configuration and
local repository exercises cover inherited checkout/environment credentials,
coordinator-token precedence, fallback credentials, rejected-push inspection, and
the actual QA workflow fetch. Hosted delivery must still be rerun after rollout.
