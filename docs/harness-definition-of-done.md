# Harness Definition Of Done

This matrix defines the repository harness-engineering completion bar.

Use this with:

- [`harness-engineering.md`](harness-engineering.md) for local controls and verification flow,
- [`harness-pipeline-checklist.md`](harness-pipeline-checklist.md) for CI parity and gate policy,
- [`behavior-harness.md`](behavior-harness.md) for behavior-to-sensor coverage.

## Ownership Model

| Role | Responsibility |
| --- | --- |
| Harness Maintainer (rotating) | Maintains harness scripts/docs and publishes weekly status. |
| Solution Owners | Keep solution-specific sensors/tests/docs aligned to this matrix. |
| PR Author | Updates harness docs/tests when change introduces risk or drift. |
| Reviewer | Enforces fix-or-justify closure for harness-affecting findings. |

Recommended rotation:

- Harness Maintainer rotates every sprint.
- Rotation and backup are recorded in sprint notes or team board.

## SLA Windows

| Category | SLA |
| --- | --- |
| Broken required harness check on main | Fix within 1 working day |
| Invalid tracked agent-memory contract (`.codex/state/session-state.example.json`) | Fix within 1 working day |
| Stale active plan / proposed decision / open debt item | Triage within 5 working days |
| Missing behavior-to-test mapping for critical flow | Add mapping before merge |
| Incident-driven harness follow-up (test/doc/rule) | Open tracked item within 2 working days; merge within 10 working days |
| Materially false security/deployment/recovery operating guidance | Fix within 1 working day |
| Stale autonomous-workflow lock or ambiguous external mutation | Triage before retry; escalate within 1 working day if unreconciled |

## DoD Matrix

| Theme | Measurable Exit Criteria | Owner | Status |
| --- | --- | --- | --- |
| Shared playbook and terminology | `docs/harness-engineering.md` exists, is linked from docs map, and defines harness scope/terms. | Harness Maintainer | Green |
| Standard verification entrypoint | `scripts/verify.ps1` is the default local gate and supports scoped `-SolutionPath` and `-ProjectPath`. | Harness Maintainer | Green |
| Harness rule checks | `scripts/check-harness-rules.ps1` runs from `verify.ps1` and fails on missing required harness wiring/docs. | Harness Maintainer | Green |
| Drift sensing | `scripts/drift_coverage_scan.ps1` reports actionable coverage/docs/dependency drift and supports scheduled runs. | Harness Maintainer + Solution Owners | Green |
| Behavior harness mapping | `docs/behavior-harness.md` maps critical behaviors to concrete executable sensors. | Solution Owners | Green |
| Local guardrails | `.githooks/` hooks install via `scripts/verify.ps1 -InstallHooks` and enforce staged harness checks. | Harness Maintainer | Green |
| PR feedback discipline | Review comments on harness-risk items are fixed or explicitly justified before resolution. | PR Author + Reviewer | Green |
| Session memory continuity | The tracked example contract is validated, local state is ignored and optionally surfaced by Codex hooks, and cross-engineer handoff uses PR descriptions or execution plans. | Harness Maintainer | Green |
| Doc gardening cadence | [`doc-gardening.md`](doc-gardening.md) defines deterministic and semantic review; scheduled drift exists, but Green requires two cycles of scoped semantic comparison and correction evidence. | Harness Maintainer (rotation) | Amber |
| Runtime feedback loop | [`agent-observability.md`](agent-observability.md) defines source, query, correlation, redaction and feedback evidence; Green requires an approved agent-readable logs/metrics/traces source and two fresh feedback cycles. | Solution Owners | Amber |
| Tool integration and executable environment | [`agent-tool-and-executable-environments.md`](agent-tool-and-executable-environments.md) defines local and Codex CI profiles; real tool/worktree/isolation probes and failure mutations run in verify and scheduled parity. Green requires two retained scheduled passes. | Platform + Workflow Owners | Amber |
| Continuous maturity evidence | [`harness-continuous-maturity.md`](harness-continuous-maturity.md) derives consecutive passes from an always-recorded scheduled ledger, rejects duplicates, and resets on failure. Green requires two real retained workflow cycles, not synthetic exercise results. | Harness Maintainer | Amber |
| Agent execution and recovery | [`agent-execution-and-recovery.md`](agent-execution-and-recovery.md) defines run records, locks, reconciliation, retries, replay limits and budgets; Green requires enrolled workflows to validate the contract and pass recovery exercises for two cycles. | Workflow Owners + Platform | Amber |
| Resource and cost controls | [`agent-resource-and-cost-controls.md`](agent-resource-and-cost-controls.md) defines required JSONL usage, per-run token/cost gates, official rate-card freshness, retained evidence, and monthly rolling ceilings; controls are operational and Green requires two scheduled passing cycles. | Workflow Owners + Platform | Amber |
| Architecture fitness gates | Automated fitness checks and repo parity gates enforce architecture rules in CI for major solutions, with docs-level evidence checked by `scripts/check-docs-harness-support.ps1`. | Solution Owners + Platform | Green |
| Adoption KPI visibility | Weekly reporting evidence for verify pass rate, drift count, stale-doc count, and mean time to fix is recorded in `docs/harness-remediation-log.md` and checked by `scripts/check-docs-harness-support.ps1`. | Harness Maintainer | Green |
| Governance durability | Plans/decisions/debt are linked to relevant code changes and release-cycle review evidence is checked through docs harness support and remediation-log gates. | Solution Owners | Green |

Status definitions:

- `Green`: exit criteria met and sustained for 2 consecutive release cycles.
- `Amber`: partially implemented, manual, or not yet sustained for 2 cycles.
- `Red`: missing or consistently failing.

## RAG Scoring Formula

Use this formula each week:

1. Score each theme:
   - `Green = 2`
   - `Amber = 1`
   - `Red = 0`
2. Compute:
   - `ActualScore = sum(theme scores)`
   - `MaxScore = theme count * 2`
   - `CoveragePercent = (ActualScore / MaxScore) * 100`
3. Derive overall RAG:
   - `Green` when `CoveragePercent >= 85` and no required-gate theme is Red.
   - `Amber` when `CoveragePercent` is between `60` and `84`, or any required-gate theme is Amber.
   - `Red` when `CoveragePercent < 60`, or any required-gate theme is Red.

Required-gate themes:

- Standard verification entrypoint
- Harness rule checks
- Behavior harness mapping
- Local guardrails
- Session memory continuity
- Runtime feedback loop
- Tool integration and executable environment
- Continuous maturity evidence

## Weekly Update Template

Use this lightweight update format in team notes:

```text
Harness DoD Week of YYYY-MM-DD
CoveragePercent: NN%
Overall: Green|Amber|Red
Themes moved this week: ...
SLA breaches: ...
Top 3 next actions: ...
```
