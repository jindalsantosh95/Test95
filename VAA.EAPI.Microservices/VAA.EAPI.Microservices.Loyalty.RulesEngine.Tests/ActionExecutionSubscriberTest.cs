using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Text.Json;
using System.Threading.Tasks;
using VAA.EAPI.Microservices.Loyalty.RulesEngine.Application.Command;
using VAA.EAPI.Microservices.Loyalty.RulesEngine.Triggers.ServiceBus;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Loyalty.RulesEngine.Tests
{
    public class ActionExecutionSubscriberTest
    {
        private readonly Mock<IMediator> _mediator;
        private readonly Mock<ILogger<ActionExecutionSubscriber>> _logger;
        private readonly ActionExecutionSubscriber _subscriber;

        public ActionExecutionSubscriberTest()
        {
            _mediator = new Mock<IMediator>();
            _logger = new Mock<ILogger<ActionExecutionSubscriber>>();
            _subscriber = new ActionExecutionSubscriber(_mediator.Object, _logger.Object);
        }
        [Fact]
        public void ConstructorShouldThrowNullExceptionWhenLoggerObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new ActionExecutionSubscriber(_mediator.Object, null));
        }
        [Fact]
        public void ConstructorShouldThrowNullExceptionWhenMediatorObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new ActionExecutionSubscriber(null, _logger.Object));
        }

        [Fact]
        public async Task ActionExecutionSubscriberSuccessAsync()
        {
            var eventRequest = TestDataProvider.GetTestData<ActionExecutionEventCommand>("InRuleEngine\\ActionExecutionEventCommand");
            var request = JsonSerializer.Serialize(eventRequest);
            _mediator.Setup(m => m.Send(It.IsAny<ActionExecutionEventCommand>(), System.Threading.CancellationToken.None)).ReturnsAsync(true);
            await _subscriber.Run(request);
        }
    }
}
