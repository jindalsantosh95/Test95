namespace VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Proxy.DeltaOffer
{
    public class PointOfSale
    {
        public string CountryCode { get; set; }
        public string OfficeTypeCode { get; set; }
        public string PointOfSaleCityCode { get; set; }
        public string PointOfSaleId { get; set; }
        public bool SoldByTravelAgency { get; set; }
        public string DeviceTypeText { get; set; }
        public string IpAddressText { get; set; }
        public string TealeafSessionId { get; set; }
        public string TealeafUserID { get; set; }
    }
}
