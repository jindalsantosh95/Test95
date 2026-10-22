using HotChocolate;

namespace VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.SeatMS
{
    [GraphQLName("TripsSeatMSResponse")]
    public class SeatResponse
    {
        public string SeatOfferId { get; set; }
        public string OfferExpirationTimeLimit { get; set; }
        public IEnumerable<FlightSegmentSeatMap> FlightSegmentSeatMap { get; set; }
    }
}