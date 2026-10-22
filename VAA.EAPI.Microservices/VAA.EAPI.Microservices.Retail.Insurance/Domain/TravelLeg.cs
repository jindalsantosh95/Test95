namespace VAA.EAPI.Microservices.Retail.Insurance.Domain
{
    public class TravelLeg
    {
        public string ClassOfService { get; set; }
        public Destination Destination { get; set; }
        public string FareClass { get; set; }
        public Origin Origin { get; set; }
        public string RouteNumber { get; set; }
        public int Sequence { get; set; }
    }
}