# Backend Voucher Automation Coverage

| Detail | Value |
| --- | --- |
| Status date | 15 October 2026 |
| Project | `VAA.EAPI.Automation.Search` |
| Environment | SIT |
| Implementation | [`SearchRewardsGraphQl.cs`](../VAA.EAPI.Automation.Search/Tests/NewBookFlow/GraphQL/OfferSearch/SearchRewardsGraphQl.cs), TC17-TC45 |
| Pull request | [#1824 - Harden voucher upgrade calculations and sector discovery](https://github.com/VAAD1ST/eapi/pull/1824) |

## Purpose

This document describes the current backend voucher acceptance automation, the business behaviour covered, the latest execution evidence, and the remaining coverage decisions for BA review.

The Search tests exercise reward search and offer price through Search GraphQL. Where the relevant feature flags are enabled, they also verify voucher selection and persistence through `retrieveBasket`. The Book GraphQL E2E pack additionally covers representative voucher journeys through payment, Order Create, booking confirmation and downstream PNR remark validation.

## Maintaining this document

Update this record whenever voucher tests, test data, feature switches or acceptance scope change. Keep the status date, latest run summary, coverage matrix, dependencies and known gaps aligned with the implemented suite so reviewers can distinguish verified coverage from scenarios that are feature-gated or awaiting data.

## Latest run summary

| Measure | Result |
| --- | --- |
| Run date | 9 October 2026 |
| Environment | SIT, executed locally against deployed services |
| Full TC17-TC44 run | 30 cases: 27 passed, 3 failed, 0 skipped |
| Full-run failures | TC22 eligibility discrepancy; TC23 basket selection persistence; TC44 historical expectation mismatch (reclassified 15 October) |
| Full-run feature switches | Summary, basket and selection validation enabled and exercised |
| Focused story pack | 22 tests: 15 passed, 7 failed initially |
| Automation fixes rerun | 6 tests: 4 passed, 2 failed |
| Confirmed automation fixes | TC17 expiry filtering; TC24/TC25 pre-selection basket contract; TC38 current Basket GraphQL schema |
| Resolved product failure | TC23 retest confirmed that `retrieveBasket` now retains the voucher selection and recalculated amounts; the remaining `membershipPoints` mismatch is an automation assertion defect |
| Inconclusive | TC31 did not obtain a genuine Upgrade/Companion value tie from live SIT fares |
| Feature switches | Voucher summary, basket and selection validation enabled |
| Repository analyser | The two remaining failures match the product behaviours described above |
| Completed-booking evidence | Companion Voucher booking completed; downstream `SPCL` remark verified through `PNRCommonV6.Retrieve`. Booking-access identifiers are retained only in the restricted workflow evidence. |
| GitHub Actions evidence | [Run 37938895674](https://github.com/VAAD1ST/eapi/actions/runs/37938895674) |

The 9 October rerun is the current acceptance evidence. Older run summaries later in this document are retained as historical evidence only. In particular, TC23 passed on 8 September but now fails reproducibly, which makes the lost basket selection a regression candidate rather than an unimplemented assertion.

On 15 October, task 877979 clarified that a Companion redemption uses a one-primary/one-companion pairing. For a direct single-sector journey, one voucher is applied and `coveredPassengers` is `2` because the field represents both people in that pairing, not only the companion. The 9 October TC44 failure was therefore an automation expectation mismatch, not a product defect. TC44 now asserts one voucher and two covered passengers without assuming that Companion is `bestOption`. TC45 separately proves that two vouchers can cover the same two-person pairing across the two sectors of a return journey; its focused SIT execution passed.

Run evidence is held locally at:

- `TestResults/voucher-story-pack-sit-20261009/voucher-story-pack-sit-20261009.trx`
- `TestResults/voucher-story-fixes-sit-20261009/voucher-story-fixes-sit-20261009.trx`
- `TestResults/voucher-full-suite-sit-20261009-all-enabled/voucher-full-suite-sit-20261009-all-enabled.trx`

## Coverage matrix

| ID | Journey and data | Behaviour covered | Expected result | Latest evidence |
| --- | --- | --- | --- | --- |
| TC17 | One-way, LHR-JFK, Economy reward, 2 adults, voucher account | Companion option is returned for an eligible voucher account | Companion redemption details cover 2 passengers; basket values are also compared when basket validation is enabled | Core assertion passed; basket sub-check gated off |
| TC18 | Return, LHR-JFK, Economy reward, 2 adults, voucher account | Companion option is returned for a return journey | Companion redemption details cover 2 passengers; basket values are also compared when enabled | Core assertion passed; basket sub-check gated off |
| TC19 | One-way, LHR-JFK, Premium reward, 1 adult, voucher account | Upgrade voucher option is returned | Upgrade redemption details are present and correctly associated with the account vouchers | Core assertion passed; basket sub-check gated off |
| TC20 | One-way, LHR-JFK, Economy reward, 2 adults, non-voucher account | Voucher details are not offered to an ineligible account | No voucher summary and no voucher basket state | Core assertion passed; basket sub-check gated off |
| TC21 | One-way, LHR-JFK, Economy reward, 1 adult, voucher account | Companion option requires sufficient passengers | Companion redemption type is absent for one passenger | Passed |
| TC22 | Return, LHR-JFK, Premium reward, 1 adult, voucher account | Upgrade voucher option is returned for a return journey | Upgrade redemption details are present | **Investigate: response returned one of two storage voucher IDs; confirm the omitted voucher's product eligibility** |
| TC23 | One-way, LHR-JFK, Economy reward, 2 adults | Explicit voucher option selection, refreshed amounts and deselection | Selected option and voucher IDs persist; refreshed totals match; deselection clears mutation and basket state | **Backend fix verified: basket retains the selection; align the automation-only `membershipPoints` comparison** |
| TC24 | One-way, LHR-JFK, Upper Class reward, 2 adults, low-points voucher account | Insufficient-points Companion flow | Companion is available and Buy Virgin Points is enabled when the member balance is insufficient | Passed |
| TC25 | One-way, LHR-JFK, Premium reward, 3 adults, exactly 2 eligible vouchers, low-points account | Insufficient-points Upgrade flow and independent best-value proof | Premium is treated as the selected cabin, Economy as its baseline; Upgrade value is at least Companion value, including Upgrade tie precedence; Buy Virgin Points is enabled | Passed |
| TC26 | One-way, LHR-JFK, Upper Class reward, voucher account | Upgrade from Premium to Upper Class | Upgrade redemption details are returned using Premium as the baseline cabin | Core assertion passed; basket sub-check gated off |
| TC27 | One-way, LHR-JFK, Economy reward | Returned vouchers remain valid for outbound travel | Every available voucher expires on or after outbound departure | Passed |
| TC28 | One-way reward flight explicitly selected with operating carrier DL | Partner-airline restriction | No voucher details are returned; the non-voucher basket flow remains usable | Passed |
| TC29 | One-way, outbound after voucher expiry | Expired-voucher exclusion | Expired vouchers are excluded and the non-voucher basket flow remains usable | Passed |
| TC30 | Return itinerary with a DL-operated outbound and VS/partner mix | Mixed-itinerary restriction | No voucher details are returned for the mixed itinerary; non-voucher basket continuation works | Passed |
| TC31 | Return, 2 adults, Upgrade/Companion equal-value case | Upgrade tie precedence and Payment-basket persistence | Upgrade is selected as best option when value ties with Companion and is persisted to the basket | **Inconclusive: live SIT search did not produce a genuine value tie** |
| TC32 | Return LHR-JFK and LHR-ATL, 2 adults, paired Economy/Premium fares | Upgrade calculation using the backend GraphQL baseline | `expected totalPointsUsed = min(searchSummary.price.awardPoints - upgradeOption.baseLinePoints, available voucher balance)`; value is positive and matches the Upgrade option | 2/2 route cases passed; basket sub-check gated off |
| TC33 | One-way, account with exactly 1 voucher and account with exactly 2 vouchers; outbound after all expiries | All-expired negative cases | No expired vouchers are returned and non-voucher basket continuation works | 2/2 voucher-count cases passed |
| TC34 | One-way, exactly 2 vouchers with different expiries; outbound between them | Partial expiry filtering | Earlier-expiring voucher is excluded and only the later valid voucher remains | Passed |
| TC35 | One-way, outbound exactly on voucher expiry date | Expiry boundary | Voucher remains eligible on its expiry date | Passed |
| TC36 | Return, outbound before expiry and inbound after expiry | Eligibility is based on outbound travel date | Voucher remains eligible because outbound is valid even though inbound is after expiry | Passed |
| TC37 | Return with inbound explicitly selected with operating carrier DL | Partner restriction on the inbound sector | No voucher details are returned; non-voucher basket continuation works | Passed |
| TC38 | Return, 2 adults, Upgrade/Companion tie | Recalculation after selection and deselection | Upgrade calculation is valid before mutation; Upgrade remains best after voucher selection, basket refresh, deselection and another refresh | Passed after updating the automation to the current Basket GraphQL schema |
| TC39 | One-way, account with more than 2 vouchers | MVP earliest-two response cap | Only the two earliest-expiring eligible vouchers are returned | Passed |
| TC40 | Return, Economy outbound/Premium inbound | Mixed-cabin Upgrade calculation | Outbound delta is zero and voucher value is consumed only by the eligible inbound upgrade | Passed |
| TC41 | One-way, two vouchers straddling outbound date | Later eligible Upgrade voucher | Only the later-expiring eligible voucher is returned and used | Passed |
| TC42 | One-way, 3 adults, exactly 1 voucher | Odd passenger Companion pricing | One Companion voucher is applied and the additional passenger remains priced in points | Passed |
| TC43 | One-way, 3 adults, exactly 2 vouchers | Companion voucher passenger boundary | Redemption is capped at one Companion voucher because two Companion passengers require four travellers | Passed |
| TC44 | Direct one-way/single-sector, 4 adults, exactly 2 vouchers | Single-sector Companion cap | Only one Companion voucher is applied to one primary/companion pair (`coveredPassengers=2`) and the remaining passengers stay priced in points | Passed on SIT, 15 October |
| TC45 | Return/two-sector, 4 adults, exactly 2 vouchers | Two-sector Companion positive boundary | Two vouchers cover the same primary/companion pair across both sectors (`coveredPassengers=2`) | Passed on SIT, 15 October |

TC32 and TC33 are parameterised. The current implementation contains TC17-TC45, including the seven story-focused cases TC39-TC45.

## Business rules represented

The automated coverage currently represents these rules:

- Companion vouchers require enough passengers. A single passenger must not receive a Companion option.
- For Upgrade, the selected reward cabin is the cabin being booked and the baseline is the cabin immediately below it:
  - Economy to Premium uses Economy as the baseline.
  - Premium to Upper Class uses Premium as the baseline.
- Upgrade voucher value is capped at the total available voucher balance.
- TC32 uses the backend-provided `baseLinePoints` value. It no longer derives return-sector value from cumulative inbound `awardPoints`.
- When Upgrade and Companion provide equal value, Upgrade is expected to win the best-value tie.
- Two or more vouchers may be applied to an Upgrade for a one-sector booking with two or more passengers when Upgrade is best value.
- The equivalent rule does not permit two Companion vouchers for only two passengers; two Companion vouchers require four passengers.
- A direct single-sector journey permits a maximum of one Companion voucher, even when four passengers and two vouchers are present. The multi-voucher Companion positive case is exercised on a two-sector journey.
- Voucher eligibility is determined from the outbound travel date. A voucher is valid on its expiry date, and an inbound date after expiry does not invalidate an otherwise eligible voucher.
- Voucher redemption is not offered for partner-operated or mixed VS/partner itineraries covered by TC28, TC30 and TC37.
- Carrier eligibility is based on the returned segment's **operating carrier**, not simply the origin/destination. The partner tests explicitly select DL-operated availability and fail if no matching DL itinerary can be proved.
- A customer must still be able to continue through the non-voucher basket path when vouchers are unavailable or ineligible.
- Low-points cases validate that Buy Virgin Points is enabled for the applicable Companion or Upgrade option.

## Automation hardening in place

- Voucher and non-voucher accounts are selected from Azure Table Storage through the existing Core/Key Vault implementation.
- Voucher account selection supports maximum points, minimum/maximum expiry, exact voucher count, all-vouchers-in-range and different-expiry-date criteria.
- Sign-in retries up to three times with fresh storage data for the known invalid-credentials response.
- Missing test data reports the requested criteria and available-data diagnostics rather than only reporting a generic failure.
- Partner-flight tests search LHR-JFK, LHR-ATL and LHR-BOS across a three-day availability window.
- Return upgrade tests search a three-day window and require direction-correct outbound and inbound candidates.
- Upgrade sector candidates require paired Economy and Premium reward fares and all selected segments to be VS-operated.
- TC24/TC25 choose the highest-priced requested cabin, not the highest baseline fare.
- TC32 logs the selected dates, flights and baseline calculation evidence to make failures reproducible.

## Test data and configuration dependencies

- MyAccount automation creates voucher accounts daily.
- Search automation reads those accounts from Azure Table Storage; credentials and connection details remain in local/secure configuration and are not documented here.
- Standard voucher searches default to accounts whose vouchers remain valid beyond the normal search-date window.
- TC24 and TC25 require a low-points account. TC25 additionally requires exactly two vouchers valid for the outbound date.
- TC33-TC36 require specific voucher counts and expiry-date shapes.
- TC28, TC30 and TC37 depend on live DL reward availability. Route/date discovery reduces, but cannot eliminate, environment availability dependency.
- Voucher checks are allowed only in configured environments; the default allowed environment is SIT.

Relevant feature switches are:

| Setting | Purpose | Default in code | Latest local run |
| --- | --- | --- | --- |
| `SearchRewardVoucherValidationEnabled` | Core voucher summary assertions | `true` | Enabled by default and executed in SIT |
| `SearchRewardVoucherBasketValidationEnabled` | Voucher persistence into `retrieveBasket` | `false` | Enabled explicitly and exercised in the latest full run |
| `SearchRewardVoucherSelectionValidationEnabled` | Explicit voucher selection/deselection mutation checks | `false` | Enabled explicitly and exercised in the latest full run |
| `SearchRewardVoucherValidationEnvironments` | Environment allow-list | `sit` | SIT allowed by default |

## Coverage gaps and BA decisions required

Current story sign-off position after the 9 October 2026 SIT run:

| Story | Position | Reason |
| --- | --- | --- |
| 857259 | **Passed — closed** | The TC23 retest confirmed that the Payment basket retains the selected voucher and recalculated amounts. Task 877978 is closed. |
| 857494 | **Passed — can close** | Companion and Upgrade booking/confirmation journeys are proven in SIT, including settled voucher and Virgin Points consistency. |
| 858265 | **Retest required** | Upgrade calculation and mixed-cabin cases pass. Run the focused Upgrade Summary/Payment case against the deployed task-877978 fix before sign-off. |
| 858286 | **Passed — can close** | One-way, return, single-passenger and insufficient-points contracts pass; a representative Companion journey also completed successfully. |
| 858309 | **Passed — can close** | TC42 and TC43 cover the three-passenger boundaries. Corrected TC44 passed with one voucher covering one primary/companion pair, and TC45 passed for the two-sector return case. |
| 858347 | **Passed — can close** | Return/two-sector, single-passenger, three-passenger and recalculation rules pass. Corrected TC44 and TC45 now provide the single-sector cap and two-sector positive evidence. |
| 858354 | **Passed — can close** | PNRCommonV6 retrieval confirmed the expected voucher type, voucher points and total-points remark on the completed Companion booking. |
| 858604 | **Passed — can close** | All five explicitly defined generic Companion/Upgrade rule scenarios passed. |
| 866950 | **Passed — can close** | Expiry eligibility, boundary and outbound-date rules pass. |

## Booking and confirmation journey coverage

The Book GraphQL E2E scenario pack now contains five voucher journeys which use the Playwright HPP path:

| Journey | Expected voucher use | Confirmation proof |
| --- | --- | --- |
| One-way LHR-JFK Economy, 2 adults | One Companion voucher | Selection, final basket, payment, Order Create, PNR and confirmation consistency |
| Return LHR-JFK Economy, 2 adults | One Companion voucher | As above, including return booking completion |
| One-way LHR-JFK Premium, 1 adult | One Upgrade voucher | Selection through confirmed Upgrade booking |
| Return LHR-JFK Premium, 1 adult | One Upgrade voucher | Selection through confirmed return Upgrade booking |
| Return LHR-JFK Economy, 4 adults | Exactly two Companion vouchers | Both voucher IDs and applied points must persist into confirmation |

The scenarios select fixtures by voucher product/count, allow a journey-specific minimum points balance, and assert the exact number of applied vouchers both at selection and confirmation. They are included in `Book-Core-Regression` and `Book-E2E-Regression`.

### Current SIT execution position (9 October 2026)

The SIT `LoyaltyVouchers` partition was refreshed and all 15 voucher creation cases passed, including authoritative Customer Transaction retrieval after each voucher award. A subsequent authenticated Companion Voucher journey successfully selected the refreshed data, used the Playwright HPP path and completed Order Create.

Evidence:

- `TestResults/voucher-sit-cleanup-20261009/voucher-sit-cleanup-20261009.trx`
- `TestResults/voucher-sit-recreation-20261009/voucher-sit-recreation-20261009.trx`
- `TestResults/voucher-companion-confirmation-sit-20261009-refreshed-retry/voucher-companion-confirmation-sit-20261009-refreshed-retry.trx`
- `TestResults/voucher-companion-confirmation-sit-20261009-workflow-hpp-config/voucher-companion-confirmation-sit-20261009-workflow-hpp-config.trx`

The completed booking was then retrieved through `PNRCommonV6.Retrieve`. Booking reference and passenger details are retained only in the restricted workflow evidence:

- Remark type/number: `SPCL` / `001`
- Remark pattern: `ONLINE <transaction-id>. COMPANION VCHR <voucher-points> PTS. TOTAL PTS <total-points>`
- GitHub Actions evidence: [run 37938895674](https://github.com/VAAD1ST/eapi/actions/runs/37938895674)

The Companion Search, selection, Playwright HPP, Order Create, confirmation and downstream remark paths are therefore proven with refreshed data. The separate TC23 failure remains a confirmed regression candidate because another selection flow loses its state in the immediately refreshed Payment basket.

On 16 October 2026, the SIT voucher partition was refreshed again and all 15 creation cases passed. The return Upgrade journey then passed end to end through voucher selection, Playwright HPP, Order Create, confirmation consistency and PNR validation. The automation now compares the confirmation with the final settled basket captured before payment; it does not retrieve the cleared basket after Order Create.

Evidence: `TestResults/voucher-upgrade-confirmation-sit-20261016-postbasketfix/voucher-upgrade-confirmation-sit-20261016-postbasketfix.trx`.

Ready-to-raise defect details are in [`BUG_VOUCHER_SIT_FINDINGS_2026-10-09.md`](BUG_VOUCHER_SIT_FINDINGS_2026-10-09.md).

The following should be confirmed before describing the suite as fully executed acceptance coverage:

1. Basket and selection switches were enabled for the 9 October full run. TC31 and TC38 passed with their test bodies exercised; TC23 failed because the selected state was not retained by the payment basket.
2. Conditional basket assertions were exercised in the full run. Continue tracking the TC23 persistence defect and the TC22 voucher-eligibility discrepancy.
3. Companion passenger and sector boundaries are now explicit: TC42 covers three passengers/one voucher, TC43 caps three passengers/two vouchers at one Companion redemption, TC44 caps a direct single-sector journey at one Companion voucher, and TC45 covers the two-sector four-passenger/two-voucher positive case.
4. Decide whether one-way versus return coverage is required for every cabin and redemption type. The suite uses representative combinations rather than a full journey/cabin/passenger Cartesian matrix.
5. Decide whether additional tiers, voucher types, currencies, infants/children, open-jaw/multi-city journeys, connecting itineraries, and more than two vouchers are in scope. They are not explicitly covered by TC17-TC38.
6. Booking completion and confirmation are implemented by the five Book E2E scenarios above. Companion execution is proven; Upgrade remains blocked by the current basket-persistence defect, while the corrected two-sector/two-voucher case requires execution.
7. Change feature-gated tests to report `Skipped` rather than returning successfully if accurate CI pass-rate reporting is required.
8. Consider failing, rather than returning, when a normal voucher search yields an empty summary. Some scenarios currently return without assertion if no usable offer-price result is produced, which can conceal an availability or environment problem.

## Acceptance-criteria traceability review (4 September 2026)

This section maps the additional acceptance criteria supplied for SIT review to the backend automation currently present in the repository. Frontend navigation, popup, presentation and copy scenarios belong to the separate FE test project and are recorded here only as out of backend scope. The status deliberately distinguishes calculation-level unit tests, deployed Search GraphQL coverage and API coverage through basket, booking and servicing. A unit test proves a business rule in isolation; it does not prove that the deployed services, basket, payment, booking and confirmation apply that rule together.

| Supplied criterion | Status | Current evidence | Remaining gap |
| --- | --- | --- | --- |
| My Account **Search and Redeem** directs a member into Reward Search | **FE-owned; out of backend scope** | Backend `memberVoucherDetails` contract is covered separately by My Account GraphQL tests | Validate navigation in the separate FE suite. No backend journey test is proposed. |
| Voucher value equals fare value | **Partial** | `VoucherRedemptionServiceTests` covers selection of a voucher that covers the required points and caps use at the fare value | No deployed SIT case explicitly fixes voucher value equal to fare and completes the journey. |
| Voucher value below fare; member pays the balance in points | **Partial** | Service tests cover voucher value below Companion fare and multi-voucher shortfalls; TC24/TC25 cover insufficient-points/Buy Points presentation | Add a sufficient-points deployed case that applies the voucher, verifies the exact points balance, and completes booking. |
| Voucher value above fare; use only the required voucher amount | **Partial** | Service tests `CapsCompanionCoverageAtTheCompanionFare_WhenVoucherValueIsHigher` and `UsesOnlyTheUpgradePointsDifference_WhenVoucherValueExceedsIt` | Add deployed basket and completed-booking assertions, including the retained voucher balance if that is exposed. |
| Multiple vouchers are used as either Companion or Upgrade in one flow | **Partial** | Service tests cover multiple vouchers for Companion and Upgrade; TC25 uses exactly two vouchers; TC31/TC38 cover best-option persistence when their feature flags are enabled | The full SIT flow through payment/confirmation is not covered. There is no explicit assertion preventing a mixed Companion-plus-Upgrade application. |
| Upgrade: base fare for all passengers is paid in points and only upgrade delta uses vouchers | **Strong calculation coverage; partial journey coverage** | `VoucherUpgradePointsCalculatorTests`, TC25, TC26 and TC32 cover eligible-passenger baseline/delta calculations and voucher caps | Enable basket checks and add payment/confirmation assertions for all passengers. |
| Companion: one primary passenger per companion; a one-way voucher covers one two-person pairing; a return can use two vouchers across the two sectors | **Implemented and passed** | TC17/TC18 cover two adults; TC21 rejects one passenger; TC42/TC43 cover the three-passenger boundary; corrected TC44 passed with one voucher and `coveredPassengers=2`; TC45 passed with two vouchers covering the same pair across two return sectors | No remaining backend automation gap for this rule. |
| Maximum two vouchers in MVP | **Covered at service/API validation level** | Service tests cap selection at two; Book mutation test `SelectingMoreThanTwoVouchers_ReturnsBadRequest` rejects more than two | Add deployed GraphQL and UI assertions, including the rule that the UI presents the two earliest-expiring eligible vouchers. |
| Lap infants cannot redeem vouchers | **Covered at unit level only** | `VoucherRedemptionEligibilityTests` excludes infants; serialization tests preserve infant award points | Add deployed Reward Search/basket cases with adult plus lap infant. |
| Seated infant cannot redeem/book online | **Backend decision required** | None in TC17-TC38 | Confirm whether the API must reject this independently. If yes, add a backend negative contract test; presentation remains FE-owned. |
| Touchdown routes count as one sector and allow Companion/Upgrade | **Missing** | None in TC17-TC38 | Add named touchdown-route one-way scenarios for both redemption types with agreed SIT data. |
| Voucher value is considered with points when deciding whether Reward booking can continue | **Partial** | Selection serialization recalculates `hasSufficientPoints`; TC24/TC25 validate the insufficient-points/Buy Points state | Add positive deployed cases for both Companion and Upgrade where points alone are insufficient but points plus voucher are sufficient. |
| Companion value above voucher value uses additional points | **Covered at unit level; partial SIT coverage** | Service test `CapsCompanionCoverageAtVoucherValue_WhenCompanionFareIsHigher`; TC24 covers the low-balance outcome | Add a sufficient-points deployed selection/basket assertion and complete booking. |
| Companion applies to all sectors; partner sectors are excluded | **Partial** | TC18 covers return Companion; TC28/TC30/TC37 cover partner and mixed-itinerary exclusion | Basket validation is feature-gated and there is no completed booking proving all sectors were ticketed with the redemption. |
| All passengers upgrade to the same cabin | **Partial** | Upgrade calculation tests apply the same delta across eligible passenger count; TC25 uses three adults in Premium | No explicit negative test attempts inconsistent passenger cabins, nor an end-to-end confirmation assertion. |
| Upgrade applies to all sectors; partner sectors are excluded | **Partial** | TC22/TC31/TC32/TC38 exercise return Upgrade calculations; TC30/TC37 cover mixed/partner exclusions | Basket flags must be enabled, then payment and confirmation still need coverage. |
| Saver offer price is used for upgrade delta/fallback | **Missing explicit coverage** | Existing tests verify backend `baseLinePoints` and adjacent-cabin baselines | No test identifies a Saver offer or asserts the specified Saver fallback when delta is unavailable. |
| Risk Free Cancellation refunds voucher and points | **Out of agreed backend automation scope** | This is a post-booking servicing concern, not part of the voucher Search/Book stories being signed off | No test required for this work. |
| Basket expires after 20 minutes and journey must restart | **Out of agreed backend automation scope** | This is generic basket lifecycle behaviour rather than a voucher-specific acceptance requirement | No test required for this work. |
| Known business errors and unknown technical failures are handled clearly | **Partial backend negatives** | Current tests cover ineligible passengers, expired vouchers, partner itineraries, missing vouchers and mutation validation | Add backend contract assertions for stable voucher error codes, retryability and redirect/restart indicators. User-facing copy remains FE-owned. |
| Payment UI restricts invalid voucher combinations | **API rule partially covered; UI out of backend scope** | More-than-two mutation rejection is covered | Add backend tests for the one-sector Companion/Upgrade selection rules and ordering/filtering of the two eligible vouchers returned to FE. |
| Three logged-out Revenue-to-Reward edit/toggle journeys show the first voucher popup after login | **FE-owned; out of backend scope** | Not applicable to backend Search automation | Validate in the separate FE suite. No backend journey test is proposed. |

### Coverage conclusion for the supplied list

Within the agreed Search/Book backend scope, the automation now covers value capping, upgrade deltas, voucher-count limits, passenger boundaries, expiry, partner restrictions, selection/basket behaviour and the required booking/confirmation contracts. Risk Free Cancellation and generic basket expiry are explicitly out of scope; My Account navigation and Revenue-to-Reward popup transitions are FE-owned. The remaining named specialist rules (for example touchdown, seated infant and Saver fallback) require confirmation that they belong to these stories before being treated as sign-off gaps. Companion booking and downstream remark evidence are proven; Upgrade confirmation remains dependent on resolving the Payment-basket persistence defect.

Recommended delivery order:

1. Enable the existing basket and selection switches and rerun TC17-TC38 so currently gated assertions actually execute.
2. Add backend SIT cases for exact/equivalent/shortfall values, positive points-plus-voucher continuation, four-passenger Companion boundaries, touchdown and Saver fallback.
3. Extend Book automation through payment and confirmation for representative Companion, Upgrade and two-voucher cases.
4. Add backend selection-validation cases for one-sector rules, two-voucher filtering/order and stable business/technical error contracts.
5. Add separate servicing API coverage for Risk Free Cancellation and refunds.

## Sprint 145/146 backend stories

The following additions target the backend acceptance criteria in Azure DevOps stories **857259** (Buy Virgin Points / Payment) and **858265** (Upgrade voucher / Summary and Payment):

| Test | Story | Backend behaviour covered |
| --- | --- | --- |
| TC23 (extended) | 857259 | Selects a non-best voucher option, verifies recalculated award/voucher/remaining points and sufficient-points state, confirms the selection and amounts are persisted in the payment basket, then removes the selection and confirms it is cleared. Requires the existing selection feature switch. |
| TC24/TC25 (extended) | 857259 | Proves Companion and Upgrade insufficient-points decisions and verifies that `hasSufficientPoints` and `pointsToPay` remain consistent between Search Summary and the payment basket. These are the backend signals used by FE to suppress card entry and offer Buy Points. |
| TC34 (extended) | 857259 | Starts with two vouchers whose expiry dates straddle outbound travel and proves that only the eligible later-expiring voucher is returned and used for Companion. |
| TC39 | 857259 | Uses an account with at least three valid vouchers and asserts that only the two earliest-expiring eligible vouchers are returned to the consumer. |
| TC40 | 858265 | Creates a return reward itinerary with Economy outbound and Premium inbound, verifies the mixed cabins, proves the outbound delta is zero and only the inbound upgrade delta consumes voucher value, and conditionally verifies payment-basket persistence. |
| TC41 | 857259 | Starts with two vouchers whose expiry dates straddle outbound travel and proves that only the eligible later-expiring voucher is returned and used for Upgrade. |
| Confirmation contract tests | 857494 | Prove that the final Book confirmation contract preserves Companion or Upgrade redemption type, selected voucher, voucher points used and remaining member points. A standard reward control proves voucher details remain absent when no voucher was applied. |

Existing TC19, TC22, TC25, TC26, TC30, TC32 and TC37 continue to cover one-way/return Upgrade eligibility, cabin calculations, insufficient points and partner-flight exclusions. Unit and mutation tests cover invalid selection requests and the maximum-two-voucher rule. The new tests are deliberately additive rather than duplicating those cases.

Story 857259's rule that the response contains only the two earliest eligible vouchers is stricter than the existing redemption calculation's maximum-two-vouchers rule. TC39 is therefore expected to identify a product defect if the deployed API still returns every available voucher while merely limiting redemption combinations to two.

Story 857494 has deterministic Web response-contract coverage for Companion, Upgrade and no-voucher confirmation summaries, plus completed Companion and Upgrade SIT/PNR evidence recorded above.

### Focused SIT execution (8 September 2026)

- **TC23 passed with selection and payment-basket validation enabled**: voucher selection, recalculated Search Summary amounts, payment-basket persistence, refreshed Summary, voucher removal and cleared basket state all completed successfully.
- **TC40 passed with payment-basket validation enabled**: Economy outbound/Premium inbound was returned, only the inbound cabin delta consumed voucher value, and the redemption details persisted into the payment basket.
- **TC39 passed** with three eligible, differently dated vouchers and confirmed that only the two earliest-expiring vouchers were returned.
- **TC24 and TC25 passed** with payment-basket validation enabled and confirmed consistent insufficient-points signals for Companion and Upgrade.
- **TC34 and TC41 passed** and confirmed one-of-two eligibility for Companion and Upgrade respectively.
- The focused story-857259 run completed **5/5 passed** for TC23, TC24, TC25, TC34 and TC41. A separate TC39 verification also passed.
- Card controls and Buy Points presentation remain FE responsibilities. Backend automation asserts the contract fields that drive those controls: `hasSufficientPoints`, `pointsToPay`, voucher selection and basket state.

## Recommended acceptance position

Story 857259 has implemented backend API coverage for selection/removal, recalculated pricing, sufficient and insufficient points decisions, payment-basket consistency, one-of-two Companion and Upgrade eligibility, and the earliest-two rule for accounts with more than two vouchers. It cannot currently be signed off because the 9 October SIT run shows that a successful selection is not retained by the payment basket.

Visible card enablement, Buy Points presentation and alternative-option rendering must be covered by the FE suite. Logging and monitoring are operational acceptance checks rather than GraphQL response assertions.
