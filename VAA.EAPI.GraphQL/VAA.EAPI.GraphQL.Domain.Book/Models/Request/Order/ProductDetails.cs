using HotChocolate;

namespace VAA.EAPI.GraphQL.Domain.Book.Models.Request.Order
{
    [GraphQLName("insuranceProductDetails")]
    public class ProductDetails
    {
        public IEnumerable<Product> Product { get; set; }
    }
}