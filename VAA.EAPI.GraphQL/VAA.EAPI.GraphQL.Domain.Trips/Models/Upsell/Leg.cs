using HotChocolate;
using VAA.EAPI.GraphQL.Domain.Trips.Models.ReferenceData;
using NodaTime;
using VAA.EAPI.GraphQL.Domain.Trips.Helper;

namespace VAA.EAPI.GraphQL.Domain.Trips.Models.Upsell
{
    [GraphQLName("UpsellLeg")]
    public class Leg
    {
        public string? Id { get; set; }
        public CarrierDetails? Operating { get; set; }
        public DateTime? ScheduledDepartureLocalDateTime { get; set; }
        public DateTime? ScheduledArrivalLocalDateTime { get; set; }
        public DateTime? ScheduledArrivalBasedOnDepartureLocalDateTime { get; set; }
        public LocalDateTime? ScheduledDepartureLocal { get; set; }
        public LocalDateTime? ScheduledArrivalLocal { get; set; }
        public LocalDateTime? ScheduledArrivalBasedOnDepartureLocal { get; set; }
        public AirportDetails? Origin { get; set; }
        public AirportDetails? Destination { get; set; }

        public static explicit operator Leg((Infrastructure.Models.UpsellOffer.Leg source, IEnumerable<AirportRefData> airportRefdata, IEnumerable<AirlineRefData> airlineRefData) data)
        {
            var (source, airportRefData, airlineRefData) = data;
            if (source == null) return null;

            return new Leg()
            {
                Destination = (AirportDetails)(source.Destination, airportRefData),
                Id = source.Id,
                Operating = (CarrierDetails)(source.Operating, airlineRefData),
                ScheduledArrivalBasedOnDepartureLocalDateTime = source.ScheduledArrivalBasedOnDepartureLocalDateTime,
                ScheduledArrivalLocalDateTime = source.ScheduledArrivalLocalDateTime,
                ScheduledDepartureLocalDateTime = source.ScheduledDepartureLocalDateTime,
                ScheduledDepartureLocal = DateTimeHelper.ToLocal(source.ScheduledDepartureLocalDateTime),
                ScheduledArrivalLocal = DateTimeHelper.ToLocal(source.ScheduledArrivalLocalDateTime),
                ScheduledArrivalBasedOnDepartureLocal = DateTimeHelper.ToLocal(source.ScheduledArrivalBasedOnDepartureLocalDateTime),
                Origin = (AirportDetails)(source.Origin, airportRefData),
            };
        }
    }
}
