# Continuous Harness Maturity Evidence

Operational capability and sustained maturity are different facts. A repository change can make a control operational; maturity requires retained evidence that the control continues to pass across consecutive scheduled cycles.

[`harness-maturity-policy.json`](harness-maturity-policy.json) defines the tracked capabilities, required consecutive pass count, evidence workflow, and bounded history. The policy is evaluated from run evidence rather than manually incrementing a status field.

## Evidence Lifecycle

1. `.github/workflows/ai-harness-drift-parity.yml` runs the enrolled computational controls.
2. Its final `always()` step downloads the latest retained maturity ledger, even when an earlier gate failed.
3. `scripts/update-harness-maturity-evidence.ps1` appends the current run exactly once, records its terminal outcome, bounds the history, and derives consecutive passing counts.
4. `scripts/check-harness-maturity-evidence.ps1` validates provenance, uniqueness, ordering, policy coverage, and derived counts.
5. The updated ledger and summary are retained as the `harness-maturity-*` workflow artifact.

A failed, cancelled, missing, duplicated, or malformed scheduled cycle cannot promote maturity. Any non-success scheduled cycle resets the consecutive count for the capabilities it covered. Manual dispatches remain diagnostic evidence but neither increment nor reset promotion. Local synthetic exercises prove the state transitions but never enter the retained workflow ledger.

## Operating Commands

```pwsh
scripts/invoke-harness-maturity-evidence-exercise.ps1 -EvidenceDirectory TestResults/harness-maturity/manual
scripts/check-harness-maturity-evidence-strength.ps1 -EvidencePath TestResults/harness-maturity/manual/summary.json
scripts/check-harness-maturity-evidence.ps1 -LedgerPath <retained-ledger.json> -SummaryPath <retained-summary.json>
scripts/check-harness-maturity-evidence.ps1 -LedgerPath <retained-ledger.json> -SummaryPath <retained-summary.json> -RequireMature
scripts/check-operational-harness.ps1 -RequireMature -MaturityLedgerPath <retained-ledger.json> -MaturitySummaryPath <retained-summary.json>
```

`-RequireMature` is a promotion decision, not the default operational gate. It succeeds only when every policy-tracked capability has the required consecutive scheduled passes.

## Ownership And Completion

- **Owner:** Harness Maintainer.
- **Retention:** twenty cycles in the derived ledger; workflow artifacts are retained for 365 days.
- **Operational complete when:** every scheduled parity outcome is recorded through an `always()` step, the ledger is retained across runs, failure resets and duplicate rejection are exercised, and the default verifier validates the policy and exercise.
- **Mature when:** the retained scheduled summary passes `-RequireMature`; synthetic or manually edited evidence cannot substitute for scheduled runs.
