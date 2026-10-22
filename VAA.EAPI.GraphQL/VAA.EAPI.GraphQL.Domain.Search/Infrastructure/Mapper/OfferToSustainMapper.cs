using VAA.EAPI.GraphQL.Domain.Search.Models.FlightOffer;
using VAA.EAPI.GraphQL.Domain.Search.Models.Sustain;

namespace VAA.EAPI.GraphQL.Domain.Search.Infrastructure.Mapper
{
    public class OfferToSustainMapper
    {
        internal static IEnumerable<SustainMSRequest> Map(FlightOfferResponse offerResponse)
        {
            IEnumerable<SustainMSRequest> sustainMSRequests = Enumerable.Empty<SustainMSRequest>();

            var fSegmentIDs = from services in offerResponse.Offers.FirstOrDefault().OfferItems.FirstOrDefault().Services
                              from jour in offerResponse.DataList.Journey
                              from fsegments in offerResponse.DataList.FlightSegments
                              where jour.JourneyId.Contains(services.JourneyRefIds.FirstOrDefault())
                              where jour.FlightSegmentRefIds.Contains(fsegments.SegmentId) && fsegments.OperatingCarrier == Constants.Application.FILTER_AIRLINE_NAME_LIST_FIRST
                              select new { fsegments.ScheduledDepartureTime.LocalDateTime, fsegments.OriginAirportCode, fsegments.DestinationAirportCode, jour.JourneyId, fsegments.MarketingCarrier, fsegments.MarketingFlightNumber };


            foreach (var fs in fSegmentIDs)
            {
                var flightDetails = new FlightDetail[]
                {
                    new ()
                    {
                        OriginAirport = fs.OriginAirportCode,
                        DestinationAirport = fs.DestinationAirportCode,
                        //FlightDate = DateTime.ParseExact(fs.LocalDateTime, "yyyy-MM-ddTHH:mm", System.Globalization.CultureInfo.InvariantCulture),// to be updated based on CHOOOSE and Suistainabiliity team response
                        FlightNumber =$"{fs.MarketingCarrier}{fs.MarketingFlightNumber}",
                        Cabin = Search.Models.Sustain.CabinType.Economy
                    },
                    new ()
                    {
                        OriginAirport = fs.OriginAirportCode,
                        DestinationAirport = fs.DestinationAirportCode,
                        //FlightDate = DateTime.ParseExact(fs.LocalDateTime, "yyyy-MM-ddTHH:mm", System.Globalization.CultureInfo.InvariantCulture),
                        FlightNumber =$"{fs.MarketingCarrier}{fs.MarketingFlightNumber}",
                        Cabin = Search.Models.Sustain.CabinType.Premium
                    },
                    new ()
                    {
                        OriginAirport = fs.OriginAirportCode,
                        DestinationAirport = fs.DestinationAirportCode,
                        //FlightDate = DateTime.ParseExact(fs.LocalDateTime, "yyyy-MM-ddTHH:mm", System.Globalization.CultureInfo.InvariantCulture),
                        FlightNumber =$"{fs.MarketingCarrier}{fs.MarketingFlightNumber}",
                        Cabin = Search.Models.Sustain.CabinType.Business
                    },
                    new ()
                    {
                        OriginAirport = fs.OriginAirportCode,
                        DestinationAirport = fs.DestinationAirportCode,
                        Cabin = Search.Models.Sustain.CabinType.Economy
                    },
                    new ()
                    {
                        OriginAirport = fs.OriginAirportCode,
                        DestinationAirport = fs.DestinationAirportCode,
                        Cabin = Search.Models.Sustain.CabinType.Premium
                    },
                    new ()
                    {
                        OriginAirport = fs.OriginAirportCode,
                       DestinationAirport = fs.DestinationAirportCode,
                        Cabin = Search.Models.Sustain.CabinType.Business
                    }
                };

                sustainMSRequests = sustainMSRequests.Append(new SustainMSRequest() { FlightDetails = flightDetails });
            }
            ;

            return sustainMSRequests;
        }
    }
}