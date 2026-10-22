namespace VAA.EAPI.GraphQL.Domain.Book.Models.Common
{
    public class Tax
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public CurrencyAmount Amount { get; set; }
        public string CountryCode { get; set; }
        public IEnumerable<CollectionPoint> CollectionPoints { get; set; }
        public bool Refundable { get; set; }


    }
}
