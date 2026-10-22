# Agent Observability

This contract defines how runtime evidence must be made safe, discoverable, and usable by coding agents. It covers evidence from local worktrees, deployed test environments, and production-derived investigations. It does not grant an agent access to an environment or relax existing security controls.

## Outcomes

- An agent can correlate a reported failure across logs, metrics, traces, and test evidence without requesting copied secrets or raw sensitive payloads.
- Runtime findings feed back into a durable test, rule, guide, decision, or remediation item.
- Observability access is least-privilege, time-bounded, attributable, and safe for customer data.

## Evidence Contract

Every agent-accessible observability source must publish or document:

| Field | Requirement |
| --- | --- |
| Source and environment | Name the application/component and classify it as local, test, staging, or production-derived. |
| Query interface | Provide an approved command, tool, dashboard, or API that returns machine-readable output. |
| Time window | Require bounded UTC start/end times; do not permit unbounded searches. |
| Correlation | Support the applicable correlation, trace, invocation, message, order, or synthetic-test identifier. |
| Redaction | Remove tokens, secrets, cookies, payment data, passenger documents, and unnecessary PII before agent access. |
| Retention | State how long the evidence remains queryable and where durable conclusions are recorded. |
| Ownership | Name the solution owner and the team responsible for access and schema changes. |
| Evidence link | Record a safe query, dashboard, trace reference, test artifact, or sanitized excerpt in the PR/incident/remediation item. |

## Local And Worktree Baseline

The repository-level `/docs` source is registered in [`agent-observability-sources.json`](agent-observability-sources.json). Run `scripts/invoke-observed-docs-harness.ps1` to create sanitized logs, metrics, traces, and a run summary under the ignored `TestResults/docs-harness-observability/` directory. `scripts/query-agent-observability.ps1` requires a correlation ID and bounded UTC start/end window, and returns machine-readable sanitized evidence. `scripts/check-agent-observability.ps1 -RequireEvidence` validates the latest captured run.

```pwsh
$start = (Get-Date).ToUniversalTime().AddMinutes(-15)
$end = (Get-Date).ToUniversalTime()
scripts/query-agent-observability.ps1 -SourceId local-docs-harness -RunId <run-id> -StartUtc $start -EndUtc $end -Signal all -RequireMatch
```

When an owning solution supports local execution, its README or project docs should define:

1. the smallest scoped command that starts the application;
2. how to start isolated dependencies for one worktree;
3. where structured logs, metrics, and traces are emitted;
4. a readiness check and clean shutdown procedure;
5. example bounded queries for failure, latency, and correlation analysis;
6. the cleanup and retention behavior for ephemeral evidence.

If local runtime evidence is not available, record that as a harness gap rather than implying that build and test output provide runtime observability.

## Investigation Workflow

1. Establish scope, environment, UTC time window, symptom, and a non-sensitive correlation identifier.
2. Reproduce locally or in an approved test environment when feasible.
3. Query the narrowest relevant logs, metrics, and traces through approved read-only tooling.
4. Preserve only sanitized evidence and the query needed to reproduce the finding.
5. Validate the change with the closest computational sensor and, when applicable, the original runtime signal.
6. Convert the finding into a durable harness improvement: test, rule, guide, decision, or remediation entry.
7. Escalate to the solution owner when access, interpretation, customer impact, or security judgment is required.

## Security And Access Rules

- Production access is read-only, least-privilege, audited, and explicitly authorized outside this document.
- Prefer synthetic identifiers and sanitized production-derived evidence.
- Never copy raw credentials, authorization headers, cookies, payment data, passenger documents, or unrestricted payloads into prompts, repository files, PRs, or remediation logs.
- An unavailable or denied data source is a valid blocker; do not bypass access controls or substitute guessed runtime behavior.
- Follow [`principles/security-principles.md`](principles/security-principles.md) and the owning service's data-handling rules.

## Policy And Lifecycle

| Control | Lifecycle | Policy |
| --- | --- | --- |
| Source contract and bounded query examples | Service onboarding/change | Required when a source is declared agent-accessible |
| Sanitization and access review | Before every runtime investigation | Required |
| Runtime validation of a fix | PR or incident remediation | Required when the original signal is safely reproducible; otherwise record why |
| Runtime-to-harness feedback entry | Incident/telemetry follow-up | Required within the DoD SLA |
| Cross-source schema and access review | Every verify plus quarterly and after material changes | Required for enrolled sources |

## Ownership, Evidence, And Acceptance

- **Owner:** the owning solution team; Platform Engineering owns shared query/access patterns.
- **Review cadence:** quarterly, after a material telemetry change, and after an incident exposes an observability gap.
- **Evidence location:** owning service README/docs plus PR, incident, execution plan, or [`harness-remediation-log.md`](harness-remediation-log.md).
- **Artifact complete when:** this contract is linked from the harness maps and every declared source can express all Evidence Contract fields.
- **Operational complete when:** at least one approved agent-readable local/test source exposes logs, metrics, and traces through a fail-closed bounded query; the collector/query failure paths are exercised locally and in scheduled parity; and sanitized findings are converted into tracked harness updates. Two retained scheduled cycles are the separate maturity signal.
