namespace VAA.EAPI.Automation.Core.Delta.Models.OfferSearch.DeltaMSResponse
{
    public class Product
    {
        public string productLongDesc { get; set; }
        public string typeCode { get; set; }
        public string brandId { get; set; }
        public List<object> priceableIn { get; set; }
        public int? priorityNum { get; set; }
    }
}
