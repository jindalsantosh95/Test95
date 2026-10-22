using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.RuleEngine;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Infrastructure.Services.RuleEngineMs
{
    public interface IRulesEngineService
    {
        Task<TResponse> ExecuteAsync<TRequest, TResponse>(string operation, HttpMethod method, TRequest request, CancellationToken cancellationToken);
        Task<TResponse> ExecuteAsync<TResponse>(string operation, HttpMethod method, CancellationToken cancellationToken);
        Task<EarnAirlineAccrualCalculatorResponse> EarnAirlineAccrualCalculatorAsync(EarnAirlineAccrualCalculatorRequest request, CancellationToken cancellationToken);
    }
}
