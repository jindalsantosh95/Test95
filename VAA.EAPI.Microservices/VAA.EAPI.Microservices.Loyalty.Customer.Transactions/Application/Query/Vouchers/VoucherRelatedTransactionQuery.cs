using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.BaseClasses;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Query.Vouchers
{
    public class VoucherRelatedTransactionQuery : TransactionBase
    {
        public string AwardId { get; set; }
        public string VoucherNumber { get; set; }
    }
}
