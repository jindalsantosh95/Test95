# Harness Enforcement Everywhere Rollout

- Status: in progress
- Owner: Platform Engineering with solution maintainers and pipeline owners
- Created: 2026-07-01
- Scope: `docs/`, `scripts/`, `.githooks/`, `.github/workflows/`, solution-owned pipeline definitions in `VAA.EAPI.Pipelines*/`

## Objective

Make repository harness controls fully enforced everywhere by ensuring required controls are computational, wired into CI, and blocking for all in-scope solutions and projects.

## Context

Current root harness documentation is strong and explicit in `docs/harness-engineering.md`, `docs/harness-contract.md`, `docs/harness-coverage.md`, and `docs/harness-pipeline-checklist.md`. The remaining gap is consistency of enforcement across all solution-owned pipelines and high-risk change paths.

This plan closes the execution gap between "documented controls" and "uniformly enforced controls."

## Acceptance Criteria

- Every active solution/project has one canonical `scripts/verify.ps1` invocation documented and used in CI.
- Every active solution/project has at least one required status check that fails closed on harness regressions.
- No pull request can merge while required harness checks are skipped, missing, or advisory-only.
- Pre-commit policy parity exists: local hook protections are either installed by default or mirrored as CI-blocking checks.
- High-risk PRs include an explicit harness coverage mapping (or accepted gap) referencing `docs/harness-coverage.md`.
- A scheduled drift/parity job runs at least weekly and reports failures to named owners.
- Repository docs are updated so enforcement policy is unambiguous, current, and auditable.

Active-scope definition:

- Active scope: solution/project currently in delivery or support rotation, enrolled in required CI harness checks, and listed as `Required` in the enforcement inventory.
- Inactive scope: solution/project retained for history or non-operational support, listed in inventory as `Inactive`, and not enrolled in required CI harness checks.

## Plan

1. Inventory current enforcement surface and owners.
2. Standardize canonical verify commands per solution/project.
3. Wire required CI checks for each solution/project and enforce fail-closed semantics.
4. Add server-side parity checks for local pre-commit protections.
5. Enforce harness-coverage evidence for high-risk pull requests.
6. Add scheduled drift/parity automation and ownership routing.
7. Run adoption audit, close gaps, and publish enforcement baseline.

## Work Breakdown

### Phase 1: Baseline Inventory (Week 1)

- Build a solution/project inventory with:
  - owning team,
  - canonical verify command (`-SolutionPath` or `-ProjectPath`),
  - pipeline path,
  - required status check name,
  - current branch protection status.
- Record inventory in a new table under `docs/harness-pipeline-checklist.md` (or linked companion file) as the source of truth.
- Flag missing ownership, missing CI path, and non-blocking checks.

Exit criteria:

- 100% of active solutions/projects are present in the inventory table.
- Inventory also lists inactive solutions/projects explicitly, marked with `Inactive` enforcement status and reason.
- Every row has owner, pipeline, verify command, and enforcement status.

### Phase 2: CI Enforcement Wiring (Weeks 1-2)

- For each inventory row, ensure pipeline executes:
  - restore/build/test for impacted scope,
  - `scripts/verify.ps1 -SolutionPath <...> -SkipBuild` or `-ProjectPath <...> -SkipBuild` at minimum,
  - full verify in main-branch or equivalent protected flow.
- Remove trigger patterns that create "required but skipped forever" statuses.
- Normalize job naming so required-check registration is stable.

Exit criteria:

- Every solution/project has a passing required check in PR.
- Required checks are impossible to bypass through path-filter skip behavior.

### Phase 3: Pre-Commit Parity Hardening (Week 2)

- Ensure central-package-management and staged-verify protections are enforced server-side in CI (not only local hooks).
- Keep local hooks for fast feedback, but treat CI checks as the non-bypassable authority.
- Document exact parity matrix in `docs/harness-contract.md`.

Exit criteria:

- Bypassing local hooks cannot allow merge of policy-breaking changes.
- CI mirrors all critical local policy guards.

### Phase 4: High-Risk PR Coverage Gate (Weeks 2-3)

- Update PR template/checklist to require harness-coverage mapping for high-risk changes.
- Add CI check or bot validation for presence of coverage mapping token/section when risk labels are applied.
- Define accepted-gap format and owner/expiry expectation.

Exit criteria:

- High-risk PRs cannot merge without coverage evidence or explicit accepted gap.
- Coverage entries reference guide + sensor + lifecycle + policy.

### Phase 5: Scheduled Drift And Parity Operations (Week 3)

- Add a weekly scheduled job to run:
  - selected full harness verifies,
  - knowledge-health check,
  - parity drift checks (required-check existence, pipeline mapping drift).
- Route failures to owning team aliases and maintain an open remediation list.

Exit criteria:

- Scheduled job runs weekly with published results.
- Failures are assigned with owner and due date.

### Phase 6: Enforcement Baseline Publication (Week 4)

- Publish final enforcement matrix and evidence links in docs.
- Update `docs/harness-engineering.md` and `docs/harness-pipeline-checklist.md` to mark controls as required/advisory with no ambiguity.
- Record unresolved gaps in `docs/tech-debt-tracker.md` with trigger-to-reopen conditions.

Exit criteria:

- Enforcement status for each control is explicit and current.
- Remaining gaps are intentionally accepted and tracked.

## Decisions

- 2026-07-01: Treat CI-required checks as the final enforcement authority; local hooks remain a speed optimization.
- 2026-07-01: Standardize on fail-closed required status checks, avoiding path-trigger skip configurations that can deadlock merges.
- 2026-07-01: Require explicit harness-coverage evidence for high-risk PRs to keep harness coverage operational, not theoretical.

## Validation

- `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/verify.ps1 -SolutionPath <solution>.sln -SkipBuild`
- `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/verify.ps1 -ProjectPath <project>.csproj -SkipBuild`
- `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/check-knowledge-health.ps1`
- Confirm required status checks in branch protection for all protected branches.
- Confirm scheduled drift/parity workflow execution history and owner notifications.

## Risks And Mitigations

- Risk: Monorepo path filters produce skipped required checks.
  - Mitigation: always-trigger workflows with internal no-op logic.
- Risk: Ownership ambiguity delays remediation.
  - Mitigation: inventory requires explicit team owner per row.
- Risk: Legacy warning/test debt causes noisy failures.
  - Mitigation: ratchet model with documented baselines and burn-down plans.

## Outcome

In progress as of 2026-07-02.

Completed now:

- Phase 1 implementation: enforcement inventory rows populated for automation and non-automation scopes in `docs/harness-pipeline-checklist.md` with explicit owner, verify command, workflow path, and stable required check mapping.
- Phase 2 baseline implementation: repo-level PR/main parity workflow added to run scoped `scripts/verify.ps1 -SkipBuild`.
- Phase 4 baseline implementation: high-risk PR harness-coverage gate added via workflow and PR template.
- Phase 4 operational hardening: high-risk PR gate now enforces context-routing fields (`Scope`, `Dependencies`, `Assumptions`, `Impacted sensors`, `Rollback/owner`) and parity script coverage.
- Phase 5 baseline implementation: weekly scheduled drift/parity workflow added.
- Phase 5 operational hardening: remediation evidence log and freshness/overdue validation sensor added (`docs/harness-remediation-log.md`, `scripts/check-harness-remediation-log.ps1`) and wired into scheduled drift workflow.
- Phase 5 autonomy hardening: added `closed-loop-gate` workflow and remediation escalation policy checks; added auto high-risk labeling workflow to reduce manual PR risk routing.
- Phase 6 baseline implementation: required/advisory policy language and enforcement inventory structure published in root harness docs.
- AI-review hardening implementation:
  - stable parity required-check (`parity-gate`) documented and enforced in workflow,
  - high-risk gate changed to always run with in-script high-risk branching and strict field-value parsing,
  - inventory drift sensor hardened to fail on unresolved `Gap`/`TBD`/placeholder/`Pending`/`Interim` markers,
  - agent-memory UTC validation hardened (including future timestamp rejection),
  - harness-rules sensor hardened to verify real wiring for agent-memory checks.

Remaining for full completion:

- Confirm branch protection required-check wiring in GitHub settings for all in-scope solutions/projects:
  - `parity-gate`,
  - `harness-coverage`,
  - `verify` (automation scope).
- Keep inventory drift check in fail-on-gap mode and review failures weekly until branch-protection confirmations are complete.
