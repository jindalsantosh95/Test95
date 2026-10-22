using System.Collections.Generic;
using VAA.EAPI.Microservices.Retail.Order.Management.Domain.CommonRefDB;
using VAA.EAPI.Microservices.Retail.Order.Management.Infrastructure.Model;

namespace VAA.EAPI.Microservices.Retail.Order.Management.Infrastructure.Mapper.CommonRefDetailsMapper
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
