using System;

namespace VAA.EAPI.Microservices.FlightInformation.Infrasturcture.Services.CommonRefDb.Models
{
    public class DimAirport
    {
        public int AirportID { get; set; }
        public string IataAirportCode { get; set; }
        public string Name { get; set; }
        public string CityIataCode { get; set; }
        public string Country { get; set; }
        public string CountryCode { get; set; }
        public DateTime EffectiveStartDate { get; set; }
        public DateTime EffectiveEndDate { get; set; }
    }
}
