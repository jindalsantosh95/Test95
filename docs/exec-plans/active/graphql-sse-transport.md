# GraphQL SSE alongside WebSockets

## Objective
Add POST-based GraphQL SSE subscriptions to GraphQL and Stitch without changing existing HTTP query/mutation or WebSocket behaviour. Branch: `codex/graphql-sse-transport`, from fetched `origin/main`.

## Design
- Distinct connections: one subscription per POST, JSON request, `Accept: text/event-stream`, `next` and `complete` events.
- Derive `/sse` as a sibling of `/ws`; no new app settings. Preserve existing native host hook and private bridge authentication.
- Reuse Redis session routing, schema execution, field authorization, bounded results and cleanup. Propagate SSE selection through the authenticated bridge to Stitch's remote resolver.
- Keep WebSocket downstream execution for WebSocket clients. SSE clients use HTTPS SSE downstream derived from `Graphql:Endpoints`.
- APIM HTTP operations require unbuffered forwarding. No live Azure changes in this implementation task.
- No replay guarantee. Host-hook hosting remains unsupported; deployed streaming duration and Front Door/WAF acceptance require DEV validation.

## Validation
Transport and Stitch regression tests completed as recorded below. Scoped repository verification follows standards corrections; deployed acceptance remains outstanding.

Implementation complete: native POST SSE handler, shared authenticated bridge client, Stitch SSE downstream selection, endpoint derivation, unbuffered DEV APIM provisioning script (preview default), and SSE.md setup guide. Existing WebSocket clients keep their transport. Earlier uncommitted DEV APIM/Redis documentation changes were preserved on the new branch; stash backup retained.

Validation: host transport suite 33 passed, Stitch suite 30 passed and one Redis integration test skipped because no test connection was configured. TRX analysers report no failures. Host loaded-assembly coverage: lines 464/6367 (7.29%), branches 316/3311 (9.54%). Stitch: lines 368/2235 (16.47%), branches 242/1425 (16.98%). These are targeted suites, not full solution coverage. Host tests demonstrate incremental ping/pong, anonymous/authenticated forwarding, validation errors and disconnect cleanup; Stitch tests cover SSE schema routing, header forwarding, cancellation and existing WebSocket behaviour. Azure Functions-host and APIM end-to-end SSE validation remain pending deployment; no live settings were changed.

Final repository verification: GraphQL and Stitch solution-scoped verify.ps1 -SkipBuild runs passed after formatting/preferred-helper corrections. The unchanged shared harness-rule exercise ran in the initial GraphQL check and was skipped on focused reruns. Host extension build passed without warnings/errors, the APIM script parsed successfully, and git diff --check passed. Nothing was committed, pushed or deployed.
