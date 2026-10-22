# Entra Native MFA Registration During Account Enrolment

- Status: completed
- Owner: Loyalty Identity maintainers
- Created: 2026-08-20
- Scope: `VAA.EAPI.Microservices.Loyalty.Identity`, sequence diagrams, and focused behavior tests

## Objective

Complete the native Entra MFA-registration continuation returned during account enrolment, without attempting registration when Entra has already issued the access, ID, and refresh tokens.

## Context

Entra Native Authentication can return `suberror=registration_required` plus a continuation token from `oauth2/v2.0/token`. The existing complete-sign-up route returned token results but did not own the `/register/v1.0` continuation. Enrolment has already verified the email, so the contract requires capability negotiation only for sign-up and an Identity-owned `preverified` registration continuation without an OTP handoff.

## Acceptance Criteria

- The sign-up start request alone requests the `registration_required` capability.
- A complete access/ID/refresh token set ends authentication even when Entra also sends a continuation token.
- `registration_required` follows native `/register/v1.0/introspect`, `/challenge`, and `/continue` behavior.
- `preverified` completion does not request an additional OTP; a non-preverified registration challenge returns a controlled Identity error.
- Sequence documentation and focused automated coverage reflect the contract.

## Decisions

- 2026-08-20: Treat a response containing all three token types as authoritative success before examining a continuation token. This prevents a duplicate registration side effect from a mixed token response.
- 2026-08-21: Keep native registration entirely in Loyalty Identity's complete-sign-up flow because account enrolment pre-verifies the email; GraphQL and the regular OTP operation retain their existing contracts.

## Validation

- `dotnet msbuild` completed successfully for the Loyalty Identity project.
- The Identity test project compiled with the three unrelated trigger-subscriber tests temporarily excluded. Its test executable could not start in this workspace (`Win32Exception (5): Access is denied`), so no current-run TRX analysis or coverage output was produced.
- `scripts/check-sequence-diagram-status.ps1` and `scripts/check-behavior-harness.ps1` passed.

## Outcome

Implemented sign-up-only capability negotiation, registration transport calls, Identity-owned CompleteSignUp native-registration handling, fixture-driven preverified coverage, and the matching sequence diagram.
