using MediatR;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Infrastructure.Services.CosmosSqlDBv2;
using VAA.EAPI.Microservices.Loyalty.Customer.Application.Query.CustomerSearch;
using VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Repositories.CustomerSearchDocument;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Tests.CustomerSearchDocument
{
    public class CustomerSearchRepoQueryHandlerTests
    {
        private readonly Mock<IMediator> _mockMediator;
        private readonly CustomerSearchRepoQueryHandler _handler;
        public CustomerSearchRepoQueryHandlerTests()
        {
            _mockMediator = new Mock<IMediator>();
            _handler = new CustomerSearchRepoQueryHandler(_mockMediator.Object);
            SettingsHelper.CreateTestEnvironmentSettings();
        }

        [Fact]
        public void ConstructorShouldThrowNullExceptionWhenMediatorIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new CustomerSearchRepoQueryHandler(null));
        }

        [Fact]
        public async Task HandleShouldSucceedWithoutError()
        {
            // Arrange
            var mockResponse = TestDataProvider.GetTestData<List<object>>("CustomerSearch\\ListOfCustomerSearchDocs");
            string memberNumber = TestDataProvider.GetTestData<string>("CustomerSearch\\MemberNumber");
            CustomerSearchQuery customerSearchQuery = new CustomerSearchQuery(memberNumber);
            _mockMediator.Setup(s => s.Send(It.IsAny<CosmosReaderServiceQuery>(), CancellationToken.None)).ReturnsAsync(mockResponse);
            CustomerSearchRepoQuery query = new CustomerSearchRepoQuery(customerSearchQuery);
            // Act
            var result = await _handler.Handle(query, CancellationToken.None);
            // Assert
            Assert.NotNull(result);
        }
    }
}
