namespace VAA.EAPI.Microservices.Retail.Insurance.Domain.Decision
{
    public class Detail
    {
        public string ResponseType { get; set; }
        public string SupplierCode { get; set; }
        public string ProductCode { get; set; }
        public double PurchasePrice { get; set; }
        public int Quantity { get; set; }
        public string ResponseQuoteId { get; set; }
    }
}
