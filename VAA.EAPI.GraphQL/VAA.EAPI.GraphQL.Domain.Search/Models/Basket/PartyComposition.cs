namespace VAA.EAPI.GraphQL.Domain.Search.Models.Basket
{
    public class PartyComposition
    {
        public int Adults { get; set; }
        public int Children { get; set; }
        public int Teens { get; set; }
        public int Infants { get; set; }
        public int Total { get => Adults + Children + Teens + Infants; }
    }
}