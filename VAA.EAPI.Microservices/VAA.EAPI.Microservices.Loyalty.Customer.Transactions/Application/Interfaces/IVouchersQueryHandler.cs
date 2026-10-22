using CSharpFunctionalExtensions;
using System.Collections.Generic;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Query.Vouchers;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.Vouchers;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Interfaces
{
    public interface IVouchersQueryHandler
    {
        Task<Result<Response<IEnumerable<VoucherResponse>>>> GetVouchersByCustomerIdAsync(VouchersQuery query);
    }
}
