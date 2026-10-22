namespace VAA.EAPI.Automation.Search.Models.OfferSearch.MSResponse
{
    public class OfferItem
    {
        public string offerItemId { get; set; }
        public List<Service> services { get; set; }
        public List<OfferitemPricing> offeritemPricing { get; set; }
    }
}
