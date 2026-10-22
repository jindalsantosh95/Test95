using System.Net;
using VAA.EAPI.GraphQL.Common.Entities;
using VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.TripMS;

namespace VAA.EAPI.GraphQL.Domain.Trips.Models.Trips.Virgin
{

    public class BookingResponseDetails
    {
        public HttpStatusCode? Status { get; set; }
        public IEnumerable<ErrorDetails>? Errors { get; set; }

        public static explicit operator BookingResponseDetails(TripsDetailsResponse response)
        {
            if (response == null) return null;

            return new()
            {

                Status = response.Status,
                Errors = response.Errors ?? null
            };
        }

        public static explicit operator BookingResponseDetails(Infrastructure.Models.HolidaysMS.TripsCore.TripsDetailsResponse response)
        {
            if (response == null) return null;

            return new()
            {

                Status = response.Status,
                Errors = response.Errors ?? null
            };
        }
    }
}