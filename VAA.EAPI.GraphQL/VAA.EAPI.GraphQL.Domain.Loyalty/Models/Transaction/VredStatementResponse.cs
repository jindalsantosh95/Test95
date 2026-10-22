using System.Collections.Generic;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Models.Transaction
{
    public class VredStatementResponse
    {
        public IEnumerable<VredStatement> Statements { get; set; }
    }
}
