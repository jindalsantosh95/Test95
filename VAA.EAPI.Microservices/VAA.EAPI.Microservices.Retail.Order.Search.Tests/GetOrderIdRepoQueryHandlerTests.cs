using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Microservices.Retail.Order.Search.Application.Query;
using VAA.EAPI.Microservices.Retail.Order.Search.Infrastructure.Repositories;
using VAA.EAPI.Microservices.Retail.Order.Search.Infrastructure.Repositories.SCVReferenceDB.Interface;
using VAA.EAPI.Microservices.Retail.Order.Search.Infrastructure.Repositories.SCVReferenceDB.Model;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Retail.Order.Search.Tests
{
    public class GetOrderIdRepoQueryHandlerTests
    {
        private readonly Mock<ISCVRefDBService> _refDBService;
        private readonly OrderIdRepoQueryHandler _handler;

        public GetOrderIdRepoQueryHandlerTests()
        {
            _refDBService = new Mock<ISCVRefDBService>();
            _handler = new OrderIdRepoQueryHandler(_refDBService.Object);
        }
        [Fact]
        public void Constructor_ShouldThrowNullException_WhenDBServiceObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new OrderIdRepoQueryHandler(null));
        }
        [Fact]
        public async Task GetOrderIdRepoQueryHandler_ShouldSucceed_WithGoodRequest()
        {
            var request = TestDataProvider.GetTestData<OrderIdRepoQuery>("OrderSearchMS\\GetOrderIdRepoQueryData");
            var response = new List<Orders>();
            _refDBService.Setup(m => m.GetCustomerOrders(It.IsAny<LastNextBookingQuery>())).ReturnsAsync(response);
            var result = await _handler.Handle(request, CancellationToken.None);
            Assert.NotNull(result);
        }
    }
}
