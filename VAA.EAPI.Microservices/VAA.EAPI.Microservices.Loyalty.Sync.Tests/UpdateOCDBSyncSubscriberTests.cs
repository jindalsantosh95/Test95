using MediatR;
using Microsoft.Azure.Functions.Worker;
using Moq;
using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Microservices.Loyalty.Sync.Application.Command.ManageAccount;
using VAA.EAPI.Microservices.Loyalty.Sync.Triggers.ServiceBus;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Loyalty.Sync.Tests
{
    public class UpdateOCDBSyncSubscriberTests
    {
        private readonly Mock<IMediator> _mockMediator;
        private readonly UpdateOCDBSyncMessageSubscriber _subscriber;
        private readonly Mock<FunctionContext> _context;

        public UpdateOCDBSyncSubscriberTests()
        {
            _mockMediator = new Mock<IMediator>();
            _subscriber = new UpdateOCDBSyncMessageSubscriber(_mockMediator.Object);
            _context = new Mock<FunctionContext>();
        }

        [Fact]
        public void ConstructorShouldThrowNullExceptionWhenMediatorObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new UpdateOCDBSyncMessageSubscriber(null));
        }

        [Fact]
        public async Task UpdateOCDBSyncMessageSubscriberSuccessAsync()
        {
            var eventRequest = TestDataProvider.GetTestData<UpdateOCDBSyncCommand>("LoyaltySyncMS\\UpdateOCDBRequest");
            var request = Json.Serialize(eventRequest);
            _context.Setup(c => c.FunctionDefinition.PathToAssembly).Returns(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));
            _mockMediator.Setup(m => m.Send(It.IsAny<UpdateOCDBSyncCommand>(), System.Threading.CancellationToken.None)).ReturnsAsync(true);
            await _subscriber.Run(request, _context.Object);
        }
    }
}
