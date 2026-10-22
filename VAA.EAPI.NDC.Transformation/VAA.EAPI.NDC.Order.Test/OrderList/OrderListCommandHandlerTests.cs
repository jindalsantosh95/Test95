using MediatR;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Hosting.Internal;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Core.Functions;
using VAA.EAPI.Core.Xunit.Fixtures;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Services;
using VAA.EAPI.Infrastructure.Services.Analytics;
using VAA.EAPI.NDC.Order.Application.OrderList;
using VAA.EAPI.NDC.Order.Domain.OrderListRQ;
using VAA.EAPI.NDC.Order.Domain.OrderListRS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.OrderDataMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.ValidationMS;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.NDC.Order.Tests.OrderList
{
    public class OrderListCommandHandlerTests : IClassFixture<HttpClientFactoryFixture>
    {
        private readonly IOrderListCommand _orderListCommand;
        private readonly HttpConnector _httpConnector;
        private readonly ILogger<HttpConnector> _logger;
        private readonly IHostEnvironment _hostEnvironment;
        private readonly IServiceProvider _serviceProvider;
        private readonly IMediator _mediator;
        private readonly ILogger<TrackAnalyticsEventCommandHandler> _trackLogger;
        private readonly ILogger<OrderListCommandHandler> _trackOrderListLogger;
        private readonly Mock<ITrackAnalyticsEventCommandHandler> _trackAnalyticsEventCommandHandler;

        public OrderListCommandHandlerTests(HttpClientFactoryFixture httpClientFactoryFixture)
        {
            _logger = Microsoft.Extensions.Logging.Abstractions.NullLogger<HttpConnector>.Instance;
            _httpConnector = new HttpConnector(_logger, httpClientFactoryFixture.Current);
            _hostEnvironment = new HostingEnvironment() { ContentRootPath = Directory.GetCurrentDirectory() };
            _serviceProvider = new DefaultServiceProvider();
            _mediator = new Mediator(_serviceProvider);
            _trackLogger = NullLogger<TrackAnalyticsEventCommandHandler>.Instance;
            _trackOrderListLogger = NullLogger<OrderListCommandHandler>.Instance;
            _trackAnalyticsEventCommandHandler = new Mock<ITrackAnalyticsEventCommandHandler>();
            _orderListCommand = new OrderListCommandHandler(new ValidationMSCommandHandler(_httpConnector), new OrderListQueryHandler(_httpConnector), _hostEnvironment, _trackAnalyticsEventCommandHandler.Object, _trackOrderListLogger);

            ConfigReader.Set("ValidationMSUrl", "https://testresources.virginatlantic.com/dev/validationmicroservice/Agency/Validation");
            ConfigReader.Set("OrderDataMSOrderListEndpoint", "https://testresources.virginatlantic.com/dev/orderdata/orderList?OrderCancellationDate=2025-09-12&PaymentStatus=unpaid&TravelDateFrom=2026-04-18&TravelDateTo=2026-06-18");
            ConfigReader.Set("ValidationMSSubscriptionKey", "Test_ValidationMSSubscriptionKey");
            ConfigReader.Set("OrderDataMSSubscriptionKey", "Test_ValidationMSSubscriptionKey");
            ConfigReader.Set("agentregistrationenvironment", "NONPROD");
            ConfigReader.Set("OrderListFilterDateRange", "180");
        }

        [Fact]
        public async Task OrderList_CommandHandlershouldReturnSuccess()
        {
            //Arrange
            
            var request = TestDataProvider.GetXmlTestData<IATA_OrderListRQ>("NDCOrder\\OrderList\\OrderListReq");
            var subscriptionKey = "1234";
            ConfigReader.Set("OrderDataMSOrderListEndpoint", "https://testresources.virginatlantic.com/dev/orderdata/orderList?OrderCancellationDate=2025-09-20");

            //Act
            var result = await _orderListCommand.Handle(request, subscriptionKey, CancellationToken.None);

            //Assert
            var items = result.Value.Value;
            Assert.True(result.IsSuccess);
            Assert.Equal(Status.OK, result.Value.Status);
            Assert.Equal("VS932PIONEF25", ((result.Value.Value.Items.First()) as OrderListResponseType).MatchedOrder.First().OrderRefID);
        }

        [Fact]
        public async Task OrderList_CommandHandlershouldThrowErrorWhenNoOrdersMatch()
        {
            //Arrange

            var request = TestDataProvider.GetXmlTestData<IATA_OrderListRQ>("NDCOrder\\OrderList\\OrderListWithAllFilters");
            var subscriptionKey = "1234";
            ConfigReader.Set("OrderDataMSOrderListEndpoint", "https://testresources.virginatlantic.com/dev/orderdata/orderList?CreationDateFrom=2025-09-08T01:30:47Z&CreationDateTo=2025-09-08T01:30:47Z&LastModifiedDateFrom=2025-09-08T00:00:00Z&LastModifiedDateTo=2025-09-12T00:00:00Z&OrderCancellationDate=2026-09-12&PaymentStatus=unpaid&TravelDateFrom=2026-04-18&TravelDateTo=2026-06-18");

            //Act
            var result = await _orderListCommand.Handle(request, subscriptionKey, CancellationToken.None);

            //Assert
            Assert.Equal(Status.BusinessException, result.Value.Status);
            Assert.Equal("129", ((result.Value.Value.Items.First()) as ErrorType).Code);
        }

    }
}
