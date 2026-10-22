# Book Core Regression and Playwright HPP Migration

## Summary

The scheduled Book GraphQL suite has been expanded and renamed from a smoke suite to a **core regression**. The work also introduces a controlled migration of direct Hosted Payment Page (HPP) automation from Selenium to Playwright.

The migration is scenario-driven. Playwright can be enabled for an individual scenario while Selenium remains available as the fallback for journeys that have not yet been migrated or verified.

## Why This Change Was Needed

The previous scheduled Book run selected only 14 scenarios and was described as a smoke run, despite being used as the main recurring regression across core booking journeys. Important journey types were either absent from the scheduled run or depended on Selenium HPP automation that had shown intermittent Chrome and redirect failures.

The main objectives were to:

- make the scheduled suite accurately represent a core regression;
- close important coverage gaps without adding Wamos scenarios to the daily run;
- improve reliability around direct HPP interaction;
- reuse the Playwright capability already present in the solution;
- preserve the access headers required when opening the direct HPP URL;
- allow gradual migration with a safe Selenium fallback.

## Changes Delivered

### Core regression coverage

- Added the `Book-Core-Regression` trait.
- Updated the scheduled workflow, test filter, result names and documentation to use core-regression terminology.
- Increased scheduled Book coverage from 14 to 21 representative scenarios.
- Added coverage across PayPal, 3DS, logged-in booking, mixed-carrier, seat clear-down, redemption, Pay With Points and multi-city journeys.
- Kept Wamos-operated-leg scenarios available for on-demand functional execution rather than adding them to the daily regression.

### Direct HPP Playwright support

- Added a direct-HPP Playwright page object supporting standard card payments, PayPal and 3DS.
- Added scenario-level `usePlaywrightForHpp` configuration so migration can be controlled per data row.
- Preserved the request headers used by the existing Selenium browser implementation.
- Added configurable headed or headless execution through `BookHppPlaywrightHeadless`.
- Retained Selenium routing for scenarios that are not opted into Playwright.
- Added handling for UK and US billing addresses, including the mandatory US state field.
- Detects the `/payment/captured` request as the HPP completion contract. This avoids requiring the non-resolving SIT marker redirect to load successfully in the browser.
- Added PayPal popup login, purchase completion and child payment-reference extraction.
- Added nested iframe handling for the 3DS challenge and test OTP submission.

Nine scenarios currently opt into the Playwright HPP path. These cover standard cards, PayPal, 3DS, redemption, partial Pay With Points and multi-city behaviour.

### Test hardening and cleanup

- Added bounded retries for known transient seat-map response failures, with a clearer final diagnostic when retries are exhausted.
- Removed post-order assigned-seat validation against the external AIR4 service. Book-owned basket and seat validation remains in place.
- Corrected platform-dependent request-file paths so they work on macOS as well as Windows.
- Improved co-brand account lookup diagnostics and retry behaviour.
- Made optional Synchrony cookie-banner interaction non-blocking.
- Added clearer Synchrony configuration and landing-page diagnostics without logging credentials, tokens or HPP header values.

## Benefits

### Better regression confidence

The daily suite now covers a broader set of commercially important booking and payment paths. The 21 scenarios remain a deliberately selected core set, keeping execution time practical while improving coverage over the former 14-test selection.

### Lower migration risk

Playwright is enabled per scenario rather than replacing Selenium globally. A journey can be proven independently, and any unsupported scenario continues to use the existing implementation.

### More stable payment automation

Playwright provides explicit waits for elements, popups, frames and network requests. Completion is based on the HPP payment callback request instead of waiting for an environment-specific marker page to resolve, removing a known source of false failures.

### Easier diagnosis

Seat-map retries and HPP/Synchrony diagnostics make it clearer whether a failure belongs to Book, browser automation, test data or an external dependency.

### Cross-platform local execution

The path corrections and headed Playwright option improve local investigation on macOS while retaining headless execution for CI.

## Configuration and Use

To enable Playwright for a Book scenario, add the following property to its row in `e2e-scenarios.json`:

```json
"usePlaywrightForHpp": true
```

The property is optional. Missing or `false` means the existing Selenium HPP implementation is used.

The Book regression workflow also provides a manual `book_hpp_mode` input:

- `scenario` honours each scenario's `usePlaywrightForHpp` value and is available as a manual opt-in mode;
- `playwright` forces supported HPP journeys through Playwright and is the default for scheduled and manual runs;
- `selenium` forces the legacy path for comparison or rollback.

The workflow always runs Playwright headlessly on its VM agent. Co-brand remains excluded from the forced Playwright mode until its external application journey can be verified.

To observe HPP execution locally, set:

```json
"BookHppPlaywrightHeadless": false
```

For shared consumers such as Trips and OCI, use these settings:

```json
"UsePlaywrightForHpp": true,
"HppPlaywrightHeadless": true,
"HppVaaDigitalFirstToken": "<secret value>",
"HppDeltaEnterprisePaymentsToken": "<secret value>"
```

`UsePlaywrightForHpp` switches the migrated Trips/OCI PNR-data-creation paths between Playwright and Selenium. It defaults to `false`, providing a configuration-only rollback. `HppPlaywrightHeadless` defaults to `true` and is the preferred shared setting; the original Book-specific headed setting remains supported for compatibility.

Environment-specific values belong in the existing ignored local settings file or the appropriate CI configuration. `HppVaaDigitalFirstToken` and `HppDeltaEnterprisePaymentsToken` are required for both Playwright and Selenium direct-HPP access. The Book regression workflow maps them from the `HPP_VAA_DIGITAL_FIRST_TOKEN` and `HPP_DELTA_ENTERPRISE_PAYMENTS_TOKEN` GitHub Actions secrets. Access headers, credentials, tokens and complete HPP URLs must not be committed or written to test output. Rotate any value that has previously appeared in source control before configuring those secrets.

If an HPP deployment calls another origin which also requires these access headers, set
`HPP_HEADER_ALLOWED_ORIGINS` to a comma-separated list of its exact HTTPS origins. The HPP page
origin is included automatically. Do not add analytics, identity, PayPal or wildcard domains: the
credentials are injected only into this explicit allowlist.

The HPP page itself must be listed in `HPP_PAGE_ALLOWED_ORIGINS` as an exact HTTPS origin for the
configured environment. Update this mandatory setting only after verifying ownership. It does not
accept paths, user information or wildcard domains.
The checked-in non-production GitHub workflows default this value to the existing SI HPP origin;
the repository variable of the same name can override it during a controlled origin migration.

Normal browser security remains enabled. If a non-production HPP environment is known to require
the historical Chromium cross-origin workaround, set `HppPlaywrightDisableWebSecurity` to `true`
for that environment only. The setting is ignored when `env=prod`; it must never be enabled on a
production or Heimdall production agent. Prefer adding the required API origin to
`HPP_HEADER_ALLOWED_ORIGINS` and correcting CORS configuration where possible.

## Pilot Evidence

The Playwright implementation has completed live direct-HPP runs for:

- a US card journey, including mandatory state selection;
- a UK card journey;
- Mastercard;
- PayPal, including child payment-reference capture;
- partial Pay With Points;
- 3DS, including the nested challenge frames;
- an end-to-end booking after removal of the external AIR4 assertion.

The final Book project build completes with **0 errors**. The scenario JSON and schema validate successfully, and all 21 scheduled scenarios carry the core-regression trait.

Some test attempts stopped before or after HPP because of unavailable inventory or downstream Book/environment responses. These were distinguishable from HPP browser failures and do not invalidate the successful payment-path pilots.

## Known Limitations

### Co-brand and Synchrony

Experimental Playwright support exists for an existing/prepopulated payment, but no co-brand scenario is currently opted into it.

End-to-end verification is blocked by the external Synchrony application endpoint returning an Akamai `503 Service Unavailable` response before the application or HPP journey can begin. This is treated as a deferred external dependency and is not a blocker for migrating the supported card, PayPal and 3DS paths.

### Test inventory and environment dependencies

Multi-city and some route-specific scenarios can legitimately fail to find a suitable itinerary in a given environment. Redemption can also progress through payment and then fail on a downstream Book response. Reporting should keep these separate from browser/HPP automation failures.

### 3DS scope

The implementation supports the current automated challenge structure and test OTP. It should be monitored for provider markup changes because the challenge is hosted in third-party nested frames.

## Recommended Rollout

1. Merge the core-regression and pilot changes with the existing scenario-level opt-ins.
2. Monitor the scheduled run and classify failures as test automation, inventory/test data, Book API or external dependency.
3. Compare Playwright pilot stability with equivalent Selenium journeys over several runs.
4. Move additional standard-card scenarios to Playwright when the evidence is consistently positive.
5. Keep Selenium fallback until all required payment variants have been verified.
6. Revisit co-brand/Synchrony when its external application endpoint is available and stable.

## Trips and OCI Consumers

The direct-HPP implementation now lives in shared Playwright infrastructure and is exposed through a shared HPP automation facade. Book, Trips and OCI therefore use the same card and PayPal locators, required request headers, completion detection and browser lifecycle.

The following data-creation paths have been routed through the shared facade:

- OCI revenue PNR creation;
- OCI redemption PNR creation;
- Trips general PNR data creation;
- Trips seat-scenario PNR creation;
- Trips partner-airline-account PNR creation.

When `UsePlaywrightForHpp` is absent or `false`, these consumers continue through Selenium. When it is `true`, card and PayPal use Playwright.

## VM Agent Requirements

The shared implementation launches the installed Google Chrome channel rather than Playwright's downloaded Chromium. A Windows VM agent therefore requires:

- a supported .NET 10 runtime/SDK for the existing test job;
- Google Chrome installed and available to the agent account;
- the Playwright runtime files produced with the built test output;
- network access to the HPP and payment-provider endpoints;
- the existing secure runtime configuration used for HPP access and payment test data.

The VM already runs Selenium Chrome, so Chrome and network access are expected to exist. No `playwright install chromium` step is required while `BrowserChannel` remains `chrome`. The first rollout should nevertheless include a short preflight that starts Playwright Chrome and reaches HPP, because service-account permissions and endpoint security policy can differ from an interactive VM login.

Book uses a maximum of four concurrent test cases and the shared HPP facade allows no more than two concurrent Playwright browser sessions by default. This avoids exhausting the Windows desktop/session resources when several asynchronous journeys reach payment together, while allowing their API stages to continue in parallel. CI sets `HPP_PLAYWRIGHT_MAX_CONCURRENT_SESSIONS=2`; other consumers may set the value from `1` to `8` after validating the capacity of their runner.

## Rollback

For an individual Book scenario, remove `usePlaywrightForHpp` or set it to `false`. For migrated Trips/OCI PNR creation, set `UsePlaywrightForHpp` to `false`. Both routes return to Selenium without requiring a code rollback.
