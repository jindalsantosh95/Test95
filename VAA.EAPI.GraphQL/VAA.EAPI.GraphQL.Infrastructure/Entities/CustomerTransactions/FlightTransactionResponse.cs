using VAA.EAPI.GraphQL.Infrastructure.Entities.CustomerTransactions.CreateTransaction;

namespace VAA.EAPI.GraphQL.Infrastructure.Entities.CustomerTransactions
{
    public class FlightTransactionResponse
    {
        public string RequestId { get; set; }

        public Transaction Transaction { get; set; }
    }
}
