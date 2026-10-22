# Remote subscriptions through the stitched schema

- Status: implementation complete; DEV deployment pending
- Owner: EAPI GraphQL
- Created: 2026-10-14
- Scope: GraphQL Stitch, shared native subscription session source, Mobile deployment

## Objective

Execute subscriptions through the existing Hot Chocolate stitched schema in the Azure Functions worker. Keep domain ownership downstream and avoid a direct reference from Stitch to the GraphQL host or domain projects.

## Context

The native host currently proxies Stitch WebSockets to one endpoint, bypassing its schema. HotChocolate.Stitching 13.9.15 delegates query results but does not provide remote subscription stream execution. The user selected an extension to the existing framework rather than a Fusion migration.

## Acceptance criteria

- Discover subscription fields from remote schemas and install subscribe resolvers using their delegation metadata.
- Validate operations in Stitch and forward them to configured schema-specific WebSocket endpoints.
- Preserve variables, aliases, authentication, cancellation and downstream errors.
- Use the existing native bridge to execute in the Stitch worker with bounded session lifetime.
- Demonstrate mutation and subscription via a stitched executor against a synthetic downstream service.
- Document HTTP/WebSocket backend alignment and run scoped verification.

## Plan

1. Extend remote field registration and implement a bounded downstream WebSocket event stream.
2. Reuse native session management without host/domain project dependencies and add a Stitch bridge trigger.
3. Update deployment configuration and operating guidance.
4. Run integration tests and scoped verification; record remaining deployment prerequisites.

## Decisions

- 2026-10-14: Retain Hot Chocolate 13.9.15 and Azure Functions. Resolve schema ownership from stitching metadata; do not hard-code Concierge fields into Stitch.
- Keep existing HTTP routes unchanged until the deployment owner selects matching HTTP/WebSocket backends. Configuration cannot make in-memory events cross Function Apps.

## Validation

- Stitch build passed. Sixteen integration/bridge tests passed in `TestResults/stitch-subscriptions-acceptance/tests.trx`; the TRX analyser found no failures. Broad instrumented coverage: 14.67% lines, 14.73% branches (includes the rest of the Stitch application).
- Seventy existing Concierge/native transport tests passed in `TestResults/stitch-shared-registry/tests.trx`; the analyser found no failures. Broad instrumented coverage: 11.13% lines, 13.47% branches.
- Tests cover schema validation, HTTP mutation publication, owning-schema routing, generated field mappings, renamed input types, variables, aliases, fragments, structured scalar results, downstream error paths/codes, credentials, cancellation and bridge protection.
- Intermediate test failures exposed generated argument-path mappings and lost error metadata; both were corrected and the complete Stitch suite passed afterward.
- Scoped repository verification is run before handoff. Live DEV acceptance has not been performed for this implementation.

## Outcome

Code and deployment settings are ready for review, without a commit or live environment changes. The existing pipeline test globs select the new Stitch test project. Before DEV acceptance, deploy the new bridge implementation, resolve the Mobile bridge secret reference, align the HTTP and WebSocket backend for each configured remote schema, and route the client-facing APIM APIs through Stitch. The README records limits and the exact local ping/pong steps.

### Configuration cleanup

Removed the fixed-target parameter and app setting from Mobile and standard Stitch templates and environment files. Standard Stitch now uses bridge mode, the per-schema subscription map and the environment bridge-secret reference. Existing enablement flags and downstream destinations are preserved; validate the `general` HTTP schema/backend alignment before deployment. The ignored local Stitch launch profile now uses bridge mode, a local callback secret and the `concierge` schema name already present in local settings. Unrelated local settings are unchanged. Live Azure configuration has not been modified by this cleanup.

### Derived subscription URLs

Stitch now derives remote subscription URLs from `Graphql:Endpoints` by changing HTTPS to WSS (HTTP to WS on loopback only) and appending `/ws`, preserving host and port. Explicit subscription endpoints remain optional per-schema overrides for nonconventional APIM routes. Unsafe derived URLs fail when a subscription is requested, leaving HTTP-only schemas usable. The local launch override was removed. Existing deployment overrides are retained until corresponding APIM routes are configured and HTTP/WS backend alignment is verified. Integration tests use derived routes; focused tests cover custom ports, trailing slashes, unsafe URLs and override precedence.

Validation: 24 Stitch tests passed in `TestResults/stitch-derived-endpoints/tests.trx`; the analyser reported no failures. Broad application coverage was 15.14% lines (378/2496) and 15.74% branches (213/1353).
