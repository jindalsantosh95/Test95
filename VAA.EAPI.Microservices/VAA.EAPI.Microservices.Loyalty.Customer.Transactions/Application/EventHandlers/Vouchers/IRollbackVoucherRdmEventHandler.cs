using CSharpFunctionalExtensions;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Infrastructure.Entities;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.EventHandlers.Vouchers
{
    public interface IRollbackVoucherRdmEventHandler
    {
        Task<Result<Response<IEnumerable<TransactionDTO>>>> Handle(RollbackVoucherRdmEvent rollbackVoucherRdmEvent, CancellationToken cancellationToken);
    }
}
