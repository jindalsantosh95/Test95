using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Proxy.DeltaOffer
{
    public class Product
    {
        public string ProductId { get; set; }
        public string ProductName { get; set; }
        public string ProductGroupId { get; set; }
        public string ProductShortDesc { get; set; }
        public string ProductLongDesc { get; set; }
        public string ProductCategoryCode { get; set; }
        public string ProductCategoryName { get; set; }
        public string ProductSubCategoryCode { get; set; }
        public string ProductSubCategoryName { get; set; }
        public string MarketedProductTypeCode { get; set; }
        public string ProductTypeCode { get; set; }
        public string ExternalProductId { get; set; }
        public string ReasonForIssuanceCode { get; set; }
        public string ReasonForIssuanceSubCode { get; set; }
        public string ProductDeliveryEntityName { get; set; }
        public IEnumerable<string> PurchaseRuleFormOfPayment { get; set; }
        public bool FlightRelated { get; set; }
        public string TypeCode { get; set; }
        public string SubTypeCode { get; set; }
        public string IataServiceCode { get; set; }
        public string BrandId { get; set; }
        public IEnumerable<string> PriceableIn { get; set; }
        public int PriorityNum { get; set; }
        public ProductAttribute ProductAttribute { get; set; }
        public Dictionary<string, string> RemarksMap { get; set; }
    }
}
