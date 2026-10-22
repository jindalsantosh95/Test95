# Data Principles
**Virgin Atlantic EAPI Platform**

---

EAPI data is distributed across domain services, Cosmos DB document repositories, Azure Table event stores, Redis caches, blob storage, SQL-backed dependencies, GraphQL models, and event payloads. AI agents must preserve data ownership, traceability, and privacy when changing models, repositories, handlers, or deployment configuration.

## Data Ownership

The service that owns a capability owns its persisted data and its data contracts.

- Retail order data belongs in retail order services; loyalty data belongs in loyalty services; CRM data belongs in CRM services; flight and ground operations data belong in their respective domains.
- Do not read or write another domain's store directly from feature code.
- Cross-domain access should occur through service APIs, GraphQL gateway composition, facades, or events.
- Shared infrastructure repositories are allowed, but ownership of the documents, collections, tables, and event types remains with the domain service.
- Any denormalised data must identify its source of truth and refresh or correction strategy.

## Storage Technology Fit

Use the storage pattern already established for the service.

- Cosmos DB document repositories are used for order, loyalty transaction, and other document-oriented service data.
- Azure Table Storage is used by event store patterns where durable event payload tracking is required.
- Redis or memory cache is used for short-lived cache and dispatcher coordination.
- Azure Blob Storage is used for file and object payloads through shared blob services.
- SQL checks and SQL-backed dependencies should use shared health check and connector patterns where present.
- Do not introduce a new database or SDK into a service without a clear platform reason.

## Document Design

Documents must be designed for the service's access patterns and lifecycle.

- Use stable identifiers and partition keys that match the dominant read/write access pattern.
- Keep documents cohesive; avoid storing unrelated domain aggregates in the same document merely for convenience.
- Include enough metadata for traceability: correlation ID, source, event type, timestamp, channel, or operation where applicable.
- Version document or event shapes before making breaking structural changes.
- Avoid full-document scans for high-volume queries; add query-oriented fields or indexes where the repository supports them.

## Events As Data

Events are durable facts and must remain useful after the publishing service has moved on.

- CloudEvents should include ID, type, subject, source, correlation context, timestamp, and schema/version information where applicable.
- Event payloads must be self-contained enough for the consumer to process without a synchronous callback when possible.
- Event store writes should not persist connection strings, table names, or other operational secrets.
- Event handlers must be idempotent because Service Bus delivery is at-least-once.
- Rollback or compensation events must carry the same correlation context as the original action.

## Data Validation

Validate data before persistence and before publication.

- Request validation should happen before a handler writes to Cosmos, Table Storage, Blob Storage, Redis, SQL, or Service Bus.
- Business validation belongs in application handlers or validators, not only in triggers or UI.
- Use existing error-code JSON payloads and error mappers for validation and technical failures.
- Reject malformed JSON/XML rather than silently storing partial objects.
- Do not persist downstream error payloads verbatim if they contain PII or implementation details.

## Data Privacy

PII and sensitive business data must be minimised and protected.

- Treat passenger details, loyalty details, identity details, booking references, passport data, contact details, payment details, and refund data as sensitive.
- Store only the fields the service genuinely needs for its business capability.
- Do not duplicate PII into analytics events, logs, cache keys, or error responses.
- Cache entries must have a short, explicit lifetime and must not become an untracked store of sensitive data.
- Test data must be synthetic and safe to share.

## Data Lifecycle

Every persisted data set needs a lifecycle.

- Define retention expectations for documents, event store rows, blob objects, cache entries, and audit records.
- Background cleanup or change-feed processors should be observable and safe to rerun.
- Data deletion, anonymisation, and correction flows must preserve regulatory and audit obligations.
- Transient booking, basket, offer, token, and dispatcher state must not become long-lived by accident.
- Deployment changes that create new stores or collections must include environment configuration and operational monitoring.

## Consistency And Concurrency

Choose consistency deliberately and document the trade-off.

- State-changing command handlers should be idempotent when invoked by retryable HTTP calls, Service Bus messages, timers, or change feeds.
- Use optimistic concurrency, unique constraints, idempotency keys, or event IDs where duplicates would cause incorrect business state.
- Keep a single authoritative writer for each aggregate or document.
- If a flow writes to a store and publishes an event, preserve the existing event store or rollback pattern rather than introducing an untracked dual-write.
- Do not hide eventual consistency from callers; return `202 Accepted` or status URLs when work continues asynchronously.

## Query Boundaries

Queries must respect domain and performance boundaries.

- Do not build cross-domain queries by reaching into multiple services' stores.
- GraphQL can compose across domains, but resolvers should call service APIs or domain services, not storage owned by another bounded context.
- Cosmos SQL queries must be parameterised through repository abstractions and scoped to the owning container.
- Avoid querying by unindexed, high-cardinality, or PII-heavy fields unless the service explicitly supports the pattern.
- Analytical and monitoring queries should not compete with transactional paths.

## AI Agent Conformance Checklist

- Does the owning service own this data and contract?
- Have I used the service's existing repository/storage pattern?
- Have I avoided storing secrets, tokens, or unnecessary PII?
- Is the document/event shape versionable and traceable?
- Is retry or duplicate delivery safe?
- Have I considered retention, cleanup, and monitoring?

