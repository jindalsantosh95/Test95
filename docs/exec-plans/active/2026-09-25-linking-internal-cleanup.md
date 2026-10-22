# EAPI-only account-linking internal cleanup

- Status: implemented; draft PR for user review
- Baseline: main 815ed571cd
- Scope: Identity implementation/tests/documentation and backward-compatible deployment parameter defaults. No backend protocol, GraphQL host or live setting changes.

## Decisions

- Production already shares one Redis connection. Remove unused self-connecting constructors instead of introducing another registration.
- Remove unbound proof store/consume and legacy token outcome completion/release methods: tracked repository references show no production callers. Migrate their tests to current bound-proof and atomic completed-outcome publication paths.
- Preserve legacy record decoding and incomplete-outcome rejection, Redis keys, encrypted formats, replacement limits and retries.
- Share only common JWT validation parameters; leave assertion-specific validation intact.
- Retain existing core Xunit distributed-cache fixture paths. Removing these without an existing atomic Redis mock would require new test abstractions or mandatory Redis for ordinary tests, neither a simplification. These tests must not be represented as cross-instance Redis verification.
- Keep all supported config overrides; distinguish defaults and paired optional settings in README. No security setting removed solely to reduce the count.

## Validation

- Additional uncommitted review pass: removed the Account resolver's repeated proof-length guard (the earlier identical limit remains, before request lookup). No new not-found outcome, error contract, authentication policy or generic GraphQL/REST change. Added oversize-proof regression cases for both GraphQL and REST paths. Providers/controllers remain intact because their forwarding and mapping express distinct contracts rather than redundant layers.
- Consolidated the two Identity request-body test classes into AccountLinkingRequestBodyTests, retaining all four original cases. Replaced three test factory registrations with direct real-type DI registrations; existing Core Xunit DistributedCache remains the only cache test dependency. No additional fixture abstraction or fake introduced.
- Validation: GraphQL Release build passed; 32 account-linking tests passed with TRX analysis clean. Targeted-run coverage across loaded assemblies is 12.64% lines and 9.59% branches, not full-suite coverage. Identity test-only build passed against existing dependencies; 23 focused tests passed and one real Redis test skipped, TRX analysis clean; native runner coverage unavailable. No commit/push authorised for this pass.

- PR review 4105465845: the proposed null-restoration failure was not reproducible. Added three cases proving the real fallback test restores null, empty and populated configuration in both ConfigReader and the process environment on .NET 8. Focused handler suite: 31 passed, one Redis test skipped; TRX analysis clean. The ordinary dependency rebuild hit DLL locks held by other processes; test-only build with BuildProjectReferences=false succeeded against existing dependencies. No shared ConfigReader or production behavior change; native runner coverage unavailable.

- Further local simplification: reuse Identity's existing Base64UrlEncoder for encoding only; retain proof decoding/canonical rejection unchanged. Separate account-number validation from transaction validation instead of supplying a dummy account during redemption. Derive stored outcome completion from its token at serialization rather than passing a redundant flag. No shared-host, protocol, Redis script/record-format or configuration changes.
- Added nine handler boundary cases using the existing service provider, real handler/repository and Core Xunit DistributedCache. Baseline handler run: 24 passed, one Redis test skipped. Post-change focused suite: 111 passed, zero failed, six Redis tests skipped; both TRX reports analysed with no failures. Release build succeeded. Native test runner did not collect coverage. User subsequently authorised committing and pushing these changes and the consolidated configuration table to draft PR #2219.
- Follow-up scoped verification with -SkipBuild completed: C# format check passed; existing sequence-goal, memory, context-routing, documentation, workflow, maturity, behavior-harness and architecture-fitness checks failed. No clean repository-wide gate is claimed.

- Configuration reduction follow-up: optional trusted-caller fallback to existing FlyingClubToken:AuthorizedParty; combined key-ring JSON in the existing Keys setting with legacy version precedence. Main/slot templates allow omitted caller/version parameters. Environment files retain those security overrides pending migration; remove only redundant lifetime/replacement literal overrides. No backend changes, key rotation, secret deletion or live writes.
- Red test run config-reduction-red/red.trx: four expected failures (three combined-ring cases and one caller fallback), 26 passed, one Redis case skipped. TRX analyser confirmed four failures before implementation.
- Configuration green run: Release build zero errors (18 existing warnings); 102 tests passed, zero failed, six Redis integration cases skipped. TRX TestResults/config-reduction-green/green.trx analysed with no failures; native runner provides no coverage. Main/slot template JSON and 120/3 defaults validated, optional defaults checked; all environment JSON parsed. No Azure deployment validation or live migration performed. Scoped verify.ps1 -SkipBuild run again; repository harness result recorded separately from functional tests.

- Scoped Release build succeeded (zero errors, 32 warnings in existing tests).
- Native xUnit/Microsoft.Testing.Platform run: 92 passed, zero failed, six Redis integration cases skipped because ACCOUNT_LINKING_REDIS_TEST_CONNECTION is unset. TRX: TestResults/internal-cleanup/internal-cleanup.trx. Scoped analyser reports no failed tests. Coverage is unavailable from this native runner configuration; cross-instance Redis behaviour was not verified in this run.
- git diff --check passed. Scoped verify.ps1 -SkipBuild invoked; repository-wide sparse-checkout/harness failures include missing workflow files. No claim of a clean full verification gate.
- User authorised a new independent branch from main and draft PR: codex/simplify-linking-internals-config. Earlier local configuration-table work remains in its separate worktree and is not overwritten.
