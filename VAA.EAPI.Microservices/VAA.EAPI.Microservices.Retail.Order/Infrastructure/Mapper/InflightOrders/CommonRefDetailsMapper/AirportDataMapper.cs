using System.Collections.Generic;
using VAA.EAPI.Microservices.Retail.Order.Domain.InflightOrders.CommonReferenceDetails.Models;
using VAA.EAPI.Microservices.Retail.Order.Infrastructure.Services.InflightOrders.CommonRefDB.Models;

namespace VAA.EAPI.Microservices.Retail.Order.Infrastructure.Mapper.InflightOrders.CommonRefDetailsMapper
{
    public class AirportDataMapper
    {
        public static List<AirportDetails> MapAirportData(IEnumerable<DimAirport> airports)
        {
            List<AirportDetails> airportDetailsList = new List<AirportDetails>();
            foreach (var m in airports)
            {
                AirportDetails airport = new AirportDetails()
                {
                    Name = m.Name,
                    IataAirportCode = m.IataAirportCode,
                    CityIataCode = m.CityIataCode,
                    Country = m.Country,
                    CountryCode = m.CountryCode
                };
                airportDetailsList.Add(airport);
            }
            return airportDetailsList;
        }
    }
}
