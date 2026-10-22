# Sequential mobile fault-injection POC

Status: first local iOS process-capture POC passed on 14 October 2026; not part of release sign-off.

## Prepared confirmation warnings — 16 October 2026

`ConfirmationWarningCase` prepares six independent code/type/message warning fragments. These are
**Draft fixture preparation**, not six implemented Appium scenarios: the live pack remains
11 cases. No Sanity/Regression membership, normal test settings or proxy behaviour changes.

| Prepared case | Returned codes | Required UI outcome once the app contract is published |
| --- | --- | --- |
| AllSeatsFailed | W001 | Confirmation retained; all seats unconfirmed warning |
| SomeSeatsFailed | W002 | Confirmation retained; partial seat fulfilment warning |
| Proximity | W003 | Confirmation retained; proximity information, no hard stop |
| AllSeatsFailedWithProximity | W001 + W003 | Both applicable warnings; no partial-fulfilment warning |
| SomeSeatsFailedWithProximity | W002 + W003 | Both applicable warnings; no all-seats-failed warning |
| NoWarnings | Empty warning array | Confirmation retained; no warning |

Source: EAPI `Retail.Order/Warnings/Warnings.json` and
`Retail.Order/Infrastructure/Mapper/FlightOrderCreate/DeltaOrderMapper.cs` at repository
baseline `25990510c0`. W003 comes from Delta FE5180. W001 is all failed/missing seat statuses;
W002 is partial failure. W001/W002 are mutually exclusive; either can accompany W003.
The GraphQL `CreateOrderMapper` carries these through `OrderCreateGraphQLResponse.Warnings`.
Code/type/message values follow that contract. The app displays the backend-returned message;
`ExpectedMessages` exposes the exact fixture text for future UI assertions. Both messages must
be displayed for combined cases, regardless of ordering. Preserve the source spelling and
spacing in the injected payload; do not substitute app-owned wording. Rendering-related
whitespace normalization, if needed, must be explicit and must not drop or paraphrase content.
The fixtures contain no passenger, payment or booking data. Inline synthetic data is the
documented pure-offline-test exception to storage-table sourcing.

The app implementation is currently developer-local and cannot yet be inspected. Code-driven
warnings can be individual or a combined presentation: future page assertions must observe
the exact set of applicable warnings without assuming ordering or one banner per code.
Do not invent accessibility IDs. The contract records the prepared cases separately from
implemented scenarios so coverage reports cannot count them as completed UI coverage.

### Activation checklist

1. Inspect the published iOS GraphQL selection, warning mapping, combined display and stable
   observation IDs. Confirm `warnings` is actually selected; an ignored JSON field proves nothing.
2. Complete the synthetic successful `bookCreateOrder` envelope for that selection. Current
   fragments are **not full responses or proxy rules**; the current error-only validator must
   continue rejecting success responses until a narrowly scoped mode is added and tested.
3. Qualify interception of every order attempt, expiry/malformed-call guards and delivery evidence.
   Do not forward a real order to obtain the synthetic confirmation. Post-HPP payment still
   requires explicit approval and the existing payment opt-in; HPP itself is not under test.
4. Add one complete scenario per class using the existing fault lifecycle, bounded observation
   and failure reporting. Assert confirmation plus all expected warnings and absence of unrelated
   warnings (no retry action). Keep the scenarios Draft until local/remote qualification.
5. Never pass a synthetic confirmation to the real-booking reporter or Trips/My Booking lookup.
   These assertions prove UI mapping only, not seat fulfilment, no-charge or refund correctness.

Offline checks cover all six combinations, exact code/type/message mappings, both expected
messages for combined cases, empty expectations for the control, mutual exclusion, and
fresh mutable JSON per call. Live UI handling and successful-response injection remain blocked,
not waived or represented by passing placeholder tests.

## Seat-map unavailable messaging qualification — 16 October 2026

TC10 and TC12 passed locally on 16 October 2026 after restoring the isolated CA to the disposable
simulator and starting process capture before airport lookup could establish reusable connections.
Both runs recorded the injection marker and verified all three approved unavailable-map messages.
TC10 continued from an outbound failure to a normal inbound map (2m06s); TC12 used an unsupported
outbound, injected the inbound failure and continued to payment without a payment action (3m54s).
TRXs: `TestResults/seatmap-proxy-repaired-tc10-20261016-c/tc10.trx` and
`TestResults/seatmap-proxy-repaired-tc12-20261016/tc12.trx`. BrowserStack remains unqualified.

## Latest qualification and post-HPP extension — 14 October

### Seat error cases added — not yet live-qualified

The pack now has **12 opt-in Draft scenarios**. TC10–TC12 reuse the shared proxy/session
lifecycle and existing guest passenger/seat page objects. They omit the skip-seat flag and
use one guest adult, LHR–JFK one way, Virgin Economy Classic. No HPP or payment is performed.

| Case | Injected operation / source | Required observable outcome |
| --- | --- | --- |
| `SearchBookFault_TC10_OutboundSeatMapFailure_ShowsReassuranceAndOpensInboundMap` | Outbound `SearchAndBookRetrieveSeatMap` / `retrieveSeatMap`; EAPI `RetrieveSeatMapQuery` generic catch | All three approved messages scoped to `seatSelection.error`; Retry absent; Continue opens the normal inbound map |
| `SearchBookFault_TC11_SeatSaveFailure_ShowsErrorWithoutOpeningPayment` | `SearchAndBookSelectSeat` / `selectSeat`; EAPI `UpdateSeatMutation` generic catch | Select/confirm a dynamically available seat, then submit; `seatSelection.submissionError` contains “We could not save your seat selection. Please choose again.”; Continue enabled and draft assignment retained; remain on seats |
| `SearchBookFault_TC12_InboundSeatMapFailure_ShowsReassuranceAndContinuesToPayment` | Unsupported outbound, then inbound `SearchAndBookRetrieveSeatMap` / `retrieveSeatMap` | All three approved messages scoped to `seatSelection.error`; Retry absent; Continue reaches payment without a payment action |

The fixtures return HTTP 200, null operation data and GraphQL `errors` with code `500` and
the source's exact generic message; exception detail is synthetic. These are generic resolver
failures, not a claim about a particular Delta error code. Current local iOS source routes them
through `SearchAndBookFlightSeatServiceAdapter` and `NativeFlightSeatSelectionViewModel`.
Existing app-owned IDs are used; no iOS code changes or fallback selectors are required.

TC10 relies on the bounded query rule injecting once, then forwarding the normal inbound map request.
TC12 continues only as far as payment and performs no payment action. Neither clicks Retry. TC11's active
mutation guard intercepts every matching save attempt, including retries, rather than forwarding
a second attempt. Unsupported protected calls fail the guard. Proxy death/routing loss still
requires isolated routing qualification; this is not an unconditional fail-closed network claim.
A selected passenger label is only a draft assignment, **not** proof of a successful seat save.
Partial multi-passenger submission/reconciliation, seat timeout and successful retry recovery
are separate gaps, not covered by these two cases.

Trigger using the documented isolated routing setup and an exact `FullyQualifiedName`
filter for TC10, TC11 or TC12. Keep `AllowPostHppTestPayment=false`. The current iOS runtime
propagates `seatSelection.error` across the unavailable-state descendants; the tests deliberately
scope all three exact-copy assertions to that owned group. These tests are not in Sanity/Regression.

Validation: **511 mobile offline/retry checks passed**, **11 live scenarios correctly skipped**
with fault injection disabled; analyser clean. TRX:
`TestResults/seat-fault-offline-20261014-b/offline.trx`.
**27 Python checks passed**, including real local TLS seat-map/save interception with no upstream
sentinel connection. `scripts/verify.ps1` passed (build/ratchet, architecture, Core, formatting).
No live seat test or payment was run for this change; no iOS source was changed.

### Previous qualification and post-HPP evidence

BrowserStack six-case run: **5 passed, 1 failed**, 19m53s. TC01–05 passed; TC06 intercepted
the request but did not find the search error within 180 seconds. Appium logs also contain
socket-hang-up/proxy-command errors; screenshot/source capture failed. This is not yet an
established application defect. Build `5c1caf74c3b4f5a2508ebd434596c7c05f953ef0`;
TRX `TestResults/fault-browserstack-pack-20261014-a/browserstack.trx`.

TC06 then **passed locally** on the existing CA-provisioned disposable iPhone 17 Pro / iOS26.5
simulator: 3m14s run, 2m40s test body. The request was withheld at 12:16:46 UTC and the run
finished at 12:18:48 UTC with the exact error/ enabled-Search assertion passing. No
`hold-completed` event was recorded before teardown, consistent with the app timing out before
the proxy's 150-second fallback. This is one local pass, not proof of BrowserStack's root cause
or identical app binaries. TRX: `TestResults/fault-tc06-local-20261014-d/local.trx`; analyser clean.
Earlier local attempts on the normal, non-provisioned simulator failed at airport lookup;
starting capture after lookup instead produced normal results and no interception. Those are
setup/routing failures, not replicated timeout-UX failures. No normal-simulator/Mac trust was changed.

Expanded code validation: **509 offline/retry checks passed**, **nine live tests skipped** with
opt-ins disabled (`TestResults/post-hpp-fault-offline-20261014-b/offline.trx`, analyser clean).
Python: **20 passed**, including real TLS order-error/order-timeout interception with a local
upstream sentinel receiving no connection. These checks do not qualify TC07–09 live.
The final `scripts/verify.ps1` gate passed: solution build/warning ratchet, 10 architecture
checks, 89 Core checks and scoped formatting. The proxy stays active through Appium session
teardown, including failure-evidence collection.

TC07–09 remain **Draft** post-HPP scenarios. TC07 passed locally on 19 October against
iOS PR1331 (`0dd5d294`) in 2m49s; TC08/09 and BrowserStack qualification remain pending.

| ID | Method | Target | Assertion |
| --- | --- | --- | --- |
| TC07 | `SearchBookFault_TC07_PostHppPaymentStatusFailure_ShowsErrorWithoutConfirmation` | `SearchAndBookUpdatePaymentStatus` / `bookUpdatePaymentStatus` | Dedicated error page with exact title/contact message and enabled Return to flight search; no Retry or confirmation |
| TC08 | `SearchBookFault_TC08_PostHppBookingFailure_ShowsErrorWithoutConfirmation` | `SearchAndBookCreateOrder` / `bookCreateOrder` | Same state after HTTP-200 GraphQL basket-unavailable error |
| TC09 | `SearchBookFault_TC09_PostHppBookingTimeout_StopsLoadingWithoutConfirmation` | `SearchAndBookCreateOrder` / `bookCreateOrder` | Same state within 180 seconds after response withholding starts |

HPP is an external prerequisite, not the subject of these assertions. These tests reuse the
existing one-adult no-seats guest checkout and submit **one test HPP payment** to reach the
post-HPP native callbacks. They require the additional explicit setting
`FaultInjection.AllowPostHppTestPayment=true`; leave it false normally. No payment retry,
HPP callback injection, or iOS changes are made. TC08/09 let the normal payment-status update
run, but intercept order creation. TC07 also blocks any unexpected order creation after its
status-update failure. This is not financial reconciliation or proof that HPP did not charge.

The confirmation screen renders an already-produced `FlightBookingConfirmation`; it does not
retrieve confirmation from a separate endpoint. Consequently these cases do **not** simulate
an order successfully created upstream followed by lost confirmation data. That ambiguous-success
recovery/idempotency scenario remains a separate gap requiring an agreed safe test contract.

UI assertions updated for iOS PR1329 / develop `de727a5e` on 19 October.
The subsequent PR1331 accessibility fix exposes the Return button's own ID. TC07 verified
the injected fault, exact title/contact message, enabled Return action and absence of
Retry/confirmation. Evidence: project-local `TestResults/payment-error-pr1331-tc07-20261019/tc07.trx`.
All 539 offline/retry-policy checks passed in `TestResults/payment-error-pr1331-offline-20261019/offline.trx`.
They observe the Return to flight search action without clicking it; reset behaviour,
phone-link activation and visual/pixel matching require separate checks.

Original service baseline: local iOS `06568cc4`, `NativeFlightPaymentHandoffViewModel.handlePaymentEvent`,
`SearchAndBookFlightBookingServiceAdapter.confirmBooking`, and
`NativeFlightBookingConfirmationViewModel`. TC07's error envelope comes from EAPI
`UpdatePaymentStatusMutation`'s generic catch (`code=500`, existing PRI error message).
TC08 uses the shared basket-load `2004` / HTTP-exception GraphQL wrapper, not an invented
confirmation-specific code. TC09 is a transport fixture, not an EAPI error code.

The active completion proxy intercepts every matching attempt, not just the first. Expired,
malformed, batched, aliased or unexpected protected calls are blocked and recorded as
`guard-blocked`; that event fails qualification rather than falsely passing on its error message.
Other setup operations remain normal. Proxy death/routing loss is not qualified as safe for
post-payment runs: provision isolated fail-closed routing and qualify that failure mode before
enrolling these Draft scenarios in unattended runs. Do not run them on the strength of offline tests.

Evidence now distinguishes `withheld` (start) from `hold-completed` (150-second hold elapsed),
with UTC timestamps and no traffic payloads. The latter is not proof the client received a response.
Search error observation plus a start marker alone cannot prove that Apollo's 120-second deadline
was the cause. The proxy returns an empty 504 after its hold if the connection survives.
Local process capture must start before airport lookup establishes reusable network connections;
starting it only at Search submission can miss an already-open connection. Use the dedicated
CA-provisioned disposable simulator, not the normal simulator. Remote proxy routing starts before
session creation. A missing injection marker is always a failure, even if normal results appear.

The dated sections below retain the original POC history; this section supersedes their
unqualified claims that BrowserStack has never run or that all faults exclude payment.

## Expanded error coverage

Source review: EAPI checkout `73fd993da6`, iOS checkout `06568cc4`; this is not a claim to
have fetched latest remote develop. No iOS code is changed.

The original TC01 POC also passed BrowserStack on 14 October: **1 passed**, 2m31s lifecycle,
iPhone 17 / iOS 26.5. Session `57141c4d07e966d8bebdcb7cf06d0643e8fdf1c7`, build
`df9a4a3ce1350e758fed1bb07c01c7f1e554dd57`. Explicit routing approval was granted, superseding
the earlier blocked attempt below. Subsequent qualification is recorded at the top: TC01–05
passed BrowserStack; TC06 remains unqualified remotely. TC07 subsequently passed locally on
19 October; TC08/09 remain unqualified live. See the test catalogue for seat-fault results.

| ID | Injected condition | Expected current iOS behaviour | Limit |
| --- | --- | --- | --- |
| FAULT-TC01 | 3001 past date, Search Error / 422 / serialised `Reason` | Exact generic search error; Search enabled | No successful retry asserted |
| FAULT-TC02 | 3002 search unavailable, same envelope | Same safe error state | No backend availability claim |
| FAULT-TC03 | 101682 no flights (Delta 453), same envelope | Current generic search error | Dedicated no-availability UI remains a gap |
| FAULT-TC04 | 100600 supplier `unknownSession`, same envelope | Same safe error state | Not authentication token expiry |
| FAULT-TC05 | 2004 basket missing, Request failed / 500 / serialised `response` | Summary error; Continue enabled again; Passenger Details must not open | Not a uniquely identified expiry; no restart-search UX claim |
| FAULT-TC06 | Withhold search response for 150 seconds | Error and enabled Search within 180 seconds | Transport fault, not an EAPI response fixture |

Every scenario has its own concrete class/session and shares the non-parallel fault collection.
Normal packs remain unchanged. These six search/basket cases are part of the eleven-case opt-in
Draft pack. Each proves the matching
injection (or withheld-request event) separately from the UI assertion. Basket testing makes a real
search and fare selection, then intercepts only the read-only retrieveBasket query before forwarding.
It never submits passengers or invokes payment/order mutations.

### Fixture provenance

Historical six-case expansion validation (superseded by the latest totals at the top):
**507 mobile offline/retry checks passed**, **six live cases
skipped** with opt-in disabled, **13 Python checks passed**, including real TLS response/timeout
tests against a local upstream sentinel. Harness verification passed (build, warning ratchet,
architecture/Core checks and new-file formatting). Exact TRX analysis found no failed tests:
`TestResults/fault-pack-offline-20261014-b/mobile.trx`. No expanded live run or push is implied.

- EAPI `VAA.EAPI.GraphQL/VAA.EAPI.GraphQL/ErrorCodes/ErrorCodes.json`: 3002, 101682, 100600.
- EAPI `VAA.EAPI.GraphQL/VAA.EAPI.GraphQL.Domain.Search/Queries/OfferSearchBFFBase.cs`:
  `Search Error`, code 422, uppercase `Reason` containing serialised error details.
- EAPI Basket `Constants/ErrorCodes.cs`, `ErrorCodes/ErrorCodes.json`,
  `Application/Query/LoadBasketRequestHandler.cs`: missing cached basket maps to 2004.
- Infrastructure `Controllers/Extensions/ResultExtension.cs`: failed result maps to HTTP 500.
- GraphQL `ErrorFilter.cs`: HTTP exception becomes `Request failed.` with `code`, lowercase
  `reason` and serialised `response`. Synthetic developer messages exclude live identifiers.
- iOS `NativeFlightSearchEntryViewModel.swift` and `NativeFlightSummaryViewModel.swift`:
  errors map to localised generic messages; assertions use the existing owned error IDs.
- iOS `ApolloURLSessionFactory.swift`: request and resource timeout both 120 seconds.

The timeout adapter assigns a local response before its asynchronous 150-second hold, so it
does not forward upstream. Matching client retries are also withheld while the rule is active.
It records `withheld`, never claims an HTTP-200 response was delivered, and disposal cancels the
owned process. The query-only one-shot response mode remains unchanged; no global sleep is added.

### Outstanding scenarios — not silently counted as implemented

- **Empty successful result:** iOS `SearchAndBookResponseMapping.swift` rejects zero mapped
  offers as `invalidResponse` before its dedicated no-matching-offers branch. TC03 tests the
  EAPI no-availability error only; an appropriate empty-results UX needs product agreement.
- **B2C authentication expiry (G001/G064):** requires a leased logged-in account and agreement
  on reauthentication/navigation. TC04 is a supplier session, not a substitute.
- **Selected fare unavailable (3006/3010):** confirm the exact failing summary/offer operation
  and returned HTTP/GraphQL envelope before broadening the proxy allowlist. A search error
  injected at the wrong operation would not validate stale-fare handling.
- **Basket update missing (2005):** not the read-only basket case. Requires narrowly guarded
  passenger/T&C mutation interception; those mutations remain outside the proxy allowlist.
- **Booking/payment preparation failure:** requires fail-closed interception of the exact
  payment-preparation mutation, including retries, before any live qualification. That operation
  is not covered by TC07–09, which target post-HPP status update and order creation and have
  a separate payment opt-in. No payment-preparation failure case is implemented.
- **Basket expiry recovery:** current iOS displays a generic retrieval error and enables retry;
  it does not expose the agreed restart-search journey. TC05 does not claim that recovery.

These are explicit gaps, not passing or empty placeholder tests. Do not mark the complete
error-handling roadmap covered on the strength of this first expanded pack.

## Plan and boundaries

1. Core owns the optional process lifecycle, exclusive host-user lease, bounded fault expiry and safe evidence.
2. An external pinned mitmdump process owns TLS. A small Python adapter parses GraphQL and
   intercepts only the six reviewed operation/root pairs at the fixed SIT endpoint listed below.
   Query response faults are one-shot; guarded mutation faults intercept every matching attempt.
   Timeout mode is allowed only for search and order creation, including matching retries.
3. The mobile project owns the sanitised EAPI fixture and app-owned error-state assertion.
4. Validate rule isolation, query one-shot consumption, mutation retry guards, expiry and
   HTTP-200 GraphQL failure offline; unsupported protected requests block and fail qualification.
5. Qualify on a disposable local simulator after routing and certificate trust are configured.
6. Qualify BrowserStack Local separately. No parallel execution is required.

| GraphQL operation | Root field | Allowed fault mode |
| --- | --- | --- |
| `SearchAndBookOffers` | `searchOffers` | Response or timeout |
| `SearchAndBookRetrieveBasket` | `retrieveBasket` | Response |
| `SearchAndBookRetrieveSeatMap` | `retrieveSeatMap` | Response |
| `SearchAndBookUpdatePaymentStatus` | `bookUpdatePaymentStatus` | Guarded mutation response |
| `SearchAndBookCreateOrder` | `bookCreateOrder` | Guarded mutation response or timeout |
| `SearchAndBookSelectSeat` | `selectSeat` | Guarded mutation response |

These are implementation boundaries, not live qualification claims. TC07–09 require the
additional `AllowPostHppTestPayment` opt-in; only TC07 has local live qualification. Proxy death/routing
loss is not qualified as fail-safe, so isolated fail-closed routing must be qualified before
unattended post-payment execution. No unlisted mutation is a supported fault target.

No iOS edits, production routing, automatic payment retries, raw payload logs, certificate installation
or host proxy changes are performed by the Core helper. The caller must configure routing explicitly.
Normal packs do not start this process. An exclusive lock in the host user's temporary directory rejects another
fault proxy rather than waiting indefinitely; xUnit fault tests also use a non-parallel collection.

mitmproxy is MIT-licensed, with no licence fee. Preserve its licence/copyright notices when
redistributing it. graphql-core is also MIT-licensed. Dependencies are optional tooling, not new
Core NuGet references. The pinned dependencies must be maintained and reviewed for advisories.

BrowserStack's documented external MITM integration uses BrowserStack Local's `local-proxy-host`,
`local-proxy-port`, `force-local` and `force-proxy` options. Network logs routed through an external
proxy are not retained in the dashboard after the session, so tests must retain safe delivery evidence.
Do not equate proxy support with qualification of our TLS setup.

Sources:
- https://www.browserstack.com/docs/app-automate/appium/test-on-internal-network/test-behind-proxy/configure-settings
- https://www.browserstack.com/docs/app-automate/appium/test-apps-locally/charles-proxy
- https://github.com/mitmproxy/mitmproxy/blob/main/LICENSE

## Optional local tooling

From the Automation solution directory, create a disposable virtual environment outside source:

```sh
python3 -m venv /tmp/eapi-fault-proxy-venv
/tmp/eapi-fault-proxy-venv/bin/pip install -r tools/fault-proxy/requirements.txt
/tmp/eapi-fault-proxy-venv/bin/python -m unittest discover -s tools/fault-proxy -p 'test_*.py'
```

The Python adapter does not persist traffic. Only ready/injected metadata is written. Each Core
session creates a unique private temporary directory containing its rule and, by default, temporary CA material.
Disposal stops only its owned process and deletes that directory. It does NOT remove certificates
installed in a simulator: use a disposable simulator and remove that simulator after qualification.
Never install this CA into the macOS system keychain. Do not enable unredacted flow dumps.

The first fixture follows `OfferSearchBFFBase.OfferSearchAsync`'s GraphQL error wrapper:
message `Search Error`, extensions `code=422` and `Reason` containing serialised ErrorDetails.
Its synthetic expired-date error uses code `3001`; no live identifiers or request bodies are copied.
This checks controlled error handling, not backend availability or a successful recovery journey.
TC30 recovery remains deferred until the actual error UI retry contract is qualified.

## App test opt-in (local POC qualified once)

`SearchBookFault_TC01_ControlledSearchError_ShowsErrorAndAllowsSearch` uses a dedicated
non-parallel collection and a discovery-time opt-in. Normal release packs remain unchanged.
The test proves proxy consumption separately from the UI assertion; an unrelated backend error
cannot make it pass. It stops on the search panel and never creates a booking.

Add this block to the gitignored mobile `local.settings.local.json` only after an isolated
simulator has been provisioned. Keep both switches false until routing and trust are verified:

```json
"FaultInjection": {
  "Enabled": false,
  "IsolatedRoutingConfirmed": false,
  "UseProcessCapture": false,
  "Executable": "/tmp/eapi-fault-proxy-venv/bin/mitmdump",
  "AddonPath": "/absolute/path/to/VAA.EAPI.Automation/tools/fault-proxy/addon.py",
  "CertificateDirectory": "/absolute/path/to/isolated-proxy-ca"
}
```

The early gate loads the same base/local/environment configuration sources as the fixture.
It validates the effective profile using the launch resolver: `IOS_BUILD_PROFILE` takes
precedence over `IOSLaunch:BuildProfile`. An absent, empty or non-SIT profile fails closed.

Requires `RunMode=Local`, `Platform=iOS`, an effective `sit_standard` profile and a disposable
simulator configured to use port 8899. The caller-provided CA directory must contain its own
`mitmproxy-ca.pem`; Core preserves this directory so simulator trust can be prepared before the
test starts. Protect its private key and remove the disposable simulator's trust before deleting
the CA. Do not reuse a production/corporate interception CA.

Alternatively on macOS, `UseProcessCapture=true` uses the separately installed and user-approved
mitmproxy Network Extension. The mobile adapter obtains the foreground app's PID through Appium
and requires the test bundle ID before passing that positive PID to Core. It never passes an
unrestricted capture mode or an app-name wildcard. Use a disposable simulator with the isolated
CA trusted there; do not change the host proxy or Mac trust. A process restart invalidates the
capture target and must fail the test rather than broaden interception automatically.

Select the explicitly approved case by exact `FullyQualifiedName`, with a run-scoped TRX and
the standard analyser. `Suite=FaultInjection` selects the whole enabled pack; do not enable
the post-HPP payment opt-in for a search/seat-only qualification run.
The proxy uses lazy upstream connections: a matching synthetic response needs no backend call.
Active mutation guards classify requests across the entire configured host before exact-path
matching. Protected mutations on alternate paths are blocked, never injected or forwarded.
Malformed, batched, oversized and non-POST requests on that host fail closed while the mutation
guard is active; parsed unprotected setup operations on alternate paths can pass before expiry.
Other hosts are unaffected. This is deliberately conservative and may reject unsupported setup
requests rather than risk submitting an order.

Ordinary non-target requests pass through unchanged; requests violating an active mutation guard
are blocked. Query response mode permits one injection; mutation response mode intercepts repeats.
The rule lifetime is ten minutes. Query rules cease injecting on expiry; guarded mutations block
protected-endpoint traffic on expiry rather than forwarding it. Timeout mode withholds matching
client retries within the active lifetime. No blind UI retry or payment retry is added.
Disposal stops the owned proxy process.

Outstanding qualification: repeated-run stability, additional error types and Android.
BrowserStack execution is opt-in through a separately owned Local tunnel; see the remote POC below.

## BrowserStack Local POC

Obtain explicit approval for routing this app's SIT traffic through BrowserStack and the local
proxy before running. Session/request traffic may contain tokens even though the adapter does
not retain payloads. The initial 14 October tunnel preflight succeeded but its first app attempt
was blocked awaiting approval. That historical status was superseded by explicit approval and
the TC01–05 BrowserStack passes recorded above. TC06 remains unqualified remotely; TC07's
19 October pass is local only. Tunnel connectivity alone is not qualification; see the test
catalogue for individual scenario results.

The caller starts and stops a **dedicated**, uniquely named BrowserStack Local tunnel. Do not use
`--force`, which could stop somebody else's tunnel. Use the official binary and a private YAML
configuration file (mode 0600 in a mode-0700 folder) so its access key is not exposed in process
arguments. Do not commit that file or upload its logs. Required tunnel settings:

```yaml
key: <your BrowserStack access key>
local-identifier: <unique fault-run identifier>
only-automate: true
disable-dashboard: true
local-proxy-host: 127.0.0.1
local-proxy-port: 8899
force-local: true
force-proxy: true
use-ca-certificate: /absolute/path/to/isolated-proxy-ca/mitmproxy-ca-cert.pem
verbose: 0
log-file: /absolute/private/path/tunnel.log
```

Run `BrowserStackLocal --config-file /absolute/private/path/tunnel.yml` and wait for its
successful connection message. The CA option scopes trust to the tunnel, not the Mac keychain.
Then configure the gitignored mobile settings: `RunMode=Remote`, `BrowserStack.Platform=iOS`,
`BrowserStack.Local=true`, matching `BrowserStack.LocalIdentifier`, `NetworkLogs=true`,
`CaptureNetworkContent=false`, and `FaultInjection.UseProcessCapture=false` alongside both
explicit fault opt-ins. Keep `IOSLaunch.BuildProfile=sit_standard` and the reviewed proxy paths.

For an initial connection check, select the exact FAULT-TC01 test; select other cases only within
their explicit run approval and qualification constraints. Each starts the proxy **before** the remote Appium session,
retains safe injection evidence in TRX, quits the session and disposes the proxy. The caller must
stop its tunnel, delete its private configuration/logs, and restore normal settings in `finally`,
including after failure. Do not leave Local enabled for normal regression. No simulator Network
Extension is needed remotely. A connected tunnel alone does not prove app TLS or injection works.

Reference: https://www.browserstack.com/docs/local-testing/local-with-SSL-Inspection

## Qualification evidence — 14 October 2026

- Python: **8 passed**, including a real TLS proxy process. Its upstream was forced to a local
  sentinel, which received no connection while the client received HTTP 200 with GraphQL errors.
  No EAPI call or simulator interception was involved.
- Core: **87 passed**, including new argument validation and process-exit/lease-release checks.
  TRX: `TestResults/fault-proxy-core-20261014/core.trx`.
- Mobile offline/retry: **491 passed**; the opt-in live scenario was **skipped** as expected.
  TRX: `TestResults/fault-proxy-offline-20261014-final/mobile.trx`.
- Exact-run analysers reported no failed tests. `scripts/verify.ps1` passed the solution build,
  warning ratchet, **10 architecture checks**, Core checks and new-file formatting.

These are infrastructure qualifications, not proof of iOS error rendering or BrowserStack support.
No iOS source, host proxy configuration or certificate trust was changed. Dependencies were installed
only into `/tmp/eapi-fault-proxy-venv`. If a runner is forcibly terminated, normal disposal cannot be
guaranteed; runner cleanup must stop its owned proxy process and remove its isolated temporary assets.

## Live local qualification — 14 October 2026

`SearchBookFault_TC01_ControlledSearchError_ShowsErrorAndAllowsSearch` **passed** on the disposable
iPhone 17 Pro / iOS 26.5 simulator: 1m17s including lifecycle (test body 44.66s).
The TRX explicitly records one injected `SearchAndBookOffers` HTTP-200 GraphQL error and a passing
app-owned error-state / enabled-Search assertion. The exact-run analyser found no failed tests.
Evidence: `TestResults/fault-local-poc-20261014-a/local.trx`.

The macOS redirector required administrator installation and separate user approval of its Network
Extension. Capture used only the foreground test app's PID. The temporary CA was trusted only on
the disposable simulator. Proxy process/session files were cleaned up; the disposable simulator
was shut down, normal simulator settings restored, and fault opt-in switched off afterwards.
The redirector remains installed for later use, and the isolated simulator/CA are retained for
repeat qualification. Mac certificate trust and system proxy settings were not changed.

Process-capture hardening: 491 mobile offline/retry checks and 89 Core checks passed; exact-run
analysers found no failures. Harness build, architecture checks and formatting passed.
This is proof of one controlled search failure, not recovery coverage or fault coverage of payment.
