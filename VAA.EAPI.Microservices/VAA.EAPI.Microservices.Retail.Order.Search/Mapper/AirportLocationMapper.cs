using System;
using VAA.EAPI.Microservices.Retail.Order.Search.Domain;

namespace VAA.EAPI.Microservices.Retail.Order.Search.Mapper
{
    public static class AirportLocationMapper
    {
        public static AirportLocation MaptoAirportLocation(AirportDetail item)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));
            return new AirportLocation()
            {
                PartitionKey = Constants.Application.PartitionKey,
                RowKey = item.IataAirportCode,
                AirportCode = item.IataAirportCode,
                AirportName = item.AirportName,
                CityCode = item.CityIataCode,
                CityName = item.CityIataCode,
                CountryCode = item.CountryCode,
                CountryName = item.CountryName,
                LocationCode = item.CityIataCode,
            };
        }
    }
}
