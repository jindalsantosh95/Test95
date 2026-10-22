# Context Routing Operating Guidance

LastReviewedUtc: 2026-10-05T10:36:30Z
Owner: Platform Engineering

This guide defines operational context-routing requirements for repository and automation harness work.

## Routing Map

| Scope | Canonical Guidance | Primary Sensors | Freshness Owner |
|---|---|---|---|
| `/docs` | `docs/readme.md`, `docs/harness-engineering.md`, `docs/harness-contract.md` | `scripts/check-context-routing.ps1`, `scripts/check-harness-rules.ps1` | Platform Engineering |
| `/VAA.EAPI.Automation/docs` | `VAA.EAPI.Automation/docs/HARNESS_ENGINEERING.md` | `scripts/check-context-routing.ps1`, `scripts/verify.ps1` | Automation Engineering |

## Required Context Fields For High-Risk PRs

High-risk PRs must include non-empty values for:

- `Scope:`
- `Dependencies:`
- `Assumptions:`
- `Impacted sensors:`
- `Rollback/owner:`

These fields are validated by CI in `.github/workflows/ai-harness-high-risk-coverage.yml`.

## Operational Freshness Rules

- Update `LastReviewedUtc` when context-routing requirements or ownership changes.
- `LastReviewedUtc` must stay within 30 days for active context guidance.
- Route context gaps into `docs/harness-remediation-log.md` with owner, due date, and closure evidence.

## Evidence Expectations

Operational `Yes` requires:

- fail-closed CI validation for high-risk context fields,
- local and CI parity checks for context-routing guidance integrity,
- scheduled drift checks proving freshness and closure discipline.

## Review Evidence

- 2026-10-05: Reviewed both routing-map destinations, the PR template's five context fields, and the high-risk workflow's missing/empty-value enforcement. Confirmed `scripts/verify.ps1`, the closed-loop gate and scheduled drift workflow retain the context-routing check and its 30-day limit. Requirements and ownership remain unchanged. This is a repository wiring review, not a new branch-protection or production-security approval; see HDR-2026-10-05-001 in `docs/harness-remediation-log.md`.
