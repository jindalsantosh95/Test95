using VAA.EAPI.Microservices.Retail.Trip.Domain;

namespace VAA.EAPI.Microservices.Retail.Trip.Infrastructure.Proxy.DeltaMyTrips
{
    public class DeltaTripsDetailsResponse
    {
        public IEnumerable<TravelReservation> TravelReservations { get; set; }
        public IEnumerable<Error> Errors { get; set; }
    }
}