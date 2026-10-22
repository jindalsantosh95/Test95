namespace VAA.EAPI.GraphQL.Domain.PaxOps.Infrastructure.Models.BaggageMS
{
    public class Passenger
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string CurrencyCode { get; set; }
        public string TotalPrice { get; set; }
        public int NumberOfExtraBags { get; set; }
        public decimal BagAmount { get; set; }
    }
}
