using NodaTime;
using VAA.EAPI.Common.Helpers;

namespace VAA.EAPI.Microservices.GroundOperations.Passenger.CheckIn.Domain
{
    public class Slice
    {
        public string SliceId { get; set; }
        public LocalDateTime? ScheduledDepartureLocalDateTime { get; set; }
        public LocalDateTime? ScheduledDepartureUtcDateTime { get; set; }
        public LocalDateTime? ScheduledArrivalLocalDateTime { get; set; }
        public LocalDateTime? ScheduledArrivalUtcDateTime { get; set; }
        public AirportAndTerminal? Origin { get; set; }
        public AirportAndTerminal? Destination { get; set; }
        public IEnumerable<Segment>? Segments { get; set; }
        // Dummy property to work around bug in Swagger generation lib that wasn't adding LocalTime type to the swagger file.
        public LocalTime? ForceLocalTimeIntoSwagger { get; set; }

        public static explicit operator Slice((DeltaSsci.TravelParty travelParty, IEnumerable<DeltaTrm.Trip> trmTrips) source)
        {
            var departure = source.trmTrips.First().Segments.First().Legs.First().Departure;
            var arrival = source.trmTrips.Last().Segments.Last().Legs.Last().Arrival;
            var response = new Slice {
                SliceId = string.Join("|", source.trmTrips.Select(t => t.Id)),
                ScheduledDepartureLocalDateTime = DateTimeHelper.ParseLocalIsoDateTime(departure.ScheduledDepartureLocalDateTime),
                ScheduledDepartureUtcDateTime = DateTimeHelper.ParseLocalIsoDateTime(departure.ScheduledDepartureUTCDateTime),
                ScheduledArrivalLocalDateTime = DateTimeHelper.ParseLocalIsoDateTime(arrival.ScheduledArrivalLocalDateTime),
                ScheduledArrivalUtcDateTime = DateTimeHelper.ParseLocalIsoDateTime(arrival.ScheduledArrivalUTCDateTime),
                Origin = (AirportAndTerminal)(departure.Station, source.travelParty.Trip.Segments.First().Terminal),
                Destination = (AirportAndTerminal)(arrival.Station, null),
                // SSCI returns one Segment per Leg (even for Direct with Stop flights).
                // TRM groups Legs into Segments based on the MarketingFlightNumber (not the OperatingFlightNumber).
                // DL31 (LHR-ATL-DFW) can be used as a test case for this. It has one DL flight number (operating), but two VS flight numbers (marketing).
                Segments = source.travelParty.Trip.Segments.GroupBy(s => s.MarketingAirlineCode + s.MarketingFlightNumber)
                    .Select(s => (Segment)(s, source.trmTrips.SelectMany(t => t.Segments).First(seg => seg.Legs.First().Departure.Station.Code == s.First().OriginCode)))
            };

            return response;
        }

        public static explicit operator Slice(DeltaTrm.Trip trmTrip)
        {
            var departure = trmTrip.Segments.First().Legs.First().Departure;
            var arrival = trmTrip.Segments.Last().Legs.Last().Arrival;
            var response = new Slice
            {
                SliceId = trmTrip.Id,
                ScheduledDepartureLocalDateTime = DateTimeHelper.ParseLocalIsoDateTime(departure.ScheduledDepartureLocalDateTime),
                ScheduledDepartureUtcDateTime = DateTimeHelper.ParseLocalIsoDateTime(departure.ScheduledDepartureUTCDateTime),
                ScheduledArrivalLocalDateTime = DateTimeHelper.ParseLocalIsoDateTime(arrival.ScheduledArrivalLocalDateTime),
                ScheduledArrivalUtcDateTime = DateTimeHelper.ParseLocalIsoDateTime(arrival.ScheduledArrivalUTCDateTime),
                Origin = (AirportAndTerminal)(departure.Station, null),
                Destination = (AirportAndTerminal)(arrival.Station, null),
                Segments = trmTrip.Segments.Select(s => (Segment)s)
            };

            return response;
        }
    }
}
