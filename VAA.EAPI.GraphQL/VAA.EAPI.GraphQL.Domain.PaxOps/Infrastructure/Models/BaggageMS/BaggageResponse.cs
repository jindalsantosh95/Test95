namespace VAA.EAPI.GraphQL.Domain.PaxOps.Infrastructure.Models.BaggageMS
{
    public class BaggageResponse
    {
        public string TravelPartyId { get; set; }
        public IEnumerable<PassengerAllowance> Passengers { get; set; }
        public bool ShowBaggageAllowance { get; set; }
        public AllAboutBags AllAboutBags { get; set; }
        public string BaggageLearnMoreUrl { get; set; }
    }
}
