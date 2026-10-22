using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.Awards;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.Transaction;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Infrastructure.Services.VoucherMonitoring
{
    public interface ICustomerTransactionDataProvider
    {
        Task<IEnumerable<Award>> GetAwardsAsync(string customerId, CancellationToken cancellationToken);
        Task<IEnumerable<TransactionDocument>> GetTransactionsAsync(string customerId, CancellationToken cancellationToken);
    }
}
