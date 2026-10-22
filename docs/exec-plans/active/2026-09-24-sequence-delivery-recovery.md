# Restart-safe sequence delivery

- Status: implemented; local validation complete
- Owner: EAPI harness maintainers
- Created: 2026-09-24
- Scope: production delivery, feedback serialization, checkpoint recovery and regression exercises

## Objective

Recover interrupted production publication without repeating generation, and continue incomplete production within a bounded budget while withholding QA until verified readiness.

## Acceptance Criteria

- Retry stale PR metadata only while the authoritative remote branch remains the expected commit.
- Production and associated feedback writers share a coordinator/surface/repository lock.
- Store outcome, goal identity, tree, verification and progress in a checkpoint commit pushed with implementation.
- Recover a pushed branch even if interrupted before PR creation; do not resurrect closed PRs.
- Replay interrupted PR publication without generating code again; resume published incomplete work with recorded remaining criteria.
- Bound automatic continuations and stop on Blocked, no progress, exhausted attempts or failed budget validation.
- Exercise interruption boundaries with real temporary Git repositories and mocked GitHub metadata, without contacting production environments.

## Decisions

- Preserve workflow-owned checkpoint metadata in a commit trailer rather than an agent-editable tracked file. A checkpoint is valid only at its exact commit, for its unchanged tree and semantic goal identity.
- A normal push must protect against concurrent branch movement; no force push during recovery.
- Local edits lost before a successful push are not recoverable; never claim otherwise.

## Validation

- Production recovery exercise: 81 passing cases using real temporary Git repositories and mocked GitHub calls, covering a fresh-runner clone after push, stale PR reads, remote movement, goal/tree/parent changes, partial/complete replay, durable attempt reservations, continuation bounds, missing-PR registration and QA reuse.
- Production readiness exercise: 47 passing cases and 62 workflow PowerShell blocks parsed, including execution of the actual attempt-reservation and partial-publication workflow statements.
- Changed workflow YAML parses; harness-rule validation passes.
- `pwsh -NoProfile -File scripts/verify.ps1 -Base origin/main -ScopePath . -SkipBuild -SkipDeploymentCheck`: passed after final test adjustments.
- Hosted cancellation/API propagation behavior is not claimed proven by local tests; retain a controlled rollout exercise before treating this as operationally proven.

## PR Review Follow-up

- Preserve production PR identity and checkpoint resource evidence when a restart reuses an already-published checkpoint.
- Require an exact production-head run title for active QA reuse. Legacy, unknown and old-head runs do not suppress current-head dispatch.
- Route missing-QA recovery through the shared dispatcher after resolving the configured production target from the latest coordinator ledger; dispatch always pins its current verified head.
- Report readiness and recovery exercise exceptions under their respective sensor names. Update registration coverage and harness routing checks to use the shared QA dispatcher.
- All four review findings are within the restart-recovery scope; no scope exclusions were needed.

- Follow-up review: the ledger QA handoff now prefers the existing cross-repository routing token, retaining coordinator-token and default-token fallbacks. A readiness regression guards this token binding so remote production PRs remain readable before dispatch.
- Follow-up review: Reuse explicitly skips production publication while retaining PR and resource evidence for the ledger. Regression cases exercise all three publication modes and execute the actual Reuse publication block with no outcome and mutation-denying Git/GitHub stubs.
- Follow-up review: enforce checkpoint eligibility on all generation entries, including default dispatches and reruns. Aggregate matrix continuation decisions into one downstream job to avoid parallel check-and-dispatch races. Added 18 entry-policy cases and four aggregate-dispatch regressions.
- Follow-up review: revalidate the authoritative remote head immediately before PR metadata publication; reject missing/non-Boolean reservation flags in otherwise valid checkpoint commits; give the aggregate continuation job Actions write and contents read permissions for its default-token fallback. Regression coverage includes six malformed reservation types and publication-boundary/permission checks. The remote read and GitHub metadata write are separate operations, not an atomic lease; readiness consumers still bind evidence to the exact current head.
- Follow-up review: bootstrap-only orphan recovery requires the exact registration hash/path, a regular Git blob, a single added-file commit and parent ancestry on the configured base. Reuse refreshes remote head, PR state/identity and readiness immediately before emitting ledger evidence. Four explicit digest regressions confirm delivery constraints, harness, detailed sequence and UX sections were already hashed; no digest implementation change was needed.
- CI fixture correction: each bootstrap scenario now owns a fresh temporary Git repository, and all five scenarios run with core.symlinks both true and false. The synthetic symlink Git object is no longer followed by a checkout over its intentionally regular worktree file; this removes the hosted-runner checkout failure without weakening provenance validation.
- Follow-up review: draft QA PRs without a pending marker remain resumable, not published evidence. QA run reconciliation requests up to 1,000 runs and stops when that bound is reached, preventing silent truncation from causing duplicate dispatch. Tests cover an active match beyond 100 entries and fail-closed saturation. Normalizing QA feedback into the production lock was excluded: feedback writes its own QA PR branch, while coordinator ledger writes already have independent serialization.
- Follow-up review: aggregate production continuation now fails closed at the 100-run query bound. Regression cases exercise 99 completed runs, 100 returned runs, and the absence of any additional dispatch on saturation.
