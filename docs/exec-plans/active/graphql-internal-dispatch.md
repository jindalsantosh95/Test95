# Internal Functions-host bridge dispatch

- Status: implemented; local acceptance passed; Azure acceptance pending
- Created: 2026-10-20
- Scope: native transport hook, GraphQL/Stitch transport regression tests and setup guidance.

## Objective
Replace HTTP self-callbacks with in-process ASP.NET Core request dispatch through the existing Functions-host middleware. Preserve the isolated worker, function authorization, bridge credential, Redis session ownership and both client transports. No live infrastructure changes.

## Design and acceptance
The startup hook precedes the normal host pipeline. Dispatch a fresh request to the fixed internal bridge route through its downstream RequestDelegate, not to worker objects or an arbitrary URI. The host still invokes the isolated worker using its normal machinery. Give every request independent features/services, bounded body/response, completion callbacks and cancellation; preserve only existing allowed caller headers and transport selection. Never inherit endpoint, authenticated principal or administrative host features from the outer connection.

Validate auth failure, worker error status, per-command scope disposal, cancellation, concurrent command isolation and disconnected transport cleanup. Run WebSocket/SSE ping/pong through actual local Core Tools hosts. Unit tests alone do not establish host compatibility. Keep the earlier short-query fix and its tests intact.

## References
- https://github.com/Azure/azure-functions-host/blob/dev/src/WebJobs.Script.WebHost/WebJobsApplicationBuilderExtension.cs
- https://github.com/Azure/azure-functions-host/blob/dev/src/WebJobs.Script.WebHost/Middleware/FunctionInvocationMiddleware.cs
- https://github.com/Azure/azure-functions-host/blob/dev/src/WebJobs.Script.WebHost/Middleware/ScriptHostRequestServiceProviderMiddleware.cs

## Validation
Local Core Tools accepted the internal requests and invoked NativeGraphQLBridge through the normal Functions host. Anonymous WebSocket ping/pong through Stitch and SSE ping/pong through both primary GraphQL and Stitch passed, with subscription message IDs matching the corresponding HTTP mutations. Both schemas expose conciergeMessageReceived. Evidence: TestResults/internal-local-ws.log and TestResults/internal-local-sse.log. Local test hosts were stopped afterwards; no live Azure changes or committed settings were made.

The focused transport, configuration, isolation, cancellation and short-query regression suite passed 44/44 tests (TestResults/internal-dispatch-verified/transport.trx). Failed-command tests verify request-scope disposal for cancellation, forbidden status, oversized response and invalid JSON. Concurrent commands have distinct scopes and do not inherit the outer principal, endpoint or function-key header; the outer HttpContextAccessor remains intact.

This still uses an unsupported host startup hook. Deploy both apps and repeat APIM WebSocket/SSE acceptance on DEV before claiming Azure compatibility. Internal dispatch removes the external self-callback but does not remove Redis or Stitch-to-GraphQL network dependencies. Local Functions reported an unrelated AzureWebJobsStorage health warning; this did not prevent HTTP or streaming acceptance. Disconnects cancel outstanding read invocations, which the Functions runtime may log as cancelled failures.

## Final verification
- Runtime acceptance used Core Tools 4.15.2 and Functions runtime 4.1053.200.26403, with .NET 10 builds.
- Focused suite: 44 passed, no failures/skips. TRX analyser: no failures; loaded-assembly coverage 1021/7750 lines (13.17%) and 429/3395 branches (12.64%).
- Stitch regression suite: 30 passed, one Redis integration test skipped, no failures. TRX analyser: no failures; coverage 452/2709 lines (16.69%) and 237/1437 branches (16.49%). These focused percentages are not whole-solution coverage.
- scripts/verify.ps1 -SolutionPath VAA.EAPI.GraphQL/VAA.EAPI.GraphQL.sln -Base origin/main -SkipBuild passed.
- scripts/verify.ps1 -SolutionPath VAA.EAPI.GraphQL/VAA.EAPI.GraphQL.Stitch.sln -Base origin/main -SkipBuild -SkipHarnessRuleCheck passed; the full harness rules were exercised in the GraphQL run.
- Explicit project-scoped dotnet format whitespace --verify-no-changes passed for all five new C# files (the harness diff discovery excludes untracked files).
- git diff --check passed. Changes remain uncommitted.
