namespace VAA.EAPI.Automation.Core.Delta.Models.OfferSearch.DeltaMSResponse
{
    public class DeltaMSResponse
    {
        public string offerRequestId { get; set; }
        public string offerRequestEpochTime { get; set; }
        public string offerResponseId { get; set; }
        public string offerResponseEpochTime { get; set; }
        public List<OffersSet> offersSets { get; set; }
        public OfferDataList offerDataList { get; set; }
        public List<object> contentLinks { get; set; }
    }
}
