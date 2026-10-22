using VAA.EAPI.GraphQL.Domain.Search.Infrastructure.Models.ReferenceMS;

namespace VAA.EAPI.GraphQL.Domain.Search.Models.BFFSummary
{
    public class Airline
    {
        public string? Code { get; set; }
        public string? Name { get; set; }

        public static explicit operator Airline(AirlineInfo source)
        {
            return new Airline
            {
                Code = source.CarrierCode,
                Name = source.CarrierName
            };
        }
    }
}