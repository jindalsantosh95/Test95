namespace VAA.EAPI.Microservices.Retail.Trip.Infrastructure.Proxy.DeltaUpsellOffers
{
    public class UpsellSegment
    {
        public DatedOperatingSegment DatedOperatingSegment { get; set; }
        public IEnumerable<UpsellOffer> UpsellOffers { get; set; }
    }
}
