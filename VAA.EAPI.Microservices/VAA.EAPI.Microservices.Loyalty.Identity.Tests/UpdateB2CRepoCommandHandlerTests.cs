using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Models.B2CGraph;
using VAA.EAPI.Microservices.Loyalty.Identity.Infrastructure.Repositories;
using VAA.EAPI.Microservices.Loyalty.Identity.Infrastructure.Services;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Loyalty.Identity.Tests
{
    public class UpdateB2CRepoCommandHandlerTests
    {
        private readonly Mock<IMediator> _mockMediator;
        private readonly Mock<ILogger<UpdateB2CRepoCommandHandler>> _mockLogger;
        private readonly UpdateB2CRepoCommandHandler _handler;

        public UpdateB2CRepoCommandHandlerTests()
        {
            _mockMediator = new Mock<IMediator>();
            _mockLogger = new Mock<ILogger<UpdateB2CRepoCommandHandler>>();
            _handler = new UpdateB2CRepoCommandHandler(_mockMediator.Object, _mockLogger.Object);
            IdentityTestsCommon.CreateTestEnvironmentSettings();
        }

        [Fact]
        public void EmptyLogger_ShouldThrowNullArgumentException()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new UpdateB2CRepoCommandHandler(_mockMediator.Object, null));
        }

        [Fact]
        public void EmptyMediator_ShouldThrowNullArgumentException()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new UpdateB2CRepoCommandHandler(null, _mockLogger.Object));
        }

        [Fact]
        public async Task Handler_ShouldReturnFalse_ForMemberNumberNotPresentinB2C()
        {
            // Arrange
            var request = TestDataProvider.GetTestData<UpdateB2CRepoCommand>("IdentityMS\\UpdateB2CRepoCommandRequest");

            // Act
            _mockMediator.Setup(m => m.Send(It.IsAny<UnVerifiedMemberValidationCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(new EAPI.Domain.Models.B2CGraph.B2CGraphResponse { Value = null });

            var result = await _handler.Handle(request, CancellationToken.None);

            // Assert
            Assert.IsType<bool>(result);
            Assert.False(result);
        }

        [Fact]
        public async Task Handler_ShouldReturnFalse_ForInvalidMember()
        {
            // Arrange
            var request = TestDataProvider.GetTestData<UpdateB2CRepoCommand>("IdentityMS\\UpdateB2CRepoCommandRequest");

            var b2CResponse = TestDataProvider.GetTestData<B2CGraphResponse>("IdentityMS\\B2CMemberResponse");
            b2CResponse.Value.First().AccountEnabled = false;

            // Act

            _mockMediator.Setup(m => m.Send(It.IsAny<UnVerifiedMemberValidationCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(b2CResponse);

            var result = await _handler.Handle(request, CancellationToken.None);

            // Assert
            Assert.IsType<bool>(result);
            Assert.False(result);
        }

        [Fact]
        public async Task Handler_ShouldReturnTrue_ForGoodRequest()
        {
            // Arrange
            var request = TestDataProvider.GetTestData<UpdateB2CRepoCommand>("IdentityMS\\UpdateB2CRepoCommandRequest");

            var b2CResponse = TestDataProvider.GetTestData<B2CGraphResponse>("IdentityMS\\B2CMemberResponse");

            // Act
            _mockMediator.Setup(m => m.Send(It.IsAny<UnVerifiedMemberValidationCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(b2CResponse);

            _mockMediator.Setup(m => m.Send(It.IsAny<UpdateB2CServiceCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);

            var result = await _handler.Handle(request, CancellationToken.None);

            // Assert
            Assert.IsType<bool>(result);
            Assert.True(result);
        }
    }
}
