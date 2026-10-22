using NodaTime;
using VAA.EAPI.GraphQL.Domain.Search.Infrastructure.Models.ReferenceMS;
using VAA.EAPI.GraphQL.Domain.Search.Models.BFFResponse;

namespace VAA.EAPI.GraphQL.Domain.Search.Models.BFFSummary
{
    [GraphQLName("Leg_BFF")]
    public class Leg
    {
        public Origin? Origin { get; set; }
        public Destination? Destination { get; set; }
        public string? Duration { get; set; }
        public LocalDateTime? Departure { get; set; }
        public LocalDateTime? Arrival { get; set; }
        public string? StopOver { get; set; }
        public bool IsDominantLeg { get; set; }
        public SegmentAirline? OperatingAirline { get; set; }
        public SegmentAirline? MarketingAirline { get; set; }

        public static explicit operator Leg((FlightOffer.Legs leg, IEnumerable<AirportInfo> airportInfo, IEnumerable<AirlineInfo> airlineInfo) source)
        {
            return new Leg()
            {
                Origin = (Origin)(source.leg.OriginAirportCode, source.airportInfo),
                Destination = (Destination)(source.leg.DestinationAirportCode, source.airportInfo),
                OperatingAirline = string.IsNullOrWhiteSpace(source.leg.OperatingCarrier) ? null : (SegmentAirline)(source.leg.OperatingCarrier, source.airlineInfo),
                MarketingAirline = string.IsNullOrWhiteSpace(source.leg.MarketingCarrier) ? null : (SegmentAirline)(source.leg.MarketingCarrier, source.airlineInfo),
                Duration = Helpers.DateTimeHelper.ConvertColonDurationToIso(source.leg.LegDuration),
                Departure = Common.Helper.DateTimeHelper.ParseLocalDateTime(source.leg.ScheduledDepartureTime.LocalDateTime),
                Arrival = Common.Helper.DateTimeHelper.ParseLocalDateTime(source.leg.ScheduledArrivalTime.LocalDateTime),
                StopOver = string.IsNullOrEmpty(source.leg.StopOverDuration) ? null : Helpers.DateTimeHelper.ConvertColonDurationToIso(source.leg.StopOverDuration),
                IsDominantLeg = (bool)source.leg.IsDominantLeg
            };
        }
    }
}