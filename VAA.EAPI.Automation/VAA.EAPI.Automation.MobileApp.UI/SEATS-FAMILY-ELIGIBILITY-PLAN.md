# Seats: family rules and operating-carrier eligibility

Status: source review and implementation plan, 8 October 2026.
TC49 implements the adult/lap-infant scenario, TC50 implements unsupported-carrier
bypass with a same-session Virgin positive control, and TC51–54 implement child-guidance
display/absence boundaries. Each independently runnable scenario has its own class;
remaining scenarios are proposed, not executable or passing coverage. Allocate sequential
SearchBook TC numbers only when each scenario is implemented.
Existing adult coverage and evidence: [SEATS-COVERAGE.md](SEATS-COVERAGE.md).

## Reviewed baseline

Local iOS checkout `ios-seat-sheet-accessibility`, commit `f1d87fb9`.
This is not a fresh remote-develop audit and does not establish the installed binary's SHA.
Paths below are relative to `Modules/vaa-mobile-ios-search-and-book/Sources/SearchAndBook/`.

| Confirmed implementation | Source | Test implication |
| --- | --- | --- |
| VS/DL only; operating code preferred, marketing code used only when operating is absent; case-insensitive; ECONOMY-LIGHT and ECOLIGHT excluded | Models/FlightBooking/FlightBookingBasketContext.swift, isSeatMapEligible | Test actual operating carrier and fare; a non-Virgin result can still be eligible Delta |
| Infants filtered out of seat travellers | UI/NativeSeatSelection/NativeFlightSeatSelectionViewModel+State.swift, travellers | A lap infant must not get an independent seat-passenger card |
| Cards indexed over filtered travellers | UI/NativeSeatSelection/NativeFlightSeatMapPresentation.swift, passengerCards | Match expected non-infant party, not original unfiltered passenger indexes |
| Seat selectable only with active traveller's offer, no other traveller's draft assignment and no active submission | UI/NativeSeatSelection/NativeFlightSeatSelectionViewModel+Actions.swift, isSelectable | Availability is passenger-specific; do not assume an adult's offered seat is offered to a child |
| Submission refreshes offers between selections and reconciles the basket | Services/FlightSearch/SearchAndBookFlightSeatServiceAdapter.swift | Some rules may be enforced downstream at submission, not by the initial map |
| Passenger carousel uses LazyHStack | UI/NativeSeatSelection/NativeFlightSeatSelectionPassengerCarousel.swift | Absence of an offscreen card in one snapshot is not proof that the infant is excluded |

No explicit child-distance/adjacency rule was found in the reviewed native seat-selection,
seat-map presentation or seat adapter source. This is not proof that the backend lacks the rule.
The Trips backend SeatMapTests reuse Core storage-table lookup and validate VS/DL maps and
optional linkedInfant response structure; they do not establish a child-proximity policy.
Reuse their clients/models where applicable, not their entire existing-booking setup as a
substitute for a new-booking basket.

## Confirmed child-proximity behaviour — 8 October 2026

Product clarification from the user: child proximity displays warning/information only;
there is **no hard stop**. Test the advisory content and continued ability to proceed.
Do not expect proximity alone to disable a seat, disable Continue or reject submission.
Other independently applicable seat restrictions remain separate rules.
The trigger, age boundaries, display timing and dismissal/update behaviour still require
confirmation from the implementation or acceptance criteria. Do not assume that the message
is conditional on a particular seat distance rather than general information for child parties.

## Delivery order and assertions

| Order / planned scenario | Data and setup | Required outcome | Readiness |
| --- | --- | --- | --- |
| 1. Adult + lap infant | One adult, one infant, eligible VS Classic sector; seat feature enabled | Only the adult is independently assignable; an offered permitted adult seat can be selected/replaced/removed; no infant assignment or auto-advance to an infant | Rule confirmed; implement reliable complete carousel observation first |
| 2. Unsupported operating carrier bypass (TC50) | One adult; all operating legs explicitly AF/KL; Economy Standard | Passenger submission goes directly to payment without any seat Continue/Skip. Same-session Virgin Classic must then open seats, ruling out a global skip override. Neither journey pays | Implemented; local qualification pending |
| 3. Delta positive control | One adult; confirmed direct DL-operated sector using an offered Delta Main fare; skip OFF | Seat screen, correct sector and selectable offered seat; stop before submission | TC54 selects by exact DL operating code and the seat-eligible Delta Main cabin without assuming Virgin fare names; available inventory is still required |
| 4. Mixed supported/unsupported sectors | Itinerary with one eligible VS/DL sector and one unsupported sector; verify all legs first | Seat route/progress includes eligible sectors only and assignments remain associated with correct sector | Needs multi-sector navigation and observable route/progress |
| 5. Codeshare eligibility | Marketing and operating carriers differ, with known operating code | Apply the operating-carrier rule; a VS marketing code must not enable seats for an unsupported operator | Needs suitable offer; missing operating-code fallback needs explicit fixture/contract |
| 6. Child proximity information | Adult + child within the agreed age range; available offered seats | Required warning/information is shown at the agreed point and is readable; assignments remain usable and proximity alone does not block Continue | Advisory-only behaviour confirmed; trigger/content/timing and observation contract pending |
| 7. Child proximity advisory after changes | Adult + child; change/remove/reselect the accompanying adult's seat using offered inventory | Advisory persists, updates or dismisses according to the confirmed design; proximity alone never disables seats or blocks continuation; assignments remain correct | Confirm whether information is static or dynamically triggered before defining distance-based cases; stop at payment handoff if submission is required |
| 8. Adult with infant restrictions | Known adult/infant link and restricted offer (e.g. exit seat only if policy confirms it) | Restricted assignment unavailable or rejected at agreed boundary; valid alternatives remain usable | Blocked on restriction/linkage contract and reliable inventory |
| 9. Infant occupying own seat | Explicit supported passenger type and API representation | Separate seat assignment under the approved child-restraint/age rules | Not assumed supported: current UI model excludes all infants |
| 10. Representative family E2E | Small approved family, eligible seats, synthetic data | Payment/confirmation and authoritative passenger/sector seat retention | After #177 and Trips lookup fix; explicit booking/payment authorization |

Light-fare bypass is a separate negative control: do not accidentally use Light to prove
unsupported-carrier bypass, or the global skip flag to prove either eligibility rule.
VS Classic entry already has TC43–47 coverage; Delta requires its own positive control.

## Automation changes needed before the first scenarios

1. Reuse SeatScenarioTestBase, NativeSeatSelectionPage and NativeBookingCheckoutFlow for family entry.
   Preserve existing synthetic traveller/contact construction. No new login or storage connector.
2. Add a complete passenger-carousel observation path. Use an app-owned bounded scroll container
   and observable end/count if exposed; otherwise request an accessibility-only contract.
   Do not infer no infant card merely from missing index 1 in a lazy/offscreen hierarchy.
3. Add exact operating-carrier/ordered-leg selection to the existing search page. Retain
   evidence for each leg; never treat virginFlight=false as neither VS nor DL.
4. Add an explicit passenger-to-payment bypass assertion. The existing
   ContinueFromPassengerDetails method automatically continues through an unexpected seat screen,
   so reusing that method unchanged would conceal an eligibility failure.
5. Keep locators and driver calls in page objects, complete scenarios in separate xUnit classes,
   and retain failure screenshots, Appium session output and BrowserStack status reporting.
6. Register implemented scenarios in the JSON contract and TEST-CATALOG; initially Qualification.
   No skipped placeholders or unimplemented scenarios counted as regression coverage.

## Rule decisions needed from product / backend

- Child age boundaries, evaluated at booking or travel date; treatment of teens.
- Definition of proximity: same row, adjoining seat, permitted row distance, across aisle,
  same cabin; whether carrier/aircraft changes this rule.
- Which adult accompanies each child/infant, and how that relationship is represented.
- When child-proximity information appears: general child-party advice or a conditional
  warning during selection/after assignment/on Continue; expected content and dismissal/update
  behaviour. Advisory only: no hard stop for proximity.
- Advisory behaviour when nearby inventory is unavailable or a child/adult is left unassigned;
  distinguish any separate assignment requirement from the non-blocking proximity guidance.
- Exit-row, oxygen-mask, bassinet and other infant restrictions: confirm which are in scope
  rather than treating plausible airline policies as this application's contract.
- Whether infants in their own seat are supported by this native journey.

## Data, execution and completion gates

- Prefer shared Core storage/API capabilities for approved deterministic data; do not copy
  Azure clients or hard-code a seat number, passenger identity or assumed airline inventory.
- Discover actual offered seats and geometry. Letter/number adjacency alone does not prove
  adjacency across aisles or nonstandard layouts.
- Capture unavailable required inventory as a data blocker, never a pass or silently changed scenario.
- Pure helper/policy tests validate automation mechanics only; they are not iOS functional coverage.
- Build and run mobile offline/retry checks, architecture checks and scoped harness verification.
- Qualify each live case locally with exact-run TRX analysis, then BrowserStack. Most cases stop
  before payment; submission and purchase tests remain explicitly scoped and authorized.
- Keep controlled retrieval/submission failures separate from naturally intermittent outages.
- Preserve #177 as the seat-payment blocker; no automatic payment retries.
- Record source revision, app build provenance, data preconditions and actual assertions exercised.
- Promote to Regression only after live qualification; do not claim Swift code-coverage percentages.

## Current result

TC49 reuses the guest checkout and seat lifecycle with one adult and one lap infant.
PR1273 commit c6d9ba84 supplies the complete passenger count through
`seatSelection.passengers`; missing/non-numeric values fail without counting visible cards.
375 mobile offline checks passed. TC49 passed locally on iPhone 17 Pro / iOS 26.5
on 8 October 2026 in 4m08s (4m27s including session lifecycle). Exact-run analysis
found no failures: `TestResults/lap-infant-20261008/local.trx`. It verified complete
count one, adult selection/replacement/removal and retained active passenger zero.
Scoped formatting passed. Wider `verify.ps1 -SkipBuild` failed on context-routing,
operational-harness and a format restore failure; this is not a green harness gate.
No seat submission/payment is performed. TC50 now implements carrier bypass:
bounded result selection reads every leg using existing detail IDs, requires exact
partner Economy Standard retention on Summary, and fails if seats appear. Unknown operators
and mixed VS/DL itineraries are rejected. A second Virgin Classic search in the same
session must open seats, proving a saved global skip override cannot explain bypass.
No iOS changes, new dependencies or storage connector. Search inventory may block
qualification; do not substitute Light or a mixed Delta connection to make it pass.

TC50 local attempt on 8 October stopped after selecting KLM Economy Standard with
the app's "We could not start this search just now" fare-selection error. Screenshot
shows Standard on both LHR-AMS-JFK legs; no passenger submission/bypass/positive
control was reached. Exact TRX: `TestResults/seat-bypass-20261008/local-standard.trx`.
391 offline checks pass; backend cause and live qualification remain pending.
Develop #1318 now establishes the pre-seat contract: a `.children` traveller (ages 2–11) causes
a one-time first-sector `seatSelection.childGuidanceSheet` with supervision and next-to/front/
behind guidance. TC51–54 assert display or bypass at the sheet boundary without dismissing it.
Exact copy remains blocked on IDs for the title and two messages, and continuation remains blocked
on the Choose seats action ID; no English-text, XPath or coordinate fallback and no passing live
coverage is claimed. W003 confirmation presentation is a separate deterministic-data scenario.
