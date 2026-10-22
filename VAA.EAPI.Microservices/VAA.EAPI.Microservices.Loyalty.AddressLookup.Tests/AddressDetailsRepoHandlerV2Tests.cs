using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Microservices.Loyalty.AddressLookup.Infrastructure.Repository;
using VAA.EAPI.Microservices.Loyalty.AddressLookup.Infrastructure.Service;
using VAA.EAPI.Microservices.Loyalty.AddressLookup.Infrastructure.Proxy;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Loyalty.AddressLookup.Tests
{
    public class AddressDetailsRepoHandlerV2Tests
    {
        private readonly Mock<IMediator> _mockMediator;
        private readonly Mock<ILogger<AddressDetailsRepoHandlerV2>> _mockLogger;
        private readonly AddressDetailsRepoHandlerV2 _mockHandler;

        public AddressDetailsRepoHandlerV2Tests()
        {
            _mockMediator = new Mock<IMediator>();
            _mockLogger = new Mock<ILogger<AddressDetailsRepoHandlerV2>>();
            _mockHandler = new AddressDetailsRepoHandlerV2(_mockMediator.Object, _mockLogger.Object);
            AddressLookupTestsCommon.CreateTestEnvironmentSettings();
        }

        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenMediatorObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new AddressDetailsRepoHandlerV2(null, _mockLogger.Object));
        }

        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenLoggerObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new AddressDetailsRepoHandlerV2(_mockMediator.Object, null));
        }

        [Fact]
        public async Task Handler_ShouldThrowNullException_ForNullRequest()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => _mockHandler.Handle(null, CancellationToken.None));
        }

        [Fact]
        public void Handler_ShouldReturnNull_ForUnavailableData()
        {
            //Arrange
            var request = TestDataProvider.GetTestData<AddressDetailsRepoCommandV2>("AddressLookUpMS\\AddressDetailsRequest");

            _mockMediator.Setup(x => x.Send(It.IsAny<AddressLookUpServiceCommandV2>(), It.IsAny<CancellationToken>())).ReturnsAsync(new CompleteAddressLookupResponse()
            {

            });

            //Act
            var result = _mockHandler.Handle(request, new CancellationToken());

            //Assert
            Assert.Null(result.Result);
        }

        [Fact]
        public void Handler_ShouldReturnCompleteAddressDetails_ForGoodRequest()
        {
            //Arrange
            var request = TestDataProvider.GetTestData<AddressDetailsRepoCommandV2>("AddressLookUpMS\\AddressDetailsRequest");
            var response = TestDataProvider.GetTestData<CompleteAddressLookupResponse>("AddressLookUpMS\\CompleteAddressLookUpResponse");

            _mockMediator.Setup(x => x.Send(It.IsAny<AddressLookUpServiceCommandV2>(), It.IsAny<CancellationToken>())).ReturnsAsync(response);

            //Act
            var result = _mockHandler.Handle(request, new CancellationToken());

            //Assert
            Assert.NotNull(result.Result);
            Assert.Equal(TaskStatus.RanToCompletion, result.Status);
            Assert.Equal("71", result.Result.First().AddressDetails.Address1);
        }
    }
}
