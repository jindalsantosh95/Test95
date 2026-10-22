namespace VAA.EAPI.Microservices.Retail.Trip.Infrastructure.Proxy.DeltaUpsellOffers
{

    public class UpsellOfferResponse
    {
        public string ReservationResourceId { get; set; }
        public bool UpsellEligible { get; set; }
        public IEnumerable<UpsellTrip> Trips { get; set; }
        public IEnumerable<UpsellSegment> Segments { get; set; }
    }
}
