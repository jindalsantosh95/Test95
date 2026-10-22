using System.Collections.Generic;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Retail.Holidays.Domain.Trips;

namespace VAA.EAPI.Microservices.Retail.Holidays.Application
{

    public class HolidayTripResponse
    {
        public Booking? Booking { get; set; }
        public IEnumerable<ErrorDetails> Errors { get; set; }
        public int Status { get; set; }
    }
}

