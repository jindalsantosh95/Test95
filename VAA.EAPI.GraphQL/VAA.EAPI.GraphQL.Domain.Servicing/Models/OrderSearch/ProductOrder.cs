namespace VAA.EAPI.GraphQL.Domain.Servicing.Models.OrderSearch
{
    public class ProductOrder
    {
        public string? ProductOwner { get; set; }
        public string? ProductCode { get; set; }
        public string? ProductName { get; set; }
        public enum ProductTypes
        {
            Bag,
            Seat,
            Flight
        }
        public ProductTypes? ProductType { get; set; }
    }
}