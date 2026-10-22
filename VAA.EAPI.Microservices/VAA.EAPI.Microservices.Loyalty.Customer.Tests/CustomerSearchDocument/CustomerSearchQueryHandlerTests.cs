using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Microservices.Loyalty.Customer.Application.Query.CustomerSearch;
using VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Entities;
using VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Repositories.CustomerSearchDocument;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;


namespace VAA.EAPI.Microservices.Loyalty.Customer.Tests.CustomerSearchDocument
{
    public class CustomerSearchQueryHandlerTests
    {
        protected readonly Mock<IMediator> _mockMediator;
        protected readonly Mock<ILogger<CustomerSearchQueryHandler>> _mockLogger;
        public CustomerSearchQueryHandlerTests()
        {
            _mockMediator = new Mock<IMediator>();
            _mockLogger = new Mock<ILogger<CustomerSearchQueryHandler>>();
        }

        [Fact]
        public void ConstructorShouldThrowNullExceptionWhenMediatorIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new CustomerSearchQueryHandler(null, _mockLogger.Object));
        }

        [Fact]
        public void ConstructorShouldThrowNullExceptionWhenLoggerIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new CustomerSearchQueryHandler(_mockMediator.Object, null));
        }

        [Fact]
        public async Task HandleShouldSucceedGoodCustomerID()
        {
            // Arrange
            var mockResponse = TestDataProvider.GetTestData<CustomerSearch>("CustomerSearch\\CustomerSearchDocument");
            string memberNumber = TestDataProvider.GetTestData<string>("CustomerSearch\\MemberNumber");
            CustomerSearchQuery query = new CustomerSearchQuery(memberNumber);
            _mockMediator.Setup(s => s.Send(It.IsAny<CustomerSearchRepoQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(mockResponse);
            CustomerSearchQueryHandler handler = new CustomerSearchQueryHandler(_mockMediator.Object, _mockLogger.Object);
            // Act
            var result = await handler.Handle(query, CancellationToken.None);
            // Assert
            Assert.True(result.IsSuccess);
        }

        [Fact]
        public async Task HandleShouldSucceedWithError()
        {
            // Arrange
            var mockResponse = TestDataProvider.GetTestData<CustomerSearch>("CustomerSearch\\CustomerSearchDocument");
            string memberNumber = TestDataProvider.GetTestData<string>("CustomerSearch\\MemberNumber");
            CustomerSearchQuery query = new CustomerSearchQuery(memberNumber);
            _mockMediator.Setup(s => s.Send(It.IsAny<CustomerSearchRepoQuery>(), It.IsAny<CancellationToken>())).Throws(new Exception());
            CustomerSearchQueryHandler handler = new CustomerSearchQueryHandler(_mockMediator.Object, _mockLogger.Object);
            // Act
            var result = await handler.Handle(query, CancellationToken.None);
            // Assert
            Assert.True(result.IsFailure);

        }
    }
}
