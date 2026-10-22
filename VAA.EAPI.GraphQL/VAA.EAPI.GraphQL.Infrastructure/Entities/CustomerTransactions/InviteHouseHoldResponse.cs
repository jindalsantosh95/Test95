using System.Collections.Generic;

namespace VAA.EAPI.GraphQL.Infrastructure.Entities.CustomerTransactions
{
    public class InviteHouseHoldResponse<T>
    {
        public string CustomerId { get; set; }

        public IEnumerable<T> InviteHouseHold { get; set; }
    }
}
