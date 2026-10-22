# W3C-Aware Datadog Logging

- Status: completed
- Owner: Codex
- Created: 2026-08-03
- Scope: `VAA.EAPI.Common`, `VAA.EAPI.Infrastructure`, microservices tests, observability guidance

## Objective

Make `AsyncContext` a standards-aligned per-invocation W3C context and add an opt-in, SDK-free Datadog `ILoggerProvider` that operates alongside Application Insights without adding request latency.

## Context

The prior context preserved `traceparent` and `baggage`, but did not preserve `tracestate`, reused an inbound span for outbound HTTP, accepted arbitrary baggage into telemetry, blocked while initialising HTTP context, and was not explicitly cleared after an invocation. Datadog logging must preserve structured EAPI correlation values while keeping delivery failure out of business request paths.

## Acceptance Criteria

- `AsyncContext` snapshots the active W3C activity, including trace ID, span ID, flags, trace state, and validated baggage.
- Function middleware initialises context asynchronously and restores it after execution.
- Outbound HTTP propagation does not overwrite the runtime-created child trace context.
- Datadog logging is opt-in, buffered, batched, compressed, bounded, and coexists with Application Insights.
- Only configured baggage keys are included in Datadog logs.
- Focused tests cover W3C parsing, baggage filtering, context restoration, and Datadog payload enrichment.
- The owning project or solution passes change-related repository verification checks.

## Plan

1. Correct `AsyncContext` initialisation, validation, lifecycle, and telemetry correlation.
2. Correct outbound W3C propagation and retain EAPI correlation headers.
3. Add the Datadog provider, queue, sender, configuration, and default-service registration.
4. Add tests and update durable observability guidance.
5. Run project-scoped tests and harness verification.

## Decisions

- 2026-08-03: Use `System.Diagnostics.Activity` as the canonical current trace/span source; retain EAPI GUIDs as separate business/operational correlation dimensions.
- 2026-08-03: Send logs directly to the Datadog HTTP intake API without the Datadog SDK, using a bounded in-process queue and fail-open delivery.
- 2026-08-03: Keep Datadog disabled unless endpoint, API key, and service name are configured.
- 2026-08-03: Separate baggage propagation from log inclusion; propagated baggage is validated, while logged baggage is additionally allowlisted.

## Validation

- `dotnet build VAA.EAPI.Microservices/VAA.EAPI.Microservices.Tests/VAA.EAPI.Microservices.Tests.csproj -c Release --no-restore -v:minimal` succeeded; existing repository warnings remain.
- Native xUnit v3 runner: 10 passed, 0 failed for `AsyncContextTests`, including child-span propagation, baggage privacy, W3C grammar, shutdown draining, and payload truncation regressions.
- `scripts/analyse-test-run-results.ps1` reported no failed tests in `TestResults/datadog-w3c/datadog-w3c-native.trx`.
- The standard `dotnet test` coverage command timed out before producing a TRX; the already-built native xUnit runner was used to obtain deterministic test and TRX results.
- Reviewed the context-routing guide, PR template, high-risk workflow, and remediation evidence, then refreshed `LastReviewedUtc`.
- `scripts/verify.ps1 -ProjectPath ... -SkipBuild` passed for Common, Infrastructure, and Tests, including knowledge, harness-rule, agent-memory, context-routing, preferred-extension, and format checks.

## Outcome

Microservices now use an invocation-scoped W3C context with correct cleanup and downstream child-span behavior. Application Insights uses the W3C trace ID when it needs an operation ID and no longer receives raw caller baggage. The shared logging pipeline includes an opt-in Datadog provider that snapshots structured log state, scopes, EAPI context, W3C fields, and allowlisted baggage, then sends bounded gzip batches directly to the regional intake endpoint without a Datadog SDK. Shutdown drains the queue within the host deadline, and oversized single entries are replaced with a bounded warning record.
