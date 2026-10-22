using HotChocolate;

namespace VAA.EAPI.GraphQL.Domain.Servicing.Models.Booking
{
    public class FarePriceDetail
    {
        [GraphQLDescription("Base Currency")]
        public string? BaseCurrency { get; set; }
        [GraphQLDescription("Total Fare Currency")]
        public string? TotalFareCurrency { get; set; }
    }
}
