# Documentation Gardening

This contract defines how repository documentation is checked for structural and semantic drift. Documentation gardening is a feedback loop: detect stale guidance, compare it with authoritative implementation evidence, create a focused correction, and record closure.

## Scope

The gardening scope includes `AGENTS.md`, `docs/`, solution and project READMEs, design patterns, execution plans, decisions, technical debt, sequence diagrams, pipeline maps, and harness inventories. Generated logs are checked for shape and freshness but are not treated as design authority.

## Two Sensor Classes

| Sensor | Detects | Current examples | Policy |
| --- | --- | --- | --- |
| Computational | Missing artifacts, invalid links, stale dates, malformed metadata, inventory mismatch, or absent evidence | `check-knowledge-health.ps1`, `check-context-routing.ps1`, `check-docs-harness-support.ps1`, `drift_coverage_scan.ps1` | Required where enrolled |
| Inferential | Guidance that is syntactically valid but disagrees with code, configuration, tests, pipelines, or current operating behavior | `.github/workflows/ai-harness-docs-semantic-gardening.yml` using the comparison contract below | Advisory scheduled control; fail-closed when invoked |

## Semantic Comparison Contract

Every inferential gardening finding must identify:

- the documentation claim and file;
- the authoritative code, configuration, test, pipeline, or runtime evidence;
- whether the document is stale, ambiguous, duplicated, orphaned, or superseded;
- confidence and any unresolved judgment;
- the proposed smallest correction;
- owner, due date, and verification method.

An inferential sensor must not rewrite business or architecture intent merely because current code differs. When intent and implementation conflict, escalate to the owner and use a decision record or execution plan.

## Scheduled Workflow

1. Run computational documentation and drift sensors.
2. Select changed, stale, high-risk, and repeatedly failing areas for semantic comparison.
3. Compare claims only with repository-local or approved runtime evidence.
4. Open a focused PR or remediation item; do not combine unrelated gardening changes.
5. Run knowledge health, harness rules, docs harness support, and any sensor affected by the corrected claim.
6. Record pass/fail, stale-document count, owner, due date, and closure evidence in [`harness-remediation-log.md`](harness-remediation-log.md).
7. Escalate overdue or repeated findings using the remediation-log rules.

## Freshness And SLA

- High-risk operating guidance uses the freshness window enforced by `check-context-routing.ps1`.
- Broken links, missing required artifacts, or incorrect commands are fixed before merge when introduced by a change.
- Scheduled findings are triaged within 5 working days.
- A materially false security, deployment, recovery, or production-operating instruction is treated as a required-check defect and fixed within 1 working day.
- A finding repeated three times must produce a stronger guide or sensor, not another untracked edit.

## Safety And Change Rules

- Preserve decision history; mark superseded decisions instead of deleting their rationale.
- Move completed plans according to the execution-plan policy rather than erasing evidence.
- Do not fabricate run results, freshness dates, owners, or external confirmation.
- Keep generated content clearly identified and reproducible.
- Require human judgment for unresolved product intent, security policy, ownership, or architecture trade-offs.

## Ownership, Evidence, And Acceptance

- **Owner:** Harness Maintainer for the scheduled process; solution owners for scoped corrections.
- **Review cadence:** computational sensors on relevant PRs and weekly scheduled drift; inferential sampling at least monthly and after repeated failures.
- **Evidence location:** [`harness-remediation-log.md`](harness-remediation-log.md), focused PRs, and the affected document's verification metadata where present.
- **Artifact complete when:** this contract is linked from the harness maps and defines scope, sensor classes, comparison fields, cadence, SLA, ownership, remediation, and evidence.
- **Operational complete when:** the scheduled computational and semantic workflows open or record corrections and sustain fresh closure evidence for two review cycles. The semantic workflow is enrolled; the two-cycle condition remains outstanding.
