namespace VAA.EAPI.GraphQL.Domain.Book.Models.Common
{
    public class BaggageAllowance
    {
        public IEnumerable<CheckedBaggageAllowance> CheckedBaggageAllowances { get; set; }
        public IEnumerable<CarryOnBaggageAllowance> CarryOnBaggageAllowances { get; set; }
    }
}
