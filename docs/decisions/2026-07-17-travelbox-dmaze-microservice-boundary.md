# Coalesce TravelBox DMAZE Capabilities into Retail Holidays

- Status: accepted
- Date: 2026-07-17
- Scope: TravelBox/DMAZE integrations and `VAA.EAPI.Microservices.Retail.Holidays`

## Context

TravelBox is a third-party reservation platform. DMAZE exposes its capabilities through a broad REST API covering product search, cart and reservation workflows, pricing, pre-sale and product setup, back-office operations, administration, and utilities.

Allowing consumers to integrate with DMAZE directly would couple them to vendor authentication, response wrappers, tracking headers, service versions, error codes, and data structures. The repository already contains a Retail Holidays microservice that wraps DMAZE Cart and Document services.

## Decision

All EAPI access to TravelBox through DMAZE must cross a standard EAPI microservice boundary. Coalesce TravelBox capabilities into the existing `VAA.EAPI.Microservices.Retail.Holidays` service, which is the boundary for TravelBox capabilities in the Retail Holidays bounded context.

The microservice will expose EAPI-owned, capability-focused contracts and keep DMAZE transport models, credentials, endpoints, response wrappers, and error codes behind connector-backed gateways. Consumers, including GraphQL and other microservices, must not call DMAZE directly or depend on DMAZE DTOs.

This decision does not make Retail Holidays the owner of every DMAZE service family. Capabilities outside the Retail Holidays bounded context require an architecture decision about the correct owning microservice before implementation.

## Consequences

- Consumers are insulated from DMAZE contract and version changes.
- Authentication, authorization, observability, error translation, and sensitive-data controls have one EAPI enforcement boundary.
- Existing booking, document, and future in-context TravelBox capabilities have one cohesive ownership and deployment boundary.
- Developers can reuse the established DMAZE authentication, transport models, and operational wiring while improving touched paths to current standards.
- The microservice must translate contracts and policy; it must not become a transparent pass-through.
- Product setup, administration, identity, payment, or unrelated back-office capabilities may require a different bounded-context owner and additional security review.
- Changes must include the normal service, tests, deployment, pipeline, APIM, health, and support assets.

## Alternatives Considered

- Direct DMAZE access from each consumer: rejected because it duplicates vendor coupling and security/operational policy.
- A generic endpoint that proxies arbitrary DMAZE paths and payloads: rejected because it leaks the vendor contract and bypasses domain ownership and validation.
- A separate `VAA.EAPI.Microservices.Retail.Holidays.TravelBox` service: rejected after reviewing the existing service because it would split a cohesive Retail Holidays integration boundary and duplicate established DMAZE wiring.
- A generic endpoint in Retail Holidays: rejected because the service is a domain facade, not a transparent vendor proxy.

## Follow-up Controls

- Guide: [Retail Holidays TravelBox DMAZE flow](../sequence-diagrams/vaa-eapi-retail-holidays-travelbox-dmaze.md)
- Patterns: [External system facade adapter](../design-patterns/integration-external-system-facade-adapter.md) and [Connector-based downstream gateway](../design-patterns/integration-connector-based-downstream-gateway.md)
- Sensor: project-scoped unit/contract tests, deployment verification, APIM review, and `scripts/verify.ps1`
