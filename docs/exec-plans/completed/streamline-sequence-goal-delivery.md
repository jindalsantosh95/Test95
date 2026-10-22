# Streamline Sequence Goal Delivery

- Status: completed
- Owner: Codex
- Created: 2026-09-17
- Scope: sequence-goal GitHub Actions, coordinator/ledger scripts, harness rules, and delivery documentation

## Objective

Open the coordinator PR when a Ready goal starts, run each surface as an independent development-to-QA chain, and make the coordinator ready only after all child PRs merge and integrated verification completes.

## Context

The previous workflow ran production surfaces in parallel but waited for the whole production matrix before starting a serialized QA matrix. It also opened the coordinator PR only at the end. The revised lifecycle keeps the existing review/remediation monitors while making the coordinator PR the live delivery record from the start.

## Acceptance Criteria

- A Ready goal is promoted and an authenticated draft coordinator PR is created or reused before development jobs start.
- Every production surface dispatches only its matching QA continuation after its production PR has been recorded.
- QA continuations for different surfaces can run concurrently and do not wait for other production surfaces or production PR merges.
- Existing associated-PR review, remediation, retry, cost evidence, and closed-unmerged recovery remain active.
- After every production and QA PR is merged, integrated verification marks the goal Complete and makes the coordinator PR ready.
- Harness checks and workflow documentation enforce the streamlined dependency model.

## Plan

1. Extract idempotent early coordinator-PR publication and ledger registration into a script and wire it after coordinator initialization.
2. Add surface-scoped workflow continuations, dispatch them from production publication, and remove the global/serialized QA path.
3. Update completion, monitoring, documentation, and harness assertions.
4. Run focused workflow/script exercises and repository harness checks, then measure before/after workflow complexity.

## Decisions

- 2026-09-17: Reuse the existing workflow as a surface-scoped QA continuation instead of duplicating its substantial QA implementation into a second workflow.
- 2026-09-17: Serialize only short coordinator-branch ledger commits; long-running development and QA agents remain independent.
- 2026-09-17: Preserve the existing coordinator marker so review, comment, monitor, and finalizer workflows recognize the draft PR from the beginning without duplicating lifecycle detection.
- 2026-09-17: Keep coordinator creation exclusively in the early publisher; completion may update and mark that authenticated PR ready but cannot recreate it.
- 2026-09-17: Route production checkpoints, QA checkpoints, and retained cost evidence through workflows sharing one coordinator-ledger concurrency group.
- 2026-09-17: Give QA a dedicated two-input workflow that physically owns the bounded QA implementation, keeping continuation-only inputs and conditions out of the coordinator workflow.

## Validation

- `scripts/check-harness-rules.ps1`: passed.
- `scripts/check-sequence-diagram-status.ps1`: passed.
- `scripts/check-behavior-harness.ps1`: passed.
- Ruby YAML parsing for both changed delivery workflows: passed.
- `scripts/verify.ps1 -ScopePath . -SkipBuild -SkipDeploymentCheck`: all changed workflow/docs controls passed; the repository-wide run remains red only on pre-existing GraphQL Book preferred-extension and Moq findings outside this change.

## Outcome

The coordinator PR is now opened before production and gates product delivery; finalization can only update that existing authenticated PR. Parallel production jobs hand their checkpoint to a short coordinator-serialized workflow, which records the surface and immediately starts only that surface's dedicated QA continuation. QA checkpoints and retained token/cost decisions use the same serialized coordinator-write boundary. The former global production-to-QA barrier, internal public QA mode, and `max-parallel: 1` QA restriction are removed. Existing review/remediation behavior remains in place, with the goal becoming Complete and the coordinator PR becoming ready only after all child PRs merge and integrated verification passes.
