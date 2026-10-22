namespace VAA.EAPI.Microservices.Retail.Order.Domain.InflightOrders.CommonReferenceDetails.Models
{
    public class CarrierDetails
    {
        public string IataAlphaCode { get; set; }
        public string CarrierAlliance { get; set; }
        public string CarrierGrouping { get; set; }
        public string CarrierType { get; set; }
        public string IataAccountingCode { get; set; }
        public string IcaoCode { get; set; }
        public string AirlineName { get; set; }
        public string BaseAirport { get; set; }
        public string Country { get; set; }
    }
}
