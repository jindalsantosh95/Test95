namespace VAA.EAPI.GraphQL.Domain.Search.Models.Basket
{
    public class UpSellOptions
    {
        public string Selected { get; set; }
        public IEnumerable<string> Options { get; set; }
        public string Type { get; set; }
    }
}