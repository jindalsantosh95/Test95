# EAPI Design Patterns Catalog

These pattern documents are the maintained source of truth for reusable EAPI implementation guidance. Use them with the platform principles in [`../principles`](../principles/readme.md) and the coding standards in [`../standards`](../standards/coding-standards.md).

## API Boundary

- [EAPI Response Envelope Translator](api-boundary-eapi-response-envelope-translator.md)
- [Error Code Mapping Contract](api-boundary-error-code-mapping-contract.md)
- [OpenAPI Function Contract](api-boundary-openapi-function-contract.md)
- [Thin Trigger To Handler](api-boundary-thin-trigger-to-handler.md)

## Cross-Cutting

- [AsyncContext Correlation Spine](cross-cutting-asynccontext-correlation-spine.md)
- [Config-Driven Cache Policy](cross-cutting-config-driven-cache-policy.md)
- [Configuration Overlay And Key Lookup](cross-cutting-configuration-overlay-and-key-lookup.md)
- [Middleware Security Gate](cross-cutting-middleware-security-gate.md)

## GraphQL Composition

- [Async To Sync Conversion](graphql-composition-async-to-sync-conversion.md)
- [BFF Aggregation And Enrichment](graphql-composition-bff-aggregation-and-enrichment.md)
- [GraphQL Response Caching](graphql-composition-graphql-response-caching.md)
- [Secure/Insecure Resolver Registration](graphql-composition-secure-insecure-resolver-registration.md)

## Integration

- [Connector-Based Downstream Gateway](integration-connector-based-downstream-gateway.md)
- [External System Facade Adapter](integration-external-system-facade-adapter.md)
- [Token-Brokered Downstream Call](integration-token-brokered-downstream-call.md)
- [Validation To Mapping Pipeline](integration-validation-to-mapping-pipeline.md)

## Platform Runtime

- [EAPI Function Host Baseline](platform-runtime-eapi-function-host-baseline.md)
- [GraphQL Domain Startup Pack](platform-runtime-graphql-domain-startup-pack.md)
- [GraphQL Host Topology Pack](platform-runtime-graphql-host-topology-pack.md)
- [Operational Endpoint Pack](platform-runtime-operational-endpoint-pack.md)

## Testing And Operations

- [EAPI Function Test Harness](testing-and-operations-eapi-function-test-harness.md)
- [HTTP Mocking With MockDelegatingHandler](testing-and-operations-http-mocking-with-mockdelegatinghandler.md)

## Workflow And Messaging

- [CloudEvent With EAPI Context Envelope](workflow-and-messaging-cloudevent-with-eapi-context-envelope.md)
- [Delayed Dispatcher Recovery](workflow-and-messaging-delayed-dispatcher-recovery.md)
- [Event Store Before Publish With Rollback](workflow-and-messaging-event-store-before-publish-with-rollback.md)
- [Subscriber Context Rehydration](workflow-and-messaging-subscriber-context-rehydration.md)

## Legacy Root Pattern Consolidation

The former root-level `docs/PATTERNS.md` guidance is covered by these maintained documents:

- Azure Functions isolated worker, dependency injection, OpenAPI, and `IHttpConnector` startup: [EAPI Function Host Baseline](platform-runtime-eapi-function-host-baseline.md), [OpenAPI Function Contract](api-boundary-openapi-function-contract.md), and [Connector-Based Downstream Gateway](integration-connector-based-downstream-gateway.md).
- Redis, cache policy, Service Bus, delayed dispatch, and CloudEvent workflows: [Config-Driven Cache Policy](cross-cutting-config-driven-cache-policy.md), [CloudEvent With EAPI Context Envelope](workflow-and-messaging-cloudevent-with-eapi-context-envelope.md), and [Delayed Dispatcher Recovery](workflow-and-messaging-delayed-dispatcher-recovery.md).
- Test harness and no-Moq fixture guidance: [EAPI Function Test Harness](testing-and-operations-eapi-function-test-harness.md) and [Coding Standards](../standards/coding-standards.md).
- Deployment ownership and pipeline alignment: [Infrastructure Principles](../principles/infrastructure-principles.md) and [Coding Standards](../standards/coding-standards.md).
