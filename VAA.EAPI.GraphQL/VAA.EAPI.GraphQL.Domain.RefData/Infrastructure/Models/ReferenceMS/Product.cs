namespace VAA.EAPI.GraphQL.Domain.RefData.Infrastructure.Models.ReferenceMS
{
    [RefDataType("products", "products", "")]
    public class Product
    {
        public string ProductId { get; set; }
        public string ProductName { get; set; }
        public string ProductType { get; set; }
        public string ProductCategory { get; set; }
        public string ProductDescription { get; set; }
        public string Productpoints { get; set; }
        public string AllowCancellation { get; set; }
        public string OfferType { get; set; }
        public string DivisionCode { get; set; }
        public string ExpirationPeriod { get; set; }
        public string ExpirationPeriodType { get; set; }
        public string AwardPeriod { get; set; }
        public string AwardPeriodType { get; set; }

    }
}
