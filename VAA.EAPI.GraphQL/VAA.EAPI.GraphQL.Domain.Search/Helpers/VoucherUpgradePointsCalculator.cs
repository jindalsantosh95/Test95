using VAA.EAPI.GraphQL.Common.Extensions;
using VAA.EAPI.GraphQL.Domain.Search.Models.BFFSummary;
using VAA.EAPI.GraphQL.Domain.Search.Models.FlightOffer;


namespace VAA.EAPI.GraphQL.Domain.Search.Helpers
{
    internal static class VoucherUpgradePointsCalculator
    {
        private const string AwardEconomyFareFamily = "AWARD-ECONOMY";

        public static bool TryCalculateTotalUpgradePoints(
            IEnumerable<FlightOfferResponse?> offerDetailsResponses,
            IEnumerable<Flight>? selectedFlights,
            int eligiblePassengerCount,
            out long totalUpgradePoints)
        {
            return TryCalculateTotalUpgradePoints(
                offerDetailsResponses,
                selectedFlights,
                eligiblePassengerCount,
                out totalUpgradePoints,
                out _);
        }

        public static bool TryCalculateTotalUpgradePoints(
            IEnumerable<FlightOfferResponse?> offerDetailsResponses,
            IEnumerable<Flight>? selectedFlights,
            int eligiblePassengerCount,
            out long totalUpgradePoints,
            out long totalBaselinePoints)
        {
            totalUpgradePoints = 0;
            totalBaselinePoints = 0;

            if (eligiblePassengerCount <= 0 || !HasNonEconomySelectedFlight(selectedFlights))
            {
                return false;
            }

            var fareSegments = offerDetailsResponses
                .Where(offerDetailsResponse => offerDetailsResponse != null)
                .Select(offerDetailsResponse => offerDetailsResponse!.Offers?.FirstOrDefault())
                .Where(offer => offer != null)
                .SelectMany(offer => offer!.OfferItems?.Take(1) ?? Enumerable.Empty<OfferItem>())
                .SelectMany(offerItem => offerItem.Services ?? Enumerable.Empty<OfferService>())
                .SelectMany(service => service.FareDetails?.FareSegments ?? Enumerable.Empty<FareSegment>())
                .ToArray();

            var upgradedFareSegments = fareSegments
                .Select(fareSegment => new
                {
                    FareSegment = fareSegment,
                    PointsDifference = (long)(fareSegment.CurrentCabinSalePrice?.PointsAmount?.PointsCount ?? 0)
                        - (fareSegment.BaselineCabinSalePrice?.PointsAmount?.PointsCount ?? 0)
                })
                .Where(fareSegment => fareSegment.PointsDifference > 0)
                .ToArray();

            var upgradePointsDifference = upgradedFareSegments.Sum(fareSegment => fareSegment.PointsDifference);

            if (upgradePointsDifference <= 0)
            {
                return false;
            }

            totalUpgradePoints = checked(upgradePointsDifference * eligiblePassengerCount);
            totalBaselinePoints = checked(upgradedFareSegments.Sum(fareSegment =>
                (long)(fareSegment.FareSegment.BaselineCabinSalePrice?.PointsAmount?.PointsCount ?? 0)) * eligiblePassengerCount);
            return true;
        }

        private static bool HasNonEconomySelectedFlight(IEnumerable<Flight>? selectedFlights)
        {
            var flights = selectedFlights?.ToArray();
            return flights is { Length: > 0 } && flights.Any(flight => !flight.FareFamilyType.Is(AwardEconomyFareFamily));
        }
    }
}
