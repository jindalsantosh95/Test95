# Flying Club Application Token

- Status: completed
- Owner: Account GraphQL / Loyalty Identity
- Created: 2026-07-27
- Completed: 2026-07-27
- Scope: Account GraphQL, Loyalty Identity, deployments, tests, and sequence documentation

## Objective

Expose a role-protected secure Account mutation that delegates to Loyalty Identity and returns a short-lived Identity-issued broker JWT containing a validated Flying Club account-number claim.

## Acceptance Criteria

- Secure GraphQL mutation requires `Loyalty.Identity.ApplicationToken.Issue`.
- Identity endpoint independently requires the same role and signs through a non-exportable Key Vault key using managed identity.
- Configuration, deployment wiring, focused tests, and operational documentation are updated.

## Decisions

- Issue a distinct Identity broker JWT; never modify or present it as an Entra token.
- Use RS256 with an asymmetric Key Vault signing operation through managed identity; private key material is never loaded by the Function App.
- Bind the broker token to the authenticated GraphQL application through `azp`.
- Until a narrower business contract is supplied, accept only 1–20 ASCII digits for the account number.

## Validation

- Account GraphQL test project build: passed without warnings.
- Identity test project build: passed; existing project warnings remain.
- GraphQL focused tests: 5 passed; TRX analyser found no failures; 0.57% line and 0.77% branch coverage across the full assembly.
- Identity test suite: 163 passed, 1 skipped. The project's Microsoft Testing Platform configuration ignored VSTest TRX/filter/coverage switches, so no analyser or coverage table was available.

## Outcome

The secure mutation, Identity endpoint, Key Vault signer, role enforcement, deployment settings, managed-identity signing permission, tests, and sequence documentation are implemented. Deployment remains fail-closed until each environment supplies the approved issuer, audience, and Key Vault key ID and assigns the new app role to the approved callers.

