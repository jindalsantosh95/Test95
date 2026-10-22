using CSharpFunctionalExtensions;
using System.Linq;
using VAA.EAPI.Microservices.Retail.Offer.Application.Query.Delta.FlightOfferPrice;
using VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Proxy.DeltaOffer;
using Passenger = VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Proxy.DeltaOffer.Passenger;

namespace VAA.EAPI.Microservices.Retail.Offer.Mapper.Delta.FlightOfferPrice
{
    public class OfferMSToDeltaMapper
    {
        public static OfferSearchCriteria DeeplinkSummaryRequestMapper(OfferPriceDeeplinkQuery offerPriceDeeplinkQuery)
        {
            return new OfferSearchCriteria()
            {
                Customers = (from cus in offerPriceDeeplinkQuery.Request.CustomerDetails
                             select new Passenger
                             {
                                 PassengerTypeCode = cus.Ptc,
                                 PassengerId = cus.CustId
                             }),

                Trips = (from trip in offerPriceDeeplinkQuery.Request.FlightSegments.GroupBy(fs => fs.SliceNumber)
                         select new Trip
                         {
                             OriginAirportCode = trip.First().OriginAirportCode,
                             DestinationAirportCode = trip.Last().DestinationAirportCode,
                             TripId = trip.Key.ToString(),
                             TripBrandId = offerPriceDeeplinkQuery.Request.DominantSegmentBrandId,
                             FlightSegmentIdList = trip.Select(fs => fs.SegmentId.ToString())
                         }),

                CustomerItinerarySegmentList = (from segmentList in offerPriceDeeplinkQuery.Request.FlightSegments
                                                select new FlightSegment
                                                {
                                                    OriginAirportCode = segmentList.OriginAirportCode,
                                                    DestinationAirportCode = segmentList.DestinationAirportCode,
                                                    ScheduledDepartureLocalTs = segmentList.ScheduledDepartureLocalDateTime,
                                                    MarketingClassOfServiceCode = segmentList.MarketingClassOfService,

                                                    MarketingCarrier = new Carrier()
                                                    {
                                                        CarrierCode = segmentList.MarketingCarrierCode

                                                    },
                                                    MarketingFlightNum = segmentList.MarketingFlightNumber,
                                                    FlightSegmentId = segmentList.SegmentId.ToString(),

                                                }),

                OffersCriteria = new()
                {
                    SearchWithContext = true,
                    FlightRequestCriteria = new() { CurrentTripIndexId = offerPriceDeeplinkQuery.Request.CurrentTripIndexId },
                    PricingCriteria = new()
                    {

                        PriceableIn = [Constants.Application.Currency],
                        OfferAmount = new()
                        {
                            CurrencyEquivalentPrice = new()
                            {
                                CurrencyCode = offerPriceDeeplinkQuery.Request.TotalOfferedAmount.Currency,
                                CurrencyAmt = offerPriceDeeplinkQuery.Request.TotalOfferedAmount.Value,
                            }
                        }
                    }
                },
                Sender = (Party)offerPriceDeeplinkQuery.Request.LoyaltyMember,
                ProductGroups = [new() { ProductCategoryCode = ProductCategory.ProductCategoryCodeEnum.FLIGHTS }],
            };
        }
    }
}
