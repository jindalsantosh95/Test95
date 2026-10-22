namespace VAA.EAPI.GraphQL.Domain.RefData.Infrastructure.Models.ReferenceMS
{
    [RefDataType("productidentifiers", "productidentifiers")]
    public class ProductInfo
    {
        public string ProductName { get; set; }
        public string Airline { get; set; }
        public string CabinName { get; set; }
    }
}
