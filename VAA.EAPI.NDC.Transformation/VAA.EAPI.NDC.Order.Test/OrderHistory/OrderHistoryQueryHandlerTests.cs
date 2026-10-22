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
using VAA.EAPI.NDC.Order.Application.OrderHistory;
using VAA.EAPI.NDC.Order.Domain.OrderHistoryRQ;
using VAA.EAPI.NDC.Order.Domain.OrderHistoryRS;
using VAA.EAPI.NDC.Order.Infrastructure.Services;
using VAA.EAPI.NDC.Order.Infrastructure.Services.BrandMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.OrderDataMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.OrderSearchMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.PublishOrderDocMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.ValidationMS;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.NDC.Order.Tests.OrderHistory
{
    public class OrderHistoryQueryHandlerTests : IClassFixture<HttpClientFactoryFixture>
    {
        private readonly ILogger<HttpConnector> _logger;
        private readonly IHostEnvironment _hostEnvironment;
        private readonly HttpConnector _httpConnector;
        private readonly IMediator _mediator;
        private readonly ValidationMSCommandHandler _validationMSCommandHandler;
        private readonly OrderDataMSQueryHandler _orderDataMSQueryHandler;
        private readonly OrderSearchMSOrderHistoryCommandHandler _orderHistoryMSCommandHandler;
        private readonly PublishOrderDocMSCommandHandler _publishOrderDocMSCommandHandler;
        private readonly IBrandMSCommand _command;
        private readonly IBrandMSPTCCommand _commandPTC;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<OrderHistoryQueryHandler> _trackOrderHistoryLogger;
        private readonly OrderHistoryQueryHandler _handler;
        private readonly Mock<ITrackAnalyticsEventCommandHandler> _trackAnalyticsEventCommandHandler;

        public OrderHistoryQueryHandlerTests(HttpClientFactoryFixture httpClientFactoryFixture)
        {
            _serviceProvider = new DefaultServiceProvider();
            _mediator = new Mediator(_serviceProvider);
            _logger = Microsoft.Extensions.Logging.Abstractions.NullLogger<HttpConnector>.Instance;
            _hostEnvironment = new HostingEnvironment { ContentRootPath = Directory.GetCurrentDirectory() };
            _httpConnector = new HttpConnector(_logger, httpClientFactoryFixture.Current);
            _validationMSCommandHandler = new ValidationMSCommandHandler(_httpConnector);
            _orderDataMSQueryHandler = new OrderDataMSQueryHandler(_httpConnector);
            _orderHistoryMSCommandHandler = new OrderSearchMSOrderHistoryCommandHandler(_httpConnector);
            _publishOrderDocMSCommandHandler = new PublishOrderDocMSCommandHandler(_httpConnector);
            _command = new BrandMSCommandHandler(_httpConnector);
            _commandPTC = new BrandMSCommandHandlerPTC(_httpConnector);
            _trackOrderHistoryLogger = NullLogger<OrderHistoryQueryHandler>.Instance;
            _trackAnalyticsEventCommandHandler = new Mock<ITrackAnalyticsEventCommandHandler>();
            _handler = new OrderHistoryQueryHandler(_trackOrderHistoryLogger, _hostEnvironment, _orderDataMSQueryHandler, _validationMSCommandHandler, _orderHistoryMSCommandHandler, _publishOrderDocMSCommandHandler, _trackAnalyticsEventCommandHandler.Object, _command, _commandPTC);

            ConfigReader.Set("OrderDataMSEndpoint", "https://Testresources.virginatlantic.com/dev/orderdata/orders/");
            ConfigReader.Set("ValidationMSUrl", "https://testresources.virginatlantic.com/dev/validationmicroservice/Agency/Validation");
            ConfigReader.Set("BrandMSUrl", "https://Testresources.virginatlantic.com/dev/brandmicroservice/brand");
            ConfigReader.Set("PublishOrderDocUrl", "https://Testresources.virginatlantic.com/dev/NdcOrderSearch/PublishOrderDoc");
            ConfigReader.Set("OrderSearchMSOrderHistoryUrl", "https://Testresources.virginatlantic.com/dev/NdcOrderSearch/OrderHistory");

            ConfigReader.Set("ValidationMSSubscriptionKey", "Test_ValidationMSSubscriptionKey");
            ConfigReader.Set("BrandMSSubscriptionKey", "Test_BrandMSSubscriptionKey");
            ConfigReader.Set("agentregistrationenvironment", "NONPROD");
            ConfigReader.Set("OrderSearchMSSubscriptionKey", "Test_OrderSearchMSSubscriptionKey");
            ConfigReader.Set("OrderDataMSSubscriptionKey", "Test_OrderDataMSSubscriptionKey");
        }

        [Fact]
        public async Task OrderCreate_OrderCancelScenario()
        {
            //Arrange
            var request = TestDataProvider.GetXmlTestData<IATA_OrderHistoryRQ>("NDCOrder\\OrderHistory\\OrderHistoryRQ");
            var subscriptionKey = "1234";

            //Act
            var result = await _handler.Handle(request, subscriptionKey, CancellationToken.None);

            //Assert
            var items = result.Value.Value;
            Assert.True(result.IsSuccess);
            Assert.Equal(Status.OK, result.Value.Status);
            Assert.Equal("VS932RSNRHC25", ((OrderHistoryResponseType)items.Items.First()).OrderID);
            Assert.Equal(2, ((OrderHistoryResponseType)items.Items.First()).ChangeGroup.Length);

            var firstChangeGroup = ((OrderHistoryResponseType)items.Items.First()).ChangeGroup.Where(c => c.OrderVersionNumber == "1").First().ChangeOperation.First();

            Assert.Null(firstChangeGroup.Old);
            Assert.Equal("112", firstChangeGroup.ReasonCode);
            Assert.Equal("18", firstChangeGroup.ChangeTypeCode);
            Assert.Single(firstChangeGroup.New.Order.First().OrderItem);
            Assert.Equal("OPENED", firstChangeGroup.New.Order.First().StatusCode);

            var secondChangeGroup = ((OrderHistoryResponseType)items.Items.First()).ChangeGroup.Where(c => c.OrderVersionNumber == "2").First().ChangeOperation.First();

            Assert.NotNull(secondChangeGroup.Old);
            Assert.Equal("112", secondChangeGroup.ReasonCode);
            Assert.Equal("14", secondChangeGroup.ChangeTypeCode);
            Assert.Equal("CLOSED", secondChangeGroup.New.Order.First().StatusCode);
        }

        [Fact]
        public async Task OrderChange_NameCorrectionScenario()
        {
            ConfigReader.Set("OrderSearchMSOrderHistoryUrl", "https://Testresources.virginatlantic.com/dev/NdcOrderSearch/OrderHistoryNameCorrection");

            //Arrange
            var request = TestDataProvider.GetXmlTestData<IATA_OrderHistoryRQ>("NDCOrder\\OrderHistory\\OrderHistoryNameCorrectionRQ");
            var subscriptionKey = "1234";

            //Act
            var result = await _handler.Handle(request, subscriptionKey, CancellationToken.None);

            //Assert
            var items = result.Value.Value;
            Assert.True(result.IsSuccess);
            Assert.Equal(Status.OK, result.Value.Status);
            Assert.Equal("VS932TVAFSN26", ((OrderHistoryResponseType)items.Items.First()).OrderID);
            Assert.Equal(2, ((OrderHistoryResponseType)items.Items.First()).ChangeGroup.Length);

            var firstChangeGroup = ((OrderHistoryResponseType)items.Items.First()).ChangeGroup.Where(c => c.OrderVersionNumber == "1").First().ChangeOperation.First();

            Assert.Null(firstChangeGroup.Old);
            Assert.Equal("112", firstChangeGroup.ReasonCode);
            Assert.Equal("18", firstChangeGroup.ChangeTypeCode);
            Assert.Single(firstChangeGroup.New.Order.First().OrderItem);
            Assert.Equal("OPENED", firstChangeGroup.New.Order.First().StatusCode);

            var secondChangeGroup = ((OrderHistoryResponseType)items.Items.First()).ChangeGroup.Where(c => c.OrderVersionNumber == "2").First().ChangeOperation.First();

            Assert.NotNull(secondChangeGroup.Old);
            Assert.Equal("112", secondChangeGroup.ReasonCode);
            Assert.Equal("9", secondChangeGroup.ChangeTypeCode);
            Assert.Equal("DAVIDTEST", secondChangeGroup.New.DataLists.PaxList.First().Individual.GivenName.First());
            Assert.Equal("SMITHtest", secondChangeGroup.New.DataLists.PaxList.First().Individual.Surname);
        }

        [Fact]
        public async Task OrderCreate_Scenario()
        {
            ConfigReader.Set("OrderSearchMSOrderHistoryUrl", "https://Testresources.virginatlantic.com/dev/NdcOrderSearch/OrderHistoryCreate");

            //Arrange
            var request = TestDataProvider.GetXmlTestData<IATA_OrderHistoryRQ>("NDCOrder\\OrderHistory\\OrderHistoryCreateRQ");
            var subscriptionKey = "1234";

            //Act
            var result = await _handler.Handle(request, subscriptionKey, CancellationToken.None);

            //Assert
            var items = result.Value.Value;
            Assert.True(result.IsSuccess);
            Assert.Equal(Status.OK, result.Value.Status);
            Assert.Equal("VS932QRBMJQ26", ((OrderHistoryResponseType)items.Items.First()).OrderID);
            Assert.Single(((OrderHistoryResponseType)items.Items.First()).ChangeGroup);

            var firstChangeGroup = ((OrderHistoryResponseType)items.Items.First()).ChangeGroup.Where(c => c.OrderVersionNumber == "1").First().ChangeOperation.First();

            Assert.Null(firstChangeGroup.Old);
            Assert.Equal("112", firstChangeGroup.ReasonCode);
            Assert.Equal("18", firstChangeGroup.ChangeTypeCode);
            Assert.Single(firstChangeGroup.New.Order.First().OrderItem);
            Assert.Equal("OPENED", firstChangeGroup.New.Order.First().StatusCode);
            Assert.Equal("9322520079244", firstChangeGroup.New.TicketDocInfo.First().Ticket.First().TicketNumber);
            Assert.Equal("176900862247088462", firstChangeGroup.New.TicketDocInfo.First().PaymentRefID.First());
        }

        [Fact]
        public async Task OrderChange_AmendApids()
        {
            ConfigReader.Set("OrderSearchMSOrderHistoryUrl", "https://Testresources.virginatlantic.com/dev/NdcOrderSearch/OrderHistoryAmendAPIS");

            //Arrange
            var request = TestDataProvider.GetXmlTestData<IATA_OrderHistoryRQ>("NDCOrder\\OrderHistory\\OrderHistoryAmendAPIS");
            var subscriptionKey = "1234";

            //Act
            var result = await _handler.Handle(request, subscriptionKey, CancellationToken.None);

            //Assert
            var items = result.Value.Value;
            Assert.True(result.IsSuccess);
            Assert.Equal(Status.OK, result.Value.Status);
            Assert.Equal("VS932QULPAY26", ((OrderHistoryResponseType)items.Items.First()).OrderID);
            Assert.Equal(2, ((OrderHistoryResponseType)items.Items.First()).ChangeGroup.Length);

            var firstChangeGroup = ((OrderHistoryResponseType)items.Items.First()).ChangeGroup.Where(c => c.OrderVersionNumber == "1").First().ChangeOperation.First();
            Assert.Null(firstChangeGroup.Old);
            Assert.Equal("112", firstChangeGroup.ReasonCode);
            Assert.Equal("18", firstChangeGroup.ChangeTypeCode); 
            Assert.Equal("81610413", firstChangeGroup.New.TicketDocInfo.First().PaymentRefID.First());

            var secondChangeGroup = ((OrderHistoryResponseType)items.Items.First()).ChangeGroup.Where(c => c.OrderVersionNumber == "2").First().ChangeOperation.First();

            Assert.NotNull(secondChangeGroup.Old);
            Assert.Equal("112", secondChangeGroup.ReasonCode);
            Assert.Equal("20", secondChangeGroup.ChangeTypeCode);
            Assert.Equal("PT", secondChangeGroup.New.DataLists.PaxList.First().IdentityDoc.First().IdentityDocTypeCode);
            Assert.Equal("IND", secondChangeGroup.New.DataLists.PaxList.First().IdentityDoc.First().IssuingCountryCode);
            Assert.Equal("PT", secondChangeGroup.New.DataLists.PaxList.First().IdentityDoc.First().IdentityDocTypeCode);
            Assert.Equal("USA", secondChangeGroup.New.DataLists.PaxList.First().IdentityDoc.First().ResidenceCountryCode);
            Assert.Equal("81610413", secondChangeGroup.New.TicketDocInfo.First().PaymentRefID.First());
        }

        [Fact]
        public async Task OrderChange_HoldBooking()
        {
            ConfigReader.Set("OrderSearchMSOrderHistoryUrl", "https://Testresources.virginatlantic.com/dev/NdcOrderSearch/OrderHistoryHoldBooking");

            //Arrange
            var request = TestDataProvider.GetXmlTestData<IATA_OrderHistoryRQ>("NDCOrder\\OrderHistory\\OrderHistoryHoldBooking");
            var subscriptionKey = "1234";

            //Act
            var result = await _handler.Handle(request, subscriptionKey, CancellationToken.None);

            //Assert
            var items = result.Value.Value;
            Assert.True(result.IsSuccess);
            Assert.Equal(Status.OK, result.Value.Status);
            Assert.Equal("VS932POQAAN26", ((OrderHistoryResponseType)items.Items.First()).OrderID);

            var firstChangeGroup = ((OrderHistoryResponseType)items.Items.First()).ChangeGroup.Where(c => c.OrderVersionNumber == "1").First().ChangeOperation.First();

            Assert.Null(firstChangeGroup.Old);
            Assert.Equal("112", firstChangeGroup.ReasonCode);
            Assert.Equal("18", firstChangeGroup.ChangeTypeCode);
            Assert.Null(firstChangeGroup.New.TicketDocInfo);
            Assert.Equal("OPENED", firstChangeGroup.New.Order.First().StatusCode);
            Assert.Empty(firstChangeGroup.New.PaymentFunctions);
        }

        [Fact]
        public async Task OCNScenario()
        {
            //Arrange

            ConfigReader.Set("OrderSearchMSOrderHistoryUrl", "https://Testresources.virginatlantic.com/dev/NdcOrderSearch/OrderHistoryOCNTimeChange");
            var request = TestDataProvider.GetXmlTestData<IATA_OrderHistoryRQ>("NDCOrder\\OrderHistory\\OrderHistoryRQ");
            request.Request.Order.OrderID = "VS932SIVOZP26";
            var subscriptionKey = "1234";

            //Act
            var result = await _handler.Handle(request, subscriptionKey, CancellationToken.None);

            //Assert
            Assert.True(result.IsSuccess);
            var items = result.Value.Value;
            Assert.True(result.IsSuccess);
            Assert.Equal(Status.OK, result.Value.Status);
            Assert.Equal("VS932SIVOZP26", ((OrderHistoryResponseType)items.Items.First()).OrderID);

            Assert.Equal(2, ((OrderHistoryResponseType)items.Items.First()).ChangeGroup.Length);

            var firstChangeGroup = ((OrderHistoryResponseType)items.Items.First()).ChangeGroup.Where(c => c.OrderVersionNumber == "1").First().ChangeOperation.First();

            Assert.Null(firstChangeGroup.Old);
            Assert.Equal("112", firstChangeGroup.ReasonCode);
            Assert.Equal("18", firstChangeGroup.ChangeTypeCode);
            Assert.Equal("OPENED", firstChangeGroup.New.Order.First().StatusCode);
            Assert.Equal("2026-08-05T12:55:00", firstChangeGroup.New.DataLists.DatedMarketingSegmentList.First().Dep.AircraftScheduledDateTime.ToString("yyyy-MM-ddTHH:mm:ss"));

            var secondChangeGroup = ((OrderHistoryResponseType)items.Items.First()).ChangeGroup.Where(c => c.OrderVersionNumber == "2").First().ChangeOperation.First();

            Assert.NotNull(secondChangeGroup.Old);
            Assert.Equal("110", secondChangeGroup.ReasonCode);
            Assert.Equal("10", secondChangeGroup.ChangeTypeCode);
            Assert.Equal("2026-08-05T20:00:00", secondChangeGroup.New.DataLists.DatedMarketingSegmentList.First().Dep.AircraftScheduledDateTime.ToString("yyyy-MM-ddTHH:mm:ss"));
        }

        [Fact]
        public async Task OrderChangeFailurePayment_HoldBooking()
        {
            ConfigReader.Set("OrderSearchMSOrderHistoryUrl", "https://Testresources.virginatlantic.com/dev/NdcOrderSearch/OrderHistoryFailurePayment");

            //Arrange
            var request = TestDataProvider.GetXmlTestData<IATA_OrderHistoryRQ>("NDCOrder\\OrderHistory\\OrderHistoryFailurePaymentRQ");
            var subscriptionKey = "1234";

            //Act
            var result = await _handler.Handle(request, subscriptionKey, CancellationToken.None);

            //Assert
            var items = result.Value.Value;
            Assert.True(result.IsSuccess);
            Assert.Equal(Status.OK, result.Value.Status);
            Assert.Equal("VS932SNZCRK26", ((OrderHistoryResponseType)items.Items.First()).OrderID);
            Assert.Equal(3, ((OrderHistoryResponseType)items.Items.First()).ChangeGroup.Length);

            var firstChangeGroup = ((OrderHistoryResponseType)items.Items.First()).ChangeGroup.Where(c => c.OrderVersionNumber == "1").First().ChangeOperation.First();

            Assert.Null(firstChangeGroup.Old);
            Assert.Equal("112", firstChangeGroup.ReasonCode);
            Assert.Equal("18", firstChangeGroup.ChangeTypeCode);
            Assert.Equal("OPENED", firstChangeGroup.New.Order.First().StatusCode);
            Assert.Empty(firstChangeGroup.New.PaymentFunctions);

            var secondChangeGroup = ((OrderHistoryResponseType)items.Items.First()).ChangeGroup.Where(c => c.OrderVersionNumber == "2").First().ChangeOperation.First();

            Assert.Equal("1", secondChangeGroup.Old.Order.First().OrderVersionNumber);
            Assert.Equal("116", secondChangeGroup.ReasonCode);
            Assert.Equal("21", secondChangeGroup.ChangeTypeCode);
            Assert.Equal("OPENED", secondChangeGroup.New.Order.First().StatusCode);
            Assert.NotNull(secondChangeGroup.New.PaymentFunctions);
            Assert.NotEmpty(secondChangeGroup.New.Warning);

            var thirdChangeGroup = ((OrderHistoryResponseType)items.Items.First()).ChangeGroup.Where(c => c.OrderVersionNumber == "3").First().ChangeOperation.First();

            Assert.Equal("2", thirdChangeGroup.Old.Order.First().OrderVersionNumber);
            Assert.Equal("112", thirdChangeGroup.ReasonCode);
            Assert.Equal("10", thirdChangeGroup.ChangeTypeCode);
            Assert.Equal("OPENED", thirdChangeGroup.New.Order.First().StatusCode);
            Assert.NotNull(thirdChangeGroup.New.PaymentFunctions);
            Assert.Equal("9322520079244", thirdChangeGroup.New.TicketDocInfo.First().Ticket.First().TicketNumber);
        }


        [Fact]
        public async Task OrderChange_SplitScenario()
        {
            ConfigReader.Set("OrderSearchMSOrderHistoryUrl", "https://Testresources.virginatlantic.com/dev/NdcOrderSearch/OrderHistorySplit");

            //Arrange
            var request = TestDataProvider.GetXmlTestData<IATA_OrderHistoryRQ>("NDCOrder\\OrderHistory\\OrderHistorySplitRQ");
            var subscriptionKey = "1234";

            //Act
            var result = await _handler.Handle(request, subscriptionKey, CancellationToken.None);

            //Assert
            var items = result.Value.Value;
            Assert.True(result.IsSuccess);
            Assert.Equal(Status.OK, result.Value.Status);
            Assert.Equal("VS932SVRJZC26", ((OrderHistoryResponseType)items.Items.First()).OrderID);
            Assert.Equal(2, ((OrderHistoryResponseType)items.Items.First()).ChangeGroup.Length);

            var firstChangeGroup = ((OrderHistoryResponseType)items.Items.First()).ChangeGroup.Where(c => c.OrderVersionNumber == "1").First().ChangeOperation.First();

            Assert.Null(firstChangeGroup.Old);
            Assert.Equal("112", firstChangeGroup.ReasonCode);
            Assert.Equal("18", firstChangeGroup.ChangeTypeCode);

            var secondChangeGroup = ((OrderHistoryResponseType)items.Items.First()).ChangeGroup.Where(c => c.OrderVersionNumber == "2").First().ChangeOperation.First();

            Assert.NotNull(secondChangeGroup.Old);
            Assert.Equal("112", secondChangeGroup.ReasonCode);
            Assert.Equal("21", secondChangeGroup.ChangeTypeCode);
        }

        [Fact]
        public async Task OrderChangeSSR_Scenario()
        {
            ConfigReader.Set("OrderSearchMSOrderHistoryUrl", "https://Testresources.virginatlantic.com/dev/NdcOrderSearch/OrderHistorySSR");

            //Arrange
            var request = TestDataProvider.GetXmlTestData<IATA_OrderHistoryRQ>("NDCOrder\\OrderHistory\\OrderHistorySSR");
            var subscriptionKey = "1234";

            //Act
            var result = await _handler.Handle(request, subscriptionKey, CancellationToken.None);

            //Assert
            var items = result.Value.Value;
            Assert.True(result.IsSuccess);
            Assert.Equal(Status.OK, result.Value.Status);
            Assert.Equal("VS932SPESVS26", ((OrderHistoryResponseType)items.Items.First()).OrderID);

            var secondChangeGroup = ((OrderHistoryResponseType)items.Items.First()).ChangeGroup.Where(c => c.OrderVersionNumber == "2").First().ChangeOperation.First();
            var thirdChangeGroup = ((OrderHistoryResponseType)items.Items.First()).ChangeGroup.Where(c => c.OrderVersionNumber == "3").First().ChangeOperation.First();

            Assert.NotNull(secondChangeGroup.Old);
            Assert.Equal("116", secondChangeGroup.ReasonCode);
            Assert.Equal("6", secondChangeGroup.ChangeTypeCode);
            Assert.NotNull(secondChangeGroup.New.Warning);
            Assert.Equal("182", secondChangeGroup.New.Warning.First().Code);
            Assert.Null(thirdChangeGroup.New.Warning);
            Assert.Equal("6", thirdChangeGroup.ChangeTypeCode);
        }

        [Fact]
        public async Task PostSaleSeat()
        {
            ConfigReader.Set("OrderSearchMSOrderHistoryUrl", "https://Testresources.virginatlantic.com/dev/NdcOrderSearch/OrderHistoryPostSaleSeat");

            //Arrange
            var request = TestDataProvider.GetXmlTestData<IATA_OrderHistoryRQ>("NDCOrder\\OrderHistory\\OrderHistoryRQ");
            request.Request.Order.OrderID = "VS932TXCMNT26";
            var subscriptionKey = "1234";

            //Act
            var result = await _handler.Handle(request, subscriptionKey, CancellationToken.None);

            //Assert
            var items = result.Value.Value;
            Assert.True(result.IsSuccess);
            Assert.Equal(Status.OK, result.Value.Status);
            Assert.Equal("VS932TXCMNT26", ((OrderHistoryResponseType)items.Items.First()).OrderID);

            var firstChangeGroup = ((OrderHistoryResponseType)items.Items.First()).ChangeGroup.Where(c => c.OrderVersionNumber == "1").First().ChangeOperation.First();

            Assert.Single(firstChangeGroup.New.Order.First().OrderItem);

            var secondChangeGroup = ((OrderHistoryResponseType)items.Items.First()).ChangeGroup.Where(c => c.OrderVersionNumber == "2").First().ChangeOperation.First();

            Assert.NotNull(secondChangeGroup.Old);
            Assert.Equal("112", secondChangeGroup.ReasonCode);
            Assert.Equal("18", secondChangeGroup.ChangeTypeCode);
            Assert.Equal(2, secondChangeGroup.New.Order.First().OrderItem.Length);
            var seatOrderItem = secondChangeGroup.New.Order.First().OrderItem.Last();
            Assert.Equal("46", (seatOrderItem.Service.First().OrderServiceAssociation.Item as SeatOnLegType)!.Seat.RowNumber);
            Assert.Equal("A", (seatOrderItem.Service.First().OrderServiceAssociation.Item as SeatOnLegType)!.Seat.ColumnID);

        }
    }
}