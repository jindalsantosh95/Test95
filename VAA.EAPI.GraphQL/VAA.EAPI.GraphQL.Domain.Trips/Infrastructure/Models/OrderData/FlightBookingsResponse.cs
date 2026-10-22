using VAA.EAPI.GraphQL.Common.Entities;

namespace VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.OrderData
{
    public class FlightBookingsResponse : ErrorDetails
    {
        public IEnumerable<FlightBookingDetail> FlightBookingsDetails { get; set; }
    }
}
