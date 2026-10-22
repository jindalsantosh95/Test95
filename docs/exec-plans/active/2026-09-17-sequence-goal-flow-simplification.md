# Sequence Goal Flow Simplification

- Status: active
- Owner: Platform Engineering
- Created: 2026-09-17
- Scope: sequence-goal GitHub workflows, scripts, harness checks, and operating guidance

## Objective

Reduce sequence-goal delivery to a small, explicit state machine in which each workflow has one owner and retries do not mutate stale delivery attempts.

## Context

The coordinator currently combines initial delivery, existing-PR remediation, QA publication, evidence retention, integrated verification, and completion. A reused Mobile iOS delivery branch failed while the coordinator attempted to merge the moving `develop` base before agent execution. Existing associated PRs already have a dedicated feedback workflow, so coordinator-side remediation duplicates ownership and creates conflicting recovery paths.

## Acceptance Criteria

- The coordinator creates a production delivery PR or records an authenticated existing PR; it does not remediate an existing PR.
- A delivery attempt is bound to an explicit target-base commit, and stale or conflicted attempts are superseded rather than repeatedly merged.
- Workflow-authored coordinator pushes do not serve as implicit delivery commands.
- Associated-PR feedback enters through one classification and remediation path.
- Goal lifecycle state has one authoritative record, with GitHub queried for live PR state.
- Each implementation PR is independently useful, narrowly scoped, and covered by deterministic harness checks.

## Plan

1. Remove existing-production-PR remediation from the coordinator and leave it to the associated-PR feedback workflow.
2. Record the target-base SHA for every new delivery attempt and introduce explicit stale-attempt detection.
3. Supersede stale or conflicted attempts with a fresh branch from the current target base.
4. Replace push-driven coordinator re-entry with explicit lifecycle dispatch.
5. Consolidate overlapping associated-PR comment, review, check, and conflict monitoring.
6. Reduce duplicated ledger/evidence mutation and extract reusable worker boundaries from the main workflow.

## Decisions

- 2026-09-17: Deliver the simplification as a sequence of small PRs so each behavioral change is reviewable and rollback remains straightforward.
- 2026-09-17: An existing open production PR is a checkpoint. Only the associated-PR feedback workflow may reconcile or remediate it.
- 2026-09-17: Do not mix fresh-attempt supersession into the first PR; it requires its own lifecycle and audit review.

## Validation

- Run `scripts/check-harness-rules.ps1` for every slice.
- Run the relevant sequence-goal exercises when a slice changes their contract.
- Use a manual delivery-harness exercise before enabling stale-attempt supersession.

## Outcome

In progress. The first slice removes duplicate existing-PR mutation from the coordinator.
