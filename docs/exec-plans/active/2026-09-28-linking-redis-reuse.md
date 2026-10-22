# Account-linking Redis configuration reuse

## PR review follow-up

- Main/slot Redis parameters are mandatory secure strings with minimum length one. All four existing environment parameter files supply the shared vault reference, preventing an omitted parameter from silently overwriting Redis with an empty string.
- The null-teardown finding was not reproduced: a regression verifies the actual ConfigReader and fixture Dispose restore null, empty and populated prior values. No shared helper behavior was changed.
- The Redis eviction finding is a real high-severity operational risk but predates this PR: main already documented volatile-lru, and the removed dedicated secret values matched the shared references. Deferred eviction/capacity redesign as out of this configuration simplification scope; activation remains subject to the documented readiness concern.
- The summary-only Markdown finding is a false positive: the Redis table row has 12 balanced backticks, including a closing delimiter after the masked example.
- Release build passed; focused tests 115 passed, zero failed, eight Redis integration cases skipped. TRX analysis clean. Main/slot parameter checks and git diff --check passed. Scoped verify -SkipBuild completed with the same repository-wide harness failures; no full-gate or live Redis verification claim.

## Latest decision: one signing authority

The user requested removal of hypothetical separate-signing support. Both account-linking assertion types now use the transaction issuer/JWKS directly. Deleted the fallback/override selection method, two environment constants, all main/slot/environment deployment references and their active table rows. Assertion purpose/audience, caller, account, expiry and replay checks remain. Existing generic hosts and authentication behavior are untouched.

The integration contract now explicitly requires one backend signing authority. This is the chosen minimal design, not a claim that an external backend has been deployed or verified. Other issuer/audience/client/resource settings cannot safely be inferred from the shared user tenant; DEV's caller mismatch also remains real.

Release build passed (157 warnings, zero errors). Focused tests passed 112 cases, with eight Redis integration cases skipped without a dedicated connection. Tests cover missing signing configuration, wrong key/issuer and cross-purpose rejection; obsolete separate-signer tests were removed. No coverage or live Redis verification is claimed.

TRX analysis found no failures. Required scoped verify -SkipBuild completed; format/test standards passed, while the existing repository-wide sequence-goal, memory/context, documentation/workflow, maturity, behavior-harness and architecture-fitness gates still failed. Deployment JSON parsed successfully and no removed signer settings remain in active source/deployment documentation.

The six disabled, delivery-owned binding-signer placeholders were soft-deleted across DEV/SIT/UAT, without enabling them or reading their values. No private or shared signing keys were removed and no production Azure resources were changed. SIT/UAT owning apps and DS slots had no obsolete references.

## Initial approach (superseded by undeployed-delivery cleanup below)

- Base: latest main 149a55e4d3; independent branch, no stack.
- Scope: Identity-specific configuration selection, main/slot template inputs, tests and deployment documentation. No shared authentication, cache-wrapper, backend assertion or public API changes.
- Explicit dedicated Redis configuration remains authoritative. Blank/omitted selects the platform RedisConnectionString. Invalid explicit values and failures must not select another store. Preserve TLS, atomic repositories and all environment overrides pending deployment and endpoint/database equivalence checks.
- Preserve separate backend identity/signing settings: equivalence has not been confirmed. Do not delete encryption material or still-referenced Redis/caller/version secrets.
- Carry forward the user's uncommitted retired-setting documentation cleanup without modifying its original worktree.
- Live cleanup: soft-deleted four redundant Identity lifetime/replacement copies in SIT/UAT on 28 September, after checking 120/3 values and zero references in each app and DS slot. No purge; no live Redis or key migration.
- Validation: Release build passed; focused suite 59 passed, zero failed, six live Redis tests skipped because no dedicated test connection is configured. TRX analysis clean; native runner coverage unavailable. An initial test expected the base ArgumentException instead of the parser's ArgumentOutOfRangeException; corrected the assertion and reran to green. Main/slot JSON/defaults validated; new production C# format check and git diff --check passed.
- Scoped verify -SkipBuild executed for Identity and GraphQL REST documentation. Repository-wide sequence-goal, memory/context, docs/workflow, maturity, behavior-harness and architecture-fitness checks report existing failures; this is not a clean full gate. No runtime migration or end-to-end Redis verification claimed.
- Vault read-back: all three Identity vaults have 13 active linking-related entries and zero redundant numeric-default copies. Original GraphQL documentation changes remain uncommitted in the prior worktree; this branch includes their equivalent cleanup without disturbing that worktree.

## Undeployed-delivery cleanup

User confirmed account-linking has not been deployed. Remove dedicated Redis and separate encryption-version settings rather than maintain migration overrides. Use existing Validation Redis secrets (exact DEV/SIT/UAT value equality verified) and combined encryption rings preserving original key bytes. Retain the trusted-caller setting: DEV does not equal the existing authorized party; do not change authorization as cleanup. Backend signing equivalence remains unconfirmed. No shared secret, global auth behavior or production Azure resource may be changed.

### Fixed proof limits follow-up

- Removed lifetime/replacement configuration readers, environment constants, template parameters/app settings and configuration-table rows. The implementation fixes proof lifetime at at most 120 seconds, bounded by transaction expiry, and permits three replacements per transaction. Existing shared behavior is unchanged.
- Rechecked all three Identity vaults: zero active numeric-limit secrets. Their earlier soft deletions remain sufficient; no further Azure mutation was needed.
- Added/extended real-DI Redis contracts for the 120-second cap and rejecting a fourth replacement without losing the current proof. Release build passed; focused tests: 64 passed, zero failed, eight Redis integration cases skipped without a dedicated connection. TRX analysis clean; integration behavior is not claimed as executed.
- Updated active inventory to 12 account-linking-specific settings plus shared Redis. Separate signer configuration and DEV's differing caller restriction are retained pending confirmed contracts.
- Required scoped verify -SkipBuild completed: format/test standards passed; the same repository-wide sequence-goal, memory/context, documentation/workflow, maturity, behavior-harness and architecture-fitness failures remain.

### Earlier Redis/key-ring cleanup results

- Removed both settings from runtime configuration and main/slot/environment deployment files. Existing platform Redis references match those already used by Retail Reference/Basket. No Redis store/key changes.
- Updated all three encryption rings, verifying the active version and key bytes. Soft-deleted the six redundant Redis/version vault copies; no purge. SIT/UAT owning apps and DS slots had no obsolete references.
- Release build passed after shutting down stale MSBuild servers holding an output lock. Focused tests: 64 passed, zero failed, six Redis integration tests skipped without a dedicated connection; TRX analysis found no failures. No coverage or live atomicity verification claimed.
- Main/slot JSON and changed parameters validated. Scoped verify -SkipBuild ran: format and test standards passed; the same repository-wide sequence-goal, memory/context, docs/workflow, maturity, behavior-harness and architecture-fitness gates failed. This is not a clean full verification gate.
