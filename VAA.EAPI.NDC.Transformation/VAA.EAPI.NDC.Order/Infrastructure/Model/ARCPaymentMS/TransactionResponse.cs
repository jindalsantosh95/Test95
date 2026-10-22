using System.Collections.Generic;

namespace VAA.EAPI.NDC.Order.Infrastructure.Model.ARCPaymentMS
{
    public class TransactionResponse
    {
        public string AgentNumericCode { get; set; }
        public string AccreditationType { get; set; }
        public Arc Arc { get; set; }
        public IEnumerable<Ticketing> Ticketing { get; set; }
    }
}
