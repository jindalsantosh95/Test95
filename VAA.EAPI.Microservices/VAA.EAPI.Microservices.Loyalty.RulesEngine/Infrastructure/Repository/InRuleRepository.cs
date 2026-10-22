using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Microservices.Loyalty.RulesEngine.Domain.Entities.RuleEngine;
using VAA.EAPI.Microservices.Loyalty.RulesEngine.Infrastructure.Entities;
using VAA.EAPI.Microservices.Loyalty.RulesEngine.Infrastructure.Interfaces;
using VAA.EAPI.Microservices.Loyalty.RulesEngine.Infrastructure.Mapper;

namespace VAA.EAPI.Microservices.Loyalty.RulesEngine.Infrastructure.Repository
{
    public class InRuleRepository : IInRuleRepository
    {
        private readonly IInRuleClient _client;
        private readonly ILogger<InRuleRepository> _logger;

        public InRuleRepository(ILogger<InRuleRepository> logger, IInRuleClient client)
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        public async Task<InRuleResponse> ExecuteInRule(RuleEngineRequest request)
        {
            var inRuleRequest = InRuleMapper.MapInRuleRequest(request);
            _logger.LogInformation($"Process started to call InRule for RuleSet");

            Tuple<bool, string> result = await _client.ExecuteInRule(
                Json.Serialize(inRuleRequest, new JsonSerializerSettings()));
            ApplyRuleResponse inRuleResponse = Json.Deserialize<ApplyRuleResponse>(result.Item2);
            InRuleResponse inRuleGenricResponse = InRuleResponseMapper.MapInRuleResponse(inRuleRequest, inRuleResponse);
            return inRuleGenricResponse;
        }
    }
}
