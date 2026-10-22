namespace VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Proxy.DeltaOffer
{
    public class CountryOfCommencementFare
    {
        public Price FareDifference { get; set; }
        public Price FareTotal { get; set; }
        public Price PreviousFareTotal { get; set; }
    }
}
