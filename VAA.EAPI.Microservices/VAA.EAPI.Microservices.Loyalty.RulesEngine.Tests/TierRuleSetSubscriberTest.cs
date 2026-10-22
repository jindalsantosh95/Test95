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
    public class TierRuleSetSubscriberTest
    {
        private readonly Mock<IMediator> _mediator;
        private readonly Mock<ILogger<TierRuleSetSubscriber>> _logger;
        private readonly TierRuleSetSubscriber _subscriber;

        public TierRuleSetSubscriberTest()
        {
            _mediator = new Mock<IMediator>();
            _logger = new Mock<ILogger<TierRuleSetSubscriber>>();
            _subscriber = new TierRuleSetSubscriber(_mediator.Object, _logger.Object);
        }
        [Fact]
        public void ConstructorShouldThrowNullExceptionWhenLoggerObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new TierRuleSetSubscriber(_mediator.Object, null));
        }
        [Fact]
        public void ConstructorShouldThrowNullExceptionWhenMediatorObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new TierRuleSetSubscriber(null, _logger.Object));
        }

        [Fact]
        public async Task TierRuleSetSubscriberSuccessAsync()
        {
            var eventRequest = TestDataProvider.GetTestData<TierRuleSetEventCommand>("InRuleEngine\\TierRuleSetEventCommand");
            var request = JsonSerializer.Serialize(eventRequest);
            _mediator.Setup(m => m.Send(It.IsAny<TierRuleSetEventCommand>(), System.Threading.CancellationToken.None)).ReturnsAsync(true);
            await _subscriber.Run(request);
        }
    }
}
