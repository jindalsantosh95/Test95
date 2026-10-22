# Mobile Automation Implementation Benefits

## Purpose

This living document tracks the measurable and practical benefits of implementing native
mobile end-to-end automation in `VAA.EAPI.Automation`. Update it when the harness gains a
reusable capability, removes an interim workaround, or provides new execution evidence.

## Current implementation

The native iOS Search and Book tests use direct xUnit tests, Appium page objects, shared
handlers, and BrowserStack. They sit alongside the existing API automation while remaining
separated from backend domain test projects.

## Benefits delivered

| Area | Implementation | Benefit |
|---|---|---|
| Repository alignment | Mobile tests live in the existing .NET automation solution | Shared engineering standards, reporting, configuration, and test-data capabilities can be reused without maintaining a separate TypeScript/Cucumber harness. |
| API-aware orchestration | Mobile journeys can consume the same EAPI capabilities used to support Book, Trips, OCI and related domains | Tests can arrange eligible data through APIs, exercise the customer-critical UI, and verify backend and UI outcomes without maintaining duplicate integrations in a separate repository. |
| Test readability | xUnit journey tests contain business actions and assertions | A test describes the journey without repeating startup or selector mechanics. |
| Startup reuse | `AppEntryHandler` orchestrates onboarding, guest entry, and privacy dismissal | Every journey receives the same startup behaviour, and changes are made once. |
| Responsibility boundaries | `OnboardingPage`, `WelcomePage`, and `PrivacyBannerPage` own their respective screens | Failures identify the screen or responsibility that broke, reducing diagnosis time. |
| Synchronisation | UI transitions use explicit condition-based waits | Execution responds as soon as the required state exists and avoids fixed-delay flakiness and wasted time. |
| Native selectors | Stable accessibility IDs are the primary locator contract | Selectors are faster, clearer, and less sensitive to visual redesign than coordinates or structural XPath. |
| Native selector enforcement | `NativeAppPage` resolves mobile keys by accessibility ID only | Prevents generic XPath/text fallback from masking a missing app contract or adding repeated native-tree scans to a run. |
| iOS carousel handling | Duplicate elements are filtered to the displayed, enabled element in the active viewport | The handler remains accessibility-driven without device-specific coordinate taps. |
| Privacy handling | `Reject All` is preferred, with `I Agree` as the established fallback | Behaviour remains consistent with the existing mobile suite while using deterministic waits. |
| Cross-platform authoring | BrowserStack execution is available from Windows and macOS | Test development is not blocked on local access to Xcode or an iOS simulator. |
| Session integrity | Driver startup requires a real Appium session ID and non-empty page source | False-positive connection tests are prevented and configuration failures surface early. |
| BrowserStack organisation | Project, build, and descriptive session names are supplied as capabilities | Runs are easier to locate, group, and diagnose in BrowserStack. |
| Harness guardrails | Locator, wait, coordinate, and temporary-fallback rules are documented | New tests and Codex-assisted changes follow the same maintainable conventions. |
| Feature-flag orchestration | Native Search and Book and skip-seat-selection flags are passed through validated iOS launch arguments before Appium starts the app | Test builds enter the intended path deterministically without Debug-menu navigation or a mid-test restart. |
| Search-calendar targeting | Departure and return dates use app-owned ISO day IDs, calendar-container-scoped scrolling and immediate summary verification | Remains deterministic across device locales and long date ranges, and cannot silently continue after Appium taps the wrong date. |
| Passenger data integrity | Each field dismisses the iOS keyboard and verifies its retained value before scrolling | Prevents subsequent gestures being entered as text and fails at the field responsible rather than later in checkout. |
| Shared payment components | The mobile HPP adapter composes the existing .NET `HppForm` | Payment-field knowledge remains centralized once the embedded iOS WebView is exposed to Appium. |
| Execution profiles | Focused UI-rule checks use Fast; complete booking/payment journeys use Diagnostic with video | Iteration avoids unnecessary capture overhead while E2E failures retain evidence across native, HPP, confirmation and Trips transitions. |
| Loyalty test data | Typed criteria adapt Core's existing Key Vault and `LoyaltyAccounts` storage helper | Tests request environment, tier and point balance directly without duplicating Azure access in the mobile framework. |
| Parallel account safety | An eligible account is leased for the scenario lifetime and released during cleanup | Parallel tests in one runner cannot select the same mutable member concurrently. |

## Evidence

- Payment handoff failures are now classified before HPP interaction. If the app exposes
  `Try secure payment again`, the test reports payment-reference creation as the failure
  and directs investigation to that BrowserStack session's GraphQL logs.
- BrowserStack network/content capture can be explicitly enabled for a targeted diagnostic
  session because it cannot be enabled after a payment failure; committed defaults keep content
  capture off. Only the original failing session is queried and only
  sanitised status/operation/code/correlation metadata is written to the TRX; the scenario and
  payment are not retried and raw bodies are never printed.

### BrowserStack connectivity

- App: `bs://af9484beef55c8bc672d751d557e9fdf40253f8a`
- Project: `Virgin Atlantic Mobile - Search and Book`
- Build: `Search-and-Book-iOS`
- Confirmed real Appium sessions are created and visible in BrowserStack.

### Reusable app-entry flow

The launch smoke passed on BrowserStack after completing:

1. `Get started`
2. `Continue`
3. `Maybe later`
4. `Continue without sign in`
5. Privacy-banner dismissal when present
6. Verification that the `Book` tab is displayed

Passing session evidence:

- Session ID: `0ffbe313c15353e2000f6ee13fb1b86b3dd9df3c`
- Result: Passed
- Run duration: approximately 1 minute 7 seconds, including BrowserStack device startup

### Search-calendar optimization

- BrowserStack session: `bb03aab6fec00543a18387b4b7be01afbdfe7b20`
- The journey completed search, passenger/contact/T&Cs, seat bypass and payment handoff.
- Comparable user-flow time to the known HPP WebView boundary improved from approximately
  3 minutes 22 seconds to 2 minutes 38 seconds: about 44 seconds faster.
- The remaining failure is HPP WebView context exposure and is independent of date selection.

### Search-calendar reliability decision

The previous WebdriverIO implementation remains useful as a behavioural reference, but its
calendar helper is not copied as a framework primitive. It treats an iOS element reported as
displayed as sufficient, uses screen-level swipes and estimates a fixed number of scrolls.
SwiftUI can materialise calendar children outside the tappable viewport, and BrowserStack
devices can expose human-readable date labels in different locales.

The EAPI implementation therefore:

1. Locates the real calendar `ScrollView` by its app-owned accessibility ID.
2. Scrolls only that container, in bounded increments calculated from the target month.
3. Locates the day by its locale-independent ISO accessibility ID.
4. Requires the day to be inside the safe viewport above the sticky calendar footer.
5. Reads the app-owned departure or return summary immediately after each click and fails if
   it does not contain the requested date.

BrowserStack session `6c5513d69084d2b03b8e2784151a4cbfa1b758ab` demonstrated this by
selecting and verifying May 2027 before the scenario progressed to flight results. The later
failure was an independently missing Edit Search ID in the uploaded app version.

### Loyalty login implementation evidence

On 28 August 2026, the Red, Silver and Gold SIT login scenarios all passed against the local
iOS simulator in separate Appium sessions. The complete three-test run took approximately
1 minute 27 seconds. Each scenario selected an eligible account from the shared EAPI
`LoyaltyAccounts` table, validated its tier and minimum points before UI interaction, logged in,
verified member-only Account state, and signed out during cleanup.

The draft tier scenarios now extend that proven login path through the same native Search and
Book checkout used by guest E2E coverage. They assert the selected LHR-JFK route, Virgin Atlantic
carrier and cabin fare before checkout, then the one-adult passenger summary, non-empty total,
six-character booking reference and presence in Trips after HPP payment. This deliberately
reuses `NativeFlightSearchPage` and `NativeBookingCheckoutFlow`, so authenticated and guest
journeys cannot drift into separate selector or checkout implementations. Live completion
remains draft while the known HPP payment-reference failure is unresolved; that state is exposed
as an immediate diagnostic failure rather than a generic element timeout.

Local execution also proved two authenticated-state requirements that a fresh BrowserStack app
does not normally expose. Account entry now waits for a settled member, guest, landing or login
state and logs out a pre-existing local member before reserving the requested tier. At passenger
details, authenticated checkout validates and preserves the app-prepopulated member rather than
overwriting it with the generated guest traveller/contact fixture.

The WDIO implementation remains the behavioural source for known login selectors and transient
error states. The .NET implementation deliberately improves the reusable test contract in these
areas:

| Concern | WDIO behaviour observed | EAPI implementation |
|---|---|---|
| Account selection | Retrieves a general account list and selects accounts in test support code | A typed `LoyaltyAccountCriteria` requests environment, tier, inclusive minimum, optional exclusive maximum, exact points and child/adult status through the existing Core helper. |
| Selection correctness | Relies primarily on the storage query result | Rejects a returned account before login if its tier or points fall outside the requested criteria. |
| Parallel mutation | No account lease is applied by the login page | Process-local leases prevent concurrent scenarios in one runner from using the same Flying Club account. |
| Login synchronisation | Login and post-login helpers retain several fixed `pause` calls | Polls explicit member, invalid-credential and known transient-error outcomes; no fixed UI sleeps are used. |
| Retry policy | Retries login up to three times and handles generic errors | Retries only the known transient app error; invalid credentials fail immediately and are not hidden by retry. |
| Member detection | Uses greeting and several account-screen candidates | Requires member-only points, membership or activity controls because live inspection proved the guest heading also uses accessibility ID `greeting`. |
| Reused local app state | Logout-first recovery is distributed across account readiness/settings helpers | The login page detects a genuine existing member, signs out through accessibility IDs, performs the requested login, then signs out in scenario cleanup. |
| Sensitive output | Some WDIO diagnostics log the email or Flying Club identifier | xUnit output records tier, points and only the final four masked Flying Club digits; password and complete login identifier are never logged. |
| Local diagnostics | BrowserStack-oriented output can be mixed with local execution | Local failures report the Appium server, platform, device and session without attempting BrowserStack executor commands. |

These are implementation improvements evidenced by the inspected WDIO flow and the passing local
run. They do not imply broader feature-coverage parity; WDIO remains the reference until each
area has equivalent .NET scenarios and execution evidence.

## EAPI harness advantage and delivery speed

Moving the tests does not make every individual Appium action faster. BrowserStack device
allocation, app startup, backend response time, and the native UI still determine execution
time. The delivery gain comes from reducing the engineering work around each scenario and
making failures quicker to classify.

### Capabilities already available in EAPI

| Existing EAPI capability | How mobile automation uses or can use it | Delivery effect |
|---|---|---|
| Shared `Core` and `Core.UI` projects | Driver lifecycle, configuration, wait primitives, screenshots, BrowserStack status and cross-cutting helpers | New scenarios start from established infrastructure instead of recreating a harness. |
| Direct xUnit execution | Tests use existing traits, filters, assertions and IDE/CLI support | A scenario can be added as a normal .NET test without feature bindings or generated glue code. |
| Central package management | Appium, Selenium and supporting package versions are controlled in one place | Dependency upgrades and compatibility fixes are applied consistently. |
| Environment-based configuration | Credentials, app IDs, devices and session names are supplied at runtime | Local and pipeline execution use the same test code without committed secrets. |
| TRX output and result-analysis workflow | Mobile runs can produce the same standard result artifacts and failure classifications as other suites | Reporting and investigation do not require a separate toolchain. |
| Trait/category filtering | Launch, smoke, journey and future regression packs can be selected independently | Developers run the smallest relevant pack during iteration and reserve broader packs for pipelines. |
| Existing Book API knowledge and tests | Front-end scenarios can be designed against known basket, passenger, contact, terms, seat and payment behaviour | Less discovery is required to define expected outcomes and diagnose whether a failure belongs to UI or backend. |
| Existing Trips and OCI API capabilities | Mobile journeys can use the same PNR, trip, passenger and check-in domain capabilities already held in EAPI | Avoids rebuilding TypeScript API wrappers and enables deterministic journey setup and post-UI verification across additional mobile domains. |
| Test-data and storage abstractions | Suitable shared data capabilities can be promoted into `Core` when genuinely reusable | Mobile setup can become deterministic without coupling the UI project directly to backend test projects. |
| Existing solution guardrails and documentation | Codex and contributors follow one set of layering, selector, wait and validation rules | Reviews focus on journey behaviour rather than repeatedly correcting harness patterns. |

### Broader API-aware mobile opportunity

The architectural benefit is not limited to Search and Book. EAPI already contains API
capabilities used by the established mobile repository for areas such as Trips and OCI.
The target pattern for those domains is:

1. Arrange an eligible account, PNR, trip, passenger or basket through an existing EAPI
   capability.
2. Exercise only the customer-critical behaviour through the mobile app.
3. Verify the resulting backend state through the API harness.
4. Verify the customer-visible outcome in the app where that adds distinct coverage.

This should shorten remote-device journeys, reduce consumption of BrowserStack minutes,
and classify failures more precisely. For example, a missing trip can be distinguished as
an API/data problem or a mobile-rendering problem rather than reported only as an element
timeout. Shared domain capabilities should be exposed through clear reusable services;
the mobile project must not take direct dependencies on unrelated test projects or
duplicate their request logic.

### Where delivery should become faster

The first scenario carries setup cost because BrowserStack connectivity, Appium versions,
startup handling, selector rules, and reporting must be established. That work is now
reusable. Subsequent Search and Book scenarios should require mainly:

1. A business scenario and its expected outcome.
2. Any new screen-level page-object action or assertion.
3. Stable accessibility IDs from the app team.
4. Scenario-specific data and validation.

They should not need to rebuild BrowserStack capabilities, onboarding, guest entry,
privacy handling, feature-flag setup, driver validation, or result integration.

The largest expected gain is therefore in scenarios two onward and in maintenance after an
app startup behaviour changes. A single shared-handler change can repair every dependent
journey. This expectation must be verified using the measures below; it should not be
reported as a quantified time saving until scenario delivery data has been collected.

### Android reuse strategy

The Search panel now has five shared scenarios with iOS and Android page implementations.
This does not make the wider booking suite Android-compatible. Reuse is at the business
and orchestration layers rather than forcing both platforms' controls into one implementation.

Reusable across platforms:

- xUnit scenario intent, traits and assertions.
- Search, passenger, payment and confirmation models.
- `NativeBookingCheckoutFlow` business orchestration where its page dependencies expose
  the same customer actions.
- BrowserStack configuration, session lifecycle, reporting and diagnostics.
- API-based test-data arrangement and backend verification.
- Expected Search and Book, confirmation and Trips business outcomes.

Currently iOS-specific:

- `ByIosNSPredicate`, iOS class-chain locators and XCUITest element types.
- Keyboard dismissal, scrolling and calendar behaviour.
- iOS launch arguments and app-owned feature-flag names.
- Any accessibility identifier that is not implemented consistently on Android.

The Search panel uses `IFlightSearchPanelPage`, selected by
`FlightSearchPanelPageFactory` from the configured session's platform. The iOS adapter
reuses established `NativeFlightSearchPage` mechanics; the Android adapter uses resource IDs.
Search criteria, journey switching, date editing, passengers and recent airports share
scenario logic. Promo clearing and modal dismissal remain Android-specific where behaviour
differs. See [shared Search panel implementation](MOBILE_ANDROID_SEARCH_PANEL.md).

The preferred app contract is matching logical identifier names and customer-visible semantics:
iOS accessibility IDs and Android resource IDs exposed from test tags. Android screen-reader
descriptions remain meaningful accessibility content, not automation identifiers.
Where that contract is honoured, page actions may be shared; retain
platform-specific code only for controls whose Appium behaviour genuinely differs. Android
support is complete only after the journeys have passed on an Android device or BrowserStack
session—it must not be inferred from compilation or shared code alone.

### WDIO comparison: shared Search panel (23 September 2026)

This comparison inspected the local `vaa-mobile-automation` checkout at `e5a1f54`
(4 September 2026); it is not a freshly fetched remote audit or a claim about all WDIO areas.

| Concern | WDIO evidence inspected | EAPI implementation |
| --- | --- | --- |
| Scenario reuse | Cucumber steps call reusable page objects | Shared xUnit scenarios/flows call a common page interface |
| Platform selectors | JSON UI maps provide iOS/Android entries | Separate platform pages own IDs and interaction mechanics |
| Native Search panel Android readiness | `tests/support/ui-map/search-book/native-flight-search.json` has empty Android selectors; `tests/features/search-book/tc05-search-entry-rules.feature` is tagged `@ios` | Five shared scenarios have adapters for both platforms; live validation remains pending |
| Responsibility boundary | `tests/pages/search-book/native-flight-search.page.ts` owns reusable interactions | Tests/flows express intent; the factory selects an adapter without platform branches in the scenario |

Cross-platform reuse is not unique to .NET; WDIO can adopt the same separation. The EAPI
benefit is reuse of existing core/data capabilities and backend-test knowledge in the same
solution, reducing the need for equivalent implementations in separate repositories.
WDIO also has data services and harness guidance; do not describe it as lacking those capabilities.

The consolidation passed its offline checks, not a live cross-platform qualification.
It does not demonstrate faster execution or lower flakiness. Measure equivalent scenarios
on matched builds/devices/data, recording authoring effort, duration, failures and maintenance
before claiming a comparative improvement. Recheck the WDIO revision when updating this comparison.

### What will still constrain delivery

- Missing or unstable accessibility IDs will force slower temporary locator work.
- BrowserStack queueing and remote feedback loops remain slower than a local iOS simulator.
- Unstable test data or environments can obscure whether a failure is in the app or test.
- Large end-to-end scenarios are slower and more expensive than focused checks, regardless
  of framework language.
- Android will reuse orchestration and harness infrastructure, but platform-specific page
  behaviour and selectors still require implementation and validation.

### EAPI harness versus the existing mobile harness

| Concern | Existing mobile harness | EAPI mobile implementation | Expected delivery impact |
|---|---|---|---|
| Scenario definition | Cucumber feature, step binding and page/helper layers | Direct xUnit journey method plus page objects/handlers | Less glue for this team where Cucumber is not a requirement. |
| Existing organisational context | Separate repository and toolchain | Adjacent to Book API tests and shared .NET conventions | Faster discovery and diagnosis across frontend/backend boundaries. |
| Startup flows | Multiple app-state/page helpers | One composed `AppEntryHandler` | One implementation and one repair point for all journeys. |
| Synchronisation | Some fixed pauses and fallbacks remain | Explicit observable-state waits | Less idle execution and reduced timing-related maintenance. |
| Native interaction | Accessibility selectors plus some coordinate fallback | Accessibility-first; viewport-aware; coordinates prohibited by default | Better resilience across supported device sizes and UI changes. |
| Execution output | WebdriverIO/Cucumber reporting | Standard xUnit/TRX plus BrowserStack metadata | Reuses existing CI and test-result analysis practices. |
| Backend correlation | Requires cross-repository investigation | Can share appropriately abstracted diagnostics and known Book contracts | Shorter ownership triage when GQL returns HTTP 200 with business errors. |
| Cross-domain API reuse | Maintains mobile-specific TypeScript helpers for Book, Trips, OCI and test data | Can consume shared EAPI domain clients, models and data services | Less duplicated integration code and faster setup of API-dependent mobile scenarios. |
| Loyalty accounts | Mobile-specific Azure account service and shared test password | Typed adapter over EAPI Core's Key Vault/storage capability with tier/points validation and account leasing | One account source for backend and app tests, with safer parallel use and less duplicated integration code. |
| Tooling model | Additional semantic or live-app services are not required by the harness | Codex and contributors use the solution, compiler, Appium and BrowserStack capabilities directly, keeping setup and maintenance small. |

The existing framework remains a valuable behavioural reference and currently has broader
mobile coverage. The EAPI benefit is consolidation and simpler extension for this delivery
team, not an assumption that rewriting mature coverage is automatically cheaper.

### Lean, native-first tooling rule

The EAPI mobile harness remains native .NET and intentionally avoids additional authoring
or orchestration layers. Codex and contributors should first use capabilities already
available through the repository and standard toolchain:

- C# symbols, project boundaries and compiler references for discovery and safe reuse.
- Targeted repository search and concise harness documentation for Codex context.
- Appium and BrowserStack logs, source, screenshots and network capture for runtime evidence.
- Existing EAPI architecture, harness and PR verification for change safety.
- Focused mobile checks only where an observed recurring defect justifies their cost.

Serena, WebdriverIO MCP or another auxiliary service is not a planned dependency. Such a
tool should be considered only when a specific problem cannot be solved adequately through
the native toolchain, and only after its setup, maintenance and contributor cost has been
weighed against measurable benefit. The default answer to optional tooling is therefore
not to add it.

Guardrails must remain proportionate. Add a rule when it prevents a demonstrated class of
failure; do not reproduce checks that exist only to support Cucumber, TypeScript routing or
JSON UI-map conventions. The objective is a small, predictable path from xUnit test to
flow/page object to Appium—not a second framework around the framework.

## Comparison with the previous mobile implementation

The .NET implementation deliberately retains useful behaviour from the existing mobile
framework while simplifying its execution model.

| Previous pattern | Current pattern |
|---|---|
| Startup behaviour spread across page objects and app-state helpers | One `AppEntryHandler` composes focused page objects |
| Fixed pauses around onboarding and privacy transitions | Explicit waits for the next required UI state |
| Coordinate fallbacks for iOS carousel actions | Accessibility-ID lookup filtered to the active viewport |
| Separate TypeScript/Cucumber framework | Direct xUnit tests within the established .NET automation solution |
| A session could appear to initialise without proving a remote session existed | Fixture rejects blank session IDs or page source |

This is not intended to criticise the previous framework. It remains the behavioural
reference for known app states and selector names while the new implementation removes
workarounds that are no longer necessary.

## Measures to track as coverage grows

Record these values periodically rather than relying only on subjective improvement:

- Number of automated iOS Search and Book scenarios.
- Median and 95th-percentile scenario duration, excluding and including device allocation.
- First-run pass rate and rerun pass rate.
- Failures by app defect, environment, test data, selector contract, and automation defect.
- Number of fixed sleeps and coordinate interactions; the target is zero.
- Percentage of interactive controls located by stable accessibility ID.
- Average time to diagnose a failed BrowserStack session.
- Reuse count for shared handlers and journey components.
- Parallel capacity required for smoke and regression execution.
- Engineering time from scenario definition to first passing BrowserStack run.
- Proportion of a new scenario implemented using existing handlers/page-object actions.
- Median time to update all affected scenarios after a shared app-state change.

## Update log

| Date | Change | Evidence or impact |
|---|---|---|
| 2026-08-12 | Added BrowserStack Appium connection with named project/build/session capabilities | Real sessions are visible under the intended BrowserStack build. |
| 2026-08-12 | Ported onboarding into a reusable accessibility-driven handler | Removed fixed sleeps and coordinate taps from the new implementation. |
| 2026-08-12 | Added reusable guest privacy-banner handling | Launch smoke passed through to the native `Book` tab. |
| 2026-08-12 | Documented EAPI harness reuse and expected delivery impact | Establishes what should become faster and the metrics required to prove it. |
| 2026-08-12 | Aligned the full native selector contract to iOS PR 1028 commit `f714595e` | Removed label/UUID calendar and fare fallbacks, extended the two-adult return test through both slices, and added reusable passenger, seat, payment and confirmation page objects. |
| 2026-08-12 | Added native-entry and skip-seat feature-flag orchestration | Both flags are enabled together and require only one app restart. |
| 2026-08-12 | Replaced search-calendar fallback/XPath scanning with direct month and day IDs | Targets the observed near-one-minute departure/return date-selection bottleneck. |
| 2026-08-12 | Measured the direct-ID calendar change on BrowserStack | Comparable execution to HPP improved by approximately 44 seconds (3m22s to 2m38s). |
| 2026-08-12 | Added verified passenger/contact entry with iOS Return-key handling | BrowserStack reached HPP with passenger, contact and T&Cs completed and seat selection bypassed. |
| 2026-08-12 | Added Fast and Diagnostic BrowserStack execution profiles | Diagnostic capture can be disabled explicitly during rapid iteration without changing test code. |
| 2026-08-17 | Defined the BrowserStack profile policy by test scope | Focused Search/calendar/PAX checks use Fast; booking E2E uses Diagnostic so payment and confirmation failures retain video evidence. |
| 2026-09-02 | Captured sanitised backend evidence from the original failing BrowserStack session | Network/content capture starts with the session, but logs are fetched and safe metadata is printed only on failure; payment and tests are never retried. |
| 2026-09-02 | Added a dynamic insufficient-points reward boundary | The scenario leases a low-balance account, compares that live balance with the displayed outbound/inbound Upper Class points and asserts the owned warning plus disabled continuation instead of relying on a fixed fare. |
| 2026-08-12 | Connected the existing .NET HPP form through an Appium context adapter | Current evidence identifies WebView inspection/exposure as the remaining payment boundary. |
| 2026-08-14 | Documented reuse of EAPI capabilities beyond Book, including Trips and OCI | Establishes API-arrange, mobile-execute and API/UI-verify as the target cross-domain pattern. |
| 2026-08-14 | Established a lean, native-first tooling rule | Serena, WebdriverIO MCP and other auxiliary services are excluded by default and considered only for a proven gap the native .NET/Appium toolchain cannot meet. |
| 2026-08-14 | Defined the Android reuse strategy | Business scenarios, flows, models and harness services are reusable; platform-specific page contracts will be introduced only when Android implementation begins. |
