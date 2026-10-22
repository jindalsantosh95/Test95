namespace VAA.EAPI.Microservices.Retail.Trip.Infrastructure.Proxy.DeltaUpsellOffers
{
    public class UpsellOffer
    {
        public string OfferItemNumber { get; set; }
        public CabinClass CabinClass { get; set; }
        public IEnumerable<UpsellOfferDetail> UpsellOfferDetails { get; set; }
        public int OfferPriority { get; set; }
        public int DisplayPriority { get; set; }
        public int AvailableSeatsRemaining { get; set; }
    }
}
