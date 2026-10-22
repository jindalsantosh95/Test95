# Flight Status Uses the Existing Flights Function App

- Status: accepted by business for PR #2392
- Date: 2026-10-11
- Scope: Flight Status HTTP endpoints in Flight Operations and their Operations GraphQL integration

## Context

The PR review raised whether Flight Status must have a separate `FlightOperations.FlightStatus` service and Function App. Business direction for this work is to add Flight Status to the existing `VAA.EAPI.Microservices.FlightOperations.Flights` microservice and verify that implementation against the GDD.

## Decision

Keep the Flight Status route-search and flight-number HTTP triggers as additive endpoints in the existing Flights Function App. Reuse its project, APIM API, deployment resources, and pipeline. Keep the public GraphQL queries and provider in `VAA.EAPI.GraphQL.Domain.Operations`. Do not create a separate Flight Status microservice, Function App, APIM API, deployment project, or pipeline for this work.

## Consequences

- Existing Flights operations and contracts remain alongside the additive Flight Status routes.
- Deployment and APIM changes for these routes extend the existing Flight Operations resources.
- Tests and deployment checks verify the new routes are additive and do not require a second Function App.
- A future decision to split Flight Status into a separately deployed service requires new business approval and an updated decision record.

## References

- [Flight Operations Flight Status delivery flow](../sequence-diagrams/vaa-eapi-flight-operations-flight-status.md)
- Pull request [#2392](https://github.com/VAAD1ST/eapi/pull/2392)
