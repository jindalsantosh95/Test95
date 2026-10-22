# Production Delta Hosted Flight Status API in All Environments

- Status: accepted by goal owner for this Draft goal
- Date: 2026-09-25
- Scope: Flight Status additions to the existing Flight Operations Flights microservice, public Operations GraphQL goal, DEV/SIT/UAT/PROD deployment

## Context

Only the production Delta Hosted flight-status API behind `https://www.virginatlantic.com` is available. The normal SDLC preference is for non-production connections and no unapproved production-data copying. The goal owner explicitly permitted this exception because the covered flight-status endpoints are read-only.

## Decision

The existing Flight Operations Flights microservice, with its additive Flight Status endpoints, may call the production Delta Hosted `/flights/routes`, `/flights/details`, `/stats`, `/flights/positions/`, and `/flights/amenitiesAndMeals` GET endpoints from DEV, SIT, UAT, and PROD. Deployed instances in all four environments use that live production endpoint, not a mock downstream. This permission is limited to read-only operational flight-status data; it does not cover passenger, booking, identity, or payment data, write methods, bulk extraction, or copying live responses into fixtures or lower-environment stores.

## Consequences

- All environments depend on production availability and may observe changing real-world statuses; smoke tests use structural assertions, not fixed live values.
- Non-production traffic must be bounded by APIM/service rate and concurrency limits, timeouts, and deliberately low-frequency smoke checks. Monitoring must distinguish each caller environment and alert on sustained errors or excessive request volume.
- Synthetic fixtures and mocks are used only by unit, contract, and isolated automation tests. Deployed DEV, SIT, UAT, and PROD services and their smoke checks call the live production API. Raw downstream payloads, credentials, and request/response bodies must be suppressed or redacted in logs and diagnostics, including connector failure paths.
- Existing environment security remains unchanged: DEV, SIT, and UAT keep their standard non-production access controls, and PROD keeps its standard production controls. This exception only permits the new service to make outbound calls to the five listed HTTPS GET resources on `www.virginatlantic.com`; it does not introduce a new PROD network-egress restriction or change inbound GraphQL access. Any new endpoint, method, or data category needs separate approval.

## Alternatives Considered

- Environment-specific Delta Hosted endpoints: unavailable for this capability.

## Follow-up Controls

- Guide: [Flight Operations Flight Status Draft goal](../sequence-diagrams/vaa-eapi-flight-operations-flight-status.md), [SDLC guidance](../current-sdlc.md), and integration/security principles.
- Sensor: planned fixture-backed service tests, deployment configuration checks, payload-safe failure-telemetry tests, and bounded read-only promotion smoke evidence described in the goal.
