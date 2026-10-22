using CSharpFunctionalExtensions;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.Transaction;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Command.Vouchers
{
    public interface IPatchVoucherRelatedTxnsPnrCommandHandler
    {
        Task<Result<Response<TransactionCommandResponse>>> Handle(PatchVoucherRelatedTxnsPnrCommand request, CancellationToken cancellationToken);
    }
}
