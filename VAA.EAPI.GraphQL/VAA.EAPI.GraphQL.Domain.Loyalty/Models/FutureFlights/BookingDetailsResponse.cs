using System.Collections.Generic;
using VAA.EAPI.GraphQL.Common.Entities;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Models.FutureFlights
{
    public class BookingDetailsResponse
    {
        public IEnumerable<FlightBookingsDetail> FlightBookingsDetails { get; set; }
        public IEnumerable<ErrorDetails> Errors { get; set; }
        public string Message { get; set; }

    }
}
