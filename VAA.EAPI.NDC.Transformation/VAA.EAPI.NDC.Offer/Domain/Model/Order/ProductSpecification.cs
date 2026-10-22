using System.Collections.Generic;


namespace VAA.EAPI.NDC.Offer.Domain.Model.Order
{
    public class ProductSpecification
    {
        public string ProductOwner { get; set; }
        public string ProductCode { get; set; }
        public string FareBasisCode { get; set; }
        public string ProductName { get; set; }
        public string ShortDescription { get; set; }
        public string ProductType { get; set; }
        public string ProductClass { get; set; }
        public IATAEncoding IATAEncoding { get; set; }
        public IEnumerable<ProductFeature> ProductFeatures { get; set; }
        public IEnumerable<ProductSpecification> ProductComponents { get; set; }
        public IEnumerable<Link> RichContent { get; set; }
        public IEnumerable<RuleOrPolicy> ProductConditions { get; set; }
        public ProductDisclosures ProductDisclosures { get; set; }
        public IEnumerable<OrderBookingInstruction> OrderInstructions { get; set; }
        public InventoryDisposition Locations { get; set; }

    }
}
