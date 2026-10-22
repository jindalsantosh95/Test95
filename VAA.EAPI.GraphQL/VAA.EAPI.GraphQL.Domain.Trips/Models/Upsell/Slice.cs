using HotChocolate;
using VAA.EAPI.GraphQL.Domain.Trips.Models.ReferenceData;
using NodaTime;
using VAA.EAPI.GraphQL.Domain.Trips.Helper;

namespace VAA.EAPI.GraphQL.Domain.Trips.Models.Upsell
{
    [GraphQLName("UpsellSlice")]
    public class Slice
    {
        public string Id { get; set; }
        public DateTime? ScheduledDepartureLocalDateTime { get; set; }
        public DateTime? ScheduledArrivalLocalDateTime { get; set; }
        public LocalDateTime? ScheduledDepartureLocal { get; set; }
        public LocalDateTime? ScheduledArrivalLocal { get; set; }
        public AirportDetails? Origin { get; set; }
        public AirportDetails? Destination { get; set; }

        public static explicit operator Slice((Infrastructure.Models.UpsellOffer.Slice s, IEnumerable<AirportRefData> airportRefdata) data)
        {
            var (source, airportRefdata) = data;
            if (source == null) return null;

            return new Slice()
            {
                Id = source.Id,
                ScheduledDepartureLocalDateTime = source.ScheduledDepartureLocalDateTime,
                ScheduledArrivalLocalDateTime = source.ScheduledArrivalLocalDateTime,
                ScheduledDepartureLocal = DateTimeHelper.ToLocal(source.ScheduledDepartureLocalDateTime),
                ScheduledArrivalLocal = DateTimeHelper.ToLocal(source.ScheduledArrivalLocalDateTime),
                Origin = (AirportDetails)(source.Origin, airportRefdata),
                Destination = (AirportDetails)(source.Destination, airportRefdata)
            };
        }
    }
}
