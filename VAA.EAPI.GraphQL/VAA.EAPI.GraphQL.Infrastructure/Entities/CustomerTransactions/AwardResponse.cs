using System.Collections.Generic;

namespace VAA.EAPI.GraphQL.Infrastructure.Entities.CustomerTransactions
{
    public class AwardResponse<T>
    {
        public string customerId { get; set; }

        public IEnumerable<T> awards { get; set; }
    }
}
