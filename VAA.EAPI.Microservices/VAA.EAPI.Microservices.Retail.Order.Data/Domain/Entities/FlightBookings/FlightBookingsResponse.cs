
using System.Collections.Generic;
using VAA.EAPI.Domain.Entities.Common;

namespace VAA.EAPI.Microservices.Retail.Order.Data.Domain.Entities.FlightBookings
{
    public class FlightBookingsResponse:ErrorDetails
    {
       public IEnumerable<FlightBookingsDetails> FlightBookingsDetails { get; set; }
    }
}
