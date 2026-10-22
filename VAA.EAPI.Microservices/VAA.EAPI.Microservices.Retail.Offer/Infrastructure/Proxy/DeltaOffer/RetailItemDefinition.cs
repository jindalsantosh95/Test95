using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Proxy.DeltaOffer
{
    public class RetailItemDefinition
    {
        public string RetailItemDefinitionId { get; set; }
        public string RetailItemId { get; set; }
        public string RetailItemName { get; set; }
        public string RetailItemDesc { get; set; }
        public IEnumerable<string> AdditionalRetailItemDesc { get; set; }
        public string RetailItemCategoryText { get; set; }
        public string RetailItemBrandId { get; set; }
        public Link RetailItemBrandUrlText { get; set; }
        public string RetailItemGroupId { get; set; }
        public string RetailItemPriorityText { get; set; }
        public bool Refundable { get; set; }
        public string RetailItemOwnerCode { get; set; }
        public string BrandType { get; set; }
        public Price MinimumOfferPrice { get; set; }
        public string FareFamily { get; set; }
    }
}
