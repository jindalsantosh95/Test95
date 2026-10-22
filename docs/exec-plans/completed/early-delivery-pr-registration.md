# Early delivery PR registration

- Status: completed
- Owner: EAPI platform
- Created: 2026-09-24
- Scope: sequence-goal production, QA publication, coordinator ledger and monitoring

## Objective

Create each production and QA branch and draft PR, then persist its identity in the coordinator goal before running the agent.

## Acceptance Criteria

- Registration failure prevents agent execution.
- Retries reuse authenticated draft PRs and their branches.
- Bootstrap records cannot satisfy substantive implementation or QA coverage gates.
- Registration does not dispatch QA; production readiness remains required.
- QA publication updates the registered PR rather than silently discarding generated changes.

## Plan

1. Introduce a shared draft publisher and serialized registration barrier.
2. Integrate production and QA with early registration and existing publication checks.
3. Verify failure, reuse and publication paths with local fixtures and harness checks.

## Decisions

- Preserve configured production bases (mobile defaults to develop); Backend and QA target main.
- Use a small immutable delivery identity file to give GitHub a reviewable initial diff.
- Keep pending drafts visible while reserving their mutation for delivery, not feedback agents.

## Validation

- `scripts/invoke-sequence-goal-registration-exercise.ps1`: passed creation order, reuse, registration failure, bootstrap-only diff, QA preparation and publication to the same PR using local Git and mocked GitHub calls.
- `scripts/invoke-sequence-goal-production-readiness-exercise.ps1`: passed 21 cases and parsed 60 workflow PowerShell blocks.
- `scripts/check-harness-rules.ps1`: passed.
- `scripts/verify.ps1 -SolutionPath VAA.EAPI.Automation/VAA.EAPI.Automation.sln -SkipBuild`: passed.
- Modified YAML parsed successfully; `git diff --check` passed. No live delivery agents were invoked.

## Outcome

Both agent phases now wait for committed coordinator registration before generation. QA publishes into its existing draft PR. Pending drafts remain visible and protected from feedback-worker mutation. Generated QA evidence preserves the serialized ledger's latest rows to avoid losing another surface's registrations.

## PR review follow-up

- Missing-QA recovery checks active runs by coordinator and surface before dispatch; QA concurrency no longer cancels active work if dispatches race.
- QA preparation replaces a merged checkpoint with a collision-safe open draft before generation, preserving the historical branch.
- Both coordinator child-row consumers accept `Queued` ledger rows.
- The registration exercise covers active/completed/other-surface recovery, merged-checkpoint replacement and publication, and all supported child-row states.
- Retry registration restores and confirms pending/draft ownership for production and QA before the ledger barrier; tests cover body-update failure, draft-transition failure, failed confirmation and successful reuse without discarding the prior report. Feedback rechecks ownership immediately before pushing.
- Feedback ownership validation also runs for no-source-change turns and before attempt comments, body publication, ready transitions, replies, resolutions and verification-failure comments. Regression coverage checks no-change gating and the subsequent mutation boundaries.
