using HotChocolate;

namespace VAA.EAPI.GraphQL.Domain.Servicing.Models.Booking
{
    public class TotalFeesAndTaxes
    {
        [GraphQLDescription("Fare base currency")]
        public string? BaseCurrency { get; set; }

    }
}
