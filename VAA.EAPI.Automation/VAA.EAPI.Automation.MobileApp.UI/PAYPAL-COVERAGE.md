# PayPal sandbox login readiness

## TC57 — prepared, not live-qualified

Status, 21 October: further live qualification is on hold because the user reports
internal environment issues. TC57 remains explicit-only, disabled by default and
excluded from Sanity/Regression; do not interpret this hold as a passing payment test
or proof that the previous failure was caused by the environment.

The second local attempt reached HPP with PayPal expanded, but stopped while opening
PayPal before the login checkpoint (0 passed, 1 failed; test body 9m49s). Its exact-run
analyser completed. Evidence: `TestResults/paypal-watch-local-20261020/tc57.trx`.
The helper currently searches only the top-level WebView document. Iframe traversal
and usable WebView inspection remain unqualified; the simulator inspection target
returned an empty URL/title. Resume by inspecting the actual provider control, then
qualify login/approval and confirmation without automatic payment retries. No iOS
inspection changes were made. Local execution opt-ins have been reset to false.

Local full-scenario attempt, 20 October: **failed**, 0 passed/1 failed, 9m58s including
lifecycle. Core Vault retrieval and Review figure/readiness assertions passed. Failure
occurred before the first PayPal launch-success checkpoint; no login or approval was
reached. The initial sanitised report cannot distinguish HPP handoff from PayPal control
lookup, so cause remains unconfirmed—not attributed to the known confirmation bug.
Added safe handoff/provider-stage messages, subsequently exercised by the second run
described above. No payment retry. Evidence:
`TestResults/paypal-completion-local-20261020/tc57.trx`.
Local payment/privacy opt-ins reset to false and ordinary Appium URL restored after this run.

`SearchBook_TC57_GuestPayPalSandbox_CompletesBookingAndAppearsInTrips`
is an explicit `Suite=Readiness` scenario, excluded from Sanity/Regression. One guest adult,
LHR–JFK, Virgin Economy Classic, seats skipped. Existing checkout validates Review arithmetic,
Summary total, currency and guidance before HPP. PayPal login must expose an enabled
`button[data-id='payment-submit-btn']` on an HTTPS PayPal sandbox origin.
With explicit sandbox-payment approval, the test clicks that button once and requires app
confirmation, expected travellers, booking reference and Trips through the shared checkout.
Order confirmation is a user-reported blocker; encountering it fails the full test.
There is no automatic payment or login retry, and login alone is not a pass.

## Reuse and provider boundary

Uses the existing `Core.Vault` connector and `APMDetails`, with the same
`PayPal-Test-Email` / `PayPal-Test-Password` secrets as Core's web PayPal implementation.
No account/password is stored in source or mobile settings. `Core:KeyVaultUrl` must identify
the approved SIT vault; existing Core identity settings or DefaultAzureCredential supply access.
The provider uses an explicit Vault instance rather than mutating global Core configuration.

The external selector source is `Core.Playwright/HostedPayment/DirectHostedPaymentPage`:
HPP `div[id*='paypal-buttons']`, PayPal `input#email`, Next, `input#password`, Login and the
approval button. Appium must expose an inspectable WebView and its popup/window.
Provider DOM IDs and semantic labels are an explicit external-ownership exception to the
app-owned accessibility-ID contract, not missing iOS readiness IDs. No coordinate or broad
XPath fallback is added. Native HPP PayPal exposure and mobile WebView/popup behaviour still
require local qualification; missing/inaccessible content fails, not skips or passes.
The Playwright submit method is not invoked because it owns another browser; the native
journey reuses its selectors while retaining the Appium session and shared confirmation flow.

## Safe local qualification prerequisites

The test is disabled by default. In the existing gitignored local settings use:

```json
"PayPal": {
  "Enabled": false,
  "AllowSandboxPayment": false,
  "CredentialSafeLoggingConfirmed": false
}
```

Only set these true after explicit payment authorisation and confirming local SIT, approved vault, and a dedicated Appium/WDA
setup that does not retain credential commands, screenshots, page source, browser/network
traces or external screen recording. The logging flag is an operator attestation, **not**
automatic verification or redaction of the Appium server. Do not use debug/HTTP request-body
logging; inspect logging configuration before entering credentials. BrowserStack is blocked
until command masking, recordings and remote privacy controls are separately implemented
and qualified. Fault injection must be disabled. Do not run the shared PayPal account
concurrently from another runner.

Scenario overrides suppress harness failure screenshots, hierarchy, app/network logs and
raw exception output for the entire scenario, including pre-login failures. Only safe stage
messages may leave the page object; the test does not propagate raw provider errors.
This sacrifices some failure evidence to avoid collecting credentials. All normal tests
retain their existing evidence. CAPTCHA, MFA, changed markup and provider errors fail within
bounded waits; there is no repeated credential submission or payment retry.

Run only after explicit approval of sandbox payment:
`dotnet test -c Release --filter "FullyQualifiedName~SearchBook_TC57_"`
with run-scoped TRX and the repository analyser.

## Deferred coverage

CI follow-up (21 October): use the existing Core string extensions for account
validation. Preferred-extension validation is required in addition to the Automation
solution harness; the root CI gate checks this convention separately.

Authoring validation (21 October): 608 mobile offline/retry-policy checks passed, zero
failed/skipped. Exact-run analyser found no failures in
`TestResults/pr2519-offline-20261021/offline.trx`. Automation harness build,
architecture/Core checks and tracked-file formatting passed. These do not qualify the
provider's mobile DOM, sandbox login or any completed payment.

- Provider amount/currency comparison, cancellation and recovery.
- Live qualification of approval, app return, booking confirmation and Trips after the product blocker is fixed.
- Remote qualification, repeated-run account/session isolation, alternate provider layouts.
- No iOS code changes, payment fault injection or claims of full PayPal E2E coverage.
