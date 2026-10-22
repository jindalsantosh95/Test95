# Voucher frontend automation coverage

This is the traceability guide for the Search & Book voucher frontend stories. The executable
journeys live in `Tests/Journeys/VoucherRedemptionJourneyTests.cs`, use the page-object boundary,
and run only against `vaa-test4` while the feature remains environment-restricted.

## Story coverage

| Work item | Frontend behaviour | Automated coverage |
| --- | --- | --- |
| 862129 | Eligible VS reward itinerary shows only backend-supplied voucher choices and best value | `EligibleVoucher_IsOfferedForVirginAtlanticRewardFlight`; `ReturnVoucher_UpgradePreferredWhenAtLeastAsValuableAtPayment` |
| 862129 | Voucher expired before outbound is excluded and the booking can continue | `VoucherExpiringBeforeOutbound_IsNotOfferedAndBookingCanContinue` |
| 862129 | Partner-operated itinerary excludes voucher choices and continues without a voucher | `PartnerOperatedFlight_DoesNotOfferVoucherAndBookingCanContinue` (retained as an explicit skip until the frontend continuation is released) |
| 862129 | Voucher valid for outbound but expired before inbound remains eligible | `ReturnVoucher_ExpiringAfterOutboundBeforeInbound_RemainsEligible` |
| 862129 | One passenger with no eligible Upgrade/Companion sees the unavailable state and points option | `SinglePassengerEconomyFare_ShowsVoucherUnavailableAndPointsOption` |
| 862129 | Insufficient balance displays the voucher and points shortfall without allowing continuation | `VoucherAndPointsBalanceTooLow_ShowsInsufficientPointsAndBuyPointsOnSummary` |
| 862130 | Upgrade is preferred when it is at least as valuable as Companion; exact tie precedence remains deterministic in backend coverage | `ReturnVoucher_UpgradePreferredWhenAtLeastAsValuableAtPayment` |
| 862130 | Companion removal and re-selection refreshes pricing and exposes no technical GraphQL error | `CompanionVoucher_RemoveAndReapply_RecalculatesPaymentWithoutTechnicalError` |
| 862130 | More than two vouchers exposes only the two earliest expiry dates at Payment | `MoreThanTwoVouchers_PaymentDisplaysTwoEarliestExpiryDates` |
| 862131 | Successful redemption reaches Confirmation and renders the booking reference, voucher benefit and Virgin Points breakdown | `SuccessfulVoucherRedemption_ConfirmationShowsVoucherAndPointsBreakdown` |

The Confirmation journey submits payment and creates a test booking. It is separated with
`Category=SearchAndBook-UI-Voucher-Payment` and runs as part of the normal Test4 voucher pack.
Test bookings are permitted in that environment.

## Remaining release-dependent coverage

The following acceptance criteria need stable frontend contracts or purpose-built storage-table
data before they can be reliable scheduled journeys:

- every sold-out Economy, Premium and Upper cabin combination;
- mixed-cabin Upgrade rendering and the two-voucher/two-passenger ratio combinations;
- the Payment insufficient-points state if the released frontend permits progression past Summary;
- deterministic payment/redemption rejection responses and duplicate-submit protection;
- backend warning, information and error variants on Confirmation.

These cases must not be implemented with fixed dates, fixed member numbers, intercepted production
responses, or blind retries. Add a storage-table discriminator or an approved deterministic
frontend test seam first, then extend the same page objects and story traits.

## Selection

Use `Category=SearchAndBook-UI-Vouchers` for non-booking cases and
`Category=SearchAndBook-UI-Voucher-Payment` for the booking-producing Confirmation case. The manual
`vouchers` workflow remains non-booking; run the payment category through the custom selection when
Confirmation coverage is required. No additional environment-variable opt-in is needed. Select an
individual story with `WorkItem=862129`, `WorkItem=862130`, or `WorkItem=862131`.
