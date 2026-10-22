using VAA.EAPI.GraphQL.Common.Serialisation;
using VAA.EAPI.GraphQL.Domain.Search.Models.Basket;
using VAA.EAPI.GraphQL.Domain.Search.Models.BFFSummary;
using VAA.EAPI.GraphQL.Domain.Search.Models.OfferMS.FlightOffer;

namespace VAA.EAPI.GraphQL.Domain.Search.Mapper
{
    public class BasketRequestMapper
    {
        public static Basket CreateForDeepLink(FlightDeepLinkSummaryRequest offerRequest, string requestedBrandId)
        {
            var basketData = new Basket()
            {
                BasketId = Guid.NewGuid(),
                FlightsearchRequest = new FlightsearchRequest
                {
                    BrandId = requestedBrandId ?? offerRequest.DominantSegmentBrandId,
                    SearchOriginDestinations = offerRequest.FlightSegments.GroupBy(fs => fs.SliceNumber).Select(x => new SearchOriginDestination
                    {
                        Origin = x.First().OriginAirportCode,
                        Destination = x.Last().DestinationAirportCode,
                        DepartureDate = x.First().ScheduledDepartureLocalDateTime.ToString()
                    })
                },
                People = offerRequest.CustomerDetails.Select(c => (People)c),
                MetaPartnerId = offerRequest.MetaPartnerId
            };

            return basketData;
        }

        public static Basket UpdateFromSummary(Basket basketData, EarnPoints earnCalculatorResponse, BasketLoyaltyMemberDetails memberDetails, IEnumerable<string> offerIds, Models.BFFSummary.Summary summaryResponse, string summaryLink)
        {
            basketData.OfferIds = offerIds;
            basketData.EarnPoints = earnCalculatorResponse != null ?
            new BasketFCEarnPoints
            {
                EarnVirginPoints = int.TryParse(earnCalculatorResponse.VirginPoints, out var v) ? v : 0,
                EarnTierPoints = int.TryParse(earnCalculatorResponse.TierPoints, out var t) ? t : 0,
                EarnGroupVirginPoints = int.TryParse(earnCalculatorResponse.GroupVirginPoints, out var g) ? g : 0
            } : new BasketFCEarnPoints();
            basketData.CustomerProfile = memberDetails;
            basketData.FlightSummary = summaryResponse != null ? Json.Serialize(summaryResponse) : string.Empty;
            basketData.SummaryLink = summaryLink;
            basketData.HasSufficientPoints = summaryResponse?.HasSufficientPoints;
            basketData.VoucherPoints = summaryResponse?.Price?.VoucherPoints;
            basketData.PointsToPay = summaryResponse?.Price?.PointsToPay;
            basketData.VoucherDetails = summaryResponse?.VoucherDetails;
            basketData.VoucherApplied = summaryResponse?.VoucherApplied ?? false;
            basketData.VoucherRedemptionSelection = summaryResponse?.VoucherRedemptionSelection;
            return basketData;
        }
    }
}
