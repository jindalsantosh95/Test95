using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.Vouchers
{
    public class PatchVoucherRelatedTxnsPnrRequest
    {
        public string CustomerId { get; set; }
        public string MemberNumber { get; set; }
        public IEnumerable<string> VoucherNumbers { get; set; }
        public string InitiatedBy { get; set; }
        public string PNR { get; set; }
    }
}
