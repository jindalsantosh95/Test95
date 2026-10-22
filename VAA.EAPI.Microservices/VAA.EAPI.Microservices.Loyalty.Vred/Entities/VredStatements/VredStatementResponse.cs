using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Loyalty.Vred.Entities.VredStatements
{
    public class VredStatementResponse
    {
        public IEnumerable<Statement> Statements { get; set; }
    }
}