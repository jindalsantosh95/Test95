# Migrate existing .NET 8 projects to .NET 10

- Status: implementation complete; full validation blocked
- Owner: EAPI platform
- Created: 2026-10-07
- Scope: all 291 existing .NET 8 projects, CI, deployment runtime settings and developer setup

## Objective

Retarget all existing .NET 8 projects to .NET 10 on `codex/upgrade-dotnet-10`, based on main commit `de65d4638794825f0f7bdc9a7abbfb566419a172`.

## Acceptance Criteria

- All previously .NET 8 projects target `net10.0`.
- CI SDK installation, test artifact paths and deployment runtime settings match the target framework.
- Functions worker packages support .NET 10; central package management is preserved where present.
- Record scoped builds, offline tests and harness results, including unresolved blockers.

## Plan

1. Inventory projects and related runtime configuration.
2. Update target frameworks, compatible Functions dependencies, pipelines, deployment settings and setup documentation.
3. Build affected solutions, run offline tests and repository verification, and review residual .NET 8 references.

## Decisions

- Retain the existing Functions Worker SDK package model, using at least Worker 2.50.0 and Worker SDK 2.0.5, which support .NET 10. Avoid a separate build-SDK migration.
- Compatibility references: [Functions isolated worker guide](https://learn.microsoft.com/azure/azure-functions/dotnet-isolated-process-guide) and [Worker SDK 2.0.5 release](https://www.nuget.org/packages/Microsoft.Azure.Functions.Worker.Sdk/2.0.5).
- Align Microsoft.Extensions 8/9 package references to 10.0.12 after restore exposed dependency downgrades in shared hosting and caching dependencies.
- Align CheckIn and Utilities.Cryptography Application Insights references to the existing shared 2.23.0 baseline, and Cryptography's CSharpFunctionalExtensions reference to the existing 3.6.0 baseline, to resolve NU1605 restore failures.
- Remove explicit packages already supplied by .NET 10 where the SDK emits NU1510: System.Security.Cryptography.Cng, System.Text.Json, System.Net.Http and the web host's Microsoft.JSInterop. Do not suppress the warning or weaken the Automation warning ratchet.
- Select stable .NET 10 SDKs through `global.json` with feature-band roll-forward.
- Keep historical execution plans and decision records intact. Update current setup instructions and scaffolding defaults.
- Do not run live automation journeys or deploy infrastructure as part of local migration validation.
- Build owning solutions sequentially: they share project output directories, so concurrent solution builds can cause file-lock failures.

## Validation

- Installed SDK: 10.0.401.
- All 291 tracked C# projects now declare `net10.0`; no tracked project remains on `net8.0`.
- Initial Brand build exposed NU1605 conflicts in shared dependency injection and caching packages; the corrected Brand build passes.
- Release builds pass for GraphQL, Web, Orchestration, NDC Transformation, HealthStatusApp, LoungeManagement, Microservices.Data, Microservices.Retail, Microservices.Retail.Holidays and Microservices.Utilities. Retail and HealthStatus were rechecked sequentially after the final edits; both pass.
- The Facade runtime/test projects build successfully using an ignored validation `.slnx` assembled from tracked project files. The existing Facade solution fails because IFGFacade.Deployment references absent `azuredeploy.json` and `azuredeploy.parameters.json` files at the deployment project root.
- Initial command-line builds of the Microservices aggregate, CRM, FlightOperations, GroundOperations and Loyalty solutions failed with MSB4006 circular solution dependency errors. All 157 tracked non-backup Microservices projects build successfully using an ignored validation `.slnx` with no solution-only dependencies (0 errors, 3180 warnings). Backup project files were retargeted but not included in this combined build because they share output directories with their primary projects.
- Follow-up: the user reports successful local Visual Studio builds of the aggregate, CRM, FlightOperations and Loyalty solutions. Their solution edits are preserved. GroundOperations still reproduced MSB4006 when built alone; removed its solution-only Domain-to-Common dependency, retaining Common's valid project reference to Domain. Sequential GroundOperations builds now pass: Release has 0 errors and 47 warnings; Debug has 0 errors and 27 warnings. Logs: `groundoperations-repair-build.log` and `groundoperations-repair-debug-build.log` in the run directory. This repair changes solution build ordering only, with no application or test code changes. Required scoped verification output is retained as `groundoperations-repair-verification.log`.
- Automation's full `scripts/verify.ps1 -Base origin/main` fails restore on existing transitive vulnerabilities: log4net 2.0.12 (NU1902), Microsoft.Kiota.Abstractions 1.17.1 (NU1903), and System.Drawing.Common 5.0.0 (NU1904). The NU1510 issues found during migration have been fixed; the vulnerability gate remains enabled. Automation build and offline tests therefore remain unverified.
- Kiota follow-up: pinned Microsoft.Kiota.Abstractions 1.22.0 centrally and added versionless references in Core and Core.Delta, which consume Microsoft.Graph. Both restored dependency graphs resolve 1.22.0; Automation verification no longer reports the Kiota vulnerability, but still fails restore on log4net and System.Drawing.Common. A targeted Core.Tests run is blocked by the System.Drawing.Common audit failure before tests execute; no TRX or analyser result is available. Logs: `kiota-automation-verification.log`, `kiota-core-tests.log`, and `kiota-repository-verification.log` in the run directory.
- log4net follow-up: pinned log4net 3.3.0 centrally and added a versionless reference in MarketingLoyalty, which brings in log4net through Snowflake.Data. Restored dependency graphs resolve 3.3.0, and the log4net vulnerability error is gone. Automation verification now fails restore only on System.Drawing.Common 5.0.0 (NU1904). MarketingLoyalty test discovery and offline Core.Tests are also blocked during restore; no TRX/analyser result is available, and no live MarketingLoyalty scenarios ran. Logs use the `log4net-` prefix in the run directory.
- System.Drawing.Common follow-up: pinned the requested 5.0.3 centrally with a versionless Core reference. Sixteen dependency graphs resolve 5.0.3; seventeen retain their higher 10.0.9 transitive requirement. No graph remains on 5.0.0. Automation restore now passes, but the warning ratchet blocks compilation on CA2022 (inexact stream reads) and SYSLIB0057 (obsolete certificate constructors). Targeted Core.Tests encounters the same compile errors before execution, so no TRX/analyser result is available. Logs use the `drawing-` prefix in the run directory. These source-code compatibility errors remain outstanding; no warning suppression was added.
- Core compatibility follow-up: replaced both single stream reads with CopyTo into temporary buffers, and replaced the two certificate constructors with format-selected X509CertificateLoader calls. Added eight offline regression cases for partial/non-seekable streams, remaining/empty bodies, PFX and DER loading from bytes/files, private-key preservation, and incorrect passwords. All 84 Core.Tests pass (0 failed/skipped), and exact-run TRX analysis reports no failures. Explicit formatting verification covers the two new untracked test files. The full Automation build gets past the original four errors but stops on CS9336 in Book's `Tests/NewBookFlow/GraphQL/GraphQLAssertion.cs:623`, a redundant fare-family pattern. That business assertion is unchanged. Logs use the `compatibility-` prefix; no live integration tests or Azure DevOps uploads were executed.
- Repository verification with `scripts/verify.ps1 -SolutionPath VAA.EAPI.GraphQL/VAA.EAPI.GraphQL.sln -SkipBuild` fails the operational-harness check because its registry review is older than 30 days. The final root-scope check using the Microservices solution and `-ScopePath . -Base origin/main -SkipBuild` also reports missing deployment folders in its deployment-coverage check. No new C# files require formatting; the other checks pass.
- Changed project/props XML parses successfully; all 228 changed deployment JSON files parse successfully. Changed PowerShell scripts parse successfully and `git diff --check` passes.
- Validation logs: ignored `TestResults/net10-migration/` directory.

### Targeted tests

Both runs use Release, TRX and XPlat Code Coverage, followed by `scripts/analyse-test-run-results.ps1` against the exact run TRX. Set `-p:UseMicrosoftTestingPlatformRunner=false -p:TestingPlatformDotnetTestSupport=false` to retain the repository's VSTest TRX/coverage contract. Without these flags the xUnit v3 runner ignores the VSTest logger and collector arguments.

| Suite | Passed | Failed | Skipped | Analyser line coverage | Analyser branch coverage |
| --- | ---: | ---: | ---: | --- | --- |
| GraphQL.Common.Tests | 38 | 0 | 0 | 32.85% (1016/3093) | 18.65% (355/1903) |
| Retail.Brand.Tests | 1 | 0 | 0 | Unknown aggregate; collector reports 0% with no line counts | Unknown aggregate; collector reports 0% with no branch counts |

Both analyser reports state that no failed tests were found. These targeted tests are not a full regression run.

### Automation compiler repair (2026-10-08)

- Removed the redundant `or ECONCLASSICFLEXIBLE` alternative from the negated fare-family pattern in Book's GraphQL seat-map assertion, preserving the existing free-seat requirements.
- Eight focused offline seat-map cases pass. The full Automation verification now passes restore, Release build (0 errors, 3269 warnings), architecture tests (10 passed), and Core tests (84 passed). Exact-run TRX analysis reports no failures for all three suites; no live journeys ran.
- Explicit formatting verification passes for the new Book regression test file, which is untracked and therefore not discovered by the harness's Git-added-file check.
- Evidence: `seat-pattern-tests.log`, `seat-pattern-analysis.log`, `seat-pattern-automation-verification.log`, and `seat-pattern-format-verification.log` in the migration run directory. Required root verification with `-SolutionPath VAA.EAPI.Automation/VAA.EAPI.Automation.sln -SkipBuild` exits 1, failing only `deployment-projects` (missing HttpMocker deployment solution entry) and `operational-harness` (registry review older than 30 days). Other checks, including formatting, pass. Full output is in `seat-pattern-repository-verification.log`. The user's solution edits remain intact.

### Repository gate repair (2026-10-08)

- The user confirmed deployment projects were intentionally removed from solutions because their project type is unsupported in their Visual Studio 2022 setup. Keep those removals. Deployment coverage now validates the sibling deployment project and required assets on disk without requiring a legacy `.deployproj` solution entry.
- Reviewed all 12 operational capability entries against their entry points, evidence paths, scheduled workflow wiring and today's local harness exercise results. Refreshed the review timestamp and corrected resource-control enrollment from five to six workflows, adding the associated-PR feedback workflow to the evidence list.
- Capability status and cycle counts are unchanged: local exercise results do not prove scheduled maturity, and advisory cloud review/gardening runs were not executed as part of this local review.
- Targeted deployment coverage and operational registry checks both pass. A synthetic fixture confirms omission from the solution is accepted while missing required deployment assets still fail. Automation verification passes again with the deployment project excluded: Release build has 0 errors, architecture tests pass 10/10, Core tests pass 84/84, and both exact-run TRX analyses report no failures. Required root verification with `-SolutionPath VAA.EAPI.Automation/VAA.EAPI.Automation.sln -Base origin/main -SkipBuild` exits 0. Full evidence is recorded in `gate-repair-automation-verification.log` and `gate-repair-repository-verification.log` under `TestResults/net10-migration/`.

## Outcome

Linux parity follow-up: after the deployment-layout fixes, the only remaining failed verification batches reported four additional uppercase `Prod-azuredeploy.parameters.json` filenames (CRM Email, CRM Notification, Insights, Loyalty Identity). Normalized those files and all corresponding legacy project references, including the pre-existing PartnerDataManagment reference, to lowercase `prod`. No production parameter values changed.

Deployment parity follow-up: the expanded matrix exposed false missing-folder reports for centrally stored microservice deployments, legacy project-name variation, and Linux casing of EnrolmentFacade's production parameters. The checker now resolves central deployment folders and accepts one legacy deployment-project filename; the production parameter filename/reference is normalized to lowercase. The user explicitly selected documented legacy exceptions for missing slot/APIM and PDM project-support assets. These are recorded per app/file in `docs/deployment-asset-exceptions.json`, with reasons; core ARM templates and environment parameters cannot be exempted. All affected solution deployment checks and the nine-case deployment regression exercise pass. These exceptions acknowledge existing deployment gaps, not completed provisioning.

PR parity follow-up: run `37807221011` detected 309 verification scopes but could not start the verification matrix, exceeding GitHub Actions' 256-job matrix limit. The workflow now batches every resolved scope into at most 200 jobs (155 jobs for this PR), runs all scopes in each batch, and propagates any failure to the gate. Detection failure also fails the gate, and verification receives the fetched base ref explicitly. Seven regression cases cover empty/single/boundary/large matrices and invalid scope kinds; all scopes are retained exactly once. Existing deployment and audit findings remain visible rather than being excluded from verification.

Main integration on 2026-10-08: merged `origin/main` at `9c54fd9de2`, retaining the new WebSocket host, Concierge projects and RulesEngine mock fixtures. Resolved four conflicts while preserving .NET 10 dependencies and exclusion of unsupported deployment projects from Stitch. Deployment coverage retains main's solution-only app discovery and the migration's on-disk deployment validation; its four-case exercise passes. Retargeted the three newly added projects, WebSocket copy target and new GraphQL pipeline SDK selection to .NET 10 (294 projects total). Stitch Release build passes with 0 errors and 36 warnings. RulesEngine tests pass 95/95; Concierge tests pass 62/62; both TRX analyses report no failures. Coverage is respectively 23.41% line / 13.73% branch and 9.96% line / 13.41% branch. RulesEngine scoped `-SkipBuild` verification exits 0; scoped Stitch verification was also run. Logs use the `merge-` prefix. Existing GraphQL dependency vulnerability warnings remain; these are not suppressed by the merge.

Final solution checks on 2026-10-08: Facade Release build passes with 0 errors and 134 warnings; GraphQL.Stitch Release build passes with 0 errors and 36 warnings. Both were built sequentially. Scoped root verification passes for GraphQL.Stitch (exit 0); Facade fails only deployment coverage (exit 1): AzureB2C slot/APIM templates, EnrolmentFacade and IFGFacade APIM templates, and PDMFacade's deployproj, deployment script and targets are missing. Logs use the `final-facade-` and `final-stitch-` prefixes under `TestResults/net10-migration/`. These checks did not execute application tests.

All existing .NET 8 projects, runtime deployment settings, SDK installation steps, test output paths and current setup guidance have been migrated. Automation restore, build, offline tests and scoped root verification now pass. The two outstanding Automation repository gates are resolved. All affected solutions have successful build evidence, including user-reported Visual Studio builds and earlier checks; this is not a fresh full-repository build or regression run. Facade deployment coverage remains outstanding. No infrastructure has been deployed and no live automation journeys have run.
