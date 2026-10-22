# Security Principles
**Virgin Atlantic EAPI Platform**

---

Security controls in EAPI are layered across Azure API Management, Azure Functions, GraphQL middleware, shared infrastructure libraries, Key Vault-backed configuration, Application Insights telemetry, and service-specific validation. AI agents must treat these principles as mandatory for new work and should improve conformance when touching existing code.

## Secure By Default

Every endpoint, handler, event processor, and integration must assume hostile input and least-privilege access.

- Do not infer that an HTTP trigger is public just because it uses `AuthorizationLevel.Anonymous`; many EAPI functions rely on APIM, OAuth/OpenAPI security metadata, middleware, host keys, or subscription keys at the platform boundary.
- New HTTP endpoints must explicitly document their authentication model in OpenAPI attributes and must reject unauthenticated or unauthorised requests at the correct boundary.
- Sensitive operations, including payment, refund, identity, loyalty, customer profile, passenger document, lounge eligibility override, and order mutation flows, require an explicit authorisation decision.
- Use shared authentication, token provider, middleware, and APIM patterns where available instead of hand-rolled checks inside business handlers.
- Never trust caller-provided identity, channel, correlation, role, or customer identifiers without server-side validation.
- New systems, endpoints, components, and configuration should start from the most secure posture and loosen controls only as far as the approved user journey requires.
- Security decisions must fail closed: deny by default unless a request is well formed, authenticated, authorised, and consistent with policy.

## Defence In Depth And Zero Trust

Security must be layered, explicit, and independent of network location.

- Do not trust internal networks, private endpoints, APIM routing, or Function host boundaries as the only control.
- Apply multiple layers of protection: APIM policy, authentication, authorisation, input validation, least-privilege identities, secure configuration, telemetry, and downstream controls.
- Each service must validate the request context it depends on, even when a caller is another internal EAPI component.
- Sensitive flows should have compensating controls across identity, network, application, data, logging, and operations.
- If one layer fails or is misconfigured, another layer should still prevent unauthorised access or unsafe data exposure.

## Shared Mechanisms And Clear Abstractions

Common security-sensitive mechanisms must be reused, simple, and well defined.

- Prefer existing EAPI common and infrastructure mechanisms for authentication, token acquisition, HTTP calls, telemetry, error mapping, serialization, validation, configuration, caching, and messaging.
- For JSON serialization and deserialization, use the established `Newtonsoft.Json` stack and shared settings to avoid divergent parser behavior across services.
- Do not duplicate security mechanisms in individual services unless the existing mechanism is unsuitable and the exception is documented.
- Interfaces between GraphQL, web, facade, orchestration, microservices, and downstream adapters must clearly define the data and operations exposed.
- Keep security-critical interfaces small enough to inspect, test, and reason about.
- External models and partner protocols must remain behind explicit adapters so internal consumers are not coupled to unclear or unstable security boundaries.

## Minimized Sharing And Reduced Complexity

Reduce attack surface by sharing only what is necessary.

- Do not share data stores, caches, credentials, topics, subscriptions, storage containers, or configuration between components unless the sharing is required and explicitly justified.
- Each microservice should own its bounded-context data and expose it through APIs or events rather than shared database access.
- Minimise trusted paths, privileged components, broad configuration access, and cross-domain coupling.
- Avoid unnecessary abstractions, frameworks, dynamic behaviour, or duplicated mechanisms that make security analysis harder.
- When complexity is unavoidable, document the reason, the additional controls, and the operational ownership.

## Secrets Management

Secrets must never be committed to source and must be resolved from secure runtime configuration.

- Store connection strings, API keys, OAuth client secrets, Ocp-Apim subscription keys, Function keys, storage keys, Redis keys, and third-party credentials in Azure Key Vault or approved secret stores.
- Deployment parameter files may reference Key Vault using `@Microsoft.KeyVault(...)`; they must not contain plaintext secret values.
- Do not add secrets to `appsettings*.json`, test payloads, Bruno collections, README files, pipeline variables in source, or generated docs.
- When a service uses `IHttpConnector`, pass the bearer token through the `bearerToken` parameter or request-specific headers; do not concatenate tokens into URLs or logs.
- Treat accidental secret exposure as a security incident: rotate the value and remove it from source history through the approved process.

## Encryption And Key Management

Sensitive data must be encrypted at rest and protected with managed, rotatable keys.

- Use platform encryption at rest for all stores, including Cosmos DB, SQL, Storage, Redis, Service Bus, and logs where supported.
- Use customer-managed keys where required by data classification, regulatory obligations, or platform standards.
- Apply field-level or application-level protection for highly sensitive values where storage-level encryption is not sufficient.
- Store keys, certificates, and secrets in Azure Key Vault or approved managed key services; do not embed key material in code, config, or test assets.
- Key rotation must be supported without code changes and should not require redeploying every consumer.
- Key access must be least privilege, auditable, and environment-specific.

## Authentication And Authorisation

Authentication verifies the caller; authorisation verifies the operation. Both are required for protected capabilities.

- OAuth2/OIDC and Entra/Azure AD/B2C integrations should use the existing token provider and middleware patterns.
- API keys, subscription keys, and Function keys are platform controls, not substitutes for user or system identity where identity is required.
- Staff or back-office capabilities must enforce role or claim checks; security-sensitive GraphQL queries and mutations should be registered only in secure visibility modes unless deliberately public.
- Client credentials for downstream services must be scoped to the minimum resource and operation required.
- Avoid introducing static allowlists, magic tokens, or local-only bypasses in production code paths.
- Apply separation of duties for privileged operational and back-office actions; avoid toxic role combinations that allow one user or component to approve, execute, and conceal a sensitive action.

## Trust And Component Criticality

The protection applied to a component must match how much other components and business processes trust it.

- Identify trusted components and trust chains in architecture documentation, especially for authentication, authorisation, payment, refund, order, loyalty, passenger, and event-publishing flows.
- Components trusted by many consumers require stronger controls, tests, monitoring, change governance, and access restrictions.
- Do not introduce extraneous trusted components; security-critical code should be as small and focused as practical.
- Changes to highly trusted or business-critical components must receive proportionate review and verification.
- If the criticality or data sensitivity of a component changes, reassess the security controls before release.

## PII And Sensitive Data

Passenger, customer, loyalty, identity, payment, contact, passport, booking, and travel document data must be handled as sensitive.

- Do not log request or response bodies that may contain PII, PCI data, tokens, secrets, or travel documents.
- Logs should use correlation IDs, operation IDs, booking references, request IDs, service names, and technical error codes rather than raw personal data.
- Error responses must not expose stack traces, downstream payloads, tokens, connection details, or internal implementation details.
- Test fixtures and sample payloads should use fake data and must not include real customer or passenger information.
- When adding telemetry, record business-safe dimensions only; review telemetry properties before sending to Application Insights or analytics topics.

## Privacy By Design

Privacy must be embedded into EAPI capabilities by default, not added after implementation.

- Collect, process, store, cache, log, and publish only the personal data required for the approved purpose.
- Default user and operational flows to the highest practical privacy setting.
- Protect personal data across its full lifecycle, including ingestion, processing, downstream integration, eventing, storage, retention, export, correction, and deletion.
- Make privacy-relevant behaviour visible and documented for support, audit, and regulatory review.
- Design user-facing and staff-facing flows with user privacy interests in mind, including clear safeguards and controlled access to sensitive records.
- Collaborate with the DPO and data governance teams when introducing new PII fields, new processing purposes, new retention behaviour, or new data sharing.
- Define retention periods for personal data, audit data, event payloads, logs, caches, and exports before persistence is introduced.
- Support GDPR and other applicable data subject rights, including access, correction, deletion, portability, and restriction of processing where legally required.

## Payment And Financial Data

Payment flows must minimise PCI scope and keep financial operations auditable.

- Do not store full card numbers, CVV values, raw payment tokens, or processor secrets in EAPI services.
- Payment authorisation, capture, refund, and status flows must be idempotent where retries are possible.
- Financial records and payment status updates must emit enough correlation context for reconciliation without exposing sensitive card data.
- New payment integrations must be isolated behind application interfaces and shared connector patterns.
- Refund and payment error handling must return controlled error responses using the platform error mapping conventions.

## Input Validation

Validation belongs at boundaries and in application handlers.

- HTTP triggers should validate body presence, content type, required fields, route parameters, and headers before invoking orchestration logic.
- Application handlers must enforce business rules; UI or GraphQL validation alone is not a security control.
- Prefer existing validators, request models, and error mappers over ad hoc string checks.
- Reject unexpected, malformed, or semantically invalid payloads with consistent `400`, `401`, `403`, `409`, or `422` responses as appropriate.
- When deserialising XML or NDC payloads, use established serializers and transformation services; do not introduce unsafe parser settings.

## Transport And API Hardening

Traffic must be encrypted and public APIs must be hardened at the edge and service boundary.

- Enforce TLS for public, partner, and service-to-service HTTP traffic; plaintext HTTP is not acceptable for sensitive flows.
- Public and partner endpoints should be fronted by APIM or an approved gateway that applies policy, routing, rate limiting, authentication support, logging, and threat protection.
- HSTS, CORS allowlists, content security policy, frame protection, content-type protection, referrer policy, and other security headers must be applied where the host returns browser-consumable responses.
- CORS wildcard origins are prohibited for authenticated or sensitive endpoints.
- Rate limiting and throttling must be applied to public, partner, and high-risk endpoints, with clear `429` behaviour where applicable.
- OpenAPI metadata must describe auth, request, and response semantics accurately so consumers do not bypass intended controls.

## Outbound Security

Outbound calls must preserve identity, traceability, and secret hygiene.

- Use `IHttpConnector` for application/business outbound HTTP and keep request-specific headers in a `Dictionary<string, string>`.
- Let `HttpConnector` set or overwrite `Authorization` when a bearer token is supplied.
- Do not create `HttpClient` directly in feature code; direct clients are acceptable only for established infrastructure clients with a deliberate reason.
- Keep downstream endpoint composition in configuration-backed services and constants, not scattered string literals.
- Configure timeout, retry, and error behaviour deliberately so failures do not leak sensitive payloads.

## Secure Failure

Failures must not weaken security policy or expand access.

- If authentication, authorisation, token validation, configuration lookup, key retrieval, schema validation, or policy evaluation fails, the operation must be denied or moved to a controlled failure path.
- Partial downstream failure must not result in duplicate payments, incomplete order state, unaudited refund actions, or unprotected data writes.
- Retry and compensation paths must preserve authorisation, idempotency, correlation, and audit context.
- Error messages must be useful for support but must not disclose secrets, PII, stack traces, internal topology, or raw downstream payloads.
- Fallback modes, mock modes, and diagnostic modes must be impossible to activate accidentally in production.
- Controlled degradation is acceptable only when it preserves security policy, data integrity, and user safety.
- A failed dependency must not create a partial authorisation bypass or silently skip a required security check.

## Proportionate And Usable Security

Security controls must be strong enough for the risk and usable enough to be followed correctly.

- Controls for sensitive datasets and business-critical systems must be stronger than controls for low-risk internal utilities.
- Do not over-engineer controls beyond the trustworthiness and risk level where that harms operability without reducing material risk.
- User and staff-facing security flows should provide clear feedback for security-affecting actions and insecure choices.
- Security controls should support the intended user journey without encouraging workarounds.
- When there is tension between security, usability, cost, and delivery time, document the risk decision and involve Information Security where needed.

## Auditability

State-changing and security-sensitive actions must be traceable.

- Preserve and propagate `AsyncContext`/correlation IDs through HTTP calls, GraphQL execution, Service Bus events, and event store writes.
- Event store entries, Service Bus CloudEvents, and Application Insights traces should be sufficient to reconstruct what happened, when, and under which correlation ID.
- Avoid logging raw event payloads when they may include PII.
- Record failed authentication, authorisation, validation, payment, refund, and publish failures with controlled error codes.
- Do not remove existing telemetry or audit context while refactoring.

## Incident Response And Evidence

Security incidents must be detectable, containable, and investigable.

- Security-relevant alerts must have clear escalation paths, owners, and runbooks.
- Potential breaches, secret exposure, unauthorised access, suspicious privilege changes, and data leakage must trigger incident response procedures.
- Evidence needed for investigation must be retained securely, including correlation IDs, relevant audit events, deployment history, configuration changes, and access logs.
- Incident evidence must avoid unnecessary PII exposure and must be protected from tampering.
- Post-incident actions should update controls, tests, documentation, and monitoring so the same failure is less likely to recur.

## Secure Evolution And Modification

Systems must preserve their security properties as interfaces, functionality, dependencies, and configuration change.

- New designs should anticipate future change by keeping boundaries explicit, mechanisms reusable, and configuration externalised.
- Security-impacting changes must receive the same rigour as initial implementation: requirements, review, tests, rollout plan, monitoring, and rollback path.
- Platform, product, and third-party roadmaps should be considered when choosing security mechanisms and dependencies.
- Keep security requirements traceable from design through implementation, tests, deployment, and operational documentation.
- When a change weakens or removes a control, record the rationale, risk acceptance, owner, and expiry or follow-up action.

## Repeatable Procedures And Documentation

Security-relevant build, deployment, operation, and user procedures must be repeatable and documented.

- Infrastructure, configuration, APIM policy, Function App setup, Key Vault references, Service Bus resources, and monitoring should be reproducible from source-controlled artifacts.
- Manual security steps must be documented until automated; automation should be preferred where practical.
- Documentation created for security-sensitive systems must be reviewed and governed before general release.
- User and administrator documentation must explain secure use, operational responsibilities, and known constraints.
- Lack of documentation is a security risk when users or operators can accidentally weaken controls.

## Environment And Network Isolation

Environments and network paths must limit blast radius.

- Development, test, staging, sandbox, and production environments must be isolated and must not share secrets or privileged identities.
- Production resources must use private endpoints, firewall rules, managed identities, APIM, and least-privilege RBAC where supported by the platform.
- Public access to data stores, Key Vault, storage accounts, Service Bus, and other PaaS resources should be disabled unless an approved exception exists.
- Network rules, APIM policies, role assignments, and private connectivity must be captured in deployment artifacts rather than configured only by hand.
- Cross-environment data movement must be approved, logged, and scrubbed or anonymised where personal data is involved.

## Dependency And Supply Chain Security

Dependencies must be current, supportable, and justified.

- Follow the [repository framework policy](architecture-principles.md#new-code-standards), including coordinated upgrades of shared projects and their consumers, to keep the platform on a supported framework.
- Avoid introducing new frameworks, SDKs, or background infrastructure when existing EAPI libraries already provide the capability.
- Do not introduce MediatR or AutoMapper into new microservice scaffolding; implement request handlers and mappings explicitly unless maintaining an existing MediatR-based project.
- Keep package changes scoped to the solution being changed and run the relevant solution-level build/test command when feasible.
- Security-sensitive dependencies must be compatible with CI scanning and production hosting constraints.
- Wiz is the standard static code analysis tool for security and must be considered part of the release assurance path.
- SAST, dependency scanning, secret scanning, infrastructure scanning, and container/image scanning must run in CI/CD where applicable.
- DAST or equivalent dynamic testing should be used for public, partner, and high-risk API surfaces.
- SBOM generation should be supported for deployed services and reviewed when significant dependencies change.
- Critical and high findings must have defined patch SLAs, ownership, and risk acceptance when they cannot be remediated before release.

## AI Agent Conformance Checklist

- Have I avoided plaintext secrets and real PII?
- Have I documented security requirements, data classification, trust boundaries, and any threat-model assumptions?
- Have I used STRIDE or an approved equivalent for major security-impacting changes?
- Have I preserved authentication, authorisation, and OpenAPI security metadata?
- Have I applied defence in depth and avoided trusting internal networks as the only control?
- Have I covered TLS, APIM, CORS, headers, and rate limiting for exposed APIs?
- Have I used `IHttpConnector` for feature-level outbound HTTP?
- Have I protected data at rest with suitable encryption and key management?
- Have I reused common mechanisms instead of duplicating security-critical code?
- Have I minimised sharing, trusted components, and unnecessary complexity?
- Does the system fail closed and preserve audit context on errors?
- Have I propagated correlation context and avoided unsafe logging?
- Have I considered privacy by design, separation of duties, and proportionate usability?
- Have I defined retention, right-to-erasure, incident response, and evidence expectations?
- Have I considered Wiz findings, SAST/DAST, dependency scanning, SBOM, and patch SLAs?
- Have I preserved environment isolation, private connectivity, and least-privilege resource access?
- Have I kept security procedures, diagrams, and operational documentation repeatable and current?
- Have I used existing validators, error mappers, middleware, and token providers?
- Have I documented any unavoidable exception in the PR notes?
