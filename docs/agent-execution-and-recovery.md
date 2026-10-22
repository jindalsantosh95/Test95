# Agent Execution And Recovery

This contract defines durable execution, bounded resource use, recovery, and audit evidence for repository-driven agent workflows. It generalizes the controls used by sequence-goal delivery; workflows may implement the contract differently but must preserve its invariants.

## Execution Record

Every automated agent run that can commit, update a PR, or alter durable repository state must record:

| Field | Requirement |
| --- | --- |
| Run identity | Globally unique run ID plus workflow name and attempt number. |
| Objective and scope | Immutable task statement, allowed target paths, owning solution/project, and acceptance criteria. |
| Inputs | Base commit, triggering event, referenced artifacts, model/configuration identifier, and non-secret tool configuration. |
| State | `Queued`, `Running`, `WaitingForReview`, `Succeeded`, `Failed`, `Cancelled`, or `Abandoned`, with UTC transitions. |
| Checkpoint | Last completed phase, produced commit/artifact hashes, verification outcome, and next safe action. |
| Resource budget | Timeout, maximum attempts, model, token ceiling, standard-API-equivalent monetary ceiling, and budget decision evidence. |
| Recovery policy | Retryable failures, backoff, stale-lock threshold, cancellation behavior, and escalation owner. |
| Outputs | Changed paths, commits, PR/review references, test evidence, and sanitized failure summary. |

Secrets and raw sensitive data must never be written into the execution record.

## Idempotency And Concurrency

- Use the run ID and objective/scope identity as idempotency keys where supported.
- Acquire a scope-specific lock before mutation and record its holder and timestamp.
- A retry must inspect the current commit and existing outputs before creating new state.
- Never duplicate comments, PRs, commits, external requests, or completion transitions merely because a workflow was retried.
- Cancellation must stop new mutations and leave a checkpoint that supports triage.
- A stale lock must fail closed until an owner inspects the previous run and explicitly clears or supersedes it.

## Retry And Recovery

Classify failures before retrying:

| Classification | Examples | Action |
| --- | --- | --- |
| Transient | Rate limit, runner interruption, temporary service unavailability | Retry within attempt/time budget using bounded backoff. |
| Correctable | Build/test failure or validated review feedback | Return evidence to the agent for one scoped correction cycle, then reverify. |
| Deterministic | Invalid configuration, missing required artifact, policy violation | Do not retry unchanged input; fail closed with remediation. |
| Ambiguous mutation | Unknown result of push, comment, merge, deployment, or external write | Reconcile current external state before any retry. |
| Judgment required | Conflicting intent, security exception, architecture decision, unsafe access | Stop and escalate to the named owner. |

Recovery must resume from validated repository and external state, not blindly replay the previous command stream.

For sequence-goal delivery, the coordinator branch and its early draft PR are the durable execution record rather than an implementation branch. Each production or QA agent owns a deterministic child branch and PR created from the owning repository's configured base branch. Production surfaces run independently; a serialized checkpoint writer records each production PR and immediately dispatches only its matching bounded QA workflow. QA checkpoints and retained cost evidence pass through the same coordinator concurrency boundary rather than pushing from parallel agent runs. A resumed run reconciles and reuses those PRs before invoking an agent. After all child PRs merge, the coordinator refreshes from `main`, records integrated verification, marks the goal `Complete`, and makes the existing coordinator PR ready for review.

## Replay Contract

Sequence production now persists its outcome and verification in a workflow-owned checkpoint commit before publishing readiness. A restarted runner validates the checkpoint's parent, tree and semantic goal identity against its fetched head. It can replay an interrupted PR/ledger handoff without invoking the agent; published incomplete work resumes with recorded remaining criteria under the bounded continuation contract in [Agent Loop Workflow](agent-loop-workflow.md#interrupted-production-and-bounded-continuation). Local work that was never pushed is outside that recovery guarantee.

“Replay” means reconstructing the run's inputs and decisions sufficiently to diagnose or safely resume it; it does not promise bit-for-bit model determinism.

- Pin the base commit, objective, referenced artifacts, model/configuration identifier, and tool versions where feasible.
- Preserve prompts or instruction references without secrets.
- Preserve phase transitions, tool outcomes, commits, and verification evidence.
- Label non-deterministic decisions and external state dependencies.
- A diagnostic replay is read-only by default. Mutation requires a new run ID and reconciliation with existing outputs.

## Resource And Cost Controls

- Every workflow has a wall-clock timeout and maximum attempt count.
- Every enrolled Codex workflow emits JSONL usage and must pass the per-run token and standard-API-equivalent USD ceilings in [`agent-resource-and-cost-controls.md`](agent-resource-and-cost-controls.md).
- Missing usage, unknown models, stale pricing, malformed evidence, or exhausted budgets fail closed.
- Use the smallest capable model and context scope consistent with the risk and acceptance criteria.
- Stop when acceptance criteria are met, the budget is exhausted, or further progress requires judgment.
- Budget exhaustion is a failed or escalated outcome, never evidence of success.
- Review retained resource evidence weekly against rolling 7-day workflow ceilings.

## Human Control And Audit

- Humans own prioritization, acceptance criteria, security exceptions, architecture trade-offs, and production authorization.
- Required CI and branch protection remain authoritative; an agent cannot waive them.
- External mutations must be attributable to the workflow/run identity.
- Preserve concise, sanitized evidence sufficient to explain what changed, why it stopped, and how to resume or roll back.

## Policy And Lifecycle

| Control | Lifecycle | Policy |
| --- | --- | --- |
| Execution record and scope | Before mutation | Required |
| Lock, idempotency, and reconciliation | Mutation and retry | Required |
| Timeout and attempt budget | Workflow configuration | Required |
| Model/token/monetary ceiling | Every enrolled Codex run | Required |
| Checkpoint and sanitized failure evidence | Every terminal or paused outcome | Required |
| Diagnostic replay exercise | Quarterly for enrolled autonomous workflows | Advisory until scheduled |
| Resource trend review | Monthly | Required |

## Ownership, Evidence, And Acceptance

The shared implementation is `scripts/agent-execution-state.ps1`. It provides durable run records, scope locks, checkpoints, idempotent mutation recording, ambiguous-write reconciliation, stale-lock triage, diagnostic replay digests, terminal transitions, and lock cleanup. `scripts/invoke-agent-recovery-exercise.ps1` proves those behaviors without external mutation, and `scripts/check-agent-recovery.ps1 -RequireEvidence` validates the resulting ignored evidence under `TestResults/agent-recovery/`. The exercise runs in the default docs harness and scheduled drift workflow.

The general correction implementation is `scripts/invoke-self-correcting-loop.ps1`. A workflow supplies scoped validation and correction commands as JSON, plus maximum validation attempts and a wall-clock budget. The loop records each validation/correction phase, invokes correction only after a failing sensor, revalidates before success, and fails closed when correction fails or validation remains red. `scripts/invoke-self-correcting-loop-exercise.ps1` proves both recovery and exhausted-budget paths; evidence is validated under `TestResults/self-correcting-loop/`.

Resource enforcement is defined in [`agent-resource-and-cost-controls.md`](agent-resource-and-cost-controls.md). `scripts/check-agent-resource-budget.ps1` validates each JSONL run before its output is accepted; `scripts/check-agent-resource-trend.ps1` enforces rolling 7-day ceilings; and `scripts/invoke-agent-resource-budget-exercise.ps1` proves pass, rejection, pricing, and trend paths during local and scheduled verification.

- **Owner:** workflow owner; Platform Engineering owns shared execution conventions.
- **Review cadence:** after every ambiguous mutation or stale lock, weekly for resource trends, and quarterly for recovery exercises.
- **Evidence location:** workflow run artifacts, execution-plan progress, PR evidence, or `.codex/agent-loop/` for the existing sequence-goal implementation.
- **Artifact complete when:** this contract is linked from the harness maps and covers identity, state, checkpoints, idempotency, retry classification, replay limits, budgets, escalation, audit, and evidence.
- **Operational complete when:** the shared state manager and enrolled workflow-specific equivalents validate required record fields, locks, reconciliation, terminal evidence, per-run resource budgets, and rolling trends, and the recovery/replay and resource-budget exercises pass locally and in scheduled drift. Two scheduled passing cycles are required for the separate `mature` promotion.
