using MediatR;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Microservices.Loyalty.Sync.Application.Command.ManageAccount;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Loyalty.Sync.Tests
{
    public class UpdateOCDBCommandHandlerTests
    {
        private readonly Mock<IMediator> _mockMediator;
        private readonly UpdateOCDBSyncCommandHandler _handler;

        public UpdateOCDBCommandHandlerTests()
        {
            _mockMediator = new Mock<IMediator>();
            _handler = new UpdateOCDBSyncCommandHandler(_mockMediator.Object);
        }



        [Fact]
        public void NullMediator_ShouldThrowNullArgumentException()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new UpdateOCDBSyncCommandHandler(null));
        }

        [Fact]
        public async Task SucceededRequest_ShouldReturnTrue()
        {
            // Arrange
            var eventRequest = TestDataProvider.GetTestData<UpdateOCDBSyncCommand>("LoyaltySyncMS\\UpdateOCDBRequest");


            _mockMediator.Setup(m => m.Send(It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);


            // Act
            var result = await _handler.Handle(eventRequest, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
        }
    }
}
