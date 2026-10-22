using System.Net;
using VAA.EAPI.GraphQL.Common.Entities;

namespace VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.HolidaysMS
{

    public class HolidayTripResponse
    {
        public Booking Booking { get; set; }
        public IEnumerable<ErrorDetails> Errors { get; set; }
        public HttpStatusCode Status { get; set; }
    }
}

