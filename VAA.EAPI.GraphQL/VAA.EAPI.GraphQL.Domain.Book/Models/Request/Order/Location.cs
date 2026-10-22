using HotChocolate;

namespace VAA.EAPI.GraphQL.Domain.Book.Models.Request.Order
{
    [GraphQLName("insuranceLocation")]
    public class Location
    {
        public string Code { get; set; }
        public string Region { get; set; }
        public string Country { get; set; }
    }
}