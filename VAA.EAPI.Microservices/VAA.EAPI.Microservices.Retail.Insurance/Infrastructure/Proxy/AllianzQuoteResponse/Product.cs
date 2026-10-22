
namespace VAA.EAPI.Microservices.Retail.Insurance.Infrastructure.Proxy.AllianzQuoteResponse
{
    public class Product
    {
        public bool IsAvailable { get; set; }
        public string ExternalCode { get; set; }
        public Individualprice[] Individualprices { get; set; }
        public Groupprice[] Groupprices { get; set; }
        public QuotePrice[] Prices { get; set; }
        public Productspecificmessage[] Productspecificmessage { get; set; }
        public string PricingModelCode { get; set; }
        public string ProductCode { get; set; }
        public string ProductName { get; set; }
        public string ProductType { get; set; }
        public string SupplierCode { get; set; }
        public double TotalPrice { get; set; }
        public string QuoteId { get; set; }
        public string OfferId { get; set; }
        public double PricingModelMinAmount { get; set; }
        public double PricingModelMaxAmount { get; set; }
        public string PricingModelDescription { get; set; }
    }
}