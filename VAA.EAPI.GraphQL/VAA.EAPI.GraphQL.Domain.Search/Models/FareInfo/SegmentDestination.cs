using VAA.EAPI.GraphQL.Domain.Search.Infrastructure.Models.ReferenceMS;

namespace VAA.EAPI.GraphQL.Domain.Search.Models.FareInfo
{
    public class SegmentDestination
    {
        public string Code { get; set; }
        public string CityName { get; set; }
        public string CountryName { get; set; }
        public string AirportName { get; set; }

        public static explicit operator SegmentDestination(AirportInfo source)
        {
            return new()
            {
                Code = source.AirportCode,
                CityName = source.CityName,
                CountryName = source.CountryName,
                AirportName = source.AirportName
            };
        }
    }
}
