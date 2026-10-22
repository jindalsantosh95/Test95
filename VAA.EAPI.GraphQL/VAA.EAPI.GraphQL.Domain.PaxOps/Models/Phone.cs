using HotChocolate;
using System.Collections.Generic;

namespace VAA.EAPI.GraphQL.Domain.PaxOps.Models
{
    [GraphQLName("PaxOps:Phone")]
    public class Phone
    {
        [GraphQLDescription("International dialing code for the phone number.")]
        public string CountryCode { get; set; }

        public string Number { get; set; }


        public static explicit operator Phone(Infrastructure.Models.CheckInMS.Phone source) => new()
        {
            CountryCode = source.CountryCode,
            Number = source.Number
        };
    }
}
