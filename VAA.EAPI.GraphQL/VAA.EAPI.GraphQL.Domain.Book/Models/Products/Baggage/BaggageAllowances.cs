namespace VAA.EAPI.GraphQL.Domain.Book.Models.Products.Baggage
{
    public class BaggageAllowances
    {
        public string BaggageAllowanceId { get; set; }
        public string BaggageAllowanceTypeCode { get; set; }
        public FreeBaggageAllowance FreeBaggageAllowance { get; set; }
    }

}
