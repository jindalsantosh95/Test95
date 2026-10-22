using CSharpFunctionalExtensions;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Query.Vouchers;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.Transaction;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Interfaces
{
    public interface IVoucherRelatedTransactionQueryHandler
    {
        Task<Result<Response<TransactionResponse>>> GetVoucherRelatedTransactions(VoucherRelatedTransactionQuery query);
    }
}
