# Search and Book: iOS develop impact review — 13 October 2026

Source reviewed: `vaa-mobile-ios` remote `develop` at
`a843f7830dd1860af869bb913480360c6d2a12e8` (13 October).
Scope: Search and Book user-facing implementation and implemented Appium assertions.
This is source-to-test mapping, not measured Swift line/branch coverage or live qualification.
The uploaded BrowserStack app's source SHA is not established by checking out `ios_ref=develop`.

## Subsequent remote and execution check

Fetched remote `develop` again on 13 October 2026: it still resolves to the exact source
commit above; there are no intervening Search and Book changes. This confirms the focused
impact review is still applicable, not that the historical whole-source baseline is fresh.

The later isolated Loyalty TC02 BrowserStack run passed calendar selection and failed
waiting for the required insufficient-points warning on Flight Summary. See
[run 38420454457](https://github.com/VAAD1ST/eapi/actions/runs/38420454457) and
[bug #188](https://github.com/VAAD1ST/vaa-mobile-delivery-artifacts/issues/188).
It stopped before the points-total and disabled-Continue assertions; no checkout success
or bypass of the guard was proved by this automated run.

Reporting now spells out the maintained matrix breakdown: **14 Mapped, 5 Partial, 1 Gap**.
The five partial areas are Summary, seats, payment handoff, confirmation/Trips and loyalty;
the gap is controlled error/retry behaviour. Slack reads these names and counts directly
from the generated matrix report. A changed source tree still reports `NeedsReview` rather
than silently approving newly added functionality. No Slack message or live run is sent
as part of this documentation/reporting check.

## Decisions and follow-up

| Area | Existing coverage / source change | Action and status |
| --- | --- | --- |
| Confirmation traveller identity | iOS #1293 retains `NativeFlightBookingConfirmationPassengerCount` but changes its value from age categories to `(count) First Last, …`. TC01/TC03/TC43/TC48 and Loyalty TC01 share checkout. | Updated shared checkout to validate exact submitted count and ordered names before leaving confirmation. Guests use the entered synthetic names; members use the leased account identity. Native label/value/text must agree when populated. Names are not printed on assertion failure. Requires live qualification on the new app; old confirmation builds are deliberately not accepted. |
| Insufficient redemption points | iOS #1277 removes `NativeFlightSummaryInsufficientPoints` and the insufficient-points Continue guard. Loyalty TC02 expects both. | **Known contract mismatch / product decision required.** Keep test and release-pack membership unchanged, so the protection is not silently waived. Establish where/how low-balance redemption should stop, then update the test against that agreed requirement. A failure against this build is not a selector-retry problem. |
| Confirmation receipt | New receipt separates flights, seat fees and taxes; reward total combines points and cash. Existing E2Es only require a nonempty total. | Open: compare observed receipt amounts with retained checkout evidence. Receipt rows have no dedicated indexed/semantic IDs in the reviewed view. Request app-owned row/value IDs before adding brittle text/hierarchy lookups. No receipt-arithmetic coverage claimed. |
| Confirmation flight details | Outbound/inbound confirmation cards now open details with cabin, baggage and confirmed seats. Existing TC26/TC29/TC41/TC42 stop at pre-payment Summary. | Open: scoped confirmation itinerary action and details assertions; pre-payment checks do not prove post-booking fulfilment. Validate action ownership and per-traveller/sector seat evidence before implementation. |
| Hosted payment navigation | iOS #1294 introduces navigation-backed payment review, hosted payment and processing; cancellation returns to review. | Existing booking happy paths require requalification. Open focused dismiss → review → reopen scenario, stopping before Pay. Verify the actual hosted-payment dismiss action contract; do not confuse review Back (`NativeFlightPaymentBackButton`) with hosted-payment dismissal. No automatic payment retry. |
| Pay with Points | Updated disclosure/selection, apply/remove, minimum contribution and session reconciliation. | Loyalty TC03 remains Deferred and only selects a contribution. Selection requalification, apply/remove totals and minimum-balance checks are open; no promotion to Regression based on source inspection. |
| Retained seat selection | #1287 permits a selected seat to remain selectable even when a refreshed offer no longer contains it. | TC44/TC46/TC47 cover normal assignments, replacement/removal and totals, not this refreshed-offer branch. Needs reproducible inventory/state before a reliable focused test. |
| Other seats | TC44–47/49/50 cover lifecycle, sheets, multiple adults, prices, lap infant and unsupported carriers. | Corrected the obsolete indirect/no-seat matrix description. TC43 remains Qualification; paid-seat fulfilment is not inferred from interaction passes. |

## Release interpretation

- Test membership remains 10 Sanity, 46 UI Regression and two separate backend comparisons.
- No new live scenario is claimed by this update; confirmation assertions strengthen existing E2Es.
- Loyalty TC02 remains a known sign-off concern on this source revision. It is not skipped,
  auto-healed or treated as passing when its required warning is absent.
- The 20-component table still maps 19/20 areas (95% breadth), now with **five Partial** areas
  and one Gap. Mapping breadth must not be presented as 95% behavioural completeness.
- Do not advance the machine-readable whole-source review baseline from a focused change review:
  the open behaviours above and a complete source inventory still need assessment.
- Live qualification of the changed assertions must use a build with known source provenance.
  Offline assertion tests cannot establish Appium accessibility exposure or E2E success.

## Suggested order

1. Requalify TC48 and Loyalty TC01 against the latest app with the new confirmation assertions.
2. Agree the insufficient-points user outcome with the app/product owner.
3. Qualify hosted-payment dismissal/reopen without submitting payment.
4. Add confirmation receipt/details assertions once their observation/action contracts are verified.
5. Revisit PWP and the retained-seat refresh case using controlled data.

No iOS source, workflow, release-pack selection or active BrowserStack run is changed here.

## Offline validation

Release build and 450 offline/retry checks passed (zero failed/skipped), including 15 new
confirmation cases covering exact names/count, member identity, native whitespace, conflicting
values, missing evidence and legacy/wrong passenger text. The exact-TRX analyser found no failures.
Evidence: `VAA.EAPI.Automation/TestResults/develop-confirmation-20261013-verified/offline.trx`
(repository-relative, gitignored). Project-scoped `scripts/verify.ps1 -SkipBuild` passed.
These results do not qualify the new confirmation accessibility tree on a device.
