using System;
using System.Threading.Tasks;

namespace VAA.EAPI.Microservices.Loyalty.RulesEngine.Infrastructure.Interfaces
{
    public interface IInRuleClient
    {
        Task<Tuple<bool, string>> ExecuteInRule(string inRuleRequest);
    }
}
