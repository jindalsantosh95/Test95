using Microsoft.Azure.Cosmos.Table;

namespace VAA.EAPI.Microservices.FlightInformation.Infrasturcture.Entities
{
    public class AirportDatabaseEntity : TableEntity
    {
        public string AirportName { get; set; }
        public string CityIataCode { get; set; }
        public string CountryName { get; set; }
        public string CountryCode { get; set; }
        public string EffectiveStartDate { get; set; }
        public string EffectiveEndDate { get; set; }

        [IgnoreProperty]
        public string OpType { get; set; }

    }
}
