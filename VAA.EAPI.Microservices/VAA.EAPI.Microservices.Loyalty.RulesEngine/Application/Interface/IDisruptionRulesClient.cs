using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.RulesEngine.Domain.Entities.Disruption;

namespace VAA.EAPI.Microservices.Loyalty.RulesEngine.Application.Interface
{
    public interface IDisruptionRulesClient
    {
        Task<Response<RulesEvaluationResponse>> EvaluateAsync(InRuleEvaluationRequest request, string requestBody, CancellationToken cancellationToken);
    }
}
