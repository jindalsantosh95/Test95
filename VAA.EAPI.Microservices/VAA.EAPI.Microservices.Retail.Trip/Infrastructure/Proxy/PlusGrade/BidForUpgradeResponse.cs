namespace VAA.EAPI.Microservices.Retail.Trip.Infrastructure.Proxy.PlusGrade
{

    public class BidForUpgradeResponse
    {
        public string RequestId { get; set; }
        public string ModifyUrl { get; set; }
        public string OfferUrl { get; set; }
        public string TrackingUrl { get; set; }
        public string ImageUrl { get; set; }
        public string ResponseStatus { get; set; }
        public IEnumerable<Flight> Flights { get; set; }
    }
}

