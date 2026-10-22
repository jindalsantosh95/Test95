using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Linq;
using System.Threading.Tasks;
using VAA.EAPI.Infrastructure.PSS.Proxy.PassengerV4Proxy;
using VAA.EAPI.Infrastructure.PSS.Repository;
using VAA.EAPI.Infrastructure.PSS.Services.Interfaces;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Infrastructure.PSS.Tests
{
    public class PassengerV4ManageMultipleTravelDocsAndSecureFlightRepoHandlerTests
    {
        private readonly Mock<ILogger<PassengerV4ManageMultipleTravelDocsAndSecureFlightRepoHandler>> _logger;
        private readonly Mock<IAir4PassengerV4> _air4PassengerV4;
        private readonly PassengerV4ManageMultipleTravelDocsAndSecureFlightRepoHandler _handler;

        public PassengerV4ManageMultipleTravelDocsAndSecureFlightRepoHandlerTests()
        {
            _logger = new Mock<ILogger<PassengerV4ManageMultipleTravelDocsAndSecureFlightRepoHandler>>();
            _air4PassengerV4 = new Mock<IAir4PassengerV4>();
            _handler = new PassengerV4ManageMultipleTravelDocsAndSecureFlightRepoHandler(_air4PassengerV4.Object, _logger.Object);
            PSSTestsCommon.CreateTestEnvironmentSettings();
        }

        [Fact]
        public void Handler_ShouldThrowNullArgumentException_forNullLogger()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new PassengerV4ManageMultipleTravelDocsAndSecureFlightRepoHandler(_air4PassengerV4.Object, null));
        }

        [Fact]
        public void EmptyLogger_ShouldThrowNullArgumentException_forNullair4PassengerV4()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new PassengerV4ManageMultipleTravelDocsAndSecureFlightRepoHandler(null, _logger.Object));
        }
        [Fact]
        public async Task Handler_ShouldReturnTravelDocumnet_forGoodRequest()
        {
            //Arrange
            var request = TestDataProvider.GetTestData<PassengerV4ManageMultipleTravelDocsAndSecureFlightRepoQuery>("InfrastructurePSS\\Request");
            var response = TestDataProvider.GetTestData<ManageMultipleTravelDocsAndSecureFlightResponse>("InfrastructurePSS\\DeltaResponse");
            _air4PassengerV4.Setup(x => x.Air4ManageMultipleTravelDocsAndSecureFlight(It.IsAny<ManageMultipleTravelDocsAndSecureFlightRequestType>())).ReturnsAsync(response);

            //Act
            var result = await _handler.Handle(request, new System.Threading.CancellationToken());

            //Assert
            Assert.NotNull(result.TravelDocuments);
            Assert.Equal("Passport", result.TravelDocuments.First().DocumentType);
        }
        [Fact]
        public async Task Handler_ShouldReturnNull_forNullDocumnetDetails()
        {
            //Arrange
            var request = TestDataProvider.GetTestData<PassengerV4ManageMultipleTravelDocsAndSecureFlightRepoQuery>("InfrastructurePSS\\Request");
            var response = TestDataProvider.GetTestData<ManageMultipleTravelDocsAndSecureFlightResponse>("InfrastructurePSS\\DeltaResponse");
            response.ManageMultipleTravelDocsAndSecureFlightResponse1.DocumentDetailData = null;
            _air4PassengerV4.Setup(x => x.Air4ManageMultipleTravelDocsAndSecureFlight(It.IsAny<ManageMultipleTravelDocsAndSecureFlightRequestType>())).ReturnsAsync(response);

            //Act
            var result = await _handler.Handle(request, new System.Threading.CancellationToken());

            //Assert
            Assert.Null(result);
        }
    }
}
