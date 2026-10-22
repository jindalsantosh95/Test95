using System.Threading.Tasks;
using VAA.EAPI.Microservices.Loyalty.RulesEngine.Domain.Entities.RuleEngine;
using VAA.EAPI.Microservices.Loyalty.RulesEngine.Infrastructure.Entities;

namespace VAA.EAPI.Microservices.Loyalty.RulesEngine.Infrastructure.Interfaces
{
    public interface IInRuleRepository
    {
        Task<InRuleResponse> ExecuteInRule(RuleEngineRequest request);
    }
}
