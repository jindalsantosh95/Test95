using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.Transaction;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Infrastructure.Services.CustomerTransactionMs
{
    public interface ICustomerTransactionService
    {
        Task<TResponse> ExecuteAsync<TRequest, TResponse>(string operation, HttpMethod method, TRequest request, CancellationToken cancellationToken);
        Task<TResponse> ExecuteAsync<TResponse>(string operation, HttpMethod method, CancellationToken cancellationToken);
        Task<TransactionCommandStatusResponse> GetTransactionStatusAsync(string requestId, CancellationToken cancellationToken);
    }
}
