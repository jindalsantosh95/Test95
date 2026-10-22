using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Repository;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Query.BaseClasses;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Query.PartnerMembership;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.PartnerMembership;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Tests
{
    public class PartnerMembershipCommandStatusQueryHandlerTest
    {
        protected readonly Mock<IMediator> _mockMediator = new();
        protected readonly Mock<ILogger<CommandStatusBaseQueryHandler<PartnerMembershipCommandQueryResponse>>> _mockLogger = new();
        private readonly PartnerMembershipCommandStatusQueryHandler _handler;

        public PartnerMembershipCommandStatusQueryHandlerTest()
        {
            _mockMediator = new Mock<IMediator>();
            _mockLogger = new Mock<ILogger<CommandStatusBaseQueryHandler<PartnerMembershipCommandQueryResponse>>>();
            _handler = new PartnerMembershipCommandStatusQueryHandler(_mockMediator.Object, _mockLogger.Object);
            ContactNoteTestHelpers.CreateTestEnvironmentSettings();
        }

        [Fact]
        public void ConstructorShouldThrowNullExceptionWhenMediatorIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new PartnerMembershipCommandStatusQueryHandler(null, _mockLogger.Object));
        }

        [Fact]
        public void ConstructorShouldThrowNullExceptionWhenLoggerIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new PartnerMembershipCommandStatusQueryHandler(_mockMediator.Object, null));
        }

        [Fact]
        public async Task Handler_ShouldReturnSucceess_ForGoodRequest()
        {
            // Arrange
            var request = TestDataProvider.GetTestData<PartnerMembershipCommandStatusQuery>("PartnerMembership\\PartnerMembershipCreateResponseCommandRequest");

            var mockResponse = TestDataProvider.GetTestData<ServiceResponse>("PartnerMembership\\PartnerMembershipCreateResponse");

            _mockMediator.Setup(m => m.Send(It.IsAny<ServiceResponseQuery>(), CancellationToken.None)).ReturnsAsync(mockResponse);

            // Acts
            var result = await _handler.Handle(request, CancellationToken.None);

            // Assert
            Assert.NotNull(result.Value);
            Assert.True(result.IsSuccess);
            Assert.Equal(request.RequestId, result.Value.Value.RequestId);
            Assert.Equal("D9225263-26CC-6F04-E053-7D41D70A1575", result.Value.Value.PartnerMembershipDocument.CustomerId);
            Assert.Equal("VAA", result.Value.Value.PartnerMembershipDocument.Brand);
            Assert.Equal("Test", result.Value.Value.PartnerMembershipDocument.CreatedBy);
        }

        [Fact]
        public async Task Handler_ShouldThrowError_ForFailedUpdate()
        {
            // Arrange
            var request = TestDataProvider.GetTestData<PartnerMembershipCommandStatusQuery>("PartnerMembership\\PartnerMembershipCreateResponseCommandRequest");

            var mockErrorResponse = TestDataProvider.GetTestData<ServiceResponse>("PartnerMembership\\PartnerMembershipCreateErrorResponse");
            mockErrorResponse.Status = ServiceResponseProcessStatus.Error;

            _mockMediator.Setup(m => m.Send(It.IsAny<ServiceResponseQuery>(), CancellationToken.None)).ReturnsAsync(mockErrorResponse);

            // Acts
            var result = await _handler.Handle(request, CancellationToken.None);

            // Assert
            Assert.NotNull(result.Value.Value.Errors);
            Assert.Equal(Status.BusinessException, result.Value.Value.CommandStatus);
            Assert.NotNull(result.Value.Value.Errors.FirstOrDefault().Message);
            Assert.Contains("A member can have only one active card of a particular type at a time.", result.Value.Value.Errors.FirstOrDefault().Message);
        }
    }
}
