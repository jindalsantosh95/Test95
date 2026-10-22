namespace VAA.EAPI.GraphQL.Domain.Book.Models.Insurance.InsuranceQuoteMS
{
    public class Product
    {
        public bool IsAvailable { get; set; }
        public string ExternalCode { get; set; }
        public IEnumerable<IndividualPrice>? IndividualPrices { get; set; }
        public IEnumerable<GroupPrice>? GroupPrices { get; set; }
        public IEnumerable<PriceDetails> Prices { get; set; }
        public string PricingModelCode { get; set; }
        public double PricingModelMaxAmount { get; set; }
        public double PricingModelMinAmount { get; set; }
        public string ProductCode { get; set; }
        public string ProductName { get; set; }
        public IEnumerable<ProductSpecificMessage>? ProductSpecificMessages { get; set; }
        public string ProductType { get; set; }
        public string SupplierCode { get; set; }
        public double TotalPrice { get; set; }
        public string QuoteId { get; set; }
        public string OfferId { get; set; }
        public string PricingModelDescription { get; set; }
    }

}