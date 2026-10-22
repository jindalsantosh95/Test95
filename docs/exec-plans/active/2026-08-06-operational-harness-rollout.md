# Operational Harness Rollout

## Objective

Turn the repository-level harness contracts for memory, architecture/behavior coverage, semantic review, observability, documentation gardening, execution recovery, and resource control into executable enrollment and evidence checks.

## Scope

- `docs/` harness contracts, operational registry, DoD, and remediation evidence
- `scripts/` validation and scheduled evidence collection
- `.github/workflows/` PR and scheduled harness gates

External telemetry provisioning, production authorization, model credentials, and the passage of two review cycles are outside a repository-only change. Those items remain explicit enrollment gates rather than fabricated evidence.

## Progress

- [x] Define the three durable operating contracts.
- [x] Add a machine-readable operational capability registry.
- [x] Add a fail-closed registry/evidence validator.
- [x] Enrol a local agent-readable docs-harness telemetry source for structured logs, metrics, traces, and correlation.
- [x] Record and validate two runtime-to-harness feedback examples discovered during rollout.
- [x] Enrol a bounded read-only semantic documentation-gardening provider.
- [x] Enrol bounded read-only inferential review for every high-risk PR.
- [x] Enforce timeout, attempt-budget, credential, and terminal-evidence controls across enrolled agent workflows.
- [x] Enforce fail-closed per-run token and standard-API-equivalent USD ceilings across all five enrolled Codex workflows.
- [x] Retain per-run budget evidence and schedule required rolling 30-day resource/cost enforcement.
- [ ] Record two successful documentation-gardening cycles and close supported findings.
- [x] Enrol current mutating agent workflows in execution-control validation.
- [x] Implement the shared execution-state/recovery subsystem and pass local ambiguous-write, stale-lock, duplicate-suppression, and diagnostic-replay exercises.
- [x] Implement and locally prove the general bounded validation-correction-revalidation loop, including exhausted-budget failure.
- [x] Enrol every Complete sequence flow and achieve a 100% behavior-harness validator mutation score.
- [x] Enrol all 17 active solutions and the 281-project graph, fix the stale deployment-solution path, and achieve a 100% architecture-validator mutation score.
- [x] Implement automatic atomic agent-memory initialization, verification updates, stop checkpoints, revision history, and a passing cross-process resume exercise.
- [x] Add a bounded correlation query and failure-path exercise for local logs, metrics, traces, and runtime-to-harness learning.
- [x] Add machine-checked local/Codex-CI tool and executable-environment profiles with isolation and unsafe-path mutations.
- [x] Derive continuous maturity from an always-recorded retained scheduled ledger, including duplicate rejection and failure reset.
- [ ] Record two scheduled recovery exercise cycles for maturity promotion.
- [ ] Record two scheduled resource-budget/trend cycles for maturity promotion.
- [ ] Promote eligible DoD themes from Amber to Green.

## Decisions

- Artifact completeness and operational maturity remain separate states.
- The default gate validates honest registry shape and evidence. `-RequireMature` is the promotion gate and fails until every required capability has two passing cycles.
- Missing external systems remain visible as `planned` enrollments; the repository never invents telemetry or semantic-review evidence. Resource evidence comes only from retained Codex JSONL usage, and USD values are explicitly labelled standard-API-equivalent estimates rather than provider invoices.

## Validation

- `scripts/check-operational-harness.ps1`
- `scripts/check-docs-harness-support.ps1`
- `scripts/verify.ps1 -ScopePath docs -SkipBuild -SkipDeploymentCheck`
