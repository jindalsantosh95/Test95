using VAA.EAPI.Infrastructure.Repository.CosmosSqlDBv2;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.CommandStatus
{
    public class CommandStatusResponse
    {
        public CosmosDocument Document { get; set; }

        public CommandStatusResponse()
        {
            // Required for Serialisation
        }
    }
}
