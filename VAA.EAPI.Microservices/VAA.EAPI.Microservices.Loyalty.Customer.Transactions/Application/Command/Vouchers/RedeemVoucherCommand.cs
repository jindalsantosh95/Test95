using CSharpFunctionalExtensions;
using MediatR;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.BaseClasses;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.Transaction;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.Vouchers;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Command.Vouchers
{
    public class RedeemVoucherCommand : TransactionBaseCommand, IRequest<Result<Response<TransactionCommandResponse>>>
    {
        public RedeemVoucherRequest RedeemVoucherRequest { get; set; }

        public RedeemVoucherCommand(RedeemVoucherRequest redeemVoucherRequest) : base()
        {
            EventType = Domain.Enums.TransactionCommandType.RedeemVoucher;
            RedeemVoucherRequest = redeemVoucherRequest;
        }
    }
}
