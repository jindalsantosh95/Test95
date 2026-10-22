# Service Bus W3C Context Propagation

- Status: completed
- Owner: Codex
- Created: 2026-09-03
- Scope: `VAA.EAPI.Common`, `VAA.EAPI.Infrastructure`, direct Service Bus publishers, and shared tests

## Objective

Preserve validated W3C trace context and safe baggage across EAPI Service Bus messages without adding a Datadog package or requiring individual subscriber changes.

## Context

CloudEvent envelopes transport EAPI correlation fields but not W3C `traceparent`, `tracestate`, or `baggage`. Some Service Bus messages are not CloudEvents, and Customer's Service Bus handlers do not manually rehydrate the CloudEvent context. Propagation must therefore occur at the broker boundary rather than in event payloads.

## Acceptance Criteria

- Shared Service Bus publishers attach validated W3C context as broker metadata without changing CloudEvent payload contracts.
- Shared Functions middleware rehydrates incoming Service Bus context before handlers run, including raw-message handlers.
- Existing CloudEvent mapping does not replace restored W3C context.
- Direct Service Bus publishers use the shared helper.
- Tests cover context injection, round-trip restoration, filtering, malformed input handling, and no Datadog package is added.

## Plan

1. Review the current CloudEvent, AsyncContext, Service Bus abstractions, direct publishers, and shared tests. Completed.
2. Add central context injection and rehydration, then route direct publishers through it. Completed.
3. Add focused tests; run the smallest solution-scoped test command and record results. Completed.

## Decisions

- 2026-09-03: Transport `traceparent`, `tracestate`, and `baggage` as Service Bus application properties. Duplicate `traceparent` in `Diagnostic-Id` so Azure Service Bus tracing can consume it. Keep CloudEvent bodies unchanged.
- 2026-09-03: Use the existing `DatadogBaggageAllowList` for custom baggage propagation, alongside the standard EAPI correlation keys. This prevents an unapproved value from reaching telemetry via the broker.
- 2026-09-03: Do not add a Datadog tracing package. This preserves correlation for the existing package-free Datadog log sink; automatic Datadog APM and Data Streams Monitoring remain out of scope.

## Validation

- `dotnet build VAA.EAPI.Microservices/VAA.EAPI.Microservices.Tests/VAA.EAPI.Microservices.Tests.csproj -c Release --no-restore --disable-build-servers -m:1 -v:minimal` passed.
- Focused `AsyncContextTests` execution passed: 24 passed, 0 failed. The repository is configured for Microsoft Testing Platform, which ignores the standard `dotnet test` filter/TRX/coverage options; VSTest was used to execute the focused test DLL and emit the TRX.
- `scripts/analyse-test-run-results.ps1` reported no failed tests. Coverage was unavailable because the VSTest environment could not locate the `XPlat Code Coverage` collector.
- CRM Content and Retail NDC Sabre Order builds passed. Retail Order could not be built because the local NuGet cache has a corrupt `Microsoft.CodeAnalysis.CSharp.Workspaces` package and its worker-extension restore cannot reach NuGet.

## Outcome

All shared Service Bus paths now attach and restore W3C context. The three audited direct publishers use the same helper; Customer benefits from the shared Functions middleware without any Customer-project code change.
