# Account-linking operation registration

`loyaltyRedeemLinkingProof` is registered when the host's secure flag is set or `OperationsVisibilityMode` contains the `Secure` member of a pipe-separated list, such as `Public|Secure`. Its existing authorization role remains required.

This combined-mode handling is scoped to the new account-linking operation. Registration rules for existing account operations are unchanged. Proof creation remains registered on both surfaces and requires authentication.

## Agreed external account-linking contract

The [goal and contract](../../docs/sequence-diagrams/vaa-eapi-alexa-mcp-proxy-account-linking.md) is maintained at the repository root (follow the repository `docs/sequence-diagrams/` path).

- `accountCreateLinkingProof(input: AccountCreateLinkingProofInput!)`: `transactionId`, `clientId`, `resource`, `scope`; returns `proof`, `expiresAtUtc`.
- `loyaltyRedeemLinkingProof(input: LoyaltyRedeemLinkingProofInput!)`: `proof`, `transactionId`, `clientId`, `resource`; returns `flyingClubAccountNumber`, `consentSubject`.
- Continue with existing `loyaltyIssueFlyingClubApplicationToken(request: ...)` using the redeemed account number. Existing role, request and response contracts are unchanged.
- `loyaltyFlyingClubAccountSummary` validates the ordinary broker token through the existing application-token validation path and returns only tier, Virgin Points, Tier Points and reset date. The existing broad profile operation is not changed.

No transaction assertion, browser proof, account-binding assertion or new header is required for this journey. Use existing authentication conventions, including the application bearer plus secondary broker token where the protected summary requires both identities. The unrequested additional signed linked-token operation and dedicated validation endpoint have been removed; ordinary token issuance and validation remain unchanged.

Proof creation resolves the customer account from the authenticated customer. It prefers the distinct non-empty `email`, `emails` or email-address URI claim. If none is present, it accepts exactly one distinct `preferred_username` containing a plain email address. Conflicting email claims do not fall back to the username. The lookup must still resolve exactly one active Flying Club profile. This fallback relies on the trusted customer identity provider assigning the username to that customer; it does not verify email ownership independently. Identity checks configured client/resource and fixed `account:read`, generates expiry, compresses the binding using the existing cache helpers and consumes the proof atomically through opt-in single-use cache mode. Compression is not encryption; Redis resource security is the accepted boundary. A repeated redemption fails, even with identical inputs. After an uncertain redemption result, restart linking instead of automatically replaying it. The opaque `consentSubject` is an EAPI correlation identifier, not evidence that the external proxy captured consent.

Domain errors use the agreed `ACCOUNT_LINKING_*` codes with `httpStatusCode`, `retryable` and `correlationId` extensions. Authentication/authorization rejections before resolver execution retain the existing shared host envelopes; this change does not alter global error filters.

Account-linking resolver errors explicitly opt into generated REST/MCP status translation using the numeric `transportStatusCode` error extension. REST preserves the domain error extensions and uses the selected HTTP status; MCP preserves them in `error.data.errors` with the numeric status in `error.code` (not an HTTP status change to the MCP envelope). Other operations without this opt-in retain their existing transport handling. Customer lookup and Identity HTTP failures/timeouts produce the retryable `ACCOUNT_LINKING_DEPENDENCY_UNAVAILABLE` error with status 503; invalid profiles remain non-retryable 403 errors. These existing connector calls do not currently accept a caller cancellation token.
