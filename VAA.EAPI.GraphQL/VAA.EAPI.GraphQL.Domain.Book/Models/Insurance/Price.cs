using HotChocolate;

namespace VAA.EAPI.GraphQL.Domain.Book.Models.Insurance
{
    [GraphQLName("InsurancePrice")]
    public class Price
    {
        public bool IsAvailable { get; set; }
        public bool IsIncluded { get; set; }
        public double Amount { get; set; }
        public string Currency { get; set; }
    }
}