using MediatR;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Microservices.Loyalty.Customer.Application.Query.AddressHistory;
using VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Entities;
using VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Repositories.AddressHistoryDocument;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Tests.AddressHistoryDocument
{
    public class AddressHistoryQueryHandlerTests
    {
        protected readonly Mock<IMediator> _mockMediator;

        public AddressHistoryQueryHandlerTests()
        {
            _mockMediator = new Mock<IMediator>();
        }

        [Fact]
        public void ConstructorShouldThrowNullExceptionWhenMediatorIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new AddressHistoryQueryHandler(null));
        }

        [Fact]
        public async Task HandleShouldSucceedGoodCustomerID()
        {
            // Arrange
            var mockResponse = TestDataProvider.GetTestData<IEnumerable<AddressHistory>>("CustomerMS\\AddressHistory\\ListOfAddressHistory");
            string customerId = TestDataProvider.GetTestData<string>("CustID");
            _mockMediator.Setup(s => s.Send(It.IsAny<AddressHistoryRepoQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(mockResponse);

            AddressHistoryQuery query = new AddressHistoryQuery(customerId);
            AddressHistoryQueryHandler handler = new AddressHistoryQueryHandler(_mockMediator.Object);

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
        }
    }
}
