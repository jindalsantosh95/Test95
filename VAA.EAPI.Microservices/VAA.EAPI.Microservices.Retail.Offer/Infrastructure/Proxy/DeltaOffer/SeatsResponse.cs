namespace VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Proxy.DeltaOffer
{
    public class SeatsResponse
    {
        public string TripId { get; set; }
        public string SeatId { get; set; }
        public string PassengerId { get; set; }
        public string FlightSegmentId { get; set; }
        public string FlightLegNum { get; set; }
        public string RetainSeat { get; set; }
    }
}
