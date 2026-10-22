# Bug: Return Upgrade Search Summary omits one eligible member voucher

| Field | Detail |
| --- | --- |
| Environment | SIT |
| Date observed | 9 October 2026 |
| Area | Search GraphQL — reward voucher eligibility |
| Automation | `SearchRewardGraphQl_TC22_ReturnTrip_LoggedInVoucherAccountReturnsUpgradeVoucherSummary` |
| Suggested severity | Medium |
| Reproducibility | Seen in the fully enabled voucher suite; the same one-of-two pattern also appeared against other randomly selected Upgrade accounts in preceding runs |

## Summary

A logged-in Flying Club member with two vouchers recorded as eligible for the outbound travel date received only one voucher in `searchSummary.voucherDetails.availableVouchers` for a return Premium reward journey.

The returned voucher was used to construct a valid Upgrade option, but the second expected voucher was absent. This prevents the consumer from presenting or calculating any redemption option involving that voucher.

## Steps to reproduce

1. Use a logged-in Flying Club account whose stored voucher response contains two vouchers valid on or after the planned outbound date.
2. Search for a return LHR-JFK reward journey for one adult.
3. Select Premium reward fares and request Offer Price/Search Summary with voucher details.
4. Compare `voucherDetails.availableVouchers` with the member's eligible vouchers from the Customer Transactions voucher response/storage fixture.

## Expected result

`voucherDetails.availableVouchers` contains both eligible voucher IDs, subject to the documented maximum-two rule, and redemption options are calculated using the eligible set.

## Actual result

For the member fixture retained in the restricted execution evidence:

- Expected eligible vouchers: 2
- Actual eligible vouchers returned: 1
- Missing eligible vouchers: 1

The response returned one Upgrade option using the first voucher:

- Redemption type: `UPGRADE`
- Applied vouchers: 1
- Covered passengers: 1
- Baseline points: 24,000
- Points to pay: 24,000
- Voucher points used: 27,500

## Impact

- The frontend receives an incomplete set of eligible vouchers.
- A member may be unable to use or see a valid voucher.
- Multi-voucher Upgrade calculations may be unavailable even when the member holds two eligible vouchers.
- Behaviour depends on which account is selected, making the customer experience and regression tests inconsistent.

## Evidence

- Full suite result: 27 passed, 3 failed.
- TRX: `TestResults/voucher-full-suite-sit-20261009-all-enabled/voucher-full-suite-sit-20261009-all-enabled.trx`
- First automation frame: `SearchRewardsGraphQl.cs`, `AssertUpgradeVoucherSummary`, line 2117 in the tested branch.
- Basket and selection validation switches were enabled; this was not a feature-gated/no-op result.

## Investigation requested

Please confirm why one of the two fixture vouchers was excluded and identify the eligibility rule applied to it. The exact member and voucher identifiers are retained only in the restricted TRX/run evidence. In particular, compare both vouchers for:

- voucher product/type;
- status and remaining balance;
- expiry against the outbound date;
- route, cabin and Upgrade eligibility;
- any maximum-voucher ordering/filtering rule.

If the omitted voucher is intentionally ineligible due to its product type, the backend should expose or document the rejection reason so consumers and automation can distinguish valid filtering from lost voucher data. If both vouchers are eligible, this is a Search Summary filtering defect.

## Retest acceptance

The issue is resolved when the same account and journey either:

1. return both eligible voucher IDs in `availableVouchers`; or
2. return a defined backend eligibility reason proving why the omitted voucher must not be available.

The TC22 automation should then pass without weakening its eligible-voucher assertion.
