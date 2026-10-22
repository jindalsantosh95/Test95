# Concierge subscription proof of concept

- Status: completed
- Owner: EAPI GraphQL
- Created: 2026-09-30
- Scope: Concierge domain, shared startup contract, primary GraphQL registration, and tests

## Objective

Add dummy ping/pong queries, a posting mutation, and conversation-scoped subscriptions using the established GraphQL domain pattern. Keep Stitch unchanged and consume the composed GraphQL endpoint through its existing configuration.

## Acceptance criteria

- Concierge is a class library with shared Common/Infrastructure references, HC12 packages, `Startup : IStartup`, and type extensions.
- The primary GraphQL host manages subscription registration alongside query/mutation registration, with no Concierge-specific startup branch.
- Domain binaries follow the normal post-build discovery flow, and Concierge is included in the GraphQL solution.
- Tests verify dynamic replies, validation, event routing, and visibility. Documentation distinguishes schema execution from transport support.

## Decisions

- Follow the user's clarified scope: no Stitch changes and no independently hosted domain. The primary GraphQL Functions project owns server registration.
- Add a default no-op `IStartup.RegisterSubscriptions` callback for compatibility with all existing domains. The primary host forwards it through `ConfigurationActions`, invokes discovered callbacks, and conditionally creates the Subscription root using the existing mutation registration convention.
- Concierge registers only its message service in `ConfigureServices`. The primary GraphQL host owns the in-memory event provider through `RegisterDomainSchema(...)`. Resolver extensions are Secure-only; Concierge is excluded from the REST artifact because REST does not register a subscription provider.
- No transport rewrite, package upgrade, deployment, or pipeline changes. In-memory subscription execution is the POC; existing Functions transport limitations remain observable.
- No not-found outcomes or REST error-mapping changes.

## Validation

- Primary `VAA.EAPI.GraphQL.csproj` Release build passed with existing warnings.
- Concierge test run `TestResults/concierge-poc/run-11/concierge.trx`: 11 passed, 0 failed. TRX analysis found no failures. Aggregate coverage including shared libraries: 2.13% lines (99/4652), 2.62% branches (59/2253).
- Primary GraphQL test run `TestResults/concierge-poc/primary-01/primary.trx`: 33 passed, 0 failed. TRX analysis found no failures. Aggregate coverage: 12.62% lines (716/5673), 9.75% branches (275/2821).
- Tests ran with Release configuration, run-scoped TRX, and XPlat Code Coverage.
- `scripts/verify.ps1 -SolutionPath VAA.EAPI.GraphQL/VAA.EAPI.GraphQL.sln -Base HEAD -SkipBuild`: passed. New domain/test project format checks also passed when run explicitly with `--no-restore --verify-no-changes`.
- `git diff --check` passed. Stitch source, project, and solution have no differences.

## Outcome

Concierge composes through the domain startup contract, and in-process subscription execution delivers repeated replies only to the matching conversation. Existing GraphQL tests pass. Stitch requires only endpoint configuration pointing to the normal GraphQL Functions host.

The existing Functions transport uses buffered HTTP responses and has no WebSocket pipeline; live subscription delivery through GraphQL, Stitch, or APIM has not been established. The [Concierge guide](../../../VAA.EAPI.GraphQL/VAA.EAPI.GraphQL.Domain.Concierge/README.md) includes setup, operation examples, and this limitation.
