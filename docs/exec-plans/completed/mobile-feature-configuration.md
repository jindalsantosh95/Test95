# Mobile feature configuration

- Status: complete
- Owner: RefData / Retail Reference maintainers
- Created: 2026-10-12
- Scope: RefData GraphQL, Retail Reference, deployment settings, tests, Bruno, sequence documentation

## Objective

Expose a mobile bootstrap query returning a string dictionary owned by Reference.

## Acceptance Criteria

- `mobileFeatures { features }` returns a JSON object backed by a string dictionary.
- Reference reads configuration from its existing blob store through `GET /MobileFeatures`.
- Empty configuration succeeds; invalid/unavailable configuration cannot enable features.
- Deployment settings, executable tests, Bruno examples, and sequence diagram are present.

## Plan

1. Create `codex/mobile-feature-configuration` from the supplied clean worktree.
2. Add the typed Reference query, public GraphQL query, and deployment wiring.
3. Validate serialization and failures, document configuration and rollout, run scoped checks.

## Decisions

- Mobile features use direct DI: `MobileFeaturesController` injects `IMobileFeaturesQueryHandler`, which reads via `IAzureBlobService`. The MediatR request and generic reference-data enum additions were removed; existing unrelated endpoints retain their established implementation.

- Use brace-enclosed namespaces for all code introduced by this change.
- Reference handler tests use implemented application handlers, the shared `.mock` blob service, and direct DI registration of `VAA.EAPI.Core.Xunit.Http.HttpClientFactory`; no bespoke ReferenceDataStub or HttpClientFactoryFixture.
- Reuse the Reference blob abstraction and cache policy; no new store is needed.
- Use the existing GraphQL `Any` scalar with an object-valued dictionary projection;
  tests showed a raw string dictionary otherwise serialized as a key/value array.
- Missing server configuration is a technical error, including downstream 404.
- Publish only client-visible configuration; flags cannot replace backend authorization.

## Validation

- CI follow-up requested by the user: `offline-automation` failed in run 38239583410 because the hosted Linux runner exhausted disk space (`System.IO.IOException: No space left on device`), not a test assertion. Added cleanup of unused Android SDK and CodeQL bundles before checkout in that job, with disk-space logging. Workflow syntax parsed locally. Automation solution-scoped root verification passed using `-Base origin/main -SkipBuild -SkipHarnessRuleCheck`; repository-wide harness rules/exercises had already passed in the preceding run. The default local `main` was stale and included unrelated automation changes, so it was not used as the final comparison baseline. Live runner validation is required for disk-capacity evidence.

- Security/cancellation verification: both Reference and RefData production project-scoped `scripts/verify.ps1 -SkipBuild` runs passed (exit 0). Both test-standard checks, changed-test whitespace checks, and `git diff --check` passed.
- Security/cancellation review: sanitized blob retrieval errors without retaining connection details or inner exceptions; preserved caller cancellation. Added missing-blob/storage-failure mocks and cancellation tests. Reference: 41 passed (`TestResults/mobile-features-review-security/reference.trx`); RefData: 11 passed (`TestResults/mobile-features-review-cancellation-verified/graphql.trx`). Both TRX analyses found no failures. The shared OAuth interface cannot cancel token acquisition itself; the new flow cancels its wait and downstream HTTP request without changing the shared interface.

- PR review verification: Reference project-scoped `scripts/verify.ps1 -SkipBuild` passed; test standards, test whitespace, and `git diff --check` passed.
- PR review follow-up: validate JSON token types before creating the string dictionary to prevent boolean/number coercion. Added six invalid-type `.mock` scenarios and a valid timestamp-string assertion. All 38 Reference tests passed (`TestResults/mobile-features-review/reference.trx`); TRX analysis reported no failures. Aggregate coverage including dependencies is 11.88% lines (1000/8416), 9.91% branches (388/3914).

- Follow-up validation after the namespace and test-harness changes: 8 GraphQL tests passed (`TestResults/mobile-features-namespace-graphql/graphql.trx`), and 30 Reference tests passed (`TestResults/mobile-features-direct-factory/reference.trx`). Both analysers reported no failures. Aggregate Reference coverage is now 11.72% lines (985/8405), 9.87% branches (386/3911); GraphQL remains 19.02% lines and 10.7% branches. The RefData production project and Reference test project scoped `verify.ps1 -SkipBuild` runs passed; the explicit whitespace check for the updated Reference test passed.
- Direct-DI follow-up verification: Reference production project `verify.ps1 -SkipBuild` passed (exit 0), including all five new production C# files. Reference test standards and the updated test whitespace check also passed.
- Direct-DI follow-up: 32 Reference tests passed, including HTTP endpoint serialization through the registered handler (`TestResults/mobile-features-direct-di-final/reference.trx`). TRX analysis found no failures; aggregate coverage including dependencies is 11.81% lines (993/8409), 9.87% branches (386/3912).
- RefData GraphQL: 8 tests passed, including dictionary shape and explicit 404-to-500 coverage; analyser reports no failures.
- Reference tests use command-line `TestingPlatformDotnetTestSupport=false` and
  `UseMicrosoftTestingPlatformRunner=false` to enable the repository TRX/coverage contract.
- `scripts/verify.ps1 -ProjectPath VAA.EAPI.GraphQL/VAA.EAPI.GraphQL.Domain.RefData/VAA.EAPI.GraphQL.Domain.RefData.csproj -SkipBuild` passed.
- Reference scoped `verify.ps1 -SkipBuild` initially found a built-in string-helper usage. Replaced it with the shared extension, then reran `scripts/verify.ps1 -ProjectPath VAA.EAPI.Microservices/VAA.EAPI.Microservices.Retail.Reference/VAA.EAPI.Microservices.Retail.Reference.csproj -SkipBuild`; the full final run passed with exit code 0.
- Explicit whitespace-format checks passed for new production and test C# files (the broad harness skips untracked files for its format step).
- Test standards checks passed for both owning test projects; sequence status, deployment JSON parsing, and `git diff --check` passed.
- Mermaid render validation could not run because `npx` is unavailable. Diagram metadata validation passed; no rendered output is claimed.

Final run evidence: `TestResults/mobile-features-graphql-final/graphql.trx` and
`TestResults/mobile-features-direct-di-final/reference.trx` (local, not committed).

| Analyser aggregate (includes dependencies) | Line coverage | Branch coverage | Lines | Branches |
| --- | ---: | ---: | ---: | ---: |
| RefData GraphQL | 19.12% | 10.85% | 944/4936 | 252/2323 |
| Reference | 11.94% | 9.94% | 1006/8422 | 389/3914 |

## Outcome

Tracked by [mobile delivery issue #180](https://github.com/VAAD1ST/vaa-mobile-delivery-artifacts/issues/180), assigned to `vaa-dimitris-dimitriou` on the Mobile Book Flow board with status In progress for delivery tracking.

Implementation and local test validation complete. On 2026-10-12, the user requested
direct DEV/SIT/UAT blob provisioning in place of the repository example. The
`Examples/mobile-features.json` file was removed, and `{"features":{}}` was uploaded
to `referencedata/mobile-features.json` in `avadeunstreferencems` (DEV),
`avateunstreferencems` (SIT), and `avaseunstreferencems` (UAT). Existing configured
Key Vault-backed SAS credentials were used without recording credentials. All
three blobs were absent; create-only uploads prevented overwriting existing data.
Downloaded content matched the uploaded payload by SHA-256 in each environment.
Actual feature activation, production provisioning, and application deployment
remain rollout work.

On 2026-10-12, fast-forwarded the feature branch to latest fetched `origin/main`
(`9368517fae`) while preserving all local feature changes. Updated the sequence
document to match every applicable section of `_template.sequence-diagram.md`
in order, omitting the optional UX section because no UX artefact was supplied.
Removed the separate APIM participant at the user's request; the diagram now
shows GraphQL calling Reference directly. Template-heading comparison, sequence
status, `git diff --check`, and the RefData project-scoped
`scripts/verify.ps1 -ProjectPath <RefData.csproj> -SkipBuild` all passed after the update.
