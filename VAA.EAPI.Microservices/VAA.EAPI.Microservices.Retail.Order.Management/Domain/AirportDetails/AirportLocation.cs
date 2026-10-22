using Microsoft.Azure.Cosmos.Table;

namespace VAA.EAPI.Microservices.Retail.Order.Management.Domain.AirportDetails
{
    public class AirportLocation : TableEntity
    {
        public string AirportCode { get; set; }
        public string AirportName { get; set; }
        public string CityCode { get; set; }
        public string CityName { get; set; }
        public string CountryCode { get; set; }
        public string CountryName { get; set; }
        public string LocationCode { get; set; }
    }
}
