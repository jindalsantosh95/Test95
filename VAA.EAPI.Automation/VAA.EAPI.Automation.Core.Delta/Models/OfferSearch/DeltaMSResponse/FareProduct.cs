namespace VAA.EAPI.Automation.Core.Delta.Models.OfferSearch.DeltaMSResponse
{
    public class FareProduct
    {
        public string brandId { get; set; }
        public List<object> priceableIn { get; set; }
        public string productGroupId { get; set; }
        public string productShortDesc { get; set; }
        public string productLongDesc { get; set; }
        public string productCategoryCode { get; set; }
        public string typeCode { get; set; }
        public string subTypeCode { get; set; }
    }
}
