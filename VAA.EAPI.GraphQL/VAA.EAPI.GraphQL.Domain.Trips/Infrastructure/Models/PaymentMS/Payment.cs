namespace VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.PaymentMS
{
    public class Payment
    {
        public CurrencyEquivalentPrice CurrencyEquivalentPrice { get; set; }
        public MilesEquivalentPrice MilesEquivalentPrice { get; set; }
    }
}
