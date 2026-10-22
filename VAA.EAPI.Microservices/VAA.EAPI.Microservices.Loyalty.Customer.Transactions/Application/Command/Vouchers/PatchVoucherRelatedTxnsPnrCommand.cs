using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.BaseClasses;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.Vouchers;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Command.Vouchers
{
    public class PatchVoucherRelatedTxnsPnrCommand : TransactionBaseCommand
    {
        public PatchVoucherRelatedTxnsPnrRequest PatchVoucherRelatedTxnsPnrRequest { get; set; }

        public PatchVoucherRelatedTxnsPnrCommand(PatchVoucherRelatedTxnsPnrRequest patchVoucherRelatedTxnsPnrRequest) : base()
        {
            PatchVoucherRelatedTxnsPnrRequest = patchVoucherRelatedTxnsPnrRequest;
            EventType = Domain.Enums.TransactionCommandType.UpdateVoucherRelatedTxnsPnr;
        }
    }
}
