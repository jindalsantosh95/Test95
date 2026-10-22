using VAA.EAPI.GraphQL.Domain.Search.Infrastructure.Models.ReferenceMS;
using VAA.EAPI.GraphQL.Domain.Search.Models.Brand;
using VAA.EAPI.GraphQL.Common.Extensions;
using VAA.EAPI.GraphQL.Domain.Search.Models.FlightOffer;

namespace VAA.EAPI.GraphQL.Domain.Search.Infrastructure.Mapper
{
    public class BrandMsRequestMapper
    {
        private const int CabinEconomy = 1;
        private const int CabinPremium = 2;
        private const int CabinUpper = 3;
        private static readonly Dictionary<string, int> fareFamilyDictionary = new()
            {
                { "ECONOMY-LIGHT", CabinEconomy },
                { "ECON", CabinEconomy },
                { "ECON-CLASSIC-FLEXIBLE", CabinEconomy },
                { "ECONOMY-DELIGHT", CabinEconomy },
                { "ECONOMY-DELIGHT-LIGHT", CabinEconomy },
                { "ECON-DELIGHT-FLEXIBLE", CabinEconomy },
                { "AWARD-ECONOMY", CabinEconomy },
                { "COMFORT-PLUS-PREMIUM-ECON", CabinPremium },
                { "COMFORT-PLUS-PREMIUM-LIGHT", CabinPremium },
                { "COMFORT-PLUS-PREMIUM-FLEXIBLE", CabinPremium },
                { "AWARD-COMFORT-PLUS-PREMIUM-ECONOMY", CabinPremium },
                { "BUS-FIRST", CabinUpper },
                { "BUS-FIRST-LIGHT", CabinUpper },
                { "BUS-FIRST-FLEXIBLE", CabinUpper },
                { "AWARD-BUSINESS-FIRST", CabinUpper }
            };

        public static BrandQueryRequest BrandMsReqMapper(FlightOfferResponse offerDbResponse, IEnumerable<CabinInfo> cabinInfo, IEnumerable<string> passengerTypeCodes, string? promoCode = null)
        {
            return new BrandQueryRequest()
            {
                Currency = offerDbResponse.Offers.First().SalePrice!.First().Amount.BaseCurrency,
                Itinerary = new List<BrandQueryRequestTrip>
                {
                    new BrandQueryRequestTrip {
                        //PointOfSale = "",//need to apply logic to map POS
                        Segments = GetSegments(offerDbResponse, cabinInfo, offerDbResponse.IsAwardSearchResult),
                        PassengerTypeCodes = offerDbResponse.IsAwardSearchResult ? [Constants.Application.AwardSearchPTCs_ATPCO] : passengerTypeCodes,
                        Source = offerDbResponse.IsAwardSearchResult ? Constants.Application.AwardSearchSource_ATPCO: default ,
                    }
                },
                PseudoCityCode = offerDbResponse.IsAwardSearchResult ? Constants.Application.AwardSearchPseudoCityCode_ATPCO : default,
                AccountCode = offerDbResponse.IsAwardSearchResult
                    ? Constants.Application.AwardSearchAccountCode_ATPCO
                    : !promoCode.IsNullOrWhiteSpace() ? promoCode.ToUpperInvariant() : default
            };
        }

        public static string? GetAppliedPromoCode(FlightOfferResponse offerDbResponse)
        {
            var offers = offerDbResponse.Offers.ToList();
            var selectedOffer = offers.FirstOrDefault(offer => offer.Selected);

            if (selectedOffer is not null)
            {
                return selectedOffer.PromoCodeApplied;
            }

            return offers.Count == 1 ? offers[0].PromoCodeApplied : null;
        }

        private static IEnumerable<BrandQueryRequestSegment> GetSegments(FlightOfferResponse offerDbResponse, IEnumerable<CabinInfo> cabinInfo, bool isAwardSearch)
        {
            var services = offerDbResponse
                .Offers.First()
                .OfferItems.First()
                .Services;

            return services
                .SelectMany(service => service.FlightSegmentsRefIds!.Select(refId => GetSegment(
                    offerDbResponse.DataList.FlightSegments.Single(f => f.SegmentId == refId),
                    service.FareDetails.FareSegments.First(f => f.FlightSegmentRefID == refId),
                    service.FareDetails.FareBasisCode,
                    cabinInfo,
                    isAwardSearch
                )))
                .OrderBy(s => s.DepartureDate);
        }

        private static BrandQueryRequestSegment GetSegment(Search.Models.FlightOffer.Segment fseg, FareSegment fareSegment, string fareBasisCode, IEnumerable<CabinInfo> cabinInfo, bool isAwardSearch)
        {
            var fareFamily = isAwardSearch ? cabinInfo.FirstOrDefault(ci => ci.BrandID == fareSegment.CabinName)?.AwardFareFamily : cabinInfo.FirstOrDefault(ci => ci.BrandID == fareSegment.CabinName)?.FareFamily;

            if (!fareFamilyDictionary.ContainsKey(fareFamily))
            {
                throw new InvalidDataException($"No cabin mapping for fareFamily: {fareFamily} (Cabin: {fareSegment.CabinName})");
            }

            return new BrandQueryRequestSegment()
            {
                Carrier = fseg.MarketingCarrier,
                FlightNumber = Convert.ToInt16(fseg.MarketingFlightNumber),
                DepartureDate = Common.Helper.DateTimeHelper.ParseLocalDateTime(fseg.ScheduledDepartureTime!.LocalDateTime),
                Origin = fseg.OriginAirportCode,
                Destination = fseg.DestinationAirportCode,
                Cabin = fareFamilyDictionary[fareFamily],
                FareBasis = fareSegment.FareBasisCode ?? fareBasisCode,
                TicketDesignator = fareSegment.TicketDesignator,
                OperatingCarrier = fseg.OperatingCarrier,
                FareFamily = fareFamily
            };
        }
    }
}
