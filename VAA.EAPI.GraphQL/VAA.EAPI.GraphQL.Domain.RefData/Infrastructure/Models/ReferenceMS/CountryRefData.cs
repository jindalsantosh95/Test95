using HotChocolate;

namespace VAA.EAPI.GraphQL.Domain.RefData.Infrastructure.Models.ReferenceMS
{
    [RefDataType("countries", "countries", "")]
    public class CountryRefData
    {
        [GraphQLName("isoCountryCode")]
        public string ISOCountryCode { get; set; }
        [GraphQLName("iataCountryCode")]
        public string IATACountryCode { get; set; }
        public string CountryName { get; set; }
        public string DialingPrefix { get; set; }
    }
}
