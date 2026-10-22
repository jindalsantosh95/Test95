namespace VAA.EAPI.Automation.Core.Delta.Models.OfferSearch.DeltaMSResponse
{
    public class RetailItem
    {
        public string retailItemDefinitionId { get; set; }
        public List<string> tripIds { get; set; }
        public List<string> flightSegmentIds { get; set; }
        public List<RetailItemPricing> retailItemPricing { get; set; }
        public RetailItemMetaData retailItemMetaData { get; set; }
        public RetailItemMetaDataLink retailItemMetaDataLink { get; set; }
    }
}
