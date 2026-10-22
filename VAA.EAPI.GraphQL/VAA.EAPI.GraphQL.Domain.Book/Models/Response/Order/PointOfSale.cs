using HotChocolate;

namespace VAA.EAPI.GraphQL.Domain.Book.Models.Response.Order
{
    [GraphQLName("bookPointOfSale")]
    public class PointOfSale
    {
        public string CountryCode { get; set; }
        public string CityCode { get; set; }
    }
}