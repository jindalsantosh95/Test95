# Sequence Agent Individual Pull Requests

- Status: complete
- Owner: EAPI harness engineering
- Created: 2026-09-09
- Scope: sequence-goal workflow, associated-PR monitoring/feedback/finalization, goal ledger, resource evidence

## Objective

Every production and QA agent delivers through its own pull request. The coordinator branch contains only goal state and lifecycle evidence, so every agent's code, checks, costs, comments, reviews, and closure can be tracked independently.

## Context

Run `34359401505` allowed `dev-jim-api-ai` to commit EAPI automation changes directly to `docs/native-book-seat-selection-ready` as commit `2aff93ed8f`. No associated EAPI PR was created, and the goal ledger recorded only the MobileIos PR. Repository-local production and QA work therefore bypassed the associated-PR feedback and monitoring lifecycle.

## Acceptance Criteria

- Backend production runs on a dedicated branch from `main` and creates or reuses an associated EAPI PR.
- Each selected QA specialist runs on a dedicated branch from `main` and creates or reuses its own associated EAPI PR.
- The coordinator branch contains goal/run/lock/ledger evidence but no generated production or automation implementation.
- Goal ledgers and their retained child-PR evidence identify phase, agent, repository, branch, PR, state, merge time, and resource evidence for every delivery agent.
- Completion waits until every production and QA PR is merged, then verifies the integrated result from refreshed `main`.
- Existing associated-PR feedback, failing-check, conflict, comment, cost, and finalizer controls cover EAPI production and QA PRs.
- Harness checks fail if repository-local agents can commit generated implementation directly to the coordinator branch.

## Plan

1. Extend routing matrices to include repository-local Backend production and per-surface QA deliveries.
2. Run all implementation agents in isolated target checkouts based on the owning repository base branch.
3. Create/reuse and publish evidence for each production and QA PR.
4. generalize the goal ledger and associated-PR monitor to all production and QA row types.
5. make coordinator completion resume only after all associated PRs merge and verify refreshed integrated code.
6. update durable workflow documentation and executable harness checks.

## Decisions

- 2026-09-09: Use independent branches from the owning base rather than stacked PRs, so checks and merges remain isolated and order-independent.
- 2026-09-09: Reuse the associated-PR feedback worker and monitor; do not create separate EAPI-only comment handling.
- 2026-09-09: Keep the coordinator PR as the goal/lifecycle ledger and final integrated verification PR.

## Validation

- PowerShell parser validation for changed scripts.
- `scripts/check-harness-rules.ps1`.
- `scripts/check-agent-workflow-controls.ps1`.
- `scripts/check-docs-harness-support.ps1`.
- Workflow YAML parse and `git diff --check`.

## Outcome

Backend production and every selected QA specialist now create or reuse isolated child PRs. Coordinator branches execute no implementation agent, retain the child ledger and normalized resource decisions, wait for every child merge, verify refreshed integrated code, and originally opened their evidence-only PR only after the goal was `Complete`. That publication rule is historical and is superseded by the maintained workflow contract: verified question-blocked goals now publish a draft coordinator PR while remaining `InProgress`, then become ready after the questions are resolved and completion is reverified. Workflow YAML, PowerShell parsing, harness rules, workflow controls, sequence status, docs support, and the full no-build verification suite pass locally.
