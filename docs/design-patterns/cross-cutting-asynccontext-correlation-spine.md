# AsyncContext Correlation Spine

**Pattern group:** Cross-Cutting

**Status:** Recommended

**Last reviewed:** 2026-06-01

**Owner:** EAPI Technical Leads

## Intent

Use `AsyncContext` as the per-execution spine for diagnostics, channel, session, operation, and correlation data across HTTP requests, Service Bus messages, telemetry, logs, downstream calls, and CloudEvents.

This pattern makes distributed EAPI journeys traceable even when work crosses service, messaging, and GraphQL boundaries.

## Use When

- Handling an HTTP request or Function invocation.
- Publishing or consuming CloudEvents.
- Logging, tracing, or calling downstream systems.
- Writing tests that depend on correlation context.

## Do Not Use When

- Passing explicit business data between components.
- Storing long-lived state outside the current operation.
- Replacing proper method arguments with ambient context.

## Pattern Shape

1. Middleware creates `AsyncContext` from `FunctionContext`.
2. Context reads or generates diagnostics, channel, session, operation, and correlation values and snapshots the active W3C `Activity`.
3. Shared helpers read the context for logs, telemetry, headers, and events; `Activity` remains the canonical trace/span source.
4. Event producers and subscribers map context into and out of CloudEvents.

## Implementation Guidance

- Use `AsyncContext` for cross-cutting operational metadata only.
- Do not store business payloads in `AsyncContext`.
- Preserve inbound correlation IDs where provided.
- Initialise the context asynchronously in Function middleware and restore the previous ambient value in `finally`.
- Preserve `traceparent`, `tracestate`, and validated `baggage`; create child spans for downstream calls rather than forwarding an inbound span unchanged.
- Treat baggage propagation and baggage logging as separate policies. Only explicitly allowlisted baggage keys may be written to telemetry providers.
- Datadog direct logging is opt-in and runs alongside Application Insights through the shared `ILogger` pipeline. Delivery must remain buffered, bounded, batched, compressed, and fail-open.
- Initialise test context in unit tests that depend on correlation behavior.

## Datadog Logging Configuration

`AddDefaultServices()` registers the provider, but it remains disabled unless all required settings are present. Secrets must be supplied through Key Vault-backed application settings.

| Setting | Required | Purpose |
| --- | --- | --- |
| `LoggingProvider` | No | Selects registered logging providers: `AppInsights` (default), `Both`, or `Datadog`. Datadog delivery is enabled only for explicit `Datadog` or `Both` values; missing or invalid values fall back to `AppInsights`. |
| `DatadogLogsEndpoint` | Yes | HTTPS regional intake URL ending in `/api/v2/logs`. |
| `DatadogApiKey` | Yes | Datadog API key; use a Key Vault reference. |
| `DatadogService` | Yes | Datadog service name. |
| `environment` | No | Value emitted as `dd.env`. |
| `DatadogSource` | No | Defaults to `csharp`. |
| `DatadogVersion` | No | Value emitted as `dd.version`. |
| `DatadogTags` | No | Comma-separated Datadog tags. |
| `DatadogBaggageAllowList` | No | Comma-separated baggage keys permitted in logs. Empty means no baggage is logged. |
| `DatadogMinimumLogLevel` | No | Defaults to `Information`. |
| `DatadogQueueCapacity` | No | Bounded queue capacity; defaults to `5000`. |
| `DatadogBatchSize` | No | Entries per request, from 1 to 1000; defaults to `100`. |
| `DatadogFlushIntervalMilliseconds` | No | Batch flush interval; defaults to `2000`. |

## Code References

- [AsyncContext.cs](../../VAA.EAPI.Microservices/VAA.EAPI.Common/AsyncContext.cs:13): ambient context and CloudEvent mapping.
- [FunctionContextAccessorMiddleware.cs](../../VAA.EAPI.Microservices/VAA.EAPI.Infrastructure/Middleware/FunctionContextAccessorMiddleware.cs:10): middleware seeding `AsyncContext`.
- [DefaultTelemetryInitializer.cs](../../VAA.EAPI.Microservices/VAA.EAPI.Common/Diagnostics/Telemetry/DefaultTelemetryInitializer.cs:29): correlation ID added to telemetry.

## Related Principles And Standards

- [Infrastructure Principles](../principles/infrastructure-principles.md)
- [Integration Principles](../principles/integration-principles.md)
- [Coding Standards](../standards/coding-standards.md)

## AI Agent Checklist

- Context is initialised before use.
- Correlation metadata is preserved in logs and events.
- Business data is passed explicitly, not hidden in ambient context.
- Tests set `AsyncContext.Current` where needed.

## Change History

| Date | Change | Author |
| --- | --- | --- |
| 2026-06-01 | Initial draft. | Seb Harvey |
| 2026-06-01 | Approved. | EAPI Technical Leads |
| 2026-08-03 | Added W3C lifecycle and direct Datadog logging guidance. | Codex |
