namespace VAA.EAPI.GraphQL.Domain.Search.Models.Reference
{
    public class RefPaidBaggageAllowance : RefFreeBaggageAllowance
    {
        public decimal FirstBagCost { get; set; }
        public decimal SecondBagCost { get; set; }
        public string? Currency { get; set; }
    }
}
