# Native Functions WebSocket review preview

- Status: active; implementation prepared for review, Azure acceptance outstanding
- Owner: EAPI GraphQL
- Created: 2026-10-05
- Scope: primary Functions host, independent shared transport, Stitch relay, Concierge tests and deployment switches

## Objective

Test genuine GraphQL subscriptions while preserving the existing Azure Functions SDK hosting model on Windows App Service. Keep Stitch independent from the primary GraphQL/domain assemblies. Leave changes uncommitted for review.

## Decision and mechanism

Use `DOTNET_STARTUP_HOOKS` to register an ASP.NET Core hosting-startup assembly and startup filter in the Functions host. The filter handles an explicit WebSocket path. It forwards authenticated commands through same-instance HTTP Function invocations to the isolated worker's existing HotChocolate executor. This is an experimental, unsupported Functions host integration and must be revalidated against host updates.

The extension assembly has no GraphQL, domain or Functions worker project references. Both apps package it under `hostextensions`. Stitch uses its proxy mode with a fixed WSS backend. Worker registration stays in `HostBuilderInitialiser` through a service-registration extension; domain schema registration remains unchanged. Transport HTTP uses a named framework HttpClient with redirects, cookies and proxies disabled because it lives outside the worker/container that owns the platform connector.

The earlier Web PubSub alternative has been removed. Concierge now publishes only through the in-memory GraphQL subscription topic sender. Its negotiation mutation, SDK dependency, deployment parameters and alternative-specific tests are removed.

## Deployment evidence and constraints

Deployment templates and read-only DEV configuration show Windows App Service plan hosting. Both live apps have platform WebSockets disabled. Primary has a stopped DS slot; Stitch has no slot. No Azure resources or configuration have been changed.

One instance and one worker process are required. The acknowledgement setting does not control autoscale. Session state is in memory, expires after five minutes, and has no replay or cross-instance backplane. Callback loopback on the actual Windows Azure Functions host is not yet proven. The bridge must never traverse a load balancer to another worker.

The platform WebSockets ARM switch defaults false in all seven relevant templates. Experimental app settings are deliberately supplied through environment configuration; preserve existing startup hooks and reapply/integrate these settings when deploying templates that replace app settings. The [transport README](../../../VAA.EAPI.GraphQL/VAA.EAPI.GraphQL.WebSocketHost/README.md) contains the exact settings, APIM requirements, Postman operations and rollback.

## Local acceptance evidence before removal of the alternative

- 32 Concierge tests passed, including real HotChocolate subscription execution, selected fields/variables, caller ownership rejection, cancellation and a real WebSocket bridge/relay integration test. The integration fixture uses a test authentication endpoint; it does not claim live Azure authentication.
- TRX: `TestResults/native-transport-final/concierge.trx`; analyser found no failures. Referenced-assembly coverage is 7.14% lines (442/6192) and 8.96% branches (289/3225); this is not whole-solution coverage.
- Both Function projects build and publish with the shared hook DLL in `hostextensions`. Existing compiler warnings remain.
- Production hook smoke test used the normal Core Tools Functions host for primary on port 7188 and Stitch on port 7189. Both isolated workers initialized; both upgraded using `graphql-transport-ws`. Missing and invalid bearer tokens closed with 4403 through both routes. Invalid-token requests invoked the actual primary callback Function. Hosts were stopped after the test.
- Host version: Core Tools 4.15.2, Functions runtime 4.1053.200.26403, bundled host .NET 10.0.12; application/hook target .NET 8. Evidence: `TestResults/native-host-smoke.json` and host logs.
- Scoped `verify.ps1 -SkipBuild -Base HEAD` initially failed on existing stale context-routing metadata. Primary solution and Stitch project verification then passed with only `-SkipContextRoutingCheck`. A Stitch solution run also hit the existing deployment scanner assumption that every sibling app's deployment project belongs in that solution; primary solution verification covers both deployment projects. Logs: `TestResults/native-verify-primary-scoped.log` and `TestResults/native-verify-stitch-scoped.log`.
- Untracked new C# files were formatted explicitly with project-scoped `dotnet format whitespace`, because the harness's Git-diff new-file check does not see untracked files. All seven templates parse with the native switch disabled by default. `git diff --check` passes.

## Remaining acceptance

Validate deployment packaging/path resolution, startup-hook loading, same-instance callback connectivity, real bearer identity, ping/pong event IDs through APIM, expiry/reconnect, caller isolation and rollback on an approved Windows staging environment. A successful socket upgrade alone is insufficient. No claim of production readiness or official Azure Functions WebSocket support is made.

## Native-only cleanup

Removed the unused Web PubSub service/interface/model, negotiation mutation, domain settings, package dependency, deployment parameters and eleven alternative-specific tests. Domain publication now always uses the in-memory GraphQL topic sender. Updated domain and transport guidance to describe only the retained route.

The current run `TestResults/native-cleanup/concierge.trx` passed all 21 remaining tests; the analyser found no failures. Aggregate referenced-assembly coverage: 6.13% lines (377/6147), 7.42% branches (236/3179). Restored domain/test dependency graphs contain no Web PubSub package. Deployment JSON parses and `git diff --check` passes. No Azure resources were modified and no changes were committed.

## Anonymous operations and existing GraphQL authorization

The socket transport no longer mandates bearer authentication or Secure visibility mode. It forwards Authorization when supplied and lets the existing Functions authentication middleware and GraphQL authorization handler decide access. The internal bridge credential remains mandatory. A server-generated connection ID binds every session to its socket, in addition to its validated principal or anonymous state. GraphQL field visibility registration remains unchanged; Concierge is still registered on the Secure schema surface, which is distinct from caller authentication.

Anonymous Concierge topics are public within their conversation ID. Connection binding protects session reads and stops; it does not make public events private. Queries and mutations remain on the existing HTTP endpoint; this change does not add query execution over WebSocket.

Validation: 23 tests passed, including anonymous and authenticated relay delivery, anonymous connection/session isolation, and rejection of a protected field using the production GraphQL authorization handler. TRX `TestResults/native-anonymous-final/concierge.trx`; analyser found no failures. Referenced-assembly coverage: 6.34% lines (390/6149), 7.36% branches (233/3167).

A real Core Tools Functions-host test initialized without Authorization, subscribed, published an anonymous HTTP mutation and received pong with the matching message ID. Evidence: `TestResults/native-anonymous-host-evidence.json`. The host was stopped afterwards. Solution-scoped verification passed with `-SkipBuild -Base HEAD -SkipContextRoutingCheck`, retaining the known stale context-routing exception. Azure acceptance remains outstanding.

## Deployment configuration wiring

The enable-by-default configuration recorded below was superseded by the PR review corrections at the end of this plan.

All seven primary/Stitch ARM templates now emit native transport settings and compose DOTNET_STARTUP_HOOKS from an existing hook list plus the deployed DLL path. The NativeWebSocketsEnabled template default is true, as requested. All thirteen environment parameter files explicitly enable transport, acknowledge the single-instance prerequisite, use D:\home\site\wwwroot\hostextensions\VAA.EAPI.GraphQL.WebSocketHost.dll and retain configurable route/origin values. Enabled mode forces one worker; disabling restores the supplied FunctionsWorkerProcessCount and existing hooks.

Primary configuration references GraphQL-Native-WebSocket-Bridge-Secret in the existing environment vault. Provision that secret and grant the existing identity access before applying the deployment. Stitch targets the primary FunctionAppName in its matching environment, over WSS; deployments using APIM or a slot-specific backend must override that target. Plans are not resized and no new Azure resources or secrets were created by this repository change. Existing hook lists are explicit desired-state parameters, not values fetched from live apps.

Validation: all twenty deployment JSON files parse, defaults and secure-string secret typing are checked, all non-native settings/resources are structurally identical to HEAD, and git diff --check passes. Live ARM validation/deployment and Azure WebSocket acceptance remain outstanding. See the transport README for environment parameters and deployment prerequisites.

## DEV startup-hook path discovery

The GraphQL and Stitch deployment templates now conditionally prepare and activate native transport for DEV only. Preparation preserves existing unrelated startup hooks and disables the native hook during ARM/package deployment. Activation discovers the remote HOME path through authenticated Kudu, verifies the deployed DLL against the package hash, then enables and restarts the existing app. Failed activation restores the disabled post-ARM baseline and fails deployment. No cloud deployment or resource creation was performed for this change.

Offline regression coverage exercises hook preservation, stale-package rejection, discovered absolute paths, activation failure recovery and rejection of non-DEV environments. A connection acknowledgement is the pipeline smoke gate; actual Concierge events and APIM routing remain separate acceptance checks. Slot and digital pipeline wiring is outside this DEV GraphQL/Stitch flow. Review per-app Stitch targets and single-instance hosting before running it.

Validation: four offline deployment regression scenarios passed; five pipeline YAML files parsed; non-DEV deployment structures match HEAD after resolving the DEV conditional branches. `scripts/verify.ps1 -SolutionPath VAA.EAPI.GraphQL/VAA.EAPI.GraphQL.sln -SkipBuild -Base HEAD` failed only the existing context-routing check. The same command with `-SkipContextRoutingCheck` passed; evidence is in `TestResults/native-dev-path-verify-scoped.log`. No Azure activation or subscription acceptance test was performed.

## PR review corrections

Review identified in-scope packaging, routing, configuration and validation defects. Template defaults are now opt-in, with all three PROD parameter files explicitly disabled; DEV remains explicitly enabled. This supersedes the earlier requested enable-by-default setting because omitted prerequisites otherwise prevent worker startup. Digital/Sandbox native settings now live in the authoritative child appsettings resource. Public Stitch is explicitly disabled in every environment and its DEV activation step removed, preventing the shared Secure target from bypassing public-schema visibility.

Concierge no longer copies into REST, whose host does not register its required subscription provider. Main GraphQL CI explicitly builds/tests/publishes/merges Concierge. The bridge rejects supplied credentials without an authenticated principal or the existing resolver-validated Flying Club token feature; no credentials remains a valid anonymous request. Malformed read/stop session IDs now return 400. Regression coverage compares mutation/event IDs and exercises distinct validated AsyncContext callers on one conversation, including an anonymous event that neither authenticated subscriber may receive. Historical provider-ownership/usage guidance and the sequence index were corrected. Azure/APIM acceptance is still outstanding.

Review validation: 35 targeted tests passed, with no failures in TRX analysis and aggregate referenced-assembly coverage of 9.23% lines (568/6157), 11.88% branches (378/3183). Evidence: `TestResults/pr-review-2298-final/concierge.trx`. The offline deployment exercise passed, including new template-default, PROD-disabled and authoritative-appsettings assertions. YAML assertions verified Concierge build/test/publish/merge wiring and Public Stitch's disabled flag/absence of activation. Solution verification passed with `-SkipBuild -Base HEAD -SkipContextRoutingCheck`, retaining the existing context-routing exception; log: `TestResults/pr-review-verify.log`. New test files were explicitly formatted using the owning test project.

## Main integration and CI parameter-limit correction

Merged main at `72e015a727`, preserving both sides of the behavior-sensor and sequence-index additions. Six Azure DevOps builds (GraphQL, Account, Book, Loyalty, NDCHealthCheck and Operations) failed ARM validation because the shared template exceeded Azure's 256 input-parameter limit. Six non-secret WebSocket parameters now live in `NativeWebSocketsConfiguration`; the enable switch, secure bridge credential and Stitch target remain separate. Existing runtime app-setting names and values are unchanged. All seven templates, thirteen parameter files, DEV activation scripts and setup guidance use the grouped contract. The largest templates now contain 254 parameters, including main's additions.

The offline regression exercise now asserts the ARM parameter limit. The 35 targeted tests passed after merging main, with no TRX failures and 9.23% line / 11.88% branch aggregate coverage. Azure read-only DEV template validation passed using the current template and DEV parameters; the temporary CLI input normalized existing parameter-name casing without changing values. No deployment was performed. Evidence: `TestResults/main-merge-2298/concierge.trx` and `TestResults/main-merge-arm-validate.log`.

Full solution verification passed using `scripts/verify.ps1 -SolutionPath VAA.EAPI.GraphQL/VAA.EAPI.GraphQL.sln -SkipBuild -Base HEAD`, including context routing and format checks. No context-routing exclusion is needed after integrating main. Log: `TestResults/main-merge-verify.log`.
