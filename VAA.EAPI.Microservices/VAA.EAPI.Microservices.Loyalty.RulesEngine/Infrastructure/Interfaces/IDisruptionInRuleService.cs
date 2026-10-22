using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Microservices.Loyalty.RulesEngine.Domain.Entities.Disruption;

namespace VAA.EAPI.Microservices.Loyalty.RulesEngine.Infrastructure.Interfaces
{
    public interface IDisruptionInRuleService
    {
        Task<InRuleResponse> EvaluateAsync(string requestBody, CancellationToken cancellationToken);
    }
}
