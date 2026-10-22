using VAA.EAPI.Microservices.Retail.Order.Management.Domain.AirportDetails;

namespace VAA.EAPI.Microservices.Retail.Order.Management.Mapper.AirportDetail
{
    public static class AirportLocationMapper
    {
        public static AirportLocation MaptoAirportLocation(Domain.AirportDetails.AirportDetail item)
        {
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
