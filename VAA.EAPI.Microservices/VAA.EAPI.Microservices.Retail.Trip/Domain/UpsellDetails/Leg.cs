using VAA.EAPI.Microservices.Retail.Trip.Infrastructure.Proxy.DeltaUpsellOffers;

namespace VAA.EAPI.Microservices.Retail.Trip.Domain.UpsellDetails
{
    public class Leg
    {
        public string? Id { get; set; }
        public CarrierDetails? Operating { get; set; }
        public DateTime? ScheduledDepartureLocalDateTime { get; set; }
        public DateTime? ScheduledArrivalLocalDateTime { get; set; }
        public DateTime? ScheduledArrivalBasedOnDepartureLocalDateTime { get; set; }
        public AirportDetails? Origin { get; set; }
        public AirportDetails? Destination { get; set; }

        public static explicit operator Leg(DatedOperatingLeg source)
        {
            if (source == null) return null;

            return new Leg
            {
                Id = source.LegId.ToString(),
                Operating = (CarrierDetails)(source.Carrier, source.FlightNum),
                ScheduledDepartureLocalDateTime = source.TransportOrigin?.ScheduledDepartureLocalDateTime,
                ScheduledArrivalLocalDateTime = source.TransportDestination?.ScheduledArrivalLocalDateTime,
                Origin = (AirportDetails)source.TransportOrigin.Station,
                Destination = (AirportDetails)source.TransportDestination.Station
            };
        }
    }
}
