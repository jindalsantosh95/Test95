
namespace VAA.EAPI.Microservices.Loyalty.Vred.Domain.Transaction
{
    public class TransactionResponse
    {
        public string Id { get; set; }
        public string RequestId { get; set; }
        public string PartnerTransactionId { get; set; }
        public string PartnerCode { get; set; }
        public string PartnerUserId { get; set; }
        public string VAA { get; set; }
    }
}
