namespace VAA.EAPI.Automation.Search.Models.OfferSearch.MSResponse
{
    public class Offer
    {
        public string offerId { get; set; }
        public List<SalePrice> salePrice { get; set; }
        public List<OfferItem> offerItems { get; set; }
        public DateTime offerExpirationTimeLimit { get; set; }
        public bool? refundable { get; set; }
    }
}
