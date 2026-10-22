# Associated Surface PR Feedback

- Status: completed
- Owner: Platform Engineering
- Created: 2026-09-04
- Scope: sequence-goal workflows, cross-repository PR feedback, and completion gating

## Objective

Apply the established human and AI/bot feedback lifecycle to every associated surface pull request, while allowing review to begin immediately and preventing goal completion until every associated pull request is merged.

## Context

The EAPI coordinator PR has event-driven human-comment and AI-review handling. Associated iOS, Android, and WebUI pull requests are currently polled only for merge state, so feedback on the production surface can remain unhandled by the delivery loop.

## Acceptance Criteria

- Associated repositories are discovered from the goal ledger rather than a hard-coded repository list.
- Human comments and AI/bot review feedback on associated pull requests dispatch the matching surface remediation agent.
- Processing is idempotent, replies on the originating pull request, and records non-sensitive evidence.
- Target-branch changes run repository-owned verification before being pushed.
- A goal remains `InProgress` until every associated pull request is merged; feedback handling cannot promote it early.
- The EAPI coordinator pull request is registered in the same goal ledger and its merge is the final transition to `Complete`.
- Scheduled polling recovers feedback events that cannot trigger EAPI directly.
- Failed GitHub check runs and external commit-status contexts on the current associated-PR head dispatch the same surface remediation worker.
- Confirmed merge conflicts dispatch the worker with the associated head and base branches so it can backflush without rewriting branch history.

## Plan

1. Extend associated-PR discovery to identify unhandled human and AI/bot feedback.
2. Add a cross-repository feedback worker with least-privilege GitHub App authentication, target checkout, agent execution, evidence, verification, push, and replies.
3. Harden completion gating and add deterministic harness checks for discovery, idempotency, and permissions.
4. Update operating documentation and validate the enrolled workflows.

## Decisions

- 2026-09-04: Keep privileged orchestration in EAPI and discover targets from the associated-PR ledger. Target repositories do not need duplicated secrets or workflow logic.
- 2026-09-04: Use scheduled polling as the cross-repository event bridge because target-repository PR events cannot directly trigger an EAPI workflow.
- 2026-09-04: Treat the EAPI delivery PR as the `Coordinator` ledger entry. Its finalizer records merge or abandonment and is the only component permitted to complete the goal after all remote PRs have merged.
- 2026-09-08: Treat failing checks as another feedback source. Key durable markers by check/status identifier, inspect only the current associated-PR head, and reuse the existing surface-specific remediation and verification worker.
- 2026-09-08: Treat a confirmed conflict as repository-state feedback keyed by head SHA. Merge the base into the associated branch; do not rebase or force-push delivery history.

## Validation

- `scripts/check-agent-workflow-controls.ps1`
- `scripts/check-harness-rules.ps1`
- YAML parsing for every changed workflow
- Focused dry-run fixtures for human and AI/bot feedback discovery

## Outcome

Implemented an event-driven feedback monitor with an hourly ledger recovery poll and a bounded associated-surface worker. The worker validates trusted same-repository PR branches, classifies human and AI/bot feedback, uses durable response markers for idempotency, runs on macOS for iOS and Linux for other surfaces, requires recorded repository-owned verification, enforces protected-path and resource budgets, pushes valid fixes, and replies on the originating PR. Existing merge-timestamp completion gating remains authoritative.

Extended on 2026-09-08 to collect failed check runs, commit statuses, and confirmed merge conflicts for the current associated-PR head. CI failures and conflicts use the same evidence, agent budget, target-repository verification, push, and durable-response path as review feedback.

Validation passed on 2026-09-04:

- `scripts/verify.ps1 -ScopePath . -SkipBuild -SkipDeploymentCheck`
- `scripts/check-agent-workflow-controls.ps1`
- `scripts/check-harness-rules.ps1`
- YAML parsing for all sequence-goal workflows
