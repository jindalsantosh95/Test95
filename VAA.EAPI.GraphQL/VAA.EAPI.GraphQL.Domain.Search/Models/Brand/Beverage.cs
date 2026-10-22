namespace VAA.EAPI.GraphQL.Domain.Search.Models.Brand
{
    public class Beverage : AmenityBase
    {
        public string? AlcoholicCost { get; set; }
        public string? Exists { get; set; }
        public string? NonAlcoholicCost { get; set; }
    }
}