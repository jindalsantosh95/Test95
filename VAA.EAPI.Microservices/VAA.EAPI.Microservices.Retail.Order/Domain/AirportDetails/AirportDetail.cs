namespace VAA.EAPI.Microservices.Retail.Order.Domain.AirportDetails
{
    public class AirportDetail
    {
        public string AirportID { get; set; }
        public string IataAirportCode { get; set; }
        public string AirportName { get; set; }
        public string CityIataCode { get; set; }
        public string CountryName { get; set; }
        public string CountryCode { get; set; }
        public string EffectiveStartDate { get; set; }
        public string EffectiveEndDate { get; set; }
        public string OpType { get; set; }
    }
}
