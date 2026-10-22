# Integration Principles
**Virgin Atlantic EAPI Platform**

---

EAPI integrates through HTTP APIs, GraphQL gateway domains, Azure Service Bus events, NDC transformations, facades, shared connectors, and partner adapters. Integrations should be explicit, observable, versionable, and easy to test.

## API Contracts

HTTP APIs must be contract-first in practice, even when implemented through attributes.

- Every HTTP trigger must include OpenAPI operation metadata, request body documentation, security scheme, and response documentation.
- Request and response DTOs should live in the owning domain or feature area and be named by business capability.
- Use meaningful routes that match the existing service convention; avoid adding action names when a resource route is clearer.
- Response status codes must reflect the outcome: `200` for successful reads, `202` for accepted async work, `400` for invalid input, `401`/`403` for auth failures, `404` for missing resources, `409` for conflicts, `422` for business validation, and `500` for controlled technical failures.
- Keep generated OpenAPI discoverability intact when refactoring triggers.

## Outbound HTTP

Feature code must use the shared HTTP connector.

- Inject `IHttpConnector` into handlers, services, gateways, or repositories that make downstream HTTP calls.
- Register `IHttpConnector` as `services.AddSingleton<IHttpConnector, HttpConnector>()` and ensure the required `IHttpClientFactory` registration exists through `AddDefaultServices()`, `AddHttpClient()`, or the established infrastructure factory.
- Pass request-specific headers as `Dictionary<string, string>`.
- Pass bearer tokens through the `bearerToken` parameter so the connector owns the `Authorization` header.
- Do not create `HttpClient`, store static clients, or inject `IHttpClientFactory` in general feature code unless maintaining an established infrastructure client.

## Connector Behaviour

Let shared connector and middleware concerns stay shared.

- `HttpConnector` owns request construction, common headers, bearer token handling, tracing, response deserialisation, and error translation.
- JSON serialisation and deserialisation for service contracts must use the established `Newtonsoft.Json` configuration used by the owning service or shared platform library.
- Do not duplicate connector logic in individual handlers.
- When a downstream returns XML, use the connector and existing XML serializers where supported.
- Avoid logging full downstream request and response payloads.
- If connector behaviour must change, make the change centrally and add focused tests.

## Synchronous Composition

Synchronous cross-service composition belongs at explicit composition layers.

- Web APIs, GraphQL gateway domains, facades, NDC transformation projects, and orchestration services may compose multiple downstream capabilities.
- New domain microservice code should stay focused on its own capability and should not accumulate unrelated orchestration logic.
- If existing service code already calls downstream systems, keep the call behind an interface and do not spread direct integration logic into triggers or DTOs.
- Design timeouts and failures so callers receive deterministic responses and correlation IDs.
- Use `202 Accepted` and status lookup patterns when work cannot complete safely within the request lifecycle.

## Asynchronous Integration

Use Azure Service Bus for decoupled, retryable, downstream reactions.

- Publish domain facts as CloudEvents rather than command-like messages whenever the consumer should decide how to react.
- Include correlation ID, operation ID, session ID, channel, diagnostics flag, event type, subject, and version where applicable.
- Subscribers must restore `AsyncContext` from the CloudEvent before invoking application handlers.
- Consumers must be idempotent because messages can be delivered more than once.
- Dead-letter, publish failure, and rollback paths must be observable and use existing error mapping conventions.

## NDC And External Standards

External standards belong at the boundary.

- NDC XML and Sabre/provider-specific models should be transformed in NDC transformation or infrastructure adapter projects.
- Internal services should remain as JSON-native and domain-oriented as their existing contracts allow.
- Do not let partner-specific naming, XML structure, or transport constraints leak into unrelated domain models.
- Preserve correlation identifiers across NDC request/response callbacks and status URLs.
- Partner keys, Ocp-Apim keys, and external credentials must be Key Vault-backed and passed only through approved headers.

## GraphQL Integration

GraphQL is a channel/gateway layer, not a service-to-service protocol.

- GraphQL resolvers should delegate to domain services or connector-backed providers.
- Register new query and mutation types in the owning `VAA.EAPI.GraphQL.Domain.*` `Startup` class.
- Keep secure/public visibility decisions explicit.
- Avoid placing business orchestration in resolver methods when an application handler or provider can own it.
- Use GraphQL error filtering and telemetry already configured in the host.
- Keep GraphQL REST router and MCP handlers as transport adapters over the same domain registrations; avoid parallel integration implementations that drift from resolver behavior.
- Preserve operation contracts across GraphQL, REST-router, and MCP surfaces when exposing the same capability.
- Keep generated route/capability metadata current when query or mutation signatures change so support and consumer onboarding remain accurate.

## Error And Response Consistency

Integration failures must be consistent and actionable.

- Use existing `Response<T>`, `ErrorDetails`, error-code JSON, and `ErrorMapper` conventions where the project uses them.
- Return controlled technical errors for downstream outages instead of leaking raw downstream payloads.
- Include correlation IDs in responses or logs so support teams can trace the failure.
- Preserve cancellation tokens through handler and connector calls.
- Do not swallow exceptions that should affect caller status or event rollback.

## Contract Testing

Integrations must remain testable without live dependencies.

- Handler tests should mock `IHttpConnector`, repositories, message bus factories, token providers, and downstream providers.
- Add happy path and failure/edge tests for new integration handlers.
- Use existing automation projects and Bruno/mock payload folders where they are part of the service workflow.
- Keep test payloads synthetic and safe.
- If an integration changes a public or partner contract, update automation and API documentation with the code change.

## AI Agent Conformance Checklist

- Did I use `IHttpConnector` for outbound feature HTTP?
- Did I keep external models at the boundary?
- Did I preserve OpenAPI metadata and response semantics?
- Did I propagate correlation context through HTTP and Service Bus?
- Is retry or duplicate message delivery safe?
- Are integration tests or mocks updated where contracts changed?

