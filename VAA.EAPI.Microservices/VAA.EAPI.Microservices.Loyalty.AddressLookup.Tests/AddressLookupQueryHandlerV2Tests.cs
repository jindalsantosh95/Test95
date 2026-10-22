using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.AddressLookup.Application.Query;
using VAA.EAPI.Microservices.Loyalty.AddressLookup.Infrastructure.Entity.VAA.EAPI.Microservices.Loyalty.AddressLookup.Infrastructure.Entity;
using VAA.EAPI.Microservices.Loyalty.AddressLookup.Infrastructure.Repository;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Loyalty.AddressLookup.Tests
{
    public class AddressLookupQueryHandlerV2Tests
    {
        private readonly Mock<IMediator> _mockMediator;
        private readonly Mock<ILogger<AddressLookupQueryHandlerV2>> _mockLogger;
        private readonly AddressLookupQueryHandlerV2 _mockHandler;

        public AddressLookupQueryHandlerV2Tests()
        {
            _mockMediator = new Mock<IMediator>();
            _mockLogger = new Mock<ILogger<AddressLookupQueryHandlerV2>>();
            _mockHandler = new AddressLookupQueryHandlerV2(_mockMediator.Object, _mockLogger.Object);
            AddressLookupTestsCommon.CreateTestEnvironmentSettings();
        }

        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenMediatorObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new AddressLookupQueryHandlerV2(null, _mockLogger.Object));
        }

        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenLoggerObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new AddressLookupQueryHandlerV2(_mockMediator.Object, null));
        }

        [Fact]
        public void Handler_ShouldReturnG018_ForNullCountryCode()
        {
            //Arrange
            var request = TestDataProvider.GetTestData<AddressLookupQueryV2>("AddressLookUpMS\\AddressDetailsRequest");
            request.CountryCode = null;

            //Act
            var result = _mockHandler.Handle(request, new CancellationToken());

            //Assert
            Assert.Equal(Status.BusinessException, result.Result.Value.Status);
            Assert.Equal("G018", result.Result.Value.Errors.First().Code);
            Assert.Contains("One of the mandatory field is missing in the request", result.Result.Value.Errors.First().Message);
        }

        [Fact]
        public void Handler_ShouldReturn10150_ForInvalidCountryCode()
        {
            //Arrange
            var request = TestDataProvider.GetTestData<AddressLookupQueryV2>("AddressLookUpMS\\AddressDetailsRequest");
            request.CountryCode = "test";

            //Act
            var result = _mockHandler.Handle(request, new CancellationToken());

            //Assert
            Assert.Equal(Status.BusinessException, result.Result.Value.Status);
            Assert.Equal("10150", result.Result.Value.Errors.First().Code);
            Assert.Contains("Country is not valid for address lookup", result.Result.Value.Errors.First().Message);
        }

        [Fact]
        public void Handler_ShouldReturnG016_ForNullAddressLine1()
        {
            //Arrange
            var request = TestDataProvider.GetTestData<AddressLookupQueryV2>("AddressLookUpMS\\AddressDetailsRequest");
            request.AddressLine1 = null;

            //Act
            var result = _mockHandler.Handle(request, new CancellationToken());

            //Assert
            Assert.Equal(Status.BusinessException, result.Result.Value.Status);
            Assert.Equal("G016", result.Result.Value.Errors.First().Code);
            Assert.Contains("One of the mandatory field is missing in the request", result.Result.Value.Errors.First().Message);
        }

        [Fact]
        public void Handler_ShouldReturnG016_ForNullPostCode()
        {
            //Arrange
            var request = TestDataProvider.GetTestData<AddressLookupQueryV2>("AddressLookUpMS\\AddressDetailsRequest");
            request.PostCode = null;

            //Act
            var result = _mockHandler.Handle(request, new CancellationToken());

            //Assert
            Assert.Equal(Status.BusinessException, result.Result.Value.Status);
            Assert.Equal("10149", result.Result.Value.Errors.First().Code);
            Assert.Contains("One of the mandatory field is missing in the request", result.Result.Value.Errors.First().Message);
        }

        [Fact]
        public void Handler_ShouldReturnG007_ForUnavailableData()
        {
            //Arrange
            var request = TestDataProvider.GetTestData<AddressLookupQueryV2>("AddressLookUpMS\\AddressDetailsRequest");

            _mockMediator.Setup(x => x.Send(It.IsAny<AddressDetailsRepoCommandV2>(), It.IsAny<CancellationToken>())).ReturnsAsync(new List<CompleteAddressDetails>()
            {

            });

            //Act
            var result = _mockHandler.Handle(request, new CancellationToken());

            //Assert
            Assert.Equal(Status.BusinessException, result.Result.Value.Status);
            Assert.Equal("G007", result.Result.Value.Errors.First().Code);
            Assert.Contains("Data is unavailable for the set of input provided", result.Result.Value.Errors.First().Message);
        }

        [Fact]
        public void Handler_ShouldReturnAddress_ForgoodRequest()
        {
            //Arrange
            var request = TestDataProvider.GetTestData<AddressLookupQueryV2>("AddressLookUpMS\\AddressDetailsRequest");
            var reponse = TestDataProvider.GetTestData<List<CompleteAddressDetails>>("AddressLookUpMS\\CompleteAddressDetailsResponse");

            _mockMediator.Setup(x => x.Send(It.IsAny<AddressDetailsRepoCommandV2>(), It.IsAny<CancellationToken>())).ReturnsAsync(reponse);

            //Act
            var result = _mockHandler.Handle(request, new CancellationToken());

            //Assert
            Assert.NotNull(result);
            Assert.Equal(Status.OK, result.Result.Value.Status);
            Assert.Equal("Marmadon Road", result.Result.Value.Value.First().AddressLine2);
            Assert.Contains("Y", result.Result.Value.Value.First().Valid);
        }
    }
}
