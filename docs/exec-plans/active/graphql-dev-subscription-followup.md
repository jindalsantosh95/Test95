# DEV subscription acceptance follow-up

## Scope
Resolve deployed DEV subscription blockers following SSE/WebSocket deployment. Work branch: `codex/dev-subscription-followup`, based on current `origin/main`.

## Live changes and evidence (20 October 2026)
- Applied `Configure-DevSseRoute.ps1 -Apply`. Both public HTTP APIs now have POST `/sse` operations, correct Function App backends and unbuffered forwarding; policy readback passed.
- Confirmed the native hook DLL is present in both deployed apps. Redis and bridge Key Vault references resolve.
- Restarted DEV Stitch after a self-callback probe from its SCM environment reported the deployment-slot (`-ds`) hostname. Afterwards the public Stitch WebSocket route returned `connection_ack`, and SSE returned HTTP 200 with properly framed validation errors and completion. This demonstrates recovery after restart, not proof of the exact worker environment before restart.
- Restarted DEV primary GraphQL to rule out stale deployment state; its SSE route still returns 403 and its WebSocket initialization fails.
- A private callback probe from primary GraphQL's SCM environment returned HTTP 403 with `x-ms-forbidden-ip`. Its App Service access restrictions allow selected trusted sources and otherwise deny. The signed-in identity cannot read the app's integration subnet; the user will contact the networking team. No access restrictions were loosened, and the internal bridge was not exposed through APIM.
- Both public HTTP introspection requests expose `conciergeMessageReceived`. End-to-end WebSocket ping/pong through Stitch still fails while opening the downstream GraphQL subscription. A real SSE subscription through Stitch returns framed errors identifying downstream HTTP 403. No successful DEV subscription event is claimed.

## HTTP query defect
The gateway's logging fallback used `st.AsSpan(50)`, which throws on queries shorter than 50 characters and logs the suffix for longer queries. Use the full short query or the first 50 characters plus an ellipsis. A regression through the HTTP function/gateway reproduced the short-query failure before the fix; both short and long queries pass afterwards.

Validation: 2/2 focused regression tests passed, no skips; TRX analysis reports no failures. Loaded-assembly coverage is 348/6450 lines (5.40%) and 166/3347 branches (4.96%); this is not full solution coverage. The fix is local and needs normal review/deployment. Repository verification is recorded in the delivery response.

## Remaining acceptance
The user selected internal host dispatch to remove the external self-callback dependency; see [internal dispatch implementation and acceptance](graphql-internal-dispatch.md). Local WebSocket/SSE ping/pong now passes. Review and deploy the hook update to both apps, together with the HTTP query logging fix, then rerun anonymous ping/pong through DEV APIM. The original networking remedy is an alternative for the currently deployed version, not a requirement for the new internal dispatch design. Existing Redis and Stitch-to-GraphQL connectivity remains necessary.

Scoped verify.ps1 -SolutionPath VAA.EAPI.GraphQL/VAA.EAPI.GraphQL.sln -SkipBuild -SkipHarnessRuleCheck completed: GraphQL formatting, preferred extensions and test standards passed. Overall verification failed solely on existing UI automation architecture violations in VoucherRedemptionJourneyTests.cs (PlaywrightBrowserSession and session.Page), unchanged from origin/main. git diff --check passed. No commit or code deployment was performed.
