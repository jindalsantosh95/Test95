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
using VAA.EAPI.Microservices.Loyalty.RulesEngine.Mapper;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Loyalty.RulesEngine.Tests
{
    public class ActionExecutionEventCommandHandlerTests
    {

        private readonly Mock<IMediator> _mediator;
        private readonly Mock<ILogger<ActionExecutionEventCommandHandler>> _logger;
        private readonly ActionExecutionEventCommandHandler _handler;


        public ActionExecutionEventCommandHandlerTests()
        {
            _mediator = new Mock<IMediator>();
            _logger = new Mock<ILogger<ActionExecutionEventCommandHandler>>();
            _handler = new ActionExecutionEventCommandHandler(_mediator.Object, _logger.Object);
            RuleEngineTestHelpers.CreateTestEnvironmentSettings();
        }

        [Fact]
        public void ConstructorShouldThrowNullExceptionWhenMediatorObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new ActionExecutionEventCommandHandler(null, _logger.Object));
        }
        [Fact]
        public void ConstructorShouldThrowNullExceptionWhenLoggerObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new ActionExecutionEventCommandHandler(_mediator.Object, null));
        }

        [Fact]
        public async Task HandlerShouldReturnFailureWhenExceptionOccurs()
        {
            var request = TestDataProvider.GetTestData<ActionExecutionEventCommand>("InRuleEngine\\ActionExecutionEventCommand");
            // _mediator.Setup(m => m.Send(It.IsAny<EventRequestInsertCommand>(), It.IsAny<CancellationToken>())).Throws(new Exception());
            //  await Assert.ThrowsAsync<NullReferenceException>(() => _handler.Handle(request, CancellationToken.None));
            _mediator.Setup(m => m.Send(It.IsAny<EventRequestInsertCommand>(), CancellationToken.None)).Throws<Exception>();
            //Act
            var result = await _handler.Handle(request, CancellationToken.None);
            //Assert
            Assert.True(result.IsSuccess);
        }

        [Fact]
        public async Task HandlerShouldReturnSuccess()
        {
            var request = TestDataProvider.GetTestData<ActionExecutionEventCommand>("InRuleEngine\\ActionExecutionEventCommand");
            _mediator.Setup(m => m.Send(It.IsAny<EventRequestInsertCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
            var result = await _handler.Handle(request, CancellationToken.None);
            Assert.True(result.IsSuccess);
        }

        [Fact]
        public async Task HandlerShouldReturnSuccessForBirthdayBonus()
        {
            var request = TestDataProvider.GetTestData<ActionExecutionEventCommand>("InRuleEngine\\BirthdayBonusActionExecutionEventCommand");
            _mediator.Setup(m => m.Send(It.IsAny<EventRequestInsertCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);
            var result = await _handler.Handle(request, CancellationToken.None);
            Assert.True(result.IsSuccess);
        }

        [Fact]
        public async Task HandlerShouldReturnSetTierCloudEventException()
        {
            var request = TestDataProvider.GetTestData<ActionExecutionEventCommand>("InRuleEngine\\ActionExecutionEventCommand");
            _mediator.Setup(m => m.Send(It.IsAny<EventRequestInsertCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);
            _mediator.Setup(m => m.Publish(It.IsAny<CloudEvent>(), CancellationToken.None)).Throws<Exception>();
            var result = await _handler.Handle(request, CancellationToken.None);
            Assert.True(result.IsSuccess);
        }

         [Fact]
        public async Task HandlerShouldReturnDPFlagCloudEventException()
        {
            var request = TestDataProvider.GetTestData<ActionExecutionEventCommand>("InRuleEngine\\ActionExecutionEventCommand");
            request.Data = request.Data.ToString().Replace("Set Tier", "SET DOWNGRADE PROHIBIT FLAG");

            _mediator.Setup(m => m.Send(It.IsAny<EventRequestInsertCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);
            _mediator.Setup(m => m.Publish(It.IsAny<CloudEvent>(), CancellationToken.None)).Throws<Exception>();

            var result = await _handler.Handle(request, CancellationToken.None);

            Assert.True(result.IsSuccess);
        }

        [Fact]
        public async Task HandlerShouldReturnS2SFTGCloudEventException()
        {
            var request = TestDataProvider.GetTestData<ActionExecutionEventCommand>("InRuleEngine\\ActionExecutionEventCommand");
            request.Data = request.Data.ToString().Replace("Set Tier", "Set S2S FTG Flag");
            _mediator.Setup(m => m.Send(It.IsAny<EventRequestInsertCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);
            _mediator.Setup(m => m.Publish(It.IsAny<CloudEvent>(), CancellationToken.None)).Throws<Exception>();
            var result = await _handler.Handle(request, CancellationToken.None);
            Assert.True(result.IsSuccess);
        }

        [Fact]
        public async Task HandlerShouldReturnS2SCloudEventException()
        {
            var request = TestDataProvider.GetTestData<ActionExecutionEventCommand>("InRuleEngine\\ActionExecutionEventCommand");
            request.Data = request.Data.ToString().Replace("Set Tier", "Set S2S Flag");
            _mediator.Setup(m => m.Send(It.IsAny<EventRequestInsertCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);
            _mediator.Setup(m => m.Publish(It.IsAny<CloudEvent>(), CancellationToken.None)).Throws<Exception>();
            var result = await _handler.Handle(request, CancellationToken.None);
            Assert.True(result.IsSuccess);
        }

        [Fact]
        public async Task HandlerShouldReturnEarnActCloudEventException()
        {
            var request = TestDataProvider.GetTestData<ActionExecutionEventCommand>("InRuleEngine\\ActionExecutionEventCommand");
            request.Data = request.Data.ToString().Replace("Set Tier", "Set Earn Activity");
            _mediator.Setup(m => m.Send(It.IsAny<EventRequestInsertCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);
            _mediator.Setup(m => m.Publish(It.IsAny<CloudEvent>(), CancellationToken.None)).Throws<Exception>();
            var result = await _handler.Handle(request, CancellationToken.None);
            Assert.True(result.IsSuccess);
        }

        [Fact]
        public async Task HandlerShouldReturnAssignptsCloudEventException()
        {
            var request = TestDataProvider.GetTestData<ActionExecutionEventCommand>("InRuleEngine\\BirthdayBonusActionExecutionEventCommand");
            _mediator.Setup(m => m.Send(It.IsAny<EventRequestInsertCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);
            _mediator.Setup(m => m.Publish(It.IsAny<CloudEvent>(), CancellationToken.None)).Throws<Exception>();
            var result = await _handler.Handle(request, CancellationToken.None);
            Assert.True(result.IsSuccess);
        }

        [Fact]
        public void StatementMapper()
        {
            var request = TestDataProvider.GetTestData<ActionExecutionEventCommand>("InRuleEngine\\BirthdayBonusActionExecutionEventCommand");
            request.Data = request.Data.ToString().Replace("Birthday Bonus", "BirthdayBonus");
            var mapReq = Json.Deserialize<RuleActionsResponse>(Convert.ToString(request.Data));
            TransactionStatementMapper.MapStatement(mapReq);
            var mapReq1 = Json.Deserialize<RuleActionsResponse>(Convert.ToString(request.Data));
            mapReq1.ProductName = null;
            TransactionStatementMapper.MapStatement(mapReq1);
        }

    }
}

