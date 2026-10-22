using System;
using System.Threading.Tasks;
using VAA.EAPI.Microservices.Loyalty.RulesEngine.Domain.Entities.RuleEngine;
using VAA.EAPI.Microservices.Loyalty.RulesEngine.Infrastructure.Entities;
using VAA.EAPI.Microservices.Loyalty.RulesEngine.Infrastructure.Interfaces;

namespace VAA.EAPI.Microservices.Loyalty.RulesEngine.Tests
{
    public sealed class TestInRuleRepository : IInRuleRepository
    {
        public RuleEngineRequest Request { get; private set; } = null!;
        public InRuleResponse Response { get; set; } = null!;
        public Exception? Exception { get; set; }

        public Task<InRuleResponse> ExecuteInRule(RuleEngineRequest request)
        {
            Request = request;
            return Exception == null ? Task.FromResult(Response) : Task.FromException<InRuleResponse>(Exception);
        }
    }
}
