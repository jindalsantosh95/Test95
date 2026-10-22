# Harness Remediation Log

This log is the operational evidence trail for drift/entropy management and closed-loop harness delivery.

Use with:

- [Harness Pipeline Checklist](harness-pipeline-checklist.md)
- [Harness Contract](harness-contract.md)
- [Harness Definition Of Done](harness-definition-of-done.md)

## Branch Protection Confirmations

Required checks must be configured as blocking checks on `main`.

| Check Name | Branch | Confirmed | Bypass Policy | Confirmed By | Confirmed At UTC | Evidence |
|---|---|---|---|---|---|---|
| `parity-gate` | `main` | Yes | No bypass for required checks | Platform Engineering | 2026-07-03T14:05:00Z | [ai-harness-repository-parity.yml](../.github/workflows/ai-harness-repository-parity.yml) |
| `harness-coverage` | `main` | Yes | No bypass for required checks | Platform Engineering | 2026-07-03T14:05:00Z | [ai-harness-high-risk-coverage.yml](../.github/workflows/ai-harness-high-risk-coverage.yml) |
| `verify` | `main` | Yes | No bypass for required checks | Automation Engineering | 2026-07-03T14:05:00Z | [ai-harness-vaa-eapi-automation.yml](../.github/workflows/ai-harness-vaa-eapi-automation.yml) |
| `closed-loop-gate` | `main` | Yes | No bypass for required checks | Platform Engineering | 2026-07-03T15:35:00Z | [ai-harness-closed-loop-autonomy.yml](../.github/workflows/ai-harness-closed-loop-autonomy.yml) |

## Drift Review Log

Record each scheduled drift/parity run with owner and outcome.

| Review Date UTC | Workflow | Result | Owner | Notes |
|---|---|---|---|---|
| 2026-07-03T14:10:00Z | `.github/workflows/ai-harness-drift-parity.yml` | Pass | Platform Engineering | Baseline operational evidence log introduced; no unresolved inventory gaps. |
| 2026-07-03T15:55:00Z | `.github/workflows/ai-harness-closed-loop-autonomy.yml` | Pass | Platform Engineering | Closed-loop autonomy gate and remediation escalation checks validated in cycle 2. |
| 2026-07-13T07:17:47Z | `.github/workflows/ai-harness-drift-parity.yml` | Pass | Platform Engineering | [Scheduled run 29231614190](https://github.com/VAAD1ST/eapi/actions/runs/29231614190) passed inventory, context-routing, remediation-freshness, and repository-parity checks. |
| 2026-07-27T07:18:49Z | `.github/workflows/ai-harness-drift-parity.yml` | Fail | Platform Engineering | [Scheduled run 30245697011](https://github.com/VAAD1ST/eapi/actions/runs/30245697011) passed inventory and context-routing checks, then detected that the drift-review evidence had exceeded the 14-day freshness window. |
| 2026-07-27T16:45:00Z | `.github/workflows/ai-harness-drift-parity.yml` | Pass | Platform Engineering | Docs harness support promoted to required verification through `scripts/check-docs-harness-support.ps1`; runtime-feedback and weekly-kpi evidence are now checked in local/CI parity. |
| 2026-08-10T06:36:40Z | `.github/workflows/ai-harness-drift-parity.yml` | Pass | Platform Engineering | [Scheduled run 31362629344](https://github.com/VAAD1ST/eapi/actions/runs/31362629344) passed inventory, context-routing, remediation-freshness, operational-evidence, recovery, architecture, agent-control, and repository-parity checks. |
| 2026-08-24T06:19:53Z | `.github/workflows/ai-harness-drift-parity.yml` | Pass | Platform Engineering | [Scheduled run 32696863744](https://github.com/VAAD1ST/eapi/actions/runs/32696863744) passed inventory, context-routing, remediation-freshness, operational-evidence, recovery, architecture, agent-control, and repository-parity checks. |
| 2026-08-24T10:01:20Z | `scripts/check-harness-enforcement-inventory.ps1`, `scripts/check-context-routing.ps1`, `scripts/check-harness-remediation-log.ps1` | Fail | Platform Engineering | Local drift triage: inventory and context-routing checks passed; remediation freshness check initially failed because the prior review exceeded the 14-day window. Fresh evidence and closure were recorded in remediation item HDR-2026-08-24-001. |
| 2026-09-07T06:20:08Z | `.github/workflows/ai-harness-drift-parity.yml` | Fail | Platform Engineering | [Scheduled run 34090270885](https://github.com/VAAD1ST/eapi/actions/runs/34090270885) could not check out the repository on Windows because remote branch names differ only by case. The workflow now uses the main snapshot at depth one, with a harness-rule regression guard; a passing scheduled cycle is still required before maturity counts recover. |
| 2026-09-07T10:57:39Z | `scripts/check-harness-enforcement-inventory.ps1`, `scripts/check-context-routing.ps1`, `scripts/check-harness-remediation-log.ps1` | Pass | Platform Engineering | Local closed-loop gate verification passed enforcement inventory, context-routing, and remediation freshness checks; evidence refreshed before the 14-day threshold. |
| 2026-09-21T06:21:59Z | `.github/workflows/ai-harness-drift-parity.yml` | Pass | Platform Engineering | [Scheduled run 35567991782](https://github.com/VAAD1ST/eapi/actions/runs/35567991782) passed inventory, context-routing, remediation-freshness, operational-evidence, recovery, architecture, agent-control, and repository-parity checks. |
| 2026-09-21T13:51:21Z | `scripts/check-harness-enforcement-inventory.ps1`, `scripts/check-context-routing.ps1`, `scripts/check-harness-remediation-log.ps1` | Pass | Platform Engineering | Local PR-gate remediation review refreshed the drift evidence after the 14-day threshold; enforcement inventory and context routing remained compliant. |
| 2026-09-28T06:22:57Z | `.github/workflows/ai-harness-drift-parity.yml` | Pass | Platform Engineering | [Scheduled run 36386005146](https://github.com/VAAD1ST/eapi/actions/runs/36386005146) passed inventory, context-routing, remediation-log freshness, operational evidence, recovery, architecture, agent control, and repository-parity checks. |
| 2026-10-05T06:25:13Z | `.github/workflows/ai-harness-drift-parity.yml` | Fail | Platform Engineering | [Scheduled run 37272248591](https://github.com/VAAD1ST/eapi/actions/runs/37272248591) detected stale drift-review evidence because the 28 September passing cycle had not been recorded in this log; closure is recorded in HDR-2026-10-05-002. |
| 2026-10-12T06:25:37Z | `.github/workflows/ai-harness-drift-parity.yml` | Pass | Platform Engineering | [Scheduled run 38212324105](https://github.com/VAAD1ST/eapi/actions/runs/38212324105) passed inventory, context-routing, remediation-freshness, operational-evidence, recovery, architecture, agent-control, and repository-parity checks. |
| 2026-10-19T06:20:29Z | `.github/workflows/ai-harness-drift-parity.yml` | Pass | Platform Engineering | [Scheduled run 39209194344](https://github.com/VAAD1ST/eapi/actions/runs/39209194344) passed all drift-parity steps. This cycle and the omitted 12 October cycle were verified and recorded after [PR #2502's closed-loop run 39225587033](https://github.com/VAAD1ST/eapi/actions/runs/39225587033) found the previous log entry stale. No freshness threshold or required check was changed. |
| 2026-10-19T08:24:23Z | `scripts/check-harness-enforcement-inventory.ps1`, `scripts/check-context-routing.ps1`, `scripts/check-harness-remediation-log.ps1` | Pass | Platform Engineering | Local PR-gate drift review: enforcement inventory and context-routing checks passed; this entry closes the stale remediation-log evidence finding in HDR-2026-10-19-001. |

## Operational Evidence

### Context-routing freshness review (2026-10-05)

PR #2333's [closed-loop run 37296496226](https://github.com/VAAD1ST/eapi/actions/runs/37296496226) and [repository-parity run 37296496055](https://github.com/VAAD1ST/eapi/actions/runs/37296496055) failed because `LastReviewedUtc` was 32 days old. The review checked both routing-map destinations, the PR template, missing/empty high-risk context-field enforcement, and local/closed-loop/scheduled context-check wiring. No threshold, bypass or ownership change was made. After refreshing the review date, local enforcement-inventory, context-routing (30 days) and remediation-log (14 days) checks all passed. This is local review evidence, not a claim that the next CI run or live branch protection was verified.

| Evidence ID | Theme | Control | Owner | Last Reviewed UTC | Evidence |
|---|---|---|---|---|---|
| runtime-feedback | Runtime feedback loop | Production incidents, telemetry findings, or smoke/regression failures must create a traceable harness update in tests, docs, rules, or remediation items. | Solution Owners | 2026-07-27T16:45:00Z | `scripts/check-docs-harness-support.ps1` validates this evidence marker and `docs/harness-definition-of-done.md` keeps the runtime feedback loop Green only while the marker exists. |
| weekly-kpi | Adoption KPI visibility | Weekly harness reporting must include verify pass rate, drift count, stale-doc count, and mean time to fix in team notes or this remediation log. | Harness Maintainer | 2026-07-27T16:45:00Z | `scripts/check-docs-harness-support.ps1` validates this evidence marker; scheduled drift parity keeps remediation evidence fresh. |

## Remediation Items

Track every finding from scheduled drift/parity runs until closure.

| ID | Opened At UTC | Finding | Owner | Due At UTC | Status | Repeat Count | Escalation Control | Closed At UTC | Evidence |
|---|---|---|---|---|---|---|---|---|---|
| HDR-2026-07-03-001 | 2026-07-03T14:12:00Z | Add remediation evidence log and required validation sensor for scheduled drift checks. | Platform Engineering | 2026-07-03T18:00:00Z | Closed | 1 | n/a | 2026-07-03T14:20:00Z | [ai-harness-drift-parity.yml](../.github/workflows/ai-harness-drift-parity.yml) |
| HDR-2026-07-03-002 | 2026-07-03T15:05:00Z | Add fail-closed context-routing operational guidance checks for high-risk PRs and scheduled drift. | Platform Engineering | 2026-07-03T18:00:00Z | Closed | 1 | n/a | 2026-07-03T15:20:00Z | [check-context-routing.ps1](../scripts/check-context-routing.ps1) |
| HDR-2026-07-03-003 | 2026-07-03T15:35:00Z | Add closed-loop autonomy gate and auto high-risk labeling to reduce manual governance steps. | Platform Engineering | 2026-07-03T18:00:00Z | Closed | 1 | n/a | 2026-07-03T15:45:00Z | [ai-harness-closed-loop-autonomy.yml](../.github/workflows/ai-harness-closed-loop-autonomy.yml) |
| HDR-2026-07-27-001 | 2026-07-27T07:20:34Z | Record the latest scheduled drift result after the evidence exceeded its 14-day freshness window. | Platform Engineering | 2026-07-27T12:00:00Z | Closed | 1 | n/a | 2026-07-27T10:01:14Z | [Scheduled run 30245697011](https://github.com/VAAD1ST/eapi/actions/runs/30245697011) |
| HDR-2026-08-24-001 | 2026-08-24T10:01:20Z | Refresh drift-review evidence after the 14-day freshness threshold was exceeded. | Platform Engineering | 2026-08-24T12:00:00Z | Closed | 1 | n/a | 2026-08-24T10:01:26Z | [Drift Review Log](harness-remediation-log.md) |
| HDR-2026-09-07-001 | 2026-09-07T10:57:39Z | Refresh drift-review evidence after the 14-day freshness threshold was exceeded again. | Platform Engineering | 2026-09-07T12:00:00Z | Closed | 1 | n/a | 2026-09-07T10:57:39Z | [Drift Review Log](harness-remediation-log.md) |
| HDR-2026-09-21-001 | 2026-09-21T12:58:16Z | Refresh drift-review evidence after the closed-loop gate reported that the previous review had reached the 14-day freshness boundary. | Platform Engineering | 2026-09-21T14:00:00Z | Closed | 1 | n/a | 2026-09-21T13:10:58Z | [Scheduled run 35567991782](https://github.com/VAAD1ST/eapi/actions/runs/35567991782) |
| HDR-2026-10-05-001 | 2026-10-05T10:36:30Z | Review context-routing guidance after its 30-day freshness gate blocked PR #2333. | Platform Engineering | 2026-10-05T18:00:00Z | Closed | 1 | n/a | 2026-10-05T10:36:30Z | [Context-routing review](context-routing-operating-guidance.md#review-evidence); local inventory, context-routing and remediation-log checks passed after the review. |
| HDR-2026-10-05-002 | 2026-10-05T06:25:13Z | Record the omitted 28 September passing drift cycle after the 5 October scheduled run detected stale evidence. | Platform Engineering | 2026-10-06T09:00:00Z | Closed | 1 | n/a | 2026-10-06T08:35:58Z | [Scheduled run 36386005146](https://github.com/VAAD1ST/eapi/actions/runs/36386005146) and [Scheduled run 37272248591](https://github.com/VAAD1ST/eapi/actions/runs/37272248591) |
| HDR-2026-10-19-001 | 2026-10-19T08:24:23Z | Refresh drift-review evidence after the PR gate detected that the latest recorded review had reached the 14-day freshness boundary. | Platform Engineering | 2026-10-19T12:00:00Z | Closed | 1 | n/a | 2026-10-19T08:24:23Z | Local `check-harness-enforcement-inventory.ps1`, `check-context-routing.ps1 -MaxAgeDays 30`, and `check-harness-remediation-log.ps1 -MaxReviewAgeDays 14` checks passed after recording this review. |

Status values:

- `Open`
- `InProgress`
- `Closed`

Rules:

- `Open` and `InProgress` items must have non-empty owner and due date.
- Overdue `Open`/`InProgress` items are policy failures and must fail scheduled drift checks.
- `Closed` items must include closure timestamp and evidence reference.
- Repeat count must be a positive integer.
- When repeat count is 3 or greater, `Escalation Control` is required and must reference a concrete new or strengthened control.
- Branch protection rows must include explicit bypass policy text.
- Closed-loop autonomy evidence requires at least 2 recent drift-review cycles with `Pass` result.
