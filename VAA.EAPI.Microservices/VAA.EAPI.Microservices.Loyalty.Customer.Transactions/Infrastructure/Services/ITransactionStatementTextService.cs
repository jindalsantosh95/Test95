using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.Transaction;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Infrastructure.Services
{
    public interface ITransactionStatementTextService
    {
        Task<string> GetTransactionStatementText(StatementTextRequest request, CancellationToken cancellationToken);
    }
}
