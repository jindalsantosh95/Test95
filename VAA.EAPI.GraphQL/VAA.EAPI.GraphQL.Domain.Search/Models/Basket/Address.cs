namespace VAA.EAPI.GraphQL.Domain.Search.Models.Basket
{
    public sealed class Address
    {
        public string Id { get; set; }
        public string StreetLineOne { get; set; }
        public string StreetLineTwo { get; set; }
        public string TownCity { get; set; }
        public string PostCode { get; set; }
        public string Link { get; set; }
        public string Validation { get; set; }
    }
}