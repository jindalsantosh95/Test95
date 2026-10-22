namespace VAA.EAPI.Automation.Core.Delta.Models.OfferSearch
{
    public class PricingCriteria
    {
        public List<string> priceableIn { get; set; }
        public string currencyCode { get; set; }
        public bool waiveChangeFee { get; set; }
    }
}