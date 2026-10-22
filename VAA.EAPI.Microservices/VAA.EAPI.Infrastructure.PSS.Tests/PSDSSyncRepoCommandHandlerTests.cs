using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Infrastructure.PSS.Proxy.PassengerV4Proxy;
using VAA.EAPI.Infrastructure.PSS.Repository.Command;
using VAA.EAPI.Infrastructure.PSS.Services.Interfaces;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Infrastructure.PSS.Tests
{
    public class PSDSSyncRepoCommandHandlerTests
    {
        private readonly Mock<ILogger<PSDSSyncRepoCommandHandler>> _logger;
        private readonly Mock<IAir4PassengerV4> _air4PassengerV4;
        private readonly PSDSSyncRepoCommandHandler _handler;

        public PSDSSyncRepoCommandHandlerTests()
        {
            _logger = new Mock<ILogger<PSDSSyncRepoCommandHandler>>();
            _air4PassengerV4 = new Mock<IAir4PassengerV4>();
            _handler = new PSDSSyncRepoCommandHandler(_logger.Object, _air4PassengerV4.Object);
            PSSTestsCommon.CreateTestEnvironmentSettings();
        }

        [Fact]
        public void Constructor_ShouldThrowNullArgumentException_ForNullair4PassengerV4()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new PSDSSyncRepoCommandHandler(_logger.Object, null));
        }

        [Fact]
        public void Constructor_ShouldThrowNullArgumentException_ForNullLogger()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new PSDSSyncRepoCommandHandler(null, _air4PassengerV4.Object));
        }

        [Fact]
        public async Task Handler_ShouldReturnTrue_ForGoodRequest()
        {
            //Arrange
            var request = TestDataProvider.GetTestData<PSDSSyncRepoCommand>("InfrastructurePSS\\PSDSSyncRequest");
            var response = TestDataProvider.GetTestData<UpdateSFPDResponseType>("InfrastructurePSS\\UpdateSFPDResponse");

            _air4PassengerV4.Setup(x => x.PSDSSync(It.IsAny<UpdateSFPDRequestType>())).ReturnsAsync(response);

            //Act
            var result = await _handler.Handle(request, new System.Threading.CancellationToken());

            //Assert
            Assert.True(result);
        }

        [Fact]
        public async Task Handler_ThrowError_ForNullRequest()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => _handler.Handle(null, CancellationToken.None));
        }

        [Fact]
        public async Task Handler_ShouldThrowError_FoMissingConfigValue()
        {
            // Arrange
            var request = TestDataProvider.GetTestData<PSDSSyncRepoCommand>("InfrastructurePSS\\PSDSSyncRequest");

            System.Environment.SetEnvironmentVariable(Constants.Environment.ApplicationName, null);

            //Act//Assert
            await Assert.ThrowsAsync<NullReferenceException>(() => _handler.Handle(request, CancellationToken.None));
        }
    }
}
