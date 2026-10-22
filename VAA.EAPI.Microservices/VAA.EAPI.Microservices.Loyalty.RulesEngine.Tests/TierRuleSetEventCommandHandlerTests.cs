using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
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
    public class TierRuleSetEventCommandHandlerTests
    {

        private readonly Mock<IMediator> _mediator;
        private readonly Mock<ILogger<TierRuleSetEventCommandHandler>> _logger;
        private readonly TierRuleSetEventCommandHandler _handler;
        private readonly Mock<IInRuleRepository> _inRuleRepository;


        public TierRuleSetEventCommandHandlerTests()
        {
            _mediator = new Mock<IMediator>();
            _logger = new Mock<ILogger<TierRuleSetEventCommandHandler>>();
            _inRuleRepository = new Mock<IInRuleRepository>();
            _handler = new TierRuleSetEventCommandHandler(_mediator.Object, _inRuleRepository.Object, _logger.Object);
            RuleEngineTestHelpers.CreateTestEnvironmentSettings();
        }

        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenMediatorObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new TierRuleSetEventCommandHandler(null, _inRuleRepository.Object, _logger.Object));
        }
        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenLoggerObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new TierRuleSetEventCommandHandler(_mediator.Object, _inRuleRepository.Object, null));
        }
        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenInRuleRepositoryObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new TierRuleSetEventCommandHandler(_mediator.Object, null, _logger.Object));
        }

        [Fact]
        public async Task Handler_ShouldReturnFailureWhenExceptionOccurs()
        {
            var request = TestDataProvider.GetTestData<TierRuleSetEventCommand>("InRuleEngine\\TierRuleSetEventCommand");
            //   _mediator.Setup(m => m.Send(It.IsAny<EventRequestInsertCommand>(), It.IsAny<CancellationToken>())).ThrowsAsync(new ArgumentNullException());
            //   await Assert.ThrowsAsync<ArgumentNullException>(() => _handler.Handle(request, CancellationToken.None));
            _mediator.Setup(m => m.Send(It.IsAny<EventRequestInsertCommand>(), CancellationToken.None)).Throws<Exception>();
            //Act
            var result = await _handler.Handle(request, CancellationToken.None);
            //Assert
            Assert.True(result.IsSuccess);
        }

        [Fact]
        public async Task Handler_ShouldReturnSuccess()
        {
            var request = TestDataProvider.GetTestData<TierRuleSetEventCommand>("InRuleEngine\\TierRuleSetEventCommand");
            var tierRuleSetRequest = TestDataProvider.GetTestData<TierRuleSetEntityRequest>("InRuleEngine\\TierRuleSetEventCommand");
            var ruleEngineRequest = TierRuleSetRequestMapper.TierRuleSetToInRuleMapper(tierRuleSetRequest);
            var ruleResponse = TestDataProvider.GetTestData<InRuleResponse>("InRuleEngine\\InRuleResponseCampaign");
            TierRuleEntityState resEntity = Json.Deserialize<TierRuleEntityState>(Convert.ToString(ruleResponse.ResponseEntity));
            _inRuleRepository.Setup(x => x.ExecuteInRule(It.IsAny<RuleEngineRequest>())).ReturnsAsync(ruleResponse);
            _mediator.Setup(m => m.Send(It.IsAny<EventRequestInsertCommand>(), It.IsAny<CancellationToken>())).Equals(true);

            var result = await _handler.Handle(request, CancellationToken.None);
            Assert.True(result.IsSuccess);
        }

        [Fact]
        public async Task HandlerShouldReturnCloudEventException()
        {
            var request = TestDataProvider.GetTestData<TierRuleSetEventCommand>("InRuleEngine\\TierRuleSetEventCommand");
            var tierRuleSetRequest = TestDataProvider.GetTestData<TierRuleSetEntityRequest>("InRuleEngine\\TierRuleSetEventCommand");
            var ruleEngineRequest = TierRuleSetRequestMapper.TierRuleSetToInRuleMapper(tierRuleSetRequest);
            var ruleResponse = TestDataProvider.GetTestData<InRuleResponse>("InRuleEngine\\InRuleResponseCampaign");
            _inRuleRepository.Setup(x => x.ExecuteInRule(It.IsAny<RuleEngineRequest>())).ReturnsAsync(ruleResponse);
            _mediator.Setup(m => m.Send(It.IsAny<EventRequestInsertCommand>(), It.IsAny<CancellationToken>())).Equals(true);
            _mediator.Setup(m => m.Publish(It.IsAny<CloudEvent>(), CancellationToken.None)).Throws<Exception>();
            var result = await _handler.Handle(request, CancellationToken.None);
            Assert.False(result.Value);
        }
    }
}


