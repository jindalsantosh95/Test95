using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.RulesEngine.Domain.Entities.RuleEngine;

namespace VAA.EAPI.Microservices.Loyalty.RulesEngine.Application.Interface
{
    public interface IExecuteTierRuleSetCommand
    {
        Task<Response<ExecuteTierRuleSetInRuleResponse>> Handle(
            TierRuleSetEntityRequest request,
            CancellationToken cancellationToken);
    }
}
