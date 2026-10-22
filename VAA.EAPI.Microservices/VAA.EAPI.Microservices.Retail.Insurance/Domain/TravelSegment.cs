
namespace VAA.EAPI.Microservices.Retail.Insurance.Domain
{
    public class TravelSegment
    {
        public Destination Destination { get; set; }
        public Origin Origin { get; set; }
        public string RouteGroup { get; set; }
        public int Sequence { get; set; }
        public IEnumerable<TravelLeg> TravelLegs { get; set; }
    }
}