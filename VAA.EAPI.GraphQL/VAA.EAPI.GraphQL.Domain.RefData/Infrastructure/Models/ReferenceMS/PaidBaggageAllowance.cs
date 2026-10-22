namespace VAA.EAPI.GraphQL.Domain.RefData.Infrastructure.Models.ReferenceMS
{
    public class PaidBaggageAllowance : FreeBaggageAllowance
    {
        public decimal FirstBagCost { get; set; }
        public decimal SecondBagCost { get; set; }
        public string Currency { get; set; }
    }
}
