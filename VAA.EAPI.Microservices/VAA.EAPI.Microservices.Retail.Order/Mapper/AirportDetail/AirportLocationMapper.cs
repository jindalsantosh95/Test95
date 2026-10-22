using VAA.EAPI.Microservices.Retail.Order.Domain.AirportDetails;

namespace VAA.EAPI.Microservices.Retail.Order.Mapper.AirportDetail
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
                CityName = item.AirportName,
                CountryCode = item.CountryCode,
                CountryName = item.CountryName,
                LocationCode = item.CityIataCode,

            };

        }
    }
}
