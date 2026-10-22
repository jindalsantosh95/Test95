namespace VAA.EAPI.Automation.Core.Delta.Models.OfferSearch.DeltaMSResponse
{
    public class OffersSet
    {
        public List<Offer> offers { get; set; }
        public string tripId { get; set; }
        public AdditionalOfferSetProperties additionalOfferSetProperties { get; set; }
    }
}
