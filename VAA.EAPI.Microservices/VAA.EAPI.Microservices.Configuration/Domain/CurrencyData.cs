namespace VAA.EAPI.Microservices.Configuration.Domain
{
    public class CurrencyData
    {
        public string Country { get; set; }
        public string CountryCode { get; set; }
        public string Currency { get; set; }
        public string LocalCurrency { get; set; }
        public string PointOfSale { get; set; }
        public string OverrideCurrency { get; set; }
        public string OverridePointOfSale { get; set; }
    }
}
