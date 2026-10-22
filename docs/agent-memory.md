# Agent Memory

This document defines the repository contract for memory and state continuity across coding-agent sessions.

## Purpose

Session continuity is required so agents can resume work without re-discovering intent, scope, risk decisions, and verification state.

The memory contract must stay lightweight, machine-readable, and local to the repository.

## State Artifacts

The tracked contract artifact is:

- `.codex/state/session-state.example.json`

The live state artifact is:

- `.codex/state/session-state.json`

The example is committed and required. The live file is created automatically for an agent session, remains local to a clone or worktree, is ignored by Git, and must never be committed. This prevents unrelated engineers and concurrent workstreams from overwriting a shared snapshot.

## Session Lifetime And Ownership

- A session is one bounded coding-agent work period against this repository (for example, a PR implementation/review pass or follow-up continuation).
- Local session-state freshness is enforced by `scripts/check-agent-memory-state.ps1` with a default maximum age of 30 days.
- `.codex/state/session-state.json` supports resumption in the same clone or worktree. It is not a cross-engineer handoff mechanism.
- `SessionStart` invokes `scripts/update-agent-memory-state.ps1 -Action Resume`; this atomically creates missing state or records a revisioned resume. `scripts/check-agent-memory-state.ps1 -Initialize` remains available for manual initialization.
- Cross-engineer handoff belongs in the PR description for routine work or in `docs/exec-plans/` for long-running, risky, or multi-session work.
- Durable trade-offs and accepted gaps belong in decision records and the technical-debt tracker rather than local state.

## Required JSON Shape

Both state files use schema version 2 and this shape:

- `schemaVersion` (`integer`): must be `2`.
- `sessionId` (`GUID`): stable identity across resumes of the active session.
- `revision` (`integer`, positive): increases on every durable transition.

- `objective` (`string`, non-empty): current work objective.
- `status` (`string`): one of `active`, `blocked`, `completed`.
- `scope` (`object`):
  - `kind` (`string`): one of `SolutionPath`, `ProjectPath`, `ScopePath`.
  - `path` (`string`, non-empty): path being changed or verified.
- `nextActions` (`array[string]`): concrete next steps.
- `blockers` (`array[string]`): blockers; use empty array when none.
- `lastVerification` (`object`):
  - `command` (`string`): last verification command run.
  - `result` (`string`): one of `pass`, `fail`, `not_run`.
  - `ranAtUtc` (`string`): ISO-8601 UTC timestamp.
- `createdAtUtc` (`string`): ISO-8601 UTC creation timestamp.
- `lastResumedAtUtc` (`string`): ISO-8601 UTC timestamp of the latest resume.
- `resumeCount` (`integer`, non-negative): number of resumes after initialization.
- `updatedAtUtc` (`string`): ISO-8601 UTC timestamp for latest memory update.
- `updatedBy` (`string`, non-empty): actor updating state.
- `history` (`array[object]`): the last 20 revision events, each containing `revision`, `event`, `atUtc`, and `actor`.

## Freshness Rule

Memory must be fresh enough for reliable resumption:

- A present local `session-state.json` cannot be older than 30 days.
- The tracked example is schema-validated but is exempt from freshness because it is a stable contract, not live state.

## Lifecycle Expectations

- `SessionStart` creates or resumes local state and surfaces its session identity, revision, objective, scope, and next actions.
- `PostToolUse` records detected `dotnet build`, `dotnet test`, `verify.ps1`, and `verify-staged.ps1` command outcomes in `lastVerification`.
- `Stop` checkpoints the next action before issuing any required verification reminder.
- Explicit `Checkpoint`, `Block`, and `Complete` transitions are available through `scripts/update-agent-memory-state.ps1`.
- Writes use a per-state mutex plus atomic same-directory replacement. Invalid JSON or an invalid transition fails closed and leaves the previous state unchanged.
- A completed session is rotated to a new `sessionId` on the next resume; active and blocked sessions preserve identity and increment `resumeCount`.
- Local state must not be staged or committed.
- Cross-engineer handoff context must be recorded in the PR description or an execution plan.

Examples:

```pwsh
scripts/update-agent-memory-state.ps1 -Action Checkpoint -Objective 'Implement docs harness memory' -ScopeKind ScopePath -ScopePath docs -NextActions 'Run scoped verification'
scripts/update-agent-memory-state.ps1 -Action RecordVerification -VerificationCommand 'scripts/verify.ps1 -ScopePath docs -SkipBuild -SkipDeploymentCheck' -VerificationResult pass
scripts/update-agent-memory-state.ps1 -Action Complete
```

## Enforcement

This contract is enforced by:

- `scripts/check-agent-memory-state.ps1`, which validates the tracked example and live state when present;
- `scripts/invoke-agent-memory-lifecycle-exercise.ps1`, which proves initialization, checkpoint and verification preservation, cross-process resume, blocked-session recovery, completed-session rotation, session identity/revision continuity, and fail-closed invalid transitions;
- `scripts/check-agent-memory-lifecycle.ps1`, which validates retained exercise evidence.

The contract and lifecycle exercise are run by:

- `scripts/verify.ps1` (required local harness control),
- scheduled harness drift parity, including a separate evidence validation step.

## Change Rule

Schema version 1 local files fail closed rather than being silently rewritten. If an obsolete ignored local file exists, preserve any needed objective/next-action details, remove only that local file, and let `SessionStart` create schema version 2 state. When the state shape changes, update this file, the tracked example, lifecycle command, checker, exercise, and relevant hooks in the same change. Do not update the tracked example merely to record live work.
