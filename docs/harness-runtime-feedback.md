# Harness Runtime Feedback

This log records runtime signals that resulted in a durable harness improvement. It is intentionally limited to sanitized repository-harness evidence; production incidents remain in their approved incident system and may be referenced only by safe identifiers.

| Feedback ID | Observed At UTC | Signal | Correlation | Durable Change | Verification | Owner | Status |
| --- | --- | --- | --- | --- | --- | --- | --- |
| `HRF-2026-08-06-001` | `2026-08-06T16:10:00Z` | Operational-registry timestamp validation incorrectly treated a valid ISO UTC value as stale after culture-dependent string conversion. | `local-operational-validator` | `scripts/check-operational-harness.ps1` now evaluates the deserialized `DateTime` directly while validating the original JSON `Z` suffix. | `scripts/check-operational-harness.ps1` passed with the current registry. | Harness Maintainer | Closed |
| `HRF-2026-08-06-002` | `2026-08-06T16:25:00Z` | Agent-workflow control validation detected two mutating review loops without explicit attempt budgets. | `local-agent-workflow-controls` | Added fail-closed one-attempt guards to the sequence-goal PR-review and human-comment workflows. | `scripts/check-agent-workflow-controls.ps1` passed for five enrolled workflows. | Workflow Owners | Closed |
| `HRF-2026-08-07-003` | `2026-08-07T09:10:00Z` | The local telemetry collector exposed logs, metrics and traces but had no computationally bounded correlation-query interface. | `local-docs-harness-observability` | Added `scripts/query-agent-observability.ps1` plus oversized-window and missing-correlation rejection exercises. | `scripts/invoke-agent-observability-exercise.ps1` passed all four checks. | Harness Maintainer | Closed |

New entries must identify the runtime signal, safe correlation, durable test/rule/guide/workflow change, verification, owner, and closure state.
