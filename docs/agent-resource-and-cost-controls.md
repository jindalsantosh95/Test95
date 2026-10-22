# Agent Resource And Cost Controls

This contract makes resource limits executable for every repository-enrolled Codex workflow. It separates a deterministic, repository-calculated budget from the provider invoice: USD values are conservative standard-API-equivalent estimates based on the published model rates in `docs/agent-resource-budgets.json`; they are not Codex workspace credits, enterprise discounts, tool charges, or an OpenAI invoice.

## Enrolled Workflows And Budgets

`docs/agent-resource-budgets.json` is the source of truth for allowed models, input/output token ceilings, per-run estimated-USD ceilings, and rolling 7-day estimated-USD ceilings. Pricing entries name their official source, review date, and maximum age. An unknown model, a stale or incomplete rate card, missing usage, or an exceeded ceiling fails closed.

The enrolled workflows are:

- `sequence-goal-loop`
- `sequence-goal-pr-review-loop`
- `sequence-goal-human-comment-loop`
- `sequence-goal-associated-pr-feedback`
- `docs-semantic-gardening`
- `high-risk-inferential-review`

Budget changes require the workflow owner to justify the model and ceiling, refresh the official price source and review date, and run the exercise below. Prefer the smallest capable model and smallest relevant context.

## Per-Run Enforcement

The delivery, QA, review-feedback, and human-comment agents default to `gpt-6-sol`. Explicit GitHub repository model variables override those defaults; inspect the run's recorded model before interpreting its cost. Its standard input/cached-input/output rates are $2.00/$0.20/$10.00 per million tokens, with the published long-context multipliers retained. All enrolled model rates were rechecked against the linked official sources on 7 October 2026 and remain unchanged. The retained GPT-5.6 Sol rate card uses the published promotional $4.00/$0.40/$20.00 rates (available at least through 21 November 2026). Historical budget decisions keep their originally recorded costs and are not repriced.

Delivery and review workflows retain their $30 per-run estimated-cost ceilings. Their cumulative token ceilings are 150 million input and 3 million output tokens across all turns, replacing the legacy 25 million/40,000 limits. These are outer bounds derived from $30 divided by the cheapest enrolled cached-input rate ($0.20 per million) and output rate ($10 per million). They allow affordable cached work to continue; they are neither a single-request context limit nor permission to spend both token allowances independently. The checker still charges the selected model's actual input/cache/output mix and long-context multipliers against the combined $30 ceiling. Recalculate these bounds when changing the enrolled models or dollar budget. The smaller gardening/inferential-review allowances and all rolling seven-day ceilings remain unchanged.

These reports price the input, cached-input and output counts exposed by Codex JSONL. Separate cache-write counts are not available in that usage contract, so separately billed cache writes are excluded, as are tool fees and processing premiums. Reports remain API-equivalent estimates rather than invoice reconciliation.

Roll out the rate cards and allowlists to `main` and active coordinator branches before setting `SEQUENCE_GOAL_AGENT_MODEL`, `SEQUENCE_GOAL_REVIEW_AGENT_MODEL`, and `SEQUENCE_GOAL_HUMAN_COMMENT_AGENT_MODEL` to `gpt-6-sol`. Older models remain enrolled for a compatible transition. An old coordinator checkout without the new allowlist will reject the new repository variable.

Each enrolled workflow runs `codex exec --json` and retains its JSONL event stream. `scripts/check-agent-resource-budget.ps1` reads `turn.completed` usage, applies cached-token and long-context rates where configured, and writes a sanitized `*.budget.json` decision record. The workflow may continue only when:

- usage evidence exists and is valid;
- the selected model is allowed and priced;
- input and output tokens are within the workflow ceilings; and
- the calculated standard-API-equivalent cost is within the per-run ceiling.

Budget exhaustion is a failed terminal result. It must not be relabelled as success or bypassed by retrying unchanged work. A new attempt remains subject to the same rolling cost control.

Sequence-goal delivery uploads production and automation resource evidence with `always()` semantics, including when generation, validation, or a later delivery step fails. Every available sanitized budget decision is archived on the coordinator branch under `.codex/agent-loop/resource-attempts/` with the workflow run ID, run attempt, phase, and original surface-specific name. Retries append records instead of replacing earlier attempts, and completion uses this attempt ledger for its production, automation, and combined lifecycle totals. If a runner terminates before Codex emits a terminal usage snapshot, the raw JSONL artifact is retained when available and the missing priced snapshot is reported explicitly; cost is never invented from incomplete token evidence.

`scripts/get-agent-resource-summary.ps1` combines the configured per-run maximum budget with one or more completed budget decisions. “Maximum” means the enforced input/output token and estimated-USD ceilings for that workflow; “actual” means the measured input/output tokens and calculated standard-API-equivalent USD. Every user-facing harness cost—including PR text, workflow summaries, console success output, and failure diagnostics—is rounded up and rendered to exactly two decimal places. The underlying JSON evidence retains its precise value for enforcement and aggregation. Sequence-goal delivery retains one normalized budget decision for every measured production-agent and automation-agent attempt on the coordinator branch. Completion aggregates those retained decisions into production and automation breakdowns plus their combined actual total in the sequence diagram and coordinator PR. Resuming a coordinator reuses an existing child PR without rerunning that child agent, while a genuine rerun appends its cost instead of replacing the original delivery-cost evidence. Review/comment workers include the values for their own handling run in each PR response, distinguishing fix handling from review-only handling.

Raw JSONL can contain agent text and tool-event metadata. Treat it as workflow evidence: do not print secrets into prompts, retain it only in access-controlled workflow artifacts, and use the budget decision JSON for ordinary review.

## Rolling Trend Enforcement

Production and QA record delivery progress in the goal document. Incomplete production resumes immediately in the same Codex session while time and budget headroom permit; all turns append to one cumulative usage record under the existing per-run ceilings. The runner reserves headroom for another turn of the observed size and records a stopping reason when the allowance prevents continuation. These checks occur between turns; final budget validation and workflow timeouts still apply. The scheduled monitor recovers interrupted or incomplete delivery with up to three automatic continuations per delivery/completion stage; each dispatched workflow run has its own budget and retained cost evidence. Explicit restart resets that continuation allowance and is also a separately budgeted run. Blocked work requires intervention. Replaying completed publication reuses the goal document's verified outcome without another agent invocation.

`.github/workflows/ai-harness-agent-resource-and-cost-trend-review.yml` runs weekly and on demand. It downloads the retained `agent-resource-*` artifacts for the preceding 7 days and invokes `scripts/check-agent-resource-trend.ps1`. The check aggregates per-workflow estimated cost and fails when a rolling ceiling is exceeded; the workflow retains a trend summary and opens a focused issue for remediation.

The rolling calculation covers repository-retained evidence. Deleted, expired, unuploaded, provider-side, or manually executed usage is outside that evidence set, so this control must not be presented as invoice reconciliation.

## Operating Commands

```pwsh
scripts/check-agent-resource-budget.ps1 -ValidateConfiguration
scripts/invoke-agent-resource-budget-exercise.ps1 -EvidenceDirectory TestResults/agent-resource-budget/manual
scripts/check-agent-resource-budget-strength.ps1 -EvidencePath TestResults/agent-resource-budget/manual/summary.json
scripts/check-agent-resource-trend.ps1 -EvidenceDirectory TestResults/agent-resource -SummaryPath TestResults/agent-resource/trend-summary.json
```

The exercise proves compliant usage, token/cost rejection, missing-evidence rejection, long-context pricing, rolling-trend aggregation, and rolling-ceiling rejection without calling an external model.

## Ownership And Completion

- **Owner:** workflow owners; Platform Engineering owns the shared checker and rate-card schema.
- **Review cadence:** per run for hard ceilings, weekly for rolling trends, and whenever published pricing or model enrollment changes.
- **Operational complete when:** all enrolled workflows emit JSONL usage, apply the required per-run check before accepting output, retain budget evidence, the weekly trend workflow is active, and the six-part exercise passes in local verification and scheduled parity.
- **Mature when:** the operational controls remain passing for two scheduled cycles. Maturity is evidence of sustained operation, not a prerequisite for the operational status being `yes`.
