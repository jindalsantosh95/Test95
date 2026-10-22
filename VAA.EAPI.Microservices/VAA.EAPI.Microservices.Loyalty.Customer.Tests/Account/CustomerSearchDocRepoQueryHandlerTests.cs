using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Infrastructure.Services.CosmosSqlDBv2;
using VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Repositories.Account;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Tests.Account
{
    public class CustomerSearchDocRepoQueryHandlerTests
    {
        private readonly Mock<IMediator> _mockmediator;
        private readonly Mock<ILogger<CustomerSearchDocRepoQueryHandler>> _mockLogger;
        private readonly CustomerSearchDocRepoQueryHandler _mockHandler;

        public CustomerSearchDocRepoQueryHandlerTests()
        {
            _mockmediator = new Mock<IMediator>();
            _mockLogger = new Mock<ILogger<CustomerSearchDocRepoQueryHandler>>();
            _mockHandler = new CustomerSearchDocRepoQueryHandler(_mockmediator.Object, _mockLogger.Object);
            SettingsHelper.CreateTestEnvironmentSettings();
        }

        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenMediatorObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new CustomerSearchDocRepoQueryHandler(null, _mockLogger.Object));
        }

        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenLoggerObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new CustomerSearchDocRepoQueryHandler(_mockmediator.Object, null));
        }

        [Fact]
        public async Task Handler_ShouldSucceed_WithoutError()
        {
            // Arrange
            var request = TestDataProvider.GetTestData<CustomerSearchDocRepoQuery>("CustomerMS\\Account\\CustomerDucomentQueryRequest");
            var mockResponse = TestDataProvider.GetTestData<List<object>>("CustomerSearch\\ListOfCustomerSearchDocs");

            _mockmediator.Setup(s => s.Send(It.IsAny<CosmosReaderServiceQuery>(), CancellationToken.None)).ReturnsAsync(mockResponse);

            // Act
            var result = await _mockHandler.Handle(request, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("CustomerSearch", result.DocType);
            Assert.Equal("1074714856", result.LoyaltyMemberNumber);
        }

        [Fact(Skip = "Implemented test is not configured/implemented correctly")]
        public async Task Handler_ShouldThrowError_ForFailingToConnectToCosmos()
        {
            // Arrange
            var request = TestDataProvider.GetTestData<CustomerSearchDocRepoQuery>("CustomerMS\\Account\\CustomerDucomentQueryRequest");

            System.Environment.SetEnvironmentVariable(Loyalty.Customer.Constants.Environment.SCVCustDBEndPoint, null);

            //Act//Assert
            await Assert.ThrowsAsync<NullReferenceException>(() => _mockHandler.Handle(request, CancellationToken.None));
        }
    }
}
