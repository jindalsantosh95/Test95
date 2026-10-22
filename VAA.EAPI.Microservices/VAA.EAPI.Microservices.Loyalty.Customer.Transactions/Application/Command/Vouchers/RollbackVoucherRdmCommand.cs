using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.BaseClasses;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.Vouchers;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Command.Vouchers
{
    public class RollbackVoucherRdmCommand : TransactionBaseCommand
    {
        public RollbackVoucherRdmRequest RollbackVoucherRdmRequest { get; set; }

        public RollbackVoucherRdmCommand(RollbackVoucherRdmRequest rollbackVoucherRdmRequest) : base()
        {
            RollbackVoucherRdmRequest = rollbackVoucherRdmRequest;
            EventType = Domain.Enums.TransactionCommandType.RollbackVoucherRedemption;
        }
    }
}
