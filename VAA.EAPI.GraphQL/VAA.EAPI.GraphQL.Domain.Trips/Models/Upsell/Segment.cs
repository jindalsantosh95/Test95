using HotChocolate;
using VAA.EAPI.GraphQL.Domain.Trips.Models.ReferenceData;

namespace VAA.EAPI.GraphQL.Domain.Trips.Models.Upsell
{
    [GraphQLName("UpsellSegment")]
    public class Segment
    {
        public string Id { get; set; }
        public CarrierDetails? OperatingCarrier { get; set; }
        public CarrierDetails? MarketingCarrier { get; set; }
        public AirportDetails? Origin { get; set; }
        public AirportDetails? Destination { get; set; }
        public IEnumerable<Leg>? Legs { get; set; }
        public AirlineDetails? Operating { get; set; }
        public AirlineDetails? Marketing { get; set; }

        public IEnumerable<UpsellOffer> UpsellOffers { get; set; }

        public static explicit operator Segment((Infrastructure.Models.UpsellOffer.Segment s, IEnumerable<AirportRefData> airportRefdata, IEnumerable<CabinRefData> cabinRefData, IEnumerable<AirlineRefData> airlineRefData) data)
        {
            var (source, airportRefdata, cabinRefData, airlineRefData) = data;
            if (source == null) return null;

            return new Segment
            {
                Id = source.Id,
                OperatingCarrier = (CarrierDetails)(source.OperatingCarrier, airlineRefData),
                MarketingCarrier = (CarrierDetails)(source.MarketingCarrier, airlineRefData),
                Origin = (AirportDetails)(source.Origin, airportRefdata),
                Destination = (AirportDetails)(source.Destination, airportRefdata),
                Legs = source.Legs?.Select(l => (Leg)(l, airportRefdata, airlineRefData)),
                Operating = (AirlineDetails)(source.Operating, airlineRefData),
                Marketing = (AirlineDetails)(source.Marketing, airlineRefData),
                UpsellOffers = source.UpsellOffers?.Select(u => (UpsellOffer)(u, cabinRefData))
            };
        }
    }
}
