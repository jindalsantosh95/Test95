namespace VAA.EAPI.Automation.Core.Delta.Models.OfferSearch.DeltaMSResponse
{
    public class RetailItemDefinitionList
    {
        public string retailItemDefinitionId { get; set; }
        public string retailItemId { get; set; }
        public string retailItemName { get; set; }
        public string retailItemBrandId { get; set; }
        public string retailItemGroupId { get; set; }
        public string retailItemPriorityText { get; set; }
        public bool refundable { get; set; }
        public string retailItemOwnerCode { get; set; }
        public string brandType { get; set; }
        public string fareFamily { get; set; }
        public MinimumOfferPrice minimumOfferPrice { get; set; }
    }
}
