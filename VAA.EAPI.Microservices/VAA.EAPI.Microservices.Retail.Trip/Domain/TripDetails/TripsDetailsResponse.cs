using System.Net;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Retail.Trip.Infrastructure.Proxy.DeltaMyTrips;

namespace VAA.EAPI.Microservices.Retail.Trip.Domain.TripDetails
{
    public class TripsDetailsResponse
    {
        public Booking? Booking { get; set; }
        public IEnumerable<ErrorDetails> Errors { get; set; }
        public HttpStatusCode Status { get; set; }

        public static explicit operator TripsDetailsResponse(DeltaTripsDetailsResponse source)
        {
            if (source == null || !source.TravelReservations.Any())
            {
                return new TripsDetailsResponse();
            }

            var firstReservation = source.TravelReservations.First();

            return new TripsDetailsResponse
            {
                Status = HttpStatusCode.OK,
                Booking = (Booking)(firstReservation)
            };
        }
    }
}
