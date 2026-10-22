using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Microservices.Loyalty.Identity.Application.Command.ManageAccount;
using VAA.EAPI.Microservices.Loyalty.Identity.Infrastructure.Repositories;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Loyalty.Identity.Tests
{
    public class UpdateAzureB2CDetailsCommandHandlerTests
    {
        private readonly Mock<IMediator> _mockMediator;
        private readonly Mock<ILogger<UpdateAzureB2CDetailsCommandHandler>> _mockLogger;
        private readonly UpdateAzureB2CDetailsCommandHandler _handler;

        public UpdateAzureB2CDetailsCommandHandlerTests()
        {
            _mockMediator = new Mock<IMediator>();
            _mockLogger = new Mock<ILogger<UpdateAzureB2CDetailsCommandHandler>>();
            _handler = new UpdateAzureB2CDetailsCommandHandler(_mockMediator.Object, _mockLogger.Object);
            IdentityTestsCommon.CreateTestEnvironmentSettings();
        }

        [Fact]
        public void EmptyLogger_ShouldThrowNullArgumentException()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new UpdateAzureB2CDetailsCommandHandler(_mockMediator.Object, null));
        }

        [Fact]
        public void EmptyMediator_ShouldThrowNullArgumentException()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new UpdateAzureB2CDetailsCommandHandler(null, _mockLogger.Object));
        }

        [Fact]
        public async Task Handler_ShouldThrowNullExcepition_ForNullRequest()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => _handler.Handle(null, CancellationToken.None));
        }

        [Fact]
        public async Task Handler_ShouldReturnFalse_ForInvalidEmailFormat()
        {
            // Arrange
            var request = TestDataProvider.GetTestData<UpdateAzureB2CDetailsCommand>("IdentityMS\\B2CEmailUpdateInvalidEmail");

            // Act
            var result = await _handler.Handle(request, CancellationToken.None);

            // Assert
            Assert.IsType<Result<bool>>(result);
            Assert.False(result.Value);
        }

        [Fact]
        public async Task Handler_ShouldReturnTrue_ForGoodRequest()
        {
            // Arrange
            var request = TestDataProvider.GetTestData<UpdateAzureB2CDetailsCommand>("IdentityMS\\B2CEmailUpdateEventRequest");

            _mockMediator.Setup(m => m.Send(It.IsAny<UpdateB2CRepoCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);

            // Act
            var result = await _handler.Handle(request, CancellationToken.None);

            // Assert
            Assert.IsType<Result<bool>>(result);
            Assert.True(result.Value);
        }

        [Fact]
        public async Task Handler_ShouldReturnFalse_AfterFailingtoConecttoB2C()
        {
            // Arrange
            var request = TestDataProvider.GetTestData<UpdateAzureB2CDetailsCommand>("IdentityMS\\B2CEmailUpdateEventRequest");

            _mockMediator.Setup(m => m.Send(It.IsAny<UpdateB2CRepoCommand>(), It.IsAny<CancellationToken>())).ThrowsAsync(new Exception());

            // Act
            var result = await _handler.Handle(request, CancellationToken.None);

            // Assert
            Assert.IsType<Result<bool>>(result);
            Assert.False(result.Value);
        }
    }
}
