# Harness Contract

This contract defines the repository harness as a coherent system of guides and sensors.

## Goals

- Increase first-pass correctness for coding-agent changes.
- Shift fast quality checks left.
- Provide deterministic, machine-readable feedback loops.
- Keep repository-local knowledge legible, linked, and recoverable.
- Keep human review focused on business behavior and trade-offs.

## Policy Legend

- `Required`: blocking control that must fail closed in local/CI enforcement.
- `Advisory`: non-blocking control used for visibility, trend, or semantic review.
- `Guidance only`: feedforward context that shapes behavior but does not block.

## Control matrix

| Control | Objective | Type | Mode | Trigger | Budget | Policy |
|---|---|---|---|---|---|---|
| `AGENTS.md` + harness docs | Maintainability/architecture | Feedforward | Inferential | Session start / prompting | n/a | Guidance only |
| `SessionStart` hook context | Maintainability | Feedforward | Inferential | SessionStart | < 1s | Guidance only |
| `PreToolUse` bash policy | Safety/maintainability | Feedforward | Computational | PreToolUse (`Bash`) | < 1s | Required: deny destructive commands |
| `PreToolUse` patch policy | Architecture/maintainability | Feedforward | Computational | PreToolUse (`Edit|Write|apply_patch`) | < 1s | Required: deny inline `PackageReference Version=` in `.csproj` |
| `PermissionRequest` policy | Safety/operability | Feedforward | Computational | PermissionRequest (`Bash`) | < 1s | Required: deny destructive; require rationale for risky network/package ops |
| `PostToolUse` feedback | Maintainability | Feedback | Computational | PostToolUse (`Bash`, `Edit|Write|apply_patch`) | < 1s | Guidance only: inject self-correction reminders |
| `Stop` reminder | Process quality | Feedback | Computational | Stop | < 1s | Guidance only: continuation reminder to run `verify.ps1` |
| `scripts/verify.ps1 -SolutionPath <path> -SkipBuild` or `-ProjectPath <path> -SkipBuild` | Maintainability/behavior | Feedback | Computational | Pre-commit and manual | 1-3 min | Required: must pass before commit/PR |
| `scripts/verify-staged.ps1 -SkipBuild` | Maintainability/behavior/process | Feedback | Computational | Pre-commit and manual | 1-3 min | Required when staged pre-commit flow is used |
| `scripts/verify.ps1 -SolutionPath <path>` or `-ProjectPath <path>` | Maintainability/behavior | Feedback | Computational | Manual before PR | 3-10 min | Required before merge for owning scope |
| `scripts/check-deployment-projects.ps1` | Deployability/operability | Feedback | Computational | verify/pre-commit/CI | < 30s | Required when Azure Functions apps are present |
| `scripts/check-knowledge-health.ps1` | Agent legibility/knowledge integrity | Feedback | Computational | verify/pre-commit/CI | < 30s | Required |
| `scripts/check-agent-memory-state.ps1` plus `scripts/invoke-agent-memory-lifecycle-exercise.ps1` | Automatic, atomic, revisioned session state and cross-process resume | Feedback | Computational | session hooks/verify/CI/scheduled | < 15s | Required |
| `scripts/check-context-routing.ps1` | Context routing/operational guidance integrity | Feedback | Computational | verify/pre-commit/CI | < 10s | Required |
| `scripts/check-docs-harness-support.ps1` | Docs harness operational support | Feedback | Computational | verify/pre-commit/CI | < 10s | Required |
| `scripts/check-operational-harness.ps1` | Operational capability enrollment/evidence | Feedback | Computational | verify/CI/scheduled | < 10s | Required; `-RequireMature` is the promotion gate |
| Tool/environment profile probe and exercise | Agent tooling/isolation | Feedforward and feedback | Computational | verify/CI/scheduled | < 15s | Required |
| Bounded observability query and exercise | Operability/runtime learning | Feedback | Computational | verify/CI/scheduled | < 15s | Required |
| Continuous maturity ledger | Governance/sustained assurance | Feedback | Computational | Every scheduled parity completion | < 15s | Required; promotion requires retained scheduled cycles |
| `scripts/check-agent-resource-budget.ps1` plus resource exercise | Agent token and standard-API-equivalent cost control | Feedback | Computational | Every enrolled Codex run / verify / scheduled | Workflow-specific ceilings | Required: fail closed on missing or exceeded usage |
| `scripts/check-preferred-extensions.ps1` | Shared coding semantics/maintainability | Feedback | Computational | scoped verify/pre-commit/CI | < 10s | Required: newly added built-in string helpers covered by shared extensions must use `.Is(...)`, `.IsNullOrWhiteSpace()`, or the applicable internal helper |
| `scripts/check-test-standards.ps1` plus detector exercise | Unit-test maintainability/behavior | Feedback | Computational | scoped verify/pre-commit/CI | < 15s | Required: changed unit-test code must use repository test doubles and the shared HTTP/resource fixture rather than Moq or ad hoc connector implementations |
| `scripts/check-ui-automation-standards.ps1` | UI architecture/maintainability | Feedback | Computational | scoped verify/pre-commit/CI | < 10s | Required: committed, staged, or untracked additions and modifications in UI tests/features/flows must not introduce driver primitives or reach through journey-flow/page-object boundaries |
| `scripts/check-harness-remediation-log.ps1` | Drift remediation/operational closure | Feedback | Computational | Scheduled CI drift review | < 10s | Required for scheduled drift operations |
| `high-risk-autolabel` workflow | High-risk path routing | Feedback | Computational | PR lifecycle | < 1 min | Required support control for high-risk enforcement |
| `closed-loop-gate` workflow | Closed-loop autonomy governance | Feedback | Computational | PR/main/scheduled CI | < 5 min | Required |
| Behavior contract tests | Behavior | Feedback | Computational | test stage | 1-3 min | Required before merge |
| `.githooks/pre-commit` | Maintainability/process | Feedback | Computational | pre-commit | 1-3 min | Required locally; CI must provide non-bypassable parity |
| Inferential review (semantic) | Maintainability/behavior | Feedback | Inferential | Manual/CI (optional) | 1-5 min | Advisory: run on high-risk changes |
| Harness coverage review | Coverage/architecture fitness | Feedback | Inferential | PR, repeated failure, scheduled review | 10-30 min | Required for high-risk changes; otherwise advisory |
| Drift/coverage sensor | Maintainability | Feedback | Computational | Nightly/CI | 1-5 min | Advisory unless a scope enrolls it as required |
| Agent-readable observability source | Operability/behavior | Feedback | Computational plus inferential interpretation | Runtime investigation | Bounded by source contract | Required contract when declared; access remains separately authorized |
| Semantic documentation gardening | Knowledge integrity | Feedback | Inferential | Monthly/manual/repeated failure | 10-30 min per scoped area | Advisory and operational through the enrolled scheduled workflow; fail closed when invoked |
| Agent execution record and recovery controls | Delivery/safety/operability | Feedforward and feedback | Computational | Every autonomous mutating run | Workflow-specific bounded budget | Required for enrolled workflows |
| Agent resource trend review | Cost/resource operability | Feedback | Computational | Weekly | Rolling 7-day workflow ceilings | Required |

## Lifecycle placement

| Lifecycle stage | Required controls |
|---|---|
| In-agent authoring | SessionStart, PreToolUse, PermissionRequest, PostToolUse |
| Before commit | Stop reminder, `scripts/verify-staged.ps1 -SkipBuild` or scoped `scripts/verify.ps1 -SolutionPath <path> -SkipBuild` / `-ProjectPath <path> -SkipBuild`, `.githooks/pre-commit` |
| Before merge | `scripts/verify.ps1 -SolutionPath <path>` or `-ProjectPath <path>` |
| After merge / scheduled | Required CI parity controls; drift/coverage sensor and inferential review per owning-scope policy |
| Runtime investigation | Bounded, sanitized observability queries and runtime-to-harness feedback per `agent-observability.md` |
| Agent environment startup | Required commands, worktree isolation, writable roots, and network/secrets policy per `agent-tool-and-executable-environments.md` |
| Autonomous mutation/recovery | Execution record, lock, reconciliation, timeout, attempt budget, checkpoint, and escalation per `agent-execution-and-recovery.md` |
| Enrolled Codex execution | JSONL usage, allowed model, per-run token/cost decision, and retained evidence per `agent-resource-and-cost-controls.md` |
| Sequence-goal child delivery | Open the draft coordinator PR before delivery; use one isolated production and QA branch/PR per surface; start each bounded dedicated QA continuation when its matching production PR is recorded; serialize coordinator-ledger and cost-evidence writes; retain existing review/remediation monitoring; mark Complete and ready only after every child merges and integrated verification passes |

## CI parity mapping

Use [`harness-pipeline-checklist.md`](harness-pipeline-checklist.md) as the operational runbook.

| Local control | CI parity | Expectation |
|---|---|---|
| `scripts/verify.ps1 -SolutionPath <path> -SkipBuild` or `-ProjectPath <path> -SkipBuild` | Pull request pipeline parity run | Required: must pass |
| `scripts/verify-staged.ps1 -SkipBuild` | Local pre-commit parity for changed files | Required when staged context is available |
| `scripts/verify.ps1 -SolutionPath <path>` or `-ProjectPath <path>` | Main branch full harness stage | Required: must pass |
| `scripts/check-deployment-projects.ps1` | Deployment project policy gate | Required: must pass |
| `scripts/check-knowledge-health.ps1` | Knowledge-health policy gate | Required: must pass |
| `scripts/check-agent-memory-state.ps1`, `scripts/check-agent-memory-lifecycle.ps1` | Agent-memory contract plus retained lifecycle/resume evidence | Required: contract and exercise must pass |
| `scripts/check-context-routing.ps1` | Context-routing policy gate | Required: must pass |
| `scripts/check-docs-harness-support.ps1` | Docs harness support gate for behavior sensors, templates, DoD status, and operational evidence | Required: must pass |
| `scripts/check-agent-resource-budget.ps1`, resource exercise, and strength check | Per-run resource enforcement and mutation evidence in local/scheduled parity | Required: must pass |
| `scripts/check-agent-resource-trend.ps1` | Monthly retained-evidence aggregation | Required: rolling ceilings must pass |
| Tool/environment, observability-query, and maturity exercises | Scheduled drift parity and retained maturity ledger | Required: exercises and evidence validation must pass |
| `scripts/check-preferred-extensions.ps1` | Shared-extension usage policy gate | Required: must pass |
| `scripts/check-test-standards.ps1` plus detector exercise | Unit-test standards and shared HTTP/resource fixture policy gate | Required: must pass for changed unit-test code |
| `scripts/check-ui-automation-standards.ps1` | UI test journey-flow boundary gate | Required: must pass for newly added UI tests |
| `scripts/check-harness-remediation-log.ps1` | Drift remediation evidence gate | Required on scheduled drift workflow |
| `high-risk-autolabel` | Automatic high-risk label assignment on impacted file patterns | Required support control |
| `closed-loop-gate` | Closed-loop governance gate (inventory + context + remediation) | Required: must pass |
| Notifications/RetailTherapy test runs | Pull request and main test stages | Required: must pass |
| Pre-commit policy (`.githooks/pre-commit`) | Equivalent CI gate for build/tests/policy outcomes | Required: must fail closed on violations |
| Harness coverage review | PR checklist and scheduled drift review | Required for high-risk changes; otherwise advisory |
| Codex lifecycle hooks | Not directly executed in CI | Guidance only: keep equivalent deterministic checks in scripts/pipeline |

CI must repeat fast controls from local authoring (parity), then add heavier or trend-focused controls post-integration.

## Fail-Closed Rule

- Required controls must always resolve to pass/fail outcomes in CI.
- Required workflows must not rely on trigger-level path filters that leave checks permanently pending.
- If scope detection is needed, use always-trigger workflows with internal no-op logic that still report successful completion.

## Ownership and updates

- Primary owners: maintainers for the changed solution or project.
- Update this contract when adding/changing hooks, verify steps, or pre-commit policies.
- Use [`harness-coverage.md`](harness-coverage.md) to record material gaps between risks and controls.
- If a failure mode repeats more than twice, add or adjust a control.
- Use [`agent-observability.md`](agent-observability.md), [`agent-tool-and-executable-environments.md`](agent-tool-and-executable-environments.md), [`harness-continuous-maturity.md`](harness-continuous-maturity.md), [`doc-gardening.md`](doc-gardening.md), [`agent-execution-and-recovery.md`](agent-execution-and-recovery.md), and [`agent-resource-and-cost-controls.md`](agent-resource-and-cost-controls.md) for their respective ownership, evidence, freshness, and acceptance contracts.
