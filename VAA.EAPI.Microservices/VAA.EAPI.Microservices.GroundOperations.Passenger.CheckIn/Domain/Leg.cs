using NodaTime;
using NodaTime.Text;
using VAA.EAPI.Common.Helpers;

namespace VAA.EAPI.Microservices.GroundOperations.Passenger.CheckIn.Domain
{
    public class Leg
    {
        public string? LegId { get; set; }
        public int? Ordinal { get; set; }
        public LocalDateTime? ScheduledDepartureLocalDateTime { get; set; }
        public LocalDateTime? ScheduledDepartureUtcDateTime { get; set; }
        public LocalDateTime? ScheduledArrivalLocalDateTime { get; set; }
        public LocalDateTime? ScheduledArrivalUtcDateTime { get; set; }
        public AirportAndTerminal? Origin { get; set; }
        public AirportAndTerminal? Destination { get; set; }
        public int? DistanceMiles { get; set; }
        public string? FlightDuration { get; set; }
        public string? AircraftTypeName { get; set; }
        public string? Status { get; set; }

        // DeltaSsci.Segment actually represents a leg (i.e. there will be two "Segment" objects for a Direct with Stop flight).
        public static explicit operator Leg((DeltaSsci.Segment ssciLeg, DeltaTrm.DatedOperatingLeg trmLeg) source)
        {
            int.TryParse(source.ssciLeg.Miles, out int miles);

            var leg = (Leg)source.trmLeg;
            leg.DistanceMiles = miles;
            leg.FlightDuration = source.ssciLeg.FlightDuration;
            leg.AircraftTypeName = source.ssciLeg.Aircraft;
            leg.Status = source.ssciLeg.Status;

            return leg;
        }

        public static explicit operator Leg(DeltaTrm.DatedOperatingLeg trmLeg)
        {
            return new Leg()
            {
                LegId = trmLeg.Id,
                Ordinal = trmLeg.Ordinal,
                ScheduledDepartureLocalDateTime = DateTimeHelper.ParseLocalIsoDateTime(trmLeg.Departure.ScheduledDepartureLocalDateTime),
                ScheduledDepartureUtcDateTime = DateTimeHelper.ParseLocalIsoDateTime(trmLeg.Departure.ScheduledDepartureUTCDateTime),
                ScheduledArrivalLocalDateTime = DateTimeHelper.ParseLocalIsoDateTime(trmLeg.Arrival.ScheduledArrivalLocalDateTime),
                ScheduledArrivalUtcDateTime = DateTimeHelper.ParseLocalIsoDateTime(trmLeg.Arrival.ScheduledArrivalUTCDateTime),
                Origin = (AirportAndTerminal)(trmLeg.Departure.Station, null),
                Destination = (AirportAndTerminal)(trmLeg.Arrival.Station, null)
            };
        }
    }
}