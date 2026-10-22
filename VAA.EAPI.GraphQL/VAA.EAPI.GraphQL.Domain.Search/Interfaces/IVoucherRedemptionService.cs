using VAA.EAPI.GraphQL.Domain.Search.Models.Basket;
using VAA.EAPI.GraphQL.Domain.Search.Models.BFFSummary;
using VAA.EAPI.GraphQL.Domain.Search.Models.FlightOffer;

namespace VAA.EAPI.GraphQL.Domain.Search.Interfaces
{
    public interface IVoucherRedemptionService
    {
        Task<(IEnumerable<RedemptionOption> RedemptionOptions, BestRedemptionOption BestOption)?> GetVoucherRedemptionOffersForCompanion(
            FlightOfferResponse flightSummaryResponse,
            IEnumerable<VoucherDetail>? availableVouchers,
            int eligibleCompanionPaxCount,
            IEnumerable<BookingJourneyOfferDetails>? bookingJourneyOfferDetails,
            long totalPointsForCompanionToPay,
            long totalAwardPointsNeededToPay,
            int? maxVouchersToUse = null,
            string? voucherIdToUse = null);

        Task<(IEnumerable<RedemptionOption> RedemptionOptions, BestRedemptionOption BestOption)?> GetVoucherRedemptionOffersForUpgrade(
            FlightOfferResponse flightSummaryResponse,
            IEnumerable<VoucherDetail>? availableVouchers,
            int eligiblePaxCountExcludingInfants,
            long totalUpgradePoints,
            long totalAwardPointsNeededToPay,
            long totalBaselinePoints,
            int? maxVouchersToUse = null,
            string? voucherIdToUse = null);

        BestRedemptionOption? SelectBestRedemptionOption(IEnumerable<RedemptionOption>? redemptionOptions);
    }
}
