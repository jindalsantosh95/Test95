using MediatR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.IO;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Application.Commands.AddressUpdate;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Triggers.ServiceBus;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Tests
{
    public class AddressUpdateNotificationEventSubscriberTest
    {
        private readonly Mock<IMediator> _mediator;
        private readonly Mock<ILogger<AddressUpdateNotificationEventSubscriber>> _logger;
        private readonly AddressUpdateNotificationEventSubscriber _subscriber;
        private readonly Mock<FunctionContext> _context;

        public AddressUpdateNotificationEventSubscriberTest()
        {
            _mediator = new Mock<IMediator>();
            _logger = new Mock<ILogger<AddressUpdateNotificationEventSubscriber>>();
            _subscriber = new AddressUpdateNotificationEventSubscriber(_mediator.Object, _logger.Object);
            _context = new Mock<FunctionContext>();
        }

        [Fact]
        public void ConstructorShouldThrowNullExceptionWhenObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new AddressUpdateNotificationEventSubscriber(null, _logger.Object));

            Assert.Throws<ArgumentNullException>(() =>
            new AddressUpdateNotificationEventSubscriber(_mediator.Object, null));
        }

        [Fact]
        public async Task AddressUpdateNotificationEventSubscriberSuccessAsync()
        {
            var eventRequest = TestDataProvider.GetTestData<AddressUpdateNotificationCommand>("MemberManagement\\AddressUpdateNotificationEventData");
            var request = JsonSerializer.Serialize(eventRequest);
            _context.Setup(c => c.FunctionDefinition.PathToAssembly).Returns(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));
            _mediator.Setup(m => m.Send(It.IsAny<AddressUpdateNotificationCommand>(), System.Threading.CancellationToken.None)).ReturnsAsync(new Response<string> { Value = "Success" });
            await _subscriber.Delete(request, _context.Object);
        }
    }
}
