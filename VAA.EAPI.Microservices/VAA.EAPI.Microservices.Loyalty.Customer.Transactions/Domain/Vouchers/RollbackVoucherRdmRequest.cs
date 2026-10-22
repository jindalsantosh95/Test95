namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.Vouchers
{
    public class RollbackVoucherRdmRequest
    {
        public string CustomerId { get; set; }
        public string MemberNumber { get; set; }
        public string VoucherNumber { get; set; }
        public string RollbackInitiatedBy { get; set; }
        public string RollbackSource { get; set; }
    }
}
