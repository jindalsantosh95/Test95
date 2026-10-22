using CSharpFunctionalExtensions;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.Vouchers;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Query.Vouchers
{
    public interface IVoucherTxnsPnrUpdateCommandStatusQuery
    {
        Task<Result<Response<VoucherTxnsPnrUpdateCommandQueryResponse>>> Handle(VoucherTxnsPnrUpdateCommandStatusQuery request, CancellationToken cancellationToken);
    }
}
