using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Events;
using VAA.EAPI.Infrastructure.Repository.EventStore;
using VAA.EAPI.Microservices.Loyalty.RulesEngine.Application.Command;
using VAA.EAPI.Microservices.Loyalty.RulesEngine.Domain.Entities.RuleEngine;
using VAA.EAPI.Microservices.Loyalty.RulesEngine.Infrastructure.Entities;
using VAA.EAPI.Microservices.Loyalty.RulesEngine.Infrastructure.Interfaces;
using VAA.EAPI.Microservices.Loyalty.RulesEngine.Mapper;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Loyalty.RulesEngine.Tests
{
    public class CombinedRuleSetEventCommmandHandlerTest
    {
        private readonly Mock<IMediator> _mediator;
        private readonly Mock<ILogger<CombinedRuleSetEventCommmandHandler>> _logger;
        private readonly CombinedRuleSetEventCommmandHandler _handler;
        private readonly Mock<IInRuleRepository> _inRuleRepository;

        public CombinedRuleSetEventCommmandHandlerTest()
        {
            _mediator = new Mock<IMediator>();
            _logger = new Mock<ILogger<CombinedRuleSetEventCommmandHandler>>();
            _inRuleRepository = new Mock<IInRuleRepository>();
            _handler = new CombinedRuleSetEventCommmandHandler(_mediator.Object, _inRuleRepository.Object, _logger.Object);
            RuleEngineTestHelpers.CreateTestEnvironmentSettings();
        }

        [Fact]
        public void ConstructorShouldThrowNullExceptionWhenMediatorObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new CombinedRuleSetEventCommmandHandler(null, _inRuleRepository.Object, _logger.Object));
        }

        [Fact]
        public void ConstructorShouldThrowNullExceptionWhenLoggerObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new CombinedRuleSetEventCommmandHandler(_mediator.Object, _inRuleRepository.Object, null));
        }

        [Fact]
        public void ConstructorShouldThrowNullExceptionWhenInRuleRepositoryObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new CombinedRuleSetEventCommmandHandler(_mediator.Object, null, _logger.Object));
        }

        [Fact]
        public async Task HandlerShouldReturnSuccess()
        {
            var request = TestDataProvider.GetTestData<CombinedRuleSetEventCommmand>("InRuleEngine\\CombinedRuleSetEventCommand");
            var combinedRuleSetRequest = TestDataProvider.GetTestData<CombinedRuleSetEntityRequest>("InRuleEngine\\CombinedRuleSetEventCommand");
            var ruleEngineRequest = CombinedRuleSetRequestMapper.CombinedRuleSetToInRuleMapper(combinedRuleSetRequest);
            var ruleResponse = TestDataProvider.GetTestData<InRuleResponse>("InRuleEngine\\InRuleResponseBirthdayBonus");
            _inRuleRepository.Setup(x => x.ExecuteInRule(It.IsAny<RuleEngineRequest>())).ReturnsAsync(ruleResponse);
            _mediator.Setup(m => m.Send(It.IsAny<EventRequestInsertCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);
            var result = await _handler.Handle(request, CancellationToken.None);
            Assert.True(result.Value);
        }

        [Fact]
        public async Task HandlerShouldSuccesswithException()
        {
            var request = TestDataProvider.GetTestData<CombinedRuleSetEventCommmand>("InRuleEngine\\CombinedRuleSetEventCommand");
            var combinedRuleSetRequest = TestDataProvider.GetTestData<CombinedRuleSetEntityRequest>("InRuleEngine\\CombinedRuleSetEventCommand");
            var ruleEngineRequest = CombinedRuleSetRequestMapper.CombinedRuleSetToInRuleMapper(combinedRuleSetRequest);
            var ruleResponse = TestDataProvider.GetTestData<InRuleResponse>("InRuleEngine\\InRuleResponseBirthdayBonus");
            _inRuleRepository.Setup(x => x.ExecuteInRule(It.IsAny<RuleEngineRequest>())).Throws(new Exception());
            _mediator.Setup(m => m.Send(It.IsAny<EventRequestInsertCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);
            var result = await _handler.Handle(request, CancellationToken.None);
            Assert.False(result.Value);
        }


        [Fact]
        public async Task HandlerShouldSuccesswithCloudeventException()
        {
            var request = TestDataProvider.GetTestData<CombinedRuleSetEventCommmand>("InRuleEngine\\CombinedRuleSetEventCommand");
            var combinedRuleSetRequest = TestDataProvider.GetTestData<CombinedRuleSetEntityRequest>("InRuleEngine\\CombinedRuleSetEventCommand");
            var ruleEngineRequest = CombinedRuleSetRequestMapper.CombinedRuleSetToInRuleMapper(combinedRuleSetRequest);
            var ruleResponse = TestDataProvider.GetTestData<InRuleResponse>("InRuleEngine\\InRuleResponseBirthdayBonus");
            _inRuleRepository.Setup(x => x.ExecuteInRule(It.IsAny<RuleEngineRequest>())).ReturnsAsync(ruleResponse);
            _mediator.Setup(m => m.Send(It.IsAny<EventRequestInsertCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);
            _mediator.Setup(m => m.Publish(It.IsAny<CloudEvent>(), CancellationToken.None)).Throws<Exception>();
            var result = await _handler.Handle(request, CancellationToken.None);
            Assert.False(result.Value);
        }

    }
}
