using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.EventHandlers.GroupDoc;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Infrastructure.Repository.CustomerTransactions;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Tests
{
    public class GroupDocPatchEventHandlerTests
    {
        private readonly Mock<IMediator> _mockMediator;
        private readonly GroupDocPatchEventHandler _handler;
        private readonly Mock<ILogger<GroupDocPatchEventHandler>> _logger;

        public GroupDocPatchEventHandlerTests()
        {
            _mockMediator = new Mock<IMediator>();
            _logger = new Mock<ILogger<GroupDocPatchEventHandler>>();
            _handler = new GroupDocPatchEventHandler(_mockMediator.Object, _logger.Object);
        }

        [Fact]
        public void ConstructorShouldThrowNullExceptionWhenMediatorObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new GroupDocPatchEventHandler(null, _logger.Object));
        }

        [Fact]
        public void ConstructorShouldThrowNullExceptionWhenLoggerObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new GroupDocPatchEventHandler(_mockMediator.Object, null));
        }

        [Fact]
        public async Task GroupDocCreateEventHandlerCreateSuccessAsync()
        {
            var eventRequest = TestDataProvider.GetTestData<GroupDocPatchEvent>("CustomerTransactionMS\\GroupUpdateEvent");
            var request = JsonSerializer.Serialize(eventRequest);
            _mockMediator.Setup(m => m.Send(It.IsAny<GroupDocPatchEvent>(), CancellationToken.None)).ReturnsAsync(Unit.Value);
            _mockMediator.Setup(m => m.Send(It.IsAny<CustomerTransactionRepoCommand>(), CancellationToken.None)).ReturnsAsync(Unit.Value);
            var result = await _handler.Handle(eventRequest, new CancellationTokenSource().Token);
            Assert.True(result.IsSuccess);
        }
    }
}
