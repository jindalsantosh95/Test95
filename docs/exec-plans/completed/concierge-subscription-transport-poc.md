# Concierge subscription transport POC

- Status: historical experiment; withdrawn on 2026-10-02
- Owner: EAPI GraphQL
- Created: 2026-09-30
- Scope: primary GraphQL host, Stitch network forwarding, Concierge integration tests

## Hosting correction (2026-10-02)

The user requires both projects to run through the existing Azure Functions SDK host. The alternate local ASP.NET Core hosts, Stitch WebSocket proxy, and their transport tests have therefore been removed. Keep the sections below as historical experiment evidence only; they do not describe the current runnable setup or prove Azure Functions subscription support. Domain schema/event tests and host-owned registration remain. End-to-end subscription delivery through Functions and APIM is unresolved.

Correction validation: 11 Concierge engine/schema tests and 33 primary regression tests passed; TRX analyses found no failures. Stitch Release build passed. Both primary and Stitch workers started and initialized under Azure Functions Core Tools 4.15.2, exposing their normal HTTP Functions routes on temporary ports 7088/7089. The startup checks did not exercise authenticated HTTP operations or subscription delivery. Both temporary SDK hosts were stopped afterwards.

## Objective and acceptance

Extend the [domain POC](concierge-subscription-poc.md) with real subscription delivery. Retain domain startup registration, keep Stitch independent of primary/domain assemblies, and demonstrate HTTP publication plus repeated WebSocket events through a separately running gateway.

## Implementation decisions

- Extract domain discovery and schema registration into primary-host helpers shared by Functions and local streaming startup. Concierge remains a class library with IStartup callbacks.
- Add explicit `--subscriptions-poc` entry points to the existing executables. These start ASP.NET Core listeners on loopback only. Default Functions entry points and deployment configuration remain unchanged.
- The primary POC loads Concierge only, without the production authentication pipeline. It is a synthetic-data experiment, not production subscription support.
- Stitch continues to use configured HTTP remote schemas for queries/mutations. A named `/api/v1/graphql/subscriptions/{schema}` route relays WebSocket messages to that configured GraphQL instance, without referencing its assemblies or loading domain code.
- This is schema-specific forwarding, not automatic multi-schema subscription composition. No Hot Chocolate version upgrades: the HC12 upstream requires legacy `graphql-ws` frames. The HC13 gateway transparently forwards the selected protocol.
- The test project has a build-only dependency on Stitch (no runtime assembly assets) and launches its executable separately. Stitch itself has no ProjectReference to GraphQL or Concierge. Existing linked persisted-query content files predate this change.
- Bound upstream connection establishment and shutdown waits; propagate close frames and cancellation; reject unknown schemas, ordinary HTTP on the subscription route, unsupported protocols, and oversized/non-text frames.

## Validation

- Both executable projects build in Release.
- `streaming-03/concierge.trx`: 14 passed, including direct and forwarded network subscriptions, repeated ping/pong, conversation isolation, operation completion/connection reuse, and invalid routes.
- Initial transport tests exposed the upstream legacy protocol and keep-alive messages; tests and Postman instructions now use the observed HC12 protocol.
- Coverage collection does not instrument the separately launched Stitch child process.
- Primary regression suite: 33 passed; TRX analyser found no failures in either suite.
- Aggregate coverage (including shared libraries): Concierge 3% lines (172/5731), 2.71% branches (77/2841); primary 12.6% lines (716/5682), 9.74% branches (275/2823).
- Both required scoped verify.ps1 runs passed with -Base HEAD -SkipBuild: the primary GraphQL solution and the Stitch project. Logs: TestResults/concierge-poc/verify-streaming-primary.log and verify-streaming-stitch.log.
- Explicit scoped format verification covers the new untracked C# files, which the Git-based harness discovery omits. git diff --check passed.

## Usage

See the [Concierge README](../../../VAA.EAPI.GraphQL/VAA.EAPI.GraphQL.Domain.Concierge/README.md) for the current Functions-host setup and operation guide. The alternate-host commands and transport results above are historical and do not validate the retained native Functions transport.
