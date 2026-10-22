using HotChocolate;

namespace VAA.EAPI.GraphQL.Domain.PaxOps.Models.ReferenceData
{
    public class CountryProfile
    {
        [GraphQLName("isoCountryCode")]
        public string ISOCountryCode { get; set; }
        [GraphQLName("iataCountryCode")]
        public string IATACountryCode { get; set; }
        public string CountryName { get; set; }
        public string DialingPrefix { get; set; }
    }
}
