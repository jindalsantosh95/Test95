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
    public class CombinedRuleSetSubscriberTest
    {
        private readonly Mock<IMediator> _mediator;
        private readonly Mock<ILogger<CombinedRuleSetSubscriber>> _logger;
        private readonly CombinedRuleSetSubscriber _subscriber;

        public CombinedRuleSetSubscriberTest()
        {
            _mediator = new Mock<IMediator>();
            _logger = new Mock<ILogger<CombinedRuleSetSubscriber>>();
            _subscriber = new CombinedRuleSetSubscriber(_mediator.Object, _logger.Object);
        }
        [Fact]
        public void ConstructorShouldThrowNullExceptionWhenLoggerObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new CombinedRuleSetSubscriber(_mediator.Object, null));
        }
        [Fact]
        public void ConstructorShouldThrowNullExceptionWhenMediatorObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new CombinedRuleSetSubscriber(null, _logger.Object));
        }

        [Fact]
        public async Task CombinedRuleSetSubscriberSuccessAsync()
        {
            var eventRequest = TestDataProvider.GetTestData<CombinedRuleSetEventCommmand>("InRuleEngine\\CombinedRuleSetEventCommand");
            var request = JsonSerializer.Serialize(eventRequest);
            _mediator.Setup(m => m.Send(It.IsAny<CombinedRuleSetEventCommmand>(), System.Threading.CancellationToken.None)).ReturnsAsync(true);
            await _subscriber.Run(request);
        }
    }
}
