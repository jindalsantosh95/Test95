using VAA.EAPI.GraphQL.Common.Helper;
using Basket = VAA.EAPI.GraphQL.Domain.Book.Models.Basket.Basket;
using Summary = VAA.EAPI.GraphQL.Domain.Book.Models.FlightSummary.Summary;

namespace VAA.EAPI.GraphQL.Domain.Book.Helpers
{
    internal static class VoucherRedemptionFeatureHelper
    {
        internal static bool IsEnabled() => ConfigReader.Try<bool>(Constants.Application.VoucherRedemptionFeatureFlag, false);

        internal static Summary? Normalize(Summary? summary) => summary;

        internal static void HydrateBasketFromPersistedSummary(Basket basket, Summary? summary)
        {
            if (summary?.VoucherRedemptionSelection is null)
                return;

            basket.HasSufficientPoints = summary.HasSufficientPoints;
            basket.VoucherApplied = summary.VoucherApplied;
            basket.VoucherPoints = summary.VoucherPoints ?? summary.Price?.VoucherPoints;
            basket.PointsToPay = summary.PointsToPay ?? summary.Price?.PointsToPay;
            basket.VoucherDetails = summary.VoucherDetails;
            basket.VoucherRedemptionSelection = summary.VoucherRedemptionSelection;
        }
    }
}
