namespace VAA.EAPI.Microservices.Retail.Trip.Infrastructure.Proxy.DeltaUpsellOffers
{
    public class UpsellTrip
    {
        public Trip Trip { get; set; }
        public IEnumerable<UpsellOffer> UpsellOffers { get; set; }
    }
}
