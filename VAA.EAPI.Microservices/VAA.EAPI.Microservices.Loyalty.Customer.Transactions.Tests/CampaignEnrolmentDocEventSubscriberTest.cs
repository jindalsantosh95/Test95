using MediatR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Triggers.ServiceBus;
using Xunit;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Tests
{
    public class CampaignEnrolmentDocEventSubscriberTest
    {
        private readonly Mock<IMediator> _mediator;
        private readonly Mock<ILogger<CampaignEnrolmentDocEventSubscriber>> _logger;
        private readonly CampaignEnrolmentDocEventSubscriber _subscriber;
        private readonly Mock<FunctionContext> _context;

        public CampaignEnrolmentDocEventSubscriberTest()
        {
            _mediator = new Mock<IMediator>();
            _logger = new Mock<ILogger<CampaignEnrolmentDocEventSubscriber>>();
            _subscriber = new CampaignEnrolmentDocEventSubscriber(_mediator.Object, _logger.Object);
            _context = new Mock<FunctionContext>();
        }

        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenLoggerObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new CampaignEnrolmentDocEventSubscriber(_mediator.Object, null));
        }

        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenMediatorObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new CampaignEnrolmentDocEventSubscriber(null, _logger.Object));
        }


    }
}
