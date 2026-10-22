using MediatR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Text.Json;
using System.Threading.Tasks;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.EventHandlers.GroupDoc;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Triggers.ServiceBus;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Tests
{
    public class GroupDocEventSubscriberTests
    {
        private readonly Mock<IMediator> _mockMediator;
        private readonly GroupDocEventSubscriber _subscriber;
        private readonly Mock<FunctionContext> _context;
        private readonly Mock<ILogger<GroupDocEventSubscriber>> _logger;

        public GroupDocEventSubscriberTests()
        {
            _mockMediator = new Mock<IMediator>();
            _logger = new Mock<ILogger<GroupDocEventSubscriber>>();
            _subscriber = new GroupDocEventSubscriber(_mockMediator.Object, _logger.Object);
            _context = new Mock<FunctionContext>();
        }

        [Fact]
        public void ConstructorShouldThrowNullExceptionWhenMediatorObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new GroupDocEventSubscriber(null, _logger.Object));
        }

        [Fact]
        public void ConstructorShouldThrowNullExceptionWhenLoggerObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new GroupDocEventSubscriber(_mockMediator.Object, null));
        }

        [Fact]
        public async Task GroupDocEventSubscriberUpdateSuccessAsync()
        {
            var eventRequest = TestDataProvider.GetTestData<GroupDocPatchEvent>("CustomerTransactionMS\\GroupUpdateEvent");
            var request = JsonSerializer.Serialize(eventRequest);
            _mockMediator.Setup(m => m.Send(It.IsAny<GroupDocPatchEvent>(), System.Threading.CancellationToken.None)).ReturnsAsync(Unit.Value);
            await _subscriber.Update(request, _context.Object);
        }

        [Fact]
        public async Task GroupDocEventSubscriberCreateSuccessAsync()
        {
            var eventRequest = TestDataProvider.GetTestData<GroupDocCreateEvent>("CustomerTransactionMS\\GroupCreateEvent");
            var request = JsonSerializer.Serialize(eventRequest);
            _mockMediator.Setup(m => m.Send(It.IsAny<GroupDocCreateEvent>(), System.Threading.CancellationToken.None)).ReturnsAsync(Unit.Value);
            await _subscriber.Create(request, _context.Object);
        }
    }
}
