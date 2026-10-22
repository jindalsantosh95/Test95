# Voucher SIT defect reports — 9 October 2026

> **Resolution update — 15 October 2026:** Neither historical finding below remains an open product defect. Task 877978 was fixed and the focused TC23 retest confirmed that the Payment basket now retains the selected voucher and recalculated amounts. Task 877979 clarified that a direct single-sector Companion option applies one voucher to one primary/companion pair, so `coveredPassengers=2` is correct and Upgrade may independently be returned as `bestOption`. TC44 has been corrected to that contract; TC45 passed with two vouchers covering the same pair across two return sectors. The original detail remains below only as historical evidence of what was raised.

These were two independent findings from the focused voucher acceptance pack. The first was fixed in SIT; the second was reclassified as an automation expectation mismatch after business clarification.

No customer credentials or full Flying Club numbers are included below.

## Bug 1: Voucher selection succeeds but is not persisted in the payment basket

| Field | Detail |
| --- | --- |
| Environment | SIT |
| Story | 857259 — Buy Virgin Points / Payment |
| Automation | `SearchRewardGraphQl_TC23_OneWay_SelectedVoucherOptionUpdatesSummaryAmounts` |
| Severity suggestion | High — blocks the selected voucher state reaching the payment journey |
| Reproducibility | Reproduced in the focused SIT rerun on 9 October 2026 |

### Steps to reproduce

1. Sign in with a voucher-backed Flying Club account containing exactly two eligible vouchers.
2. Run a one-way LHR-JFK Economy reward search for two adults.
3. Offer-price the selected itinerary and choose a returned Companion voucher option.
4. Call the voucher-redemption selection mutation with the selected voucher ID.
5. Retrieve the same basket immediately afterwards.

### Expected

The retrieved basket retains the selected voucher IDs, selected option, voucher-applied state and recalculated points values returned by the successful selection mutation.

### Actual

The selection mutation reports a successful applied selection:

- `hasSufficientPoints: true`
- `pointsToPay: "12000"`
- `voucherApplied: true`
- selected voucher IDs and option are returned

The immediate `retrieveBasket` response for the same basket loses that state:

- `membershipPoints: 24000`
- `hasSufficientPoints: true`
- `voucherPoints: null`
- `pointsToPay: null`
- `voucherApplied: false`
- `voucherRedemptionSelection: null`

### Impact

The frontend cannot reliably continue from Summary to Payment with the customer's voucher choice. It also cannot reconstruct the selected state from the basket after navigation or refresh.

### Evidence

- TRX: `TestResults/voucher-story-fixes-sit-20261009/voucher-story-fixes-sit-20261009.trx`
- TRX: `TestResults/voucher-full-suite-sit-20261009-all-enabled/voucher-full-suite-sit-20261009-all-enabled.trx` (full-suite reproduction)
- TC23 previously passed on SIT on 8 September 2026, so this appears to be a regression rather than a missing automation assertion.

## Historical finding 2: Four passengers with two eligible vouchers receive a one-voucher Companion option

| Field | Detail |
| --- | --- |
| Environment | SIT |
| Story | 858309 — Companion passenger/voucher boundaries |
| Automation | `SearchRewardGraphQl_TC44_OneWaySingleSector_FourPassengersTwoVouchersCapsCompanionRedemptionAtOneVoucher` |
| Classification | Automation expectation mismatch — no product fix required |
| Reproducibility | Repeated with multiple voucher accounts and searches on 9 October 2026 |

### Steps to reproduce

1. Sign in with a Flying Club account containing exactly two eligible vouchers.
2. Run a one-way LHR-JFK Economy reward search for four adults.
3. Offer-price the selected itinerary.
4. Inspect the Companion redemption options in the Search Summary response.

### Original expectation (superseded)

The response includes a valid option applying two vouchers to two Companion passengers, with the other two passengers treated as the primary passengers and priced in points.

### Actual and confirmed contract

`Companion - 1 Voucher` is returned for the direct single-sector journey:

- `coveredPassengers: 2`, representing the one-primary/one-companion pair
- one entry in `appliedVouchers`
- Upgrade may be marked as `bestOption`; Companion remains a valid alternative option

The behaviour was repeated with different masked member accounts and different searches. Three-passenger boundary tests behave correctly: one voucher covers one Companion passenger and a second Companion voucher is not incorrectly offered.

### Resolution

No backend defect exists for this response. TC44 now validates one applied voucher and two covered passengers without assuming Companion is the best option. TC45 validates two vouchers across a two-sector return journey for the same two-person pairing.

### Evidence

- TRX: `TestResults/voucher-story-pack-sit-20261009/voucher-story-pack-sit-20261009.trx` (initial pack)
- TRX: `TestResults/voucher-story-fixes-sit-20261009/voucher-story-fixes-sit-20261009.trx` (focused rerun)
- TRX: `TestResults/voucher-full-suite-sit-20261009-all-enabled/voucher-full-suite-sit-20261009-all-enabled.trx` (reproduced using refreshed deterministic voucher data)

### Retest data hardening

The data creator now provisions deterministic Red-tier accounts with exactly two future-valid `Flying Club reward voucher - Virgin Red US credit card` vouchers. TC43 and TC44 require every stored voucher to match that type. This removes random voucher-type selection as an explanation for this failure once the refreshed data creation has run.
