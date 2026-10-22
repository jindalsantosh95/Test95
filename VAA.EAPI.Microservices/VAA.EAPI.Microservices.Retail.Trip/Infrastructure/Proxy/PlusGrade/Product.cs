namespace VAA.EAPI.Microservices.Retail.Trip.Infrastructure.Proxy.PlusGrade
{
    public class Product
    {
        public string ProductType { get; set; }
        public string ProductName { get; set; }
        public string UpgradeType { get; set; }
        public bool Eligible { get; set; }
        public string IneligibleReason { get; set; }
        public string OfferCurrencyCode { get; set; }
        public SliderRules SliderRules { get; set; }
        public InstantUpgrade InstantUpgrade { get; set; }
        public string FixedPriceAmount { get; set; }
        public IEnumerable<Offer> Offers { get; set; }
    }
}

