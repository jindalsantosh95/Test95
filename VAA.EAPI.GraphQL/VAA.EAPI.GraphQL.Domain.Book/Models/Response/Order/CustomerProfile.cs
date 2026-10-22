using HotChocolate;
using VAA.EAPI.GraphQL.Domain.Book.Models.Basket;

namespace VAA.EAPI.GraphQL.Domain.Book.Models.Response.Order
{
    [GraphQLName("bookCustomerProfile")]
    public class CustomerProfile
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public FQTMembership MembershipDetails { get; set; } = new();
    }
}