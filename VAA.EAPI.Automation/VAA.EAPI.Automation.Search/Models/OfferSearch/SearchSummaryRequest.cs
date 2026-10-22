namespace VAA.EAPI.Automation.Search.Models.OfferSearch
{
    public class SearchSummaryRequest
    {
        public string basketId { get; set; }
        public string offerId { get; set; }
        public int currentTripIndexId { get; set; }
        public string summaryLink { get; set; }
    }
}
