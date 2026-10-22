using MediatR;
using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Retail.Order.Data.Infrastructure.Repositories.PaymentUpdate
{
    public class CosmosQueryCommand : IRequest<List<object>>
    {
        public string PartitionKey { get; set; }
        public string DocType { get; set; }
        public string SqlQuery { get; set; }
    }
}
