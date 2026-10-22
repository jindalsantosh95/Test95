namespace VAA.EAPI.GraphQL.Domain.PaxOps.Models.Payment
{
    public class Segment
    {
        public string ClassOfService { get; set; }
        public string MarketingFlightNumber { get; set; }
        public string MarketingCarrier { get; set; }
        public string OperatingCarrier { get; set; }
        public IEnumerable<Leg> Legs { get; set; }
    }
}
