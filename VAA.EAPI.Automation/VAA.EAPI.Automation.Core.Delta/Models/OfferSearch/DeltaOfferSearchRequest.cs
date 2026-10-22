namespace VAA.EAPI.Automation.Core.Delta.Models.OfferSearch
{
    public class DeltaOfferSearchRequest
    {
        public List<ProductGroup> productGroups { get; set; }
        public OffersCriteria offersCriteria { get; set; }
        public List<Customer> customers { get; set; }
    }
}