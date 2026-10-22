using Microsoft.Azure.Cosmos.Table;
namespace VAA.EAPI.Microservices.Retail.Seat.Domain.OrderChange
{
    public class ReferenceDataEntity : TableEntity
    {
        public string AirportCode { get; set; }
        public string AirportName { get; set; }
        public string CityCode { get; set; }
        public string CityName { get; set; }
        public string CountryName { get; set; }
        public string CountryCode { get; set; }
        public string LocationCode { get; set; }
    }
}
