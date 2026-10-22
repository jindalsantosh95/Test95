namespace VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.UpsellOffer
{
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
    }
}
