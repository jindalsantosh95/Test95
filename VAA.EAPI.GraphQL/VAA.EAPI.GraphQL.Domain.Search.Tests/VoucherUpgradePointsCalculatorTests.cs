using System.Linq;
using VAA.EAPI.GraphQL.Domain.Search.Helpers;
using VAA.EAPI.GraphQL.Domain.Search.Models.FlightOffer;
using Xunit;
using BffFlight = VAA.EAPI.GraphQL.Domain.Search.Models.BFFSummary.Flight;

namespace VAA.EAPI.GraphQL.Domain.Search.Tests
{
    public sealed class VoucherUpgradePointsCalculatorTests
    {
        [Fact]
        public void CalculatesUpgradePointsDifference_ForOutboundAndInboundOffers()
        {
            var calculated = VoucherUpgradePointsCalculator.TryCalculateTotalUpgradePoints(
                [CreateOffer((165000, 74000)), CreateOffer((59000, 32000))],
                [new BffFlight { FareFamilyType = "AWARD-COMFORT-PLUS-PREMIUM-ECONOMY" }],
                eligiblePassengerCount: 2,
                out var totalUpgradePoints);

            Assert.True(calculated);
            Assert.Equal(236000, totalUpgradePoints);
        }

        [Fact]
        public void CalculatesUpgradePointsDifference_ForOutboundOfferOnly_WhenReturnOfferIsNull()
        {
            var calculated = VoucherUpgradePointsCalculator.TryCalculateTotalUpgradePoints(
                [CreateOffer((59000, 32000)), null],
                [new BffFlight { FareFamilyType = "AWARD-COMFORT-PLUS-PREMIUM-ECONOMY" }],
                eligiblePassengerCount: 1,
                out var totalUpgradePoints);

            Assert.True(calculated);
            Assert.Equal(27000, totalUpgradePoints);
        }

        [Fact]
        public void CalculatesUpgradePointsDifference_ForEachEligiblePassenger()
        {
            var calculated = VoucherUpgradePointsCalculator.TryCalculateTotalUpgradePoints(
                [CreateOffer((59000, 42000))],
                [new BffFlight { FareFamilyType = "AWARD-COMFORT-PLUS-PREMIUM-ECONOMY" }],
                eligiblePassengerCount: 3,
                out var totalUpgradePoints);

            Assert.True(calculated);
            Assert.Equal(51000, totalUpgradePoints);
        }

        [Fact]
        public void CalculatesEconomyBaseFare_ForEachEligiblePassenger()
        {
            var calculated = VoucherUpgradePointsCalculator.TryCalculateTotalUpgradePoints(
                [CreateOffer((60000, 40000))],
                [new BffFlight { FareFamilyType = "AWARD-COMFORT-PLUS-PREMIUM-ECONOMY" }],
                eligiblePassengerCount: 2,
                out var totalUpgradePoints,
                out var totalBaselinePoints);

            Assert.True(calculated);
            Assert.Equal(40000, totalUpgradePoints);
            Assert.Equal(80000, totalBaselinePoints);
        }

        [Fact]
        public void ExcludesEconomyReturnFare_WhenOutboundIsUpgraded()
        {
            var calculated = VoucherUpgradePointsCalculator.TryCalculateTotalUpgradePoints(
                [CreateOffer((60000, 40000)), CreateOffer((30000, 30000))],
                [
                    new BffFlight { FareFamilyType = "AWARD-COMFORT-PLUS-PREMIUM-ECONOMY" },
                    new BffFlight { FareFamilyType = "AWARD-ECONOMY" }
                ],
                eligiblePassengerCount: 2,
                out var totalUpgradePoints,
                out var totalBaselinePoints);

            Assert.True(calculated);
            Assert.Equal(40000, totalUpgradePoints);
            Assert.Equal(80000, totalBaselinePoints);
        }

        [Fact]
        public void UsesTheFirstOfferItemForThePassengerDifference_BeforeApplyingEligiblePassengerCount()
        {
            var adultOfferResponse = CreateOffer((165000, 150000));
            var adultOffer = adultOfferResponse.Offers.First();
            adultOffer.OfferItems = adultOffer.OfferItems
                .Concat(CreateOffer((165000, 150000)).Offers.First().OfferItems)
                .ToArray();

            var calculated = VoucherUpgradePointsCalculator.TryCalculateTotalUpgradePoints(
                [adultOfferResponse],
                [new BffFlight { FareFamilyType = "AWARD-COMFORT-PLUS-PREMIUM-ECONOMY" }],
                eligiblePassengerCount: 2,
                out var totalUpgradePoints);

            Assert.True(calculated);
            Assert.Equal(30000, totalUpgradePoints);
        }

        [Fact]
        public void UsesOnlyTheFirstOfferForEachJourney()
        {
            var offerResponse = CreateOffer((59000, 42000));
            offerResponse.Offers = offerResponse.Offers
                .Concat(CreateOffer((180000, 100000)).Offers)
                .ToArray();

            var calculated = VoucherUpgradePointsCalculator.TryCalculateTotalUpgradePoints(
                [offerResponse],
                [new BffFlight { FareFamilyType = "AWARD-COMFORT-PLUS-PREMIUM-ECONOMY" }],
                eligiblePassengerCount: 1,
                out var totalUpgradePoints);

            Assert.True(calculated);
            Assert.Equal(17000, totalUpgradePoints);
        }

        [Fact]
        public void IgnoresNegativeCabinDifference_WhenOtherJourneyHasAPositiveDifference()
        {
            var calculated = VoucherUpgradePointsCalculator.TryCalculateTotalUpgradePoints(
                [CreateOffer((15000, 36000)), CreateOffer((90000, 46000))],
                [new BffFlight { FareFamilyType = "AWARD-COMFORT-PLUS-PREMIUM-ECONOMY" }],
                eligiblePassengerCount: 1,
                out var totalUpgradePoints);

            Assert.True(calculated);
            Assert.Equal(44000, totalUpgradePoints);
        }

        [Fact]
        public void DoesNotCalculateUpgradePoints_WhenNoUpgradePointsDifferenceExists()
        {
            var calculated = VoucherUpgradePointsCalculator.TryCalculateTotalUpgradePoints(
                [CreateOffer((32000, 59000))],
                [new BffFlight { FareFamilyType = "AWARD-ECONOMY" }],
                eligiblePassengerCount: 1,
                out var totalUpgradePoints);

            Assert.False(calculated);
            Assert.Equal(0, totalUpgradePoints);
        }

        private static FlightOfferResponse CreateOffer(params (int CurrentCabinPoints, int BaselineCabinPoints)[] fareSegmentPoints)
        {
            return new FlightOfferResponse
            {
                Offers =
                [
                    new OfferDetails
                    {
                        OfferItems =
                        [
                            new OfferItem
                            {
                                Services =
                                [
                                    new OfferService
                                    {
                                        FareDetails = new FareDetails
                                        {
                                            FareSegments = fareSegmentPoints.Select(fareSegmentPoints => new FareSegment
                                            {
                                                CurrentCabinSalePrice = new Price { PointsAmount = new PointsAmount { PointsCount = fareSegmentPoints.CurrentCabinPoints } },
                                                BaselineCabinSalePrice = new Price { PointsAmount = new PointsAmount { PointsCount = fareSegmentPoints.BaselineCabinPoints } }
                                            })
                                        }
                                    }
                                ]
                            }
                        ]
                    }
                ]
            };
        }
    }
}
