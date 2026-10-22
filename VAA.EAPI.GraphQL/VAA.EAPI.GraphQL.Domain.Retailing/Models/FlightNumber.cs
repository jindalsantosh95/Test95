using HotChocolate;
using Newtonsoft.Json.Linq;
using VAA.EAPI.GraphQL.Common.Extensions;

namespace VAA.EAPI.GraphQL.Domain.Retailing.Models
{
    [GraphQLName("Retailing.FlightNumber")]
    public class FlightNumber
    {
        public static explicit operator FlightNumber(JObject source)
        {
            return new()
            {
                AirlineCode = source["AirlineCode"].ToString(),
                Number = source["Number"].ToString().Parse<int>(),
            };
        }

        public string AirlineCode { get; set; }
        public int Number { get; set; }
    }
}