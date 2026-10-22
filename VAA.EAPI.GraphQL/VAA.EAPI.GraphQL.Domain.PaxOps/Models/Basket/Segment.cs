namespace VAA.EAPI.GraphQL.Domain.PaxOps.Models.Basket
{
    public class Segment
    {
        public string Id { get; set; }
        public CarrierDetails OperatingCarrier { get; set; }
        public CarrierDetails MarketingCarrier { get; set; }
        public DateTime ScheduledDepartureLocalDateTime { get; set; }
        public DateTime ScheduledArrivalLocalDateTime { get; set; }
        public Infrastructure.Models.CheckInMS.AirportAndTerminal Origin { get; set; }
        public Infrastructure.Models.CheckInMS.AirportAndTerminal Destination { get; set; }
        public IEnumerable<Leg> Legs { get; set; }
    }
}
