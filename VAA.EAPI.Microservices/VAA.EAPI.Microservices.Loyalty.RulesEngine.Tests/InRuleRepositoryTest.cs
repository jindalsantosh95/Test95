using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Microservices.Loyalty.RulesEngine.Domain.Entities.RuleEngine;
using VAA.EAPI.Microservices.Loyalty.RulesEngine.Infrastructure.Entities;
using VAA.EAPI.Microservices.Loyalty.RulesEngine.Infrastructure.Interfaces;
using VAA.EAPI.Microservices.Loyalty.RulesEngine.Infrastructure.Mapper;
using VAA.EAPI.Microservices.Loyalty.RulesEngine.Infrastructure.Repository;
using VAA.EAPI.Microservices.Loyalty.RulesEngine.Mapper;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Loyalty.RulesEngine.Tests
{
    public class InRuleRepositoryTest
    {
        private readonly Mock<IInRuleClient> _client;
        private readonly Mock<ILogger<InRuleRepository>> _logger;
        private readonly InRuleRepository _repo;

        public InRuleRepositoryTest()
        {
            _client = new Mock<IInRuleClient>();
            _logger = new Mock<ILogger<InRuleRepository>>();
            _repo = new InRuleRepository(_logger.Object, _client.Object);
            RuleEngineTestHelpers.CreateTestEnvironmentSettings();
        }

        [Fact]
        public void ConstructorShouldThrowNullExceptionWhenObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new InRuleRepository(null, _client.Object));

            Assert.Throws<ArgumentNullException>(() =>
            new InRuleRepository(_logger.Object, null));
        }

        [Fact]
        public async Task ExecuteInRule()
        {
            var combinedRuleSetRequest = TestDataProvider.GetTestData<CombinedRuleSetEntityRequest>("InRuleEngine\\CombinedRuleSetEventCommand");
            var ruleEngineRequest = CombinedRuleSetRequestMapper.CombinedRuleSetToInRuleMapper(combinedRuleSetRequest);
            var inRuleRequest = InRuleMapper.MapInRuleRequest(ruleEngineRequest);
            var ruleResponse = TestDataProvider.GetTestData<string>("InRuleEngine\\ExecuteRuleResponse");
            Tuple<bool, string> result = Tuple.Create(true, ruleResponse);
            _client.Setup(x => x.ExecuteInRule(It.IsAny<string>())).ReturnsAsync(result);
            ApplyRuleResponse inRuleResponse = Json.Deserialize<ApplyRuleResponse>(result.Item2);
            InRuleResponse inRuleGenricResponse = InRuleResponseMapper.MapInRuleResponse(inRuleRequest, inRuleResponse);
            var resultRule = await _repo.ExecuteInRule(ruleEngineRequest);
            Assert.NotNull(resultRule.ResponseEntity);
            _client.Verify(
                client => client.ExecuteInRule(It.IsAny<string>()),
                Times.Once);
        }
    }
}
