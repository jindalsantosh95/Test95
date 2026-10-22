# Voucher Backend Coverage and Execution Report

> **15 October clarification:** Task 877979 confirmed that a direct single-sector Companion option applies one voucher to one primary/companion pair. The response therefore correctly reports `coveredPassengers=2`; this is the pair size, not the number of companions. Upgrade may independently be returned as `bestOption`. The TC44 result below is retained as historical evidence but is reclassified as an automation expectation mismatch. TC44 now asserts the one-voucher/two-covered-passenger contract, while TC45 passed with two vouchers covering the same pair across two return sectors.

| Detail | Value |
| --- | --- |
| Status date | 9 October 2026 |
| Environment | SIT |
| Scope | Backend GraphQL voucher eligibility, Search Summary, basket, payment contract, booking confirmation and PNR remarks |
| Regression result | **30 executed: 27 passed, 3 failed, 0 skipped** |
| Overall position | **Partially signed off — task 877978 is fixed; task 877979 was reclassified as expected behaviour; focused reruns and Upgrade confirmation remain** |

## Executive summary

The backend voucher automation provides strong coverage of the agreed Companion and Upgrade rules. It covers voucher eligibility, passenger and sector restrictions, expiry, operating-carrier restrictions, voucher-count limits, insufficient-points signals, pricing calculations, basket behaviour and representative booking completion.

A Companion Voucher booking completed successfully in SIT and the resulting voucher remark was verified downstream through `PNRCommonV6.Retrieve`.

The suite is not fully green. Two confirmed product defects prevent complete sign-off of Payment, multi-voucher Companion and Upgrade confirmation. One additional Upgrade result remains under test-data/product-eligibility investigation and has not been raised as a confirmed defect.

## Latest execution result

| Result | Count | Notes |
| --- | ---: | --- |
| Passed | 27 | Core eligibility, expiry, partner restrictions, calculations, passenger boundaries and generic voucher-rule scenarios |
| Failed | 3 | TC23 basket persistence; TC44 two-voucher Companion option; TC22 Upgrade fixture discrepancy |
| Skipped | 0 | Basket and selection validation switches were enabled |
| Total | 30 | TC17–TC44, including parameterised cases |

### Failure classification

| Test | Classification | Finding | Action |
| --- | --- | --- | --- |
| TC23 | **Confirmed product defect** | Voucher-selection mutation succeeds, but the immediately following `retrieveBasket` response loses the selected voucher and recalculated amounts | Azure DevOps task **877978** raised; fix and retest required |
| TC44 | **Reclassified: automation expectation mismatch** | Four passengers with two eligible vouchers received `Companion - 1 Voucher`, which is correct for the direct single-sector journey under AC16 | Automation aligned on 15 October; run corrected TC44 and new two-sector TC45 |
| TC22 | **Investigation** | Return Upgrade response contains one of the two stored voucher IDs | Confirm the omitted voucher's product/eligibility before classifying as a defect |

## Story sign-off position

| Story | Status | Evidence and remaining work |
| --- | --- | --- |
| [857259 — Buy Virgin Points / Payment](https://virginatlantic.visualstudio.com/Digital%20Distribution/_workitems/edit/857259) | **Passed — closed** | The deployed fix retains the voucher selection and recalculated amounts in the Payment basket. |
| [857494 — Booking Completion / Confirmation](https://virginatlantic.visualstudio.com/Digital%20Distribution/_workitems/edit/857494) | **Partially passed** | Companion booking and confirmation are proven; a completed Upgrade confirmation still requires evidence. |
| [858265 — Upgrade Summary & Payment](https://virginatlantic.visualstudio.com/Digital%20Distribution/_workitems/edit/858265) | **Retest required** | Eligibility/calculation coverage exists. Run the focused Upgrade Summary/Payment case against the deployed basket-persistence fix. |
| [858286 — Companion AC1/2/7](https://virginatlantic.visualstudio.com/Digital%20Distribution/_workitems/edit/858286) | **Passed — can close** | Eligible/ineligible passenger rules, one-way/return paths, points/pricing contract and a representative completed Companion journey passed. |
| [858309 — Companion AC4/5](https://virginatlantic.visualstudio.com/Digital%20Distribution/_workitems/edit/858309) | **Passed — can close** | TC42/TC43 cover the three-passenger boundaries. Corrected TC44 passed on SIT with one voucher covering one primary/companion pair; TC45 passed for the two-sector return case. |
| [858347 — Companion sector and journey rules AC16](https://virginatlantic.visualstudio.com/Digital%20Distribution/_workitems/edit/858347) | **Passed — can close** | Existing return/two-sector, single-passenger, three-passenger and selection-recalculation cases pass. Corrected TC44 and TC45 provide the required single-sector cap and two-sector positive evidence. |
| [858354 — PNR Remarks AC3/4](https://virginatlantic.visualstudio.com/Digital%20Distribution/_workitems/edit/858354) | **Passed — can close** | Completed booking was retrieved through PNRCommonV6 and the expected voucher/points remark was present. |
| [858604 — Generic Companion/Upgrade rules](https://virginatlantic.visualstudio.com/Digital%20Distribution/_workitems/edit/858604) | **Passed — can close** | All five explicitly defined generic-rule scenarios passed. |
| [866950 — Voucher expiry indicator AC26](https://virginatlantic.visualstudio.com/Digital%20Distribution/_workitems/edit/866950) | **Passed — can close** | One/two-voucher expiry, between-expiry, boundary and inbound-after-expiry rules passed. |

## Confirmed end-to-end evidence

| Evidence | Result |
| --- | --- |
| Journey | Authenticated Companion Voucher booking in SIT |
| Booking access details | Retained only in the restricted GitHub Actions evidence |
| GitHub Actions run | [Run 37938895674](https://github.com/VAAD1ST/eapi/actions/runs/37938895674) |
| Downstream validation | `PNRCommonV6.Retrieve` |
| Remark type / number | `SPCL` / `001` |
| Remark pattern | `ONLINE <transaction-id>. COMPANION VCHR <voucher-points> PTS. TOTAL PTS <total-points>` |

This proves that the tested Companion redemption reached booking completion and that voucher type, voucher points and total points were propagated into the downstream PNR remark.

## Acceptance coverage summary

### Passed coverage

- Eligible Companion options for one-way and return journeys.
- Companion requires at least two passengers.
- Three-passenger Companion boundary with one and two available vouchers.
- Upgrade eligibility for Premium and Upper Class reward bookings.
- Adjacent-cabin Upgrade baseline and points-delta calculations.
- Mixed-cabin Upgrade calculation.
- Insufficient-points and Buy Virgin Points backend indicators.
- Expired voucher exclusion, exact-expiry boundary and differing-expiry filtering.
- Eligibility based on outbound date; inbound may fall after expiry.
- Maximum two returned vouchers and earliest-expiry ordering.
- Partner-operated and mixed-carrier itinerary exclusion.
- Non-voucher continuation when vouchers are unavailable or ineligible.
- Generic one/two-voucher, passenger, sector, cabin and points-balance rules from story 858604.
- Companion booking completion, confirmation consistency and downstream PNR remark.

### Blocked or pending coverage

- Retained voucher selection and recalculated values in the Payment basket.
- Successful Upgrade journey through Payment and booking confirmation.
- TC22 omitted-voucher eligibility classification.
- A genuine live-fare Upgrade/Companion equal-value tie remains environment dependent.

## Raised defects

| Task | Title | Related acceptance area |
| --- | --- | --- |
| [877978](https://virginatlantic.visualstudio.com/Digital%20Distribution/_workitems/edit/877978) | Bug - Voucher selection succeeds but is not retained in the Payment basket | Payment, recalculation and Upgrade continuation; stories 857259, 857494, 858265 and 858347 |
| [877979](https://virginatlantic.visualstudio.com/Digital%20Distribution/_workitems/edit/877979) | **Expected behaviour** — one single-sector voucher covers one primary/companion pair (`coveredPassengers=2`) and Upgrade may be `bestOption` | TC44 corrected; TC45 proves the same pair across two return sectors |

TC22 has deliberately not been raised as a confirmed product bug because the omitted voucher's product eligibility still needs to be established.

## Required retest to complete sign-off

1. Align TC23's automation-only `membershipPoints` comparison and rerun the Upgrade Summary/Payment scenarios against the verified basket-persistence fix.
2. Complete at least one Upgrade Voucher booking and verify confirmation values and downstream evidence for story 857494.
3. Confirm the product/eligibility of both TC22 vouchers and either correct the fixture expectation or raise a product defect.
4. Rerun the full voucher suite and attach the new execution totals to the affected stories when SIT reward search is stable. The 15 October focused run was interrupted by backend `3002` and Delta `G052` errors before voucher assertions.

## Scope notes

- Frontend banners, popup behaviour, navigation and presentation are owned by the FE automation project.
- Risk Free Cancellation/refunds and generic 20-minute basket expiry are outside the agreed backend voucher scope.
- Audit, operational monitoring and log-retention requirements require service/operational evidence in addition to GraphQL response assertions.

## Recommendation

Close stories **858286**, **858354**, **858604** and **866950** from the backend-testing perspective. Keep **857259**, **857494**, **858265**, **858309** and **858347** open until their blocking defects and Upgrade confirmation evidence are resolved.
