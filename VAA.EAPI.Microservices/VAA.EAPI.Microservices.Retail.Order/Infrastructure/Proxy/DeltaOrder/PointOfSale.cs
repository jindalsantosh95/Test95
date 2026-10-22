namespace VAA.EAPI.Microservices.Retail.Order.Infrastructure.Proxy.DeltaOrder
{
    public class PointOfSale
    {
        public string countryCode { get; set; }
        public string officeTypeCode { get; set; }
        public string pointOfSaleCityCode { get; set; }
        public string pointOfSaleId { get; set; }
        public bool soldByTravelAgency { get; set; }
        public string deviceTypeText { get; set; }
        public string ipAddressText { get; set; }
        public string browserTypeText { get; set; }
    }
}
