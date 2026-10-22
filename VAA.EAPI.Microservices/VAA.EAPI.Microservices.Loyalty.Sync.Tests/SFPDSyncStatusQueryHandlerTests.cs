using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Repository;
using VAA.EAPI.Microservices.Loyalty.Sync.Application.Query;
using VAA.EAPI.Microservices.Loyalty.Sync.Application.Query.BaseClasses;
using VAA.EAPI.Microservices.Loyalty.Sync.Domain.Entities.SFPD;
using VAA.EAPI.Microservices.Loyalty.Sync.Tests;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Tests
{
    public class SFPDSyncStatusQueryHandlerTests
    {
        protected readonly Mock<IMediator> _mockMediator = new();
        protected readonly Mock<ILogger<EventStatusBaseQueryHandler<SFPDSyncStatusQueryResponse>>> _mockLogger = new();
        private readonly SFPDSyncStatusQueryHandler _handler;

        public SFPDSyncStatusQueryHandlerTests()
        {
            _mockMediator = new Mock<IMediator>();
            _mockLogger = new Mock<ILogger<EventStatusBaseQueryHandler<SFPDSyncStatusQueryResponse>>>();
            _handler = new SFPDSyncStatusQueryHandler(_mockMediator.Object, _mockLogger.Object);
            LoyaltySyncTestsCommon.CreateTestEnvironmentSettings();
        }

        [Fact]
        public void ConstructorShouldThrowNullExceptionWhenMediatorIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new SFPDSyncStatusQueryHandler(null, _mockLogger.Object));
        }

        [Fact]
        public void ConstructorShouldThrowNullExceptionWhenLoggerIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new SFPDSyncStatusQueryHandler(_mockMediator.Object, null));
        }

        [Fact]
        public async Task Handler_ShouldReturnSucceess()
        {
            // Arrange
            var request = TestDataProvider.GetTestData<SFPDSyncStatusQuery>("LoyaltySyncMS\\SFPDSyncStatusQueryRequest");

            var mockResponse = TestDataProvider.GetTestData<ServiceResponse>("LoyaltySyncMS\\SFPDSyncStatusSuccessResponse");

            _mockMediator.Setup(m => m.Send(It.IsAny<ServiceResponseQuery>(), CancellationToken.None)).ReturnsAsync(mockResponse);

            // Acts
            var result = await _handler.Handle(request, CancellationToken.None);

            // Assert
            Assert.NotNull(result.Value);
            Assert.True(result.IsSuccess);
            Assert.Equal(request.RequestId, result.Value.Value.RequestId);
            Assert.Equal("True", result.Value.Value.PsdsSyncStatus);                  
        }

        [Fact]
        public async Task Handler_ShouldThrowError_ForFailedUpdate()
        {
            // Arrange
            var request = TestDataProvider.GetTestData<SFPDSyncStatusQuery>("LoyaltySyncMS\\SFPDSyncStatusQueryRequest");

            var mockErrorResponse = TestDataProvider.GetTestData<ServiceResponse>("LoyaltySyncMS\\SFPDSyncStatusFailureResponse");
            mockErrorResponse.JsonResponseData = null;            

            _mockMediator.Setup(m => m.Send(It.IsAny<ServiceResponseQuery>(), CancellationToken.None)).ReturnsAsync(mockErrorResponse);

            // Acts
            var result = await _handler.Handle(request, CancellationToken.None);

            // Assert
            Assert.NotNull(result.Value.Errors);
            Assert.Equal(Status.BusinessException, result.Value.Status);
            Assert.NotNull(result.Value.Errors[0].Message);
            Assert.Contains("Request in process. Please try again later.", result.Value.Errors[0].Message);
        }
    }
}
