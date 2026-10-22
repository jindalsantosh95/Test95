# Sequence-goal production readiness

- Status: implemented, awaiting PR review
- Owner: EAPI harness maintainers
- Created: 2026-09-24
- Scope: production runner, production/ledger/QA workflows, harness regression checks

## Objective

Prevent a successful agent process that reports incomplete delivery from becoming a QA checkpoint. Resume partial production PRs instead of repeatedly sending the same incomplete implementation to QA.

## Acceptance Criteria

- Production returns a validated Complete, Incomplete, or Blocked outcome with acceptance evidence, verification and remaining work.
- Incomplete changes remain reviewable in a draft PR and cannot dispatch QA.
- Readiness is tied to the production commit; missing or stale readiness resumes production.
- Ledger and direct QA entry points independently reject missing or stale readiness.

## Plan

1. Add structured outcome validation and immutable-head readiness checks.
2. Resume authenticated existing production branches and retain incomplete work.
3. Exercise incomplete, malformed, stale and complete outcomes and run repository harness checks.

## Decisions

- 2026-09-24: Readiness is a workflow-written marker in the authenticated production PR body, bound to its full commit SHA. Backend completion additionally runs resolved scoped verification before publication. Other surfaces retain their repository-owned verification contracts and must report actual successful verification commands.
- 2026-09-24: A normal push protects resumed branches from concurrent updates; readiness publication rechecks the remote head. The workflow never force-pushes resumed work.

## Validation

- `pwsh -NoProfile -File scripts/invoke-sequence-goal-production-readiness-exercise.ps1`: passed 21 cases, including execution of the actual workflow resume decision and partial-publication gate; parsed 60 workflow PowerShell blocks.
- `pwsh -NoProfile -File scripts/verify.ps1 -Base origin/main -ScopePath . -SkipBuild -SkipDeploymentCheck`: passed.
- Parsed all three changed workflow YAML files and ran `git diff --check`: passed.
- Live GitHub delivery was not dispatched from this implementation branch; CI and subsequent controlled delivery exercise remain review/deployment validation.
