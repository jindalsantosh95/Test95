using CSharpFunctionalExtensions;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Infrastructure.Entities;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.EventHandlers.Vouchers
{
    public interface IRedeemVoucherEventHandler
    {
        Task<Result<Response<TransactionDTO>>> Handle(RedeemVoucherEvent redeemVoucherEvent, CancellationToken cancellationToken);
    }
}
