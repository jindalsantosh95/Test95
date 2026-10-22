namespace VAA.EAPI.GraphQL.Domain.Search.Models.FareInfo
{
    public class FareRuleFee
    {
        public decimal Amount { get; set; }
        public string Currency { get; set; }

        public static explicit operator FareRuleFee(Brand.Fee source)
        {
            return new()
            {
                Amount = (decimal)source.Amount,
                Currency = source.Currency
            };
        }
    }
}
