# Darwinium Member Management Flow

- Status: proposed
- Owner: Loyalty Member Management / Loyalty Customer / Facade maintainers
- Created: 2026-09-04
- Scope: Darwinium facade, Loyalty Member Management, Loyalty Customer, deployments, and integration documentation

## Objective

Deliver the Darwinium fraud-account-lock flow with Loyalty Member Management as the dedicated asynchronous orchestration owner while Loyalty Customer retains Customer-document persistence ownership.

## Context

PR #1912 documented the initial draft with the fraud subscriber in Loyalty Customer. The repository's existing bulk-member-status workflow shows that Loyalty Member Management already owns the comparable lookup and event fan-out. The revised sequence and the linked architecture decision document the new ownership boundary.

## Acceptance Criteria

- The facade publishes only the dedicated fraud-lock CloudEvent after authenticated, valid HTTP acceptance.
- Loyalty Member Management validates the member, email, and authoritative status before publishing the established event fan-out with a server-controlled Darwinium audit origin.
- Loyalty Customer receives the existing Customer-document update event and remains the sole Customer-document persistence owner.
- Terminal telemetry, partial fan-out handling, status-concurrency policy, and downstream operational ownership are explicitly tested and documented.
- Deployment, configuration, and automated verification changes are complete before release.

## Plan

1. Add the facade, fraud message contract, protected endpoint, and durable publisher.
2. Add the Loyalty Member Management subscriber, Customer-document query client, validation/policy handler, event mappings, and terminal outcome telemetry.
3. Verify Customer-document event persistence/audit attribution in Loyalty Customer without moving its data ownership.
4. Add deployment/configuration assets and focused automated tests; run solution-scoped builds/tests with TRX analysis and complete security/operational review.

## Decisions

- 2026-09-04: Loyalty Member Management owns Darwinium fraud-lock orchestration; see [decision record](../../decisions/2026-09-04-darwinium-member-management-orchestration.md).

## Validation

- `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/check-sequence-diagram-status.ps1` for this documentation PR.
- Before implementation delivery, use the repository TRX/analyser contract for the facade, Loyalty Member Management, and changed Loyalty Customer test projects.

## Outcome

Documentation updated in the draft follow-on PR. Implementation, environment provisioning, and security/operational approvals remain pending.
