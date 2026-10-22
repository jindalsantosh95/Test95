using VAA.EAPI.Domain.Entities.Common;

namespace VAA.EAPI.Microservices.Loyalty.Vred.Domain.Transaction
{
    public class TransactionQueryResponse
    {
        public Status Status { get; set; }
        public string Value { get; set; }
    }
}