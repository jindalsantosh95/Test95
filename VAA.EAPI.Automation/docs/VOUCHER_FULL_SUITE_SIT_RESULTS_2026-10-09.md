# Voucher backend full-suite results — SIT — 9 October 2026

> **15 October clarification:** The TC44 failure below is retained as historical evidence. Task 877979 confirmed that `Companion - 1 Voucher` is correct for a direct single-sector journey and that `coveredPassengers=2` represents the one-primary/one-companion pairing. Companion need not be `bestOption`; Upgrade can be selected as best while Companion remains a valid alternative. The automation expectation has been corrected, and the two-sector TC45 case passed with two vouchers covering the same two-person pairing across the return sectors.

## Result

| Measure | Result |
| --- | --- |
| Scope | Search GraphQL voucher TC17-TC44 |
| Discovered cases | 30 (TC32 and TC33 are parameterised) |
| Passed | 27 |
| Failed | 3 |
| Skipped | 0 |
| Duration | 1 minute 24 seconds |
| Summary, basket and selection switches | Enabled and exercised |
| TRX | `TestResults/voucher-full-suite-sit-20261009-all-enabled/voucher-full-suite-sit-20261009-all-enabled.trx` |

## Passed

TC17, TC18, TC19, TC20, TC21, TC24, TC25, TC26, TC27, TC28, TC29, TC30, TC31, TC32 (LHR-JFK), TC32 (LHR-ATL), TC33 (one voucher), TC33 (two vouchers), TC34, TC35, TC36, TC37, TC38, TC39, TC40, TC41, TC42 and TC43.

## Failed

### TC22 — Return Upgrade response omitted one stored voucher

- Storage expected two eligible voucher IDs.
- Search Summary returned only one of those IDs in `availableVouchers`.
- The Upgrade option used the returned voucher correctly.
- Classification: investigate eligibility/type filtering before raising as a product defect. The account is selected from mixed voucher data, so the missing voucher's product eligibility should be confirmed first.
- Ready-to-paste report: [`BUG_TC22_RETURN_UPGRADE_OMITS_ELIGIBLE_VOUCHER.md`](BUG_TC22_RETURN_UPGRADE_OMITS_ELIGIBLE_VOUCHER.md).

### TC23 — Selected voucher state was not retained by the payment basket

- The selection mutation returned `voucherApplied: true`, `pointsToPay: 6000` and the selected voucher ID/option.
- Immediate basket retrieval returned `voucherApplied: false`, null `pointsToPay` and null `voucherRedemptionSelection`.
- Classification: reproduced backend/product defect. The existing defect report remains valid.

### TC44 — Historical expectation mismatch for a direct single-sector journey

- The test used a newly provisioned deterministic Red account with two future-valid Virgin Red US reward vouchers.
- The response returned a one-voucher Companion option with `coveredPassengers=2`.
- Business clarification confirmed that this represents one primary/companion pair for the one-way sector; Upgrade may independently be `bestOption`.
- Classification: automation expectation mismatch. No product fix is required.

## Data refresh

The prerequisite SIT data refresh completed 13 of 15 rows. The deterministic Red two-voucher row passed and created the data required by TC43/TC44.

Two unrelated legacy rows failed because the available account pool was smaller than requested:

- Gold, minimum 300,000 points: requested three accounts; two available.
- Silver, minimum 300,000 points: requested three accounts; one available.

Data-refresh TRX: `TestResults/voucher-data-refresh-sit-20261009/voucher-data-refresh-sit-20261009.trx`.

## Excluded setup attempts

Earlier attempts that failed due macOS case-sensitive local configuration binding are not acceptance evidence. They used the base SIT URL instead of the configured Search/Book GraphQL paths or did not bind the member password. The result above is from the corrected run with all test bodies exercised.
