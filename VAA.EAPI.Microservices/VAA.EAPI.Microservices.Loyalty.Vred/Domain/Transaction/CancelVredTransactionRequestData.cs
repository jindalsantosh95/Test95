
namespace VAA.EAPI.Microservices.Loyalty.Vred.Domain.Transaction
{
    public class CancelVredTransactionRequestData
    {
        public string TransactionId { get; set; }
        public string MemberNumber { get; set; }
        public string ActivityType { get; set; }
        public string PartnerCode { get; set; }
        public string Details { get; set; }       
    }
}
