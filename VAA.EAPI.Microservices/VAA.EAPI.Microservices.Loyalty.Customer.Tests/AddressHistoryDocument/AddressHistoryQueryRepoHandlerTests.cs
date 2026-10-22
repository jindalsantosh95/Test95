using MediatR;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Infrastructure.Services.CosmosSqlDBv2;
using VAA.EAPI.Microservices.Loyalty.Customer.Application.Query.AddressHistory;
using VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Repositories.AddressHistoryDocument;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Tests.AddressHistoryDocument
{
    public class AddressHistoryRepoQueryHandlerTests
    {
        private readonly Mock<IMediator> _mockMediator;
        private readonly AddressHistoryRepoQueryHandler _handler;

        public AddressHistoryRepoQueryHandlerTests()
        {
            _mockMediator = new Mock<IMediator>();
            _handler = new AddressHistoryRepoQueryHandler(_mockMediator.Object);
            SettingsHelper.CreateTestEnvironmentSettings();
        }

        [Fact]
        public void ConstructorShouldThrowNullExceptionWhenMediatorIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new AddressHistoryRepoQueryHandler(null));
        }

        [Fact]
        public async Task HandleShouldSucceedWithoutError()
        {
            // Arrange
            var mockResponse = TestDataProvider.GetTestData<List<object>>("CustomerMS\\AddressHistory\\ListOfAddressHistory");
            string customerId = TestDataProvider.GetTestData<string>("CustID");
            _mockMediator.Setup(s => s.Send(It.IsAny<CosmosReaderServiceQuery>(), CancellationToken.None)).ReturnsAsync(mockResponse);

            AddressHistoryQuery addressHistoryQuery = new AddressHistoryQuery(customerId);
            AddressHistoryRepoQuery query = new AddressHistoryRepoQuery(addressHistoryQuery);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
        }
    }
}
