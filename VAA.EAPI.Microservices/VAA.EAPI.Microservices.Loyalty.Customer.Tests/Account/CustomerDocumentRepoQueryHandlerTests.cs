using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Services.CosmosSqlDBv2;
using VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Repositories.Account;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Tests.Account
{
    public class CustomerDocumentRepoQueryHandlerTests
    {
        private readonly Mock<IMediator> _mockmediator;
        private readonly Mock<ILogger<CustomerDocumentRepoQueryHandler>> _mockLogger;
        private readonly CustomerDocumentRepoQueryHandler _mockHandler;

        public CustomerDocumentRepoQueryHandlerTests()
        {
            _mockmediator = new Mock<IMediator>();
            _mockLogger = new Mock<ILogger<CustomerDocumentRepoQueryHandler>>();
            _mockHandler = new CustomerDocumentRepoQueryHandler(_mockmediator.Object, _mockLogger.Object);
            SettingsHelper.CreateTestEnvironmentSettings();
        }

        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenMediatorObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new CustomerDocumentRepoQueryHandler(null, _mockLogger.Object));
        }

        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenLoggerObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new CustomerDocumentRepoQueryHandler(_mockmediator.Object, null));
        }

        [Fact]
        public async Task Handler_ShouldSucceed_WithoutError()
        {
            // Arrange
            var request = TestDataProvider.GetTestData<CustomerDocumentRepoQuery>("CustomerMS\\Account\\CustomerDocumentRepoQueryRequest");
            var mockResponse = TestDataProvider.GetTestData<List<object>>("CustomerMS\\Account\\CustomerDocumentResponse");

            _mockmediator.Setup(s => s.Send(It.IsAny<CosmosReaderServiceQuery>(), CancellationToken.None)).ReturnsAsync(mockResponse);

            // Act
            var result = await _mockHandler.Handle(request, CancellationToken.None);

            // Assert
            Assert.NotNull(result.Value);
            Assert.Equal(Status.OK, result.Value.Status);
            Assert.Equal("Customer", result.Value.Value.DocType);
            Assert.Equal("Active", result.Value.Value.Status);
        }

        [Fact(Skip = "Implemented test is not configured/implemented correctly")]
        public async Task Handler_ShouldThrowError_ForFailingToConnectToCosmos()
        {
            // Arrange
            var request = TestDataProvider.GetTestData<CustomerDocumentRepoQuery>("CustomerMS\\Account\\CustomerDucomentQueryRequest");

            System.Environment.SetEnvironmentVariable(Loyalty.Customer.Constants.Environment.SCVCustDBCustColCntnr, null);

            //Act//Assert
            await Assert.ThrowsAsync<NullReferenceException>(() => _mockHandler.Handle(request, CancellationToken.None));
        }

        [Fact]
        public async Task Handler_ShouldFail_ForInvalidCustomerId()
        {
            // Arrange
            var request = TestDataProvider.GetTestData<CustomerDocumentRepoQuery>("CustomerMS\\Account\\CustomerDucomentQueryRequest");

            _mockmediator.Setup(s => s.Send(It.IsAny<CosmosReaderServiceQuery>(), CancellationToken.None)).ReturnsAsync(new List<object>());

            // Act
            var result = await _mockHandler.Handle(request, CancellationToken.None);

            //Assert
            Assert.Equal(Status.BusinessException, result.Value.Status);
        }
    }
}
