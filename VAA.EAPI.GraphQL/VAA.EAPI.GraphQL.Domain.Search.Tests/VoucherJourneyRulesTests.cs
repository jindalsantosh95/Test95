using System;
using System.Linq;
using VAA.EAPI.GraphQL.Domain.Search.Infrastructure.Models.ReferenceMS;
using VAA.EAPI.GraphQL.Domain.Search.Models.BFFSummary;
using VAA.EAPI.GraphQL.Domain.Search.Models.FlightOffer;
using VAA.EAPI.GraphQL.Domain.Search.Queries;
using VAA.EAPI.GraphQL.Infrastructure.Entities.VRedMS;
using Xunit;
using static VAA.EAPI.GraphQL.Framework.Tests.TestHelpers;
using BffLeg = VAA.EAPI.GraphQL.Domain.Search.Models.BFFSummary.Leg;
using BffSegment = VAA.EAPI.GraphQL.Domain.Search.Models.BFFSummary.Segment;

namespace VAA.EAPI.GraphQL.Domain.Search.Tests
{
    public sealed class VoucherJourneyRulesTests
    {
        [Fact]
        public void IsVoucherJourneyWithinSectorLimit_AllowsOneAndTwoSectorJourneys()
        {
            var oneSectorJourney = new[] { new Flight { Segments = [CreateBffSegment("3")] } };
            var twoSectorJourney = new[] { new Flight { Segments = [CreateBffSegment("3"), CreateBffSegment("4")] } };

            Assert.True(SummaryQueryBase.IsVoucherJourneyWithinSectorLimit(oneSectorJourney));
            Assert.True(SummaryQueryBase.IsVoucherJourneyWithinSectorLimit(twoSectorJourney));
        }

        [Fact]
        public void IsVoucherJourneyWithinSectorLimit_RejectsMoreThanTwoLegsInOneSegment()
        {
            var segment = CreateBffSegment("3");
            segment.Legs = [new BffLeg(), new BffLeg(), new BffLeg()];

            Assert.False(SummaryQueryBase.IsVoucherJourneyWithinSectorLimit([new Flight { Segments = [segment] }]));
        }

        [Fact]
        public void ApplyVoucherJourneyResponse_RejectsOverLimitJourneyWithoutExposingVouchersOrOptions()
        {
            var summary = new Summary
            {
                VoucherDetails = new VoucherRedemptionDetails
                {
                    RedemptionOptions = [new RedemptionOption { OptionId = 1 }]
                }
            };
            var availableVoucher = new VoucherDetail
            {
                VoucherNumber = "eligible-voucher",
                Category = "Companion",
                MaxValue = "75000",
                ExpiryDate = DateTime.Today.AddDays(30)
            };

            SummaryQueryBase.ApplyVoucherJourneyResponse(
                summary,
                new VoucherEligibility { Eligible = true },
                isVoucherJourneyWithinSectorLimit: false,
                accountAvailableVouchers: [availableVoucher],
                vouchersForRedemption: Array.Empty<VoucherDetail>());

            Assert.False(summary.VoucherEligibility!.Eligible);
            Assert.Equal("VOUCHER_JOURNEY_SECTOR_LIMIT_EXCEEDED", summary.VoucherEligibility.ReasonCode);
            Assert.Empty(summary.VoucherDetails!.AvailableVouchers!);
            Assert.Empty(summary.VoucherDetails.RedemptionOptions ?? Enumerable.Empty<RedemptionOption>());
        }

        [Fact]
        public void GetMaximumVouchersForJourney_LimitsOnlyDirectOneWayCompanionRedemptions()
        {
            Assert.Equal(1, SummaryQueryBase.GetMaximumCompanionVouchersForJourney(isDirectVirginAtlanticOneWay: true));
            Assert.Equal(2, SummaryQueryBase.GetMaximumCompanionVouchersForJourney(isDirectVirginAtlanticOneWay: false));
        }

        private static BffSegment CreateBffSegment(string operatingFlightNumber)
        {
            var offer = TestDataProvider.GetTestData<FlightOfferResponse>("OfferPriceResponse");
            var segment = offer.DataList.FlightSegments.First();
            segment.MarketingCarrier = "VS";
            segment.MarketingFlightNumber = operatingFlightNumber;
            segment.OperatingCarrier = "VS";
            segment.OperatingFlightNumber = operatingFlightNumber;
            segment.ConnectionOrStopOver = "0";

            return new BffSegment(
                segment,
                offer.Offers.First().OfferItems.First().Services.First(),
                stopCount: 0,
                cabinInfo: Enumerable.Empty<CabinInfo>(),
                aircraftInfo: Enumerable.Empty<AircraftInfo>(),
                airlineInfo: [new AirlineInfo { CarrierCode = "VS", CarrierName = "Virgin Atlantic" }],
                airportInfo: Enumerable.Empty<AirportInfo>());
        }
    }
}
