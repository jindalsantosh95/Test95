using VAA.EAPI.Microservices.Retail.Trip.Infrastructure.Proxy.DeltaUpsellOffers;

namespace VAA.EAPI.Microservices.Retail.Trip.Domain.UpsellDetails
{
    public class Slice
    {
        public string Id { get; set; }
        public DateTime? ScheduledDepartureLocalDateTime { get; set; }
        public DateTime? ScheduledArrivalLocalDateTime { get; set; }
        public AirportDetails? Origin { get; set; }
        public AirportDetails? Destination { get; set; }

        public static explicit operator Slice(UpsellTrip source)
        {
            if (source == null) return null;

            return new Slice
            {
                Id = source.Trip?.TripId.ToString(),
                ScheduledDepartureLocalDateTime = source.Trip?.TransportOrigin?.ScheduledDepartureLocalDateTime,
                ScheduledArrivalLocalDateTime = source.Trip?.TransportDestination?.ScheduledArrivalLocalDateTime,
                Origin = (AirportDetails)source.Trip?.TransportOrigin,
                Destination = (AirportDetails)source.Trip?.TransportDestination
            };
        }
    }
}
