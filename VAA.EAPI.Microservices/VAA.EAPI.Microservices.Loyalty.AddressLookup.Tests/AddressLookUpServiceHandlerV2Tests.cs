using Microsoft.Extensions.Logging;
using Moq;
using System;
using VAA.EAPI.Microservices.Loyalty.AddressLookup.Infrastructure.Interface;
using VAA.EAPI.Microservices.Loyalty.AddressLookup.Infrastructure.Service;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Loyalty.AddressLookup.Tests
{
    public class AddressLookUpServiceHandlerV2Tests
    {
        private readonly Mock<IAzureWesalApiConnector> _mockApiConnector;
        private readonly Mock<ILogger<AddressLookUpServiceHandlerV2>> _mockLogger;
        private readonly AddressLookUpServiceHandlerV2 _mockHandler;

        public AddressLookUpServiceHandlerV2Tests()
        {
            _mockApiConnector = new Mock<IAzureWesalApiConnector>();
            _mockLogger = new Mock<ILogger<AddressLookUpServiceHandlerV2>>();
            _mockHandler = new AddressLookUpServiceHandlerV2(_mockLogger.Object, _mockApiConnector.Object);
            AddressLookupTestsCommon.CreateTestEnvironmentSettings();
        }

        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenLoggerObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new AddressLookUpServiceHandlerV2(null, _mockApiConnector.Object));
        }

        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenApiConnectorObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new AddressLookUpServiceHandlerV2(_mockLogger.Object, null));
        }

        [Fact]
        public void Handler_ShouldReturnResponse_ForGoodRequest()
        {
            //Arrange
            var request = TestDataProvider.GetTestData<AddressLookUpServiceCommandV2>("AddressLookUpMS\\AddressLookUpServiceCommandV2Request");
            var response = TestDataProvider.GetTestData<string>("AddressLookUpMS\\WesalServiceAddressDetailsResponse");

            _mockApiConnector.Setup(x => x.GetAddressDetails(It.IsAny<string>())).ReturnsAsync(response);
            //Act
            var result = _mockHandler.Handle(request, new System.Threading.CancellationToken());

            //Assert
            Assert.NotNull(result);
            Assert.Equal(System.Threading.Tasks.TaskStatus.RanToCompletion, result.Status);
            Assert.NotNull(result.Result.CompleteAddressLookupResult);
        }
    }
}
