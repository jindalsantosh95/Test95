namespace VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Proxy.DeltaOffer
{
    public class UpgradeInformation
    {
        public string BrandId { get; set; }
        public bool UpgradeAtAirportOnly { get; set; }
        public bool UpgradeAvailable { get; set; }
        public bool UpgradeEligible { get; set; }
    }
}
