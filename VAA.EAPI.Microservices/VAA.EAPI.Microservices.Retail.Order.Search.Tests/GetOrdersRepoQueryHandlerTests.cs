using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Infrastructure.Repository.CosmosSqlDBv2;
using VAA.EAPI.Microservices.Retail.Order.Search.Domain.BookingDetails.Entity;
using VAA.EAPI.Microservices.Retail.Order.Search.Infrastructure.Repositories;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Retail.Order.Search.Tests
{
    public class GetOrdersRepoQueryHandlerTests
    {
        private readonly Mock<IMediator> _mediator;
        private readonly GetOrdersRepoQueryHandler _handler;
        public GetOrdersRepoQueryHandlerTests()
        {
            _mediator = new Mock<IMediator>();
            _handler = new GetOrdersRepoQueryHandler(_mediator.Object);
            OrderSearchTestsCommon.CreateTestEnvironmentSettings();
        }

        [Fact]
        public void Constructor_ShouldThrowNullException_WhenMediatorObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
           new GetOrdersRepoQueryHandler(null));
        }
        [Fact]
        public async Task GetOrdersRepoQueryHandler_ShouldSucceed_WithGoodRequest()
        {
            var response = TestDataProvider.GetTestData<List<object>>("OrderSearchMS\\OrderDocData");
            var mockResponse = TestDataProvider.GetTestData<List<OrderDoc>>("OrderSearchMS\\OrderDocData");

            _mediator.Setup(m => m.Send(It.IsAny<CosmosReaderRepoQuery>(), CancellationToken.None)).ReturnsAsync(response);
            _mediator.Setup(m => m.Send(It.IsAny<OrderDoc>(), CancellationToken.None)).ReturnsAsync(mockResponse);
            var request = TestDataProvider.GetTestData<OrdersRepoQuery>("OrderSearchMS\\GetOrdersRepoQueryData");
            var result = await _handler.Handle(request, CancellationToken.None);
            Assert.NotNull(result);
            Assert.Equal(mockResponse.Count, result.Count());
        }

        [Fact]
        public async Task PaymentUpdateEventProcessCommandHandler_ShouldThrowException_WithBadRequest()
        {
            var request = TestDataProvider.GetTestData<OrdersRepoQuery>("OrderSearchMS\\GetOrdersRepoQueryInvalidData");

            await Assert.ThrowsAsync<ArgumentNullException>(() => _handler.Handle(request, CancellationToken.None));
        }
    }
}
