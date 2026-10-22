namespace VAA.EAPI.GraphQL.Domain.Account.Infrastructure.Models.ReferenceMS
{
    [RefDataType("products", "products")]
    public class ProductDetails
    {
        public string ProductName { get; set; }
        public string RowId { get; set; }
        public string Description { get; set; }
        public string ProductCategory { get; set; }
    }
}
