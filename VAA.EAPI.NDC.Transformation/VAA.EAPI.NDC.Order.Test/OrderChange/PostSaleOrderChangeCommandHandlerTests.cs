using MediatR;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Core.Xunit.Fixtures;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Domain.Virgin.Models.Customer;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Infrastructure.Services;
using VAA.EAPI.Infrastructure.Services.Analytics;
using VAA.EAPI.NDC.Order.Application.OrderChange;
using VAA.EAPI.NDC.Order.Domain.OrderChangeRQ;
using VAA.EAPI.NDC.Order.Domain.OrderViewRS;
using VAA.EAPI.NDC.Order.Infrastructure.Interface;
using VAA.EAPI.NDC.Order.Infrastructure.Model;
using VAA.EAPI.NDC.Order.Infrastructure.Model.Air4OrderSearchMS;
using VAA.EAPI.NDC.Order.Infrastructure.Model.BrandMS;
using VAA.EAPI.NDC.Order.Infrastructure.Model.OrderMS;
using VAA.EAPI.NDC.Order.Infrastructure.Model.PaymentMS;
using VAA.EAPI.NDC.Order.Infrastructure.Model.ReferenceMS;
using VAA.EAPI.NDC.Order.Infrastructure.Model.SeatMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.Air4OrderSearchMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.ARCPaymentMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.BrandMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.ConfigMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.EmailMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.OfferMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.OrderDataMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.OrderSearchMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.PaymentIFGMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.PaymentMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.ReferenceMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.SeatMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.ValidationMS;
using VAA.EAPI.NDC.Order.Mapper.ErrorMapper;
using VAA.EAPI.NDC.Order.Mapper.Order;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.NDC.Order.Tests.OrderChange
{
    public class PostSaleOrderChangeCommandHandlerTests : IClassFixture<HttpClientFactoryFixture>
    {
        private readonly Mock<IMediator> _mediator;
        private readonly Mock<ILogger<PostSaleOrderChangeCommandHandler>> _logger;
        private readonly Mock<IHostEnvironment> _hostEnvironment;
        private readonly Mock<IGdsOrderResponseMapper> _gdsOrderResponseMapper;
        private readonly PostSaleOrderChangeCommandHandler _handler;
        private readonly Mock<IBrandMSCommand> _command;
        private readonly Mock<IBrandMSPTCCommand> _commandPTC;
        private readonly ILogger<HttpConnector> _loggers;
        private readonly HttpConnector _httpConnector;
        private readonly PaymentIFGMSAuthorizeCommandHandler _paymentIFGMSAuthorizeCommandHandler;
        private readonly PaymentIFGMSOrderCommandHandler _paymentIFGMSOrderCommandHandler;
        private readonly PaymentIFGMSAuthorizeCancelCommandHandler _paymentIFGMSAuthorizeCancelCommandHandler;
        private readonly SendOrderEmailCommandHandler _sendOrderEmailCommandHandler;
        private readonly TransactionCommandHandler _transactionCommandHandler;
        private readonly Mock<ITrackAnalyticsEventCommandHandler> _trackAnalyticsEventCommandHandler;
        private readonly ICityCodeQuery _cityCodeQuery;

        public PostSaleOrderChangeCommandHandlerTests(HttpClientFactoryFixture httpClientFactoryFixture)
        {
            _mediator = new Mock<IMediator>();
            _trackAnalyticsEventCommandHandler = new Mock<ITrackAnalyticsEventCommandHandler>();
            _logger = new Mock<ILogger<PostSaleOrderChangeCommandHandler>>();
            _hostEnvironment = new Mock<IHostEnvironment>();
            _gdsOrderResponseMapper = new Mock<IGdsOrderResponseMapper>();
            _command = new Mock<IBrandMSCommand>();
            _commandPTC = new Mock<IBrandMSPTCCommand>();
            _loggers = Microsoft.Extensions.Logging.Abstractions.NullLogger<HttpConnector>.Instance;
            _httpConnector = new HttpConnector(_loggers, httpClientFactoryFixture.Current);
            _paymentIFGMSAuthorizeCommandHandler = new PaymentIFGMSAuthorizeCommandHandler(_httpConnector);
            _paymentIFGMSOrderCommandHandler = new PaymentIFGMSOrderCommandHandler(_httpConnector);
            _paymentIFGMSAuthorizeCancelCommandHandler = new PaymentIFGMSAuthorizeCancelCommandHandler(_httpConnector);
            _sendOrderEmailCommandHandler = new SendOrderEmailCommandHandler(_httpConnector);
            _transactionCommandHandler = new TransactionCommandHandler(_httpConnector);
            _cityCodeQuery = new CityCodeQueryHandler(_httpConnector);
            _handler = new PostSaleOrderChangeCommandHandler(_mediator.Object, _logger.Object, _hostEnvironment.Object, _gdsOrderResponseMapper.Object, _command.Object, _commandPTC.Object, _paymentIFGMSAuthorizeCommandHandler, _paymentIFGMSOrderCommandHandler, _paymentIFGMSAuthorizeCancelCommandHandler, _sendOrderEmailCommandHandler, _transactionCommandHandler, _trackAnalyticsEventCommandHandler.Object, _cityCodeQuery);
            _hostEnvironment.Setup(x => x.ContentRootPath).Returns(Directory.GetCurrentDirectory());
            ConfigReader.Set("SkipGDSAgentValidation", "false");
        }

        [Fact]
        public void When_MediatorObjectIsNull_Constructor_ShouldThrowNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new PostSaleOrderChangeCommandHandler(null, _logger.Object, _hostEnvironment.Object, _gdsOrderResponseMapper.Object, _command.Object, _commandPTC.Object, _paymentIFGMSAuthorizeCommandHandler, _paymentIFGMSOrderCommandHandler, _paymentIFGMSAuthorizeCancelCommandHandler, _sendOrderEmailCommandHandler, _transactionCommandHandler, _trackAnalyticsEventCommandHandler.Object, _cityCodeQuery));
        }

        [Fact]
        public void When_LoggerObjectIsNull_Constructor_ShouldThrowNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new PostSaleOrderChangeCommandHandler(_mediator.Object, null, _hostEnvironment.Object, _gdsOrderResponseMapper.Object, _command.Object, _commandPTC.Object, _paymentIFGMSAuthorizeCommandHandler, _paymentIFGMSOrderCommandHandler, _paymentIFGMSAuthorizeCancelCommandHandler, _sendOrderEmailCommandHandler, _transactionCommandHandler, _trackAnalyticsEventCommandHandler.Object, _cityCodeQuery));
        }

        [Fact]
        public void When_HostEnvironmentObjectIsNull_Constructor_ShouldThrowNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new PostSaleOrderChangeCommandHandler(_mediator.Object, _logger.Object, null, _gdsOrderResponseMapper.Object, _command.Object, _commandPTC.Object, _paymentIFGMSAuthorizeCommandHandler, _paymentIFGMSOrderCommandHandler, _paymentIFGMSAuthorizeCancelCommandHandler, _sendOrderEmailCommandHandler, _transactionCommandHandler, _trackAnalyticsEventCommandHandler.Object, _cityCodeQuery));
        }

        [Fact]
        public void When_GdsOrderResponseMapperObjectIsNull_Constructor_ShouldThrowNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new PostSaleOrderChangeCommandHandler(_mediator.Object, _logger.Object, _hostEnvironment.Object, null, _command.Object, _commandPTC.Object , _paymentIFGMSAuthorizeCommandHandler, _paymentIFGMSOrderCommandHandler, _paymentIFGMSAuthorizeCancelCommandHandler, _sendOrderEmailCommandHandler, _transactionCommandHandler, _trackAnalyticsEventCommandHandler.Object, _cityCodeQuery));
        }

        [Fact]
        public void When_TransactionCommandObjectIsNull_Constructor_ShouldThrowNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new PostSaleOrderChangeCommandHandler(_mediator.Object, _logger.Object, _hostEnvironment.Object, _gdsOrderResponseMapper.Object, _command.Object, _commandPTC.Object, _paymentIFGMSAuthorizeCommandHandler, _paymentIFGMSOrderCommandHandler, _paymentIFGMSAuthorizeCancelCommandHandler, _sendOrderEmailCommandHandler, null, _trackAnalyticsEventCommandHandler.Object ,_cityCodeQuery));
        }

        [Fact(Skip = "To be modified as per sabre implementation")]
        public async Task ReturnResponse_WhenNotGDS()
        {
            //Arrange
            var request = TestDataProvider.GetXmlTestData<IATA_OrderChangeRQ>("NDCOrder\\OrderChange\\PostSaleOrderChangeReq");
            var seatDoc = TestDataProvider.GetTestData<SeatRS>("NDCOrder\\OrderChange\\SeatDoc_PostSale");
            var orderDoc = TestDataProvider.GetTestData<OrderRS>("NDCOrder\\OrderCreate\\PositiveFlow\\OrderDoc");
            var brandMsRs = TestDataProvider.GetTestData<BrandRS>("NDCOrder\\OrderCreate\\PositiveFlow\\BrandMSRS");
            _mediator.Setup(x => x.Send(It.IsAny<BrandMSCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(brandMsRs);

            var paymentRs = new PaymentRS()
            {
                PaymentDocument = new PaymentDocument()
                {
                    TransactionId = "123",
                    Amount = 100,
                    OrderID = "VS93212345625",
                    TransactionStatus = "Success"
                }
            };
            var currencyRS = TestDataProvider.GetTestData<string>("NDCOrder\\OrderCreate\\PositiveFlow\\CurrencyReferenceMS");
            _mediator.Setup(x => x.Send(It.IsAny<OrderSearchMSCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(orderDoc.OrderDocument);
            _mediator.Setup(x => x.Send(It.IsAny<OrderDataMSQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(orderDoc);
            _mediator.Setup(x => x.Send(It.IsAny<SeatMSQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(seatDoc);
            _mediator.Setup(x => x.Send(It.IsAny<ValidationMSCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(new Order.Infrastructure.Model.ValidationMS.ValidationRS());
            _mediator.Setup(x => x.Send(It.IsAny<Order.Infrastructure.Services.Air4OrderManagementMS.OrderManagementMSCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(new VAA.EAPI.NDC.Order.Infrastructure.Model.Air4OrderManagementMS.ChangeOrderRS()
            {
                orderChangeResponse = new()
                {
                    OrderDetails = new()
                    {
                        AccountableDocumentRefs =
                        [
                            new()
                            {
                                DocumentType = Constants.DocumentTypeEnum.Y,
                                SegmentRefId = "01",
                                CurrentSeatAssignment = "46D"
                            }
                        ]
                    }
                }
            });
            _mediator.Setup(x => x.Send(It.IsAny<PaymentMSAuthorizeCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(paymentRs);
            _mediator.Setup(x => x.Send(It.IsAny<PaymentMSCaptureCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(paymentRs);
           
            _mediator.Setup(x => x.Send(It.IsAny<ReferenceMSCurrencyQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(currencyRS);

            //Act
            var result = await _handler.Handle(new PostSaleOrderChangeCommand() { OrderChangeRQ = request }, new CancellationToken());

            //Assert
            Assert.Equal(EAPI.Domain.Entities.Common.Status.OK, result.Status);
            Assert.NotNull(result.Value.Items);
            var response = result.Value.Items as OrderViewResponseType[];
            Assert.Equal(6, response?.First().Order.First().OrderItem.Length);
            Assert.Equal("VS932131FAA24", response?.First().Order.First().OrderID);
        }

        [Fact]
        public async Task when_wrong_OrderChangePostSaleRequest_ShouldThrowError()
        {
            _hostEnvironment.Setup(x => x.ContentRootPath).Returns(Directory.GetCurrentDirectory());
            var request = TestDataProvider.GetXmlTestData<IATA_OrderChangeRQ>("NDCOrder\\OrderChange\\PostSaleOrderChangeReq");

            request.DistributionChain.First().Ordinal = null;
            request.Request.Order.OrderID = null;
            request.Request.DataLists.PaxList.First().PaxID = null;
            ((AcceptSelectedQuotedOfferListType)request.Request.ChangeOrderChoice.Item).SelectedPricedOffer.First().OfferRefID = null;
            ((AcceptSelectedQuotedOfferListType)request.Request.ChangeOrderChoice.Item).SelectedPricedOffer.First().SelectedOfferItem.First().OfferItemRefID = null;
            ((AcceptSelectedQuotedOfferListType)request.Request.ChangeOrderChoice.Item).SelectedPricedOffer.First().SelectedOfferItem.First().PaxRefID = Array.Empty<string>();

            //Act
            var result = await _handler.Handle(new PostSaleOrderChangeCommand() { OrderChangeRQ = request }, new CancellationToken());

            //Assert
            var items = (ErrorType[])result.Value.Items;
            Assert.Contains(items, x => x.Code == "912");
        }

        [Fact]
        public async Task ValidationMSTestsforGDS()
        {
            //Arrange
            _hostEnvironment.Setup(x => x.ContentRootPath).Returns(Directory.GetCurrentDirectory());
            var request = TestDataProvider.GetXmlTestData<IATA_OrderChangeRQ>("NDCOrder\\OrderChange\\PostSaleOrderChangeReq");
            var seatofferDoc = TestDataProvider.GetTestData<SeatRS>("NDCOrder\\OrderChange\\SeatOfferDoc");
            var airportDetails = TestDataProvider.GetTestData<IEnumerable<AirportDetail>>("NDCOrder\\OrderChange\\AirportDetails");

            _mediator.Setup(x => x.Send(It.IsAny<AirportDetailsQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(airportDetails);

            _mediator.Setup(x => x.Send(It.IsAny<SeatMSQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(seatofferDoc);
            _mediator.Setup(x => x.Send(It.IsAny<ValidationMSCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(new Order.Infrastructure.Model.ValidationMS.ValidationRS()
            {
                Error = new List<ErrorDetails>() { new() { Code = "G009" }, new() { Code = "10144" }, new() { Code = "10146" }, new() { Code = "10147" }, new() { Code = "10140" }, new() { Code = "10152" }, new() { Code = "10153" }, new() { Code = "10145" } }
            });

            request.DistributionChain.First().ParticipatingOrg.Name = "Atlas Travel Services Limited";

            //Act
            var result = await _handler.Handle(new PostSaleOrderChangeCommand() { OrderChangeRQ = request }, new CancellationToken());

            //Assert
            var items = (ErrorType[])result.Value.Items;
            Assert.Contains(items, x => x.Code == "911");
            Assert.Contains(items, x => x.Code == "368");
        }

        [Fact]
        public async Task When_WrongAgencyID_ValidationMS_ShouldThrowError()
        {
            //Arrange
            _hostEnvironment.Setup(x => x.ContentRootPath).Returns(Directory.GetCurrentDirectory());
            var request = TestDataProvider.GetXmlTestData<IATA_OrderChangeRQ>("NDCOrder\\OrderChange\\PostSaleOrderChangeReq");
            var seatofferDoc = TestDataProvider.GetTestData<SeatRS>("NDCOrder\\OrderChange\\SeatOfferDoc");
            var airportDetails = TestDataProvider.GetTestData<IEnumerable<AirportDetail>>("NDCOrder\\OrderChange\\AirportDetails");

            _mediator.Setup(x => x.Send(It.IsAny<AirportDetailsQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(airportDetails);
            _mediator.Setup(x => x.Send(It.IsAny<ValidationMSCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(new Order.Infrastructure.Model.ValidationMS.ValidationRS()
            {
                Error = new List<ErrorDetails>() { new() { Code = "G009" }, new() { Code = "10144" }, new() { Code = "10146" }, new() { Code = "10147" }, new() { Code = "10140" }, new() { Code = "10152" }, new() { Code = "10153" }, new() { Code = "10145" } }
            });
            _mediator.Setup(x => x.Send(It.IsAny<SeatMSQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(seatofferDoc);

            request.DistributionChain.First().ParticipatingOrg.Name = "Atlas Travel Services Limited";

            //Act
            var result = await _handler.Handle(new PostSaleOrderChangeCommand() { OrderChangeRQ = request }, new CancellationToken());

            //Assert
            var items = (ErrorType[])result.Value.Items;
            Assert.Contains(items, x => x.Code == "911");
            Assert.Contains(items, x => x.Code == "368");
        }

        [Fact]
        public async Task When_Wrong_PostSaleOrderChangeRequestForGds_ShouldThrowError()
        {
            //Arrange
            _hostEnvironment.Setup(x => x.ContentRootPath).Returns(Directory.GetCurrentDirectory());
            var request = TestDataProvider.GetXmlTestData<IATA_OrderChangeRQ>("NDCOrder\\OrderChange\\PostSaleOrderChangeReq");
            var seatofferDoc = TestDataProvider.GetTestData<SeatRS>("NDCOrder\\OrderChange\\SeatOfferDoc");
            var orderSearchRS = TestDataProvider.GetTestData<OrderSearchResponse>("NDCOrder\\OrderSearchResponse_Return_3PAX");
            var airportDetails = TestDataProvider.GetTestData<IEnumerable<AirportDetail>>("NDCOrder\\OrderChange\\AirportDetails");

            _mediator.Setup(x => x.Send(It.IsAny<AirportDetailsQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(airportDetails);
            _mediator.Setup(x => x.Send(It.IsAny<Air4OrderSearchMSQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(new Air4OrderSearchMSResponse { OrderSearchResponse = orderSearchRS });
            _mediator.Setup(x => x.Send(It.IsAny<SeatMSQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(seatofferDoc);
            _mediator.Setup(x => x.Send(It.IsAny<ValidationMSCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(new Order.Infrastructure.Model.ValidationMS.ValidationRS());

            request.DistributionChain.First().ParticipatingOrg.Name = "Atlas Travel Services Limited";
            request.Request.DataLists.PaxList.First().Individual.Surname = null;
            request.Request.DataLists.PaxList.First().Individual.GivenName = null;

            //Act
            var result = await _handler.Handle(new PostSaleOrderChangeCommand() { OrderChangeRQ = request }, new CancellationToken());

            //Assert
            var items = (ErrorType[])result.Value.Items;
            Assert.Contains(items, x => x.Code == "912");
        }

        [Fact]
        public async Task When_NoOrderDoc_ValidateOrderSearchResponse_ShouldThrowError()
        {
            //Arrange
            var request = TestDataProvider.GetXmlTestData<IATA_OrderChangeRQ>("NDCOrder\\OrderChange\\PostSaleOrderChangeReq");
            var seatofferDoc = TestDataProvider.GetTestData<SeatRS>("NDCOrder\\OrderChange\\SeatOfferDoc");
            var airportDetails = TestDataProvider.GetTestData<IEnumerable<AirportDetail>>("NDCOrder\\OrderChange\\AirportDetails");

            _mediator.Setup(x => x.Send(It.IsAny<AirportDetailsQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(airportDetails);
            _mediator.Setup(x => x.Send(It.IsAny<ValidationMSCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(new Order.Infrastructure.Model.ValidationMS.ValidationRS());
            _mediator.Setup(x => x.Send(It.IsAny<SeatMSQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(seatofferDoc);
            request.DistributionChain.First().ParticipatingOrg.Name = "Atlas Travel Services Limited";

            //Act
            var result = await _handler.Handle(new PostSaleOrderChangeCommand() { OrderChangeRQ = request }, new CancellationToken());

            //Assert
            var items = (ErrorType[])result.Value.Items;
            Assert.Contains(items, x => x.Code == "914");
        }

        [Fact]
        public async Task When_NoSeatDoc_ValidatePostSaleOrderChangeSeatRQ_ShouldThrowError()
        {
            //Arrange
            var request = TestDataProvider.GetXmlTestData<IATA_OrderChangeRQ>("NDCOrder\\OrderChange\\PostSaleOrderChangeReq");
            var orderSearchRS = TestDataProvider.GetTestData<OrderSearchResponse>("NDCOrder\\OrderSearchResponse_Return_3PAX");

            _mediator.Setup(x => x.Send(It.IsAny<OrderDataMSQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(new Order.Infrastructure.Model.OrderMS.OrderRS() { OrderDocument = null });
            _mediator.Setup(x => x.Send(It.IsAny<Air4OrderSearchMSQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(new Air4OrderSearchMSResponse { OrderSearchResponse = orderSearchRS });
            _mediator.Setup(x => x.Send(It.IsAny<ValidationMSCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(new Order.Infrastructure.Model.ValidationMS.ValidationRS());

            //Act
            var result = await _handler.Handle(new PostSaleOrderChangeCommand() { OrderChangeRQ = request }, new CancellationToken());

            //Assert
            var items = (ErrorType[])result.Value.Items;
            Assert.Contains(items, x => x.Code == "913");
        }

        [Fact]
        public async Task When_AgencyDetailsNotmatchwithSeatDoc_ValidatePostSaleOrderChangeSeatRQ_ShouldThrowError()
        {
            //Arrange
            var request = TestDataProvider.GetXmlTestData<IATA_OrderChangeRQ>("NDCOrder\\OrderChange\\PostSaleOrderChangeReq");
            var orderSearchRS = TestDataProvider.GetTestData<OrderSearchResponse>("NDCOrder\\OrderSearchResponse_Return_3PAX");
            var seatofferDoc = TestDataProvider.GetTestData<SeatRS>("NDCOrder\\OrderChange\\SeatOfferDoc");
            var airportDetails = TestDataProvider.GetTestData<IEnumerable<AirportDetail>>("NDCOrder\\OrderChange\\AirportDetails");

            _mediator.Setup(x => x.Send(It.IsAny<AirportDetailsQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(airportDetails);
            _mediator.Setup(x => x.Send(It.IsAny<OrderDataMSQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(new Order.Infrastructure.Model.OrderMS.OrderRS() { OrderDocument = null });
            _mediator.Setup(x => x.Send(It.IsAny<Air4OrderSearchMSQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(new Air4OrderSearchMSResponse { OrderSearchResponse = orderSearchRS });
            _mediator.Setup(x => x.Send(It.IsAny<ValidationMSCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(new Order.Infrastructure.Model.ValidationMS.ValidationRS());
            _mediator.Setup(x => x.Send(It.IsAny<SeatMSQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(seatofferDoc);

            //Act
            var result = await _handler.Handle(new PostSaleOrderChangeCommand() { OrderChangeRQ = request }, new CancellationToken());

            //Assert
            var items = (ErrorType[])result.Value.Items;
            Assert.Contains(items, x => x.Code == "368");
        }

        [Fact]
        public async Task When_PriceNotmatch_ValidatePostSaleOrderChangeRQAgainstSeatOfferDoc_ShouldThrowError()
        {
            //Arrange
            var request = TestDataProvider.GetXmlTestData<IATA_OrderChangeRQ>("NDCOrder\\OrderChange\\PostSaleOrderChangeReq");
            var orderSearchRS = TestDataProvider.GetTestData<OrderSearchResponse>("NDCOrder\\OrderSearchResponse_Return_3PAX");
            var seatofferDoc = TestDataProvider.GetTestData<SeatRS>("NDCOrder\\OrderChange\\SeatOfferDoc");
            var airportDetails = TestDataProvider.GetTestData<IEnumerable<AirportDetail>>("NDCOrder\\OrderChange\\AirportDetails");

            _mediator.Setup(x => x.Send(It.IsAny<AirportDetailsQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(airportDetails);
            _mediator.Setup(x => x.Send(It.IsAny<OrderDataMSQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(new Order.Infrastructure.Model.OrderMS.OrderRS() { OrderDocument = null });
            _mediator.Setup(x => x.Send(It.IsAny<Air4OrderSearchMSQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(new Air4OrderSearchMSResponse { OrderSearchResponse = orderSearchRS });
            _mediator.Setup(x => x.Send(It.IsAny<ValidationMSCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(new Order.Infrastructure.Model.ValidationMS.ValidationRS());
            _mediator.Setup(x => x.Send(It.IsAny<SeatMSQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(seatofferDoc);

            ((AcceptSelectedQuotedOfferListType)request.Request.ChangeOrderChoice.Item).SelectedPricedOffer.First().SelectedOfferItem.First().OfferItemRefID = "5d25b65b-8e8b-4a00-a02e-337a97d9f0d2_FA|1_18_35_52_69";
            request.DistributionChain.First().ParticipatingOrg.Name = "Atlas Travel Services Limited";

            //Act
            var result = await _handler.Handle(new PostSaleOrderChangeCommand() { OrderChangeRQ = request }, new CancellationToken());

            //Assert
            var items = (ErrorType[])result.Value.Items;
            Assert.Contains(items, x => x.Code == "290");
        }

        [Fact]
        public async Task When_CurCodeNotmatch_ValidatePostSaleOrderChangeRQAgainstSeatOfferDoc_ShouldThrowError()
        {
            //Arrange
            var request = TestDataProvider.GetXmlTestData<IATA_OrderChangeRQ>("NDCOrder\\OrderChange\\PostSaleOrderChangeReq");
            var orderSearchRS = TestDataProvider.GetTestData<OrderSearchResponse>("NDCOrder\\OrderSearchResponse_Return_3PAX");
            var seatofferDoc = TestDataProvider.GetTestData<SeatRS>("NDCOrder\\OrderChange\\SeatOfferDoc");
            var airportDetails = TestDataProvider.GetTestData<IEnumerable<AirportDetail>>("NDCOrder\\OrderChange\\AirportDetails");

            _mediator.Setup(x => x.Send(It.IsAny<AirportDetailsQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(airportDetails);
            _mediator.Setup(x => x.Send(It.IsAny<OrderDataMSQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(new Order.Infrastructure.Model.OrderMS.OrderRS() { OrderDocument = null });
            _mediator.Setup(x => x.Send(It.IsAny<Air4OrderSearchMSQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(new Air4OrderSearchMSResponse { OrderSearchResponse = orderSearchRS });
            _mediator.Setup(x => x.Send(It.IsAny<ValidationMSCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(new Order.Infrastructure.Model.ValidationMS.ValidationRS());
            _mediator.Setup(x => x.Send(It.IsAny<SeatMSQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(seatofferDoc);

            ((AcceptSelectedQuotedOfferListType)request.Request.ChangeOrderChoice.Item).SelectedPricedOffer.First().SelectedOfferItem.First().OfferItemRefID = "5d25b65b-8e8b-4a00-a02e-337a97d9f0d2_FA|1_18_35_52_69";
            request.DistributionChain.First().ParticipatingOrg.Name = "Atlas Travel Services Limited";
            request.Request.PaymentFunctions.First().PaymentProcessingDetails.Amount.Value = 69.20M;
            ((AcceptSelectedQuotedOfferListType)request.Request.ChangeOrderChoice.Item).SelectedPricedOffer.First().TotalOfferPriceAmount.Value = 69.20M;

            //Act
            var result = await _handler.Handle(new PostSaleOrderChangeCommand() { OrderChangeRQ = request }, new CancellationToken());

            //Assert
            var items = (ErrorType[])result.Value.Items;
            Assert.Contains(items, x => x.Code == "727");
        }

        [Fact]
        public async Task When_InvalidSeat_ValidatePostSaleOrderChangeRQAgainstSeatOfferDoc_ShouldThrowError()
        {
            //Arrange
            var request = TestDataProvider.GetXmlTestData<IATA_OrderChangeRQ>("NDCOrder\\OrderChange\\PostSaleOrderChangeReq");
            var orderSearchRS = TestDataProvider.GetTestData<OrderSearchResponse>("NDCOrder\\OrderSearchResponse_Return_3PAX");
            var seatofferDoc = TestDataProvider.GetTestData<SeatRS>("NDCOrder\\OrderChange\\SeatDoc_PostSale");
            var airportDetails = TestDataProvider.GetTestData<IEnumerable<AirportDetail>>("NDCOrder\\OrderChange\\AirportDetails");

            _mediator.Setup(x => x.Send(It.IsAny<AirportDetailsQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(airportDetails);

            _mediator.Setup(x => x.Send(It.IsAny<OrderDataMSQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(new Order.Infrastructure.Model.OrderMS.OrderRS() { OrderDocument = null });
            _mediator.Setup(x => x.Send(It.IsAny<Air4OrderSearchMSQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(new Air4OrderSearchMSResponse { OrderSearchResponse = orderSearchRS });
            _mediator.Setup(x => x.Send(It.IsAny<ValidationMSCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(new Order.Infrastructure.Model.ValidationMS.ValidationRS());
            _mediator.Setup(x => x.Send(It.IsAny<SeatMSQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(seatofferDoc);

            ((AcceptSelectedQuotedOfferListType)request.Request.ChangeOrderChoice.Item).SelectedPricedOffer.First().SelectedOfferItem.First().OfferItemRefID = "5d25b65b-8e8b-4a00-a02e-337a97d9f0d2_FA|1_18_35_52_69";
            request.DistributionChain.First().ParticipatingOrg.Name = "Atlas Travel Services Limited";
            request.Request.PaymentFunctions.First().PaymentProcessingDetails.Amount.Value = 69.20M;
            request.Request.PaymentFunctions.First().PaymentProcessingDetails.Amount.CurCode = "USD";
            ((AcceptSelectedQuotedOfferListType)request.Request.ChangeOrderChoice.Item).SelectedPricedOffer.First().TotalOfferPriceAmount.Value = 69.20M;
            ((AcceptSelectedQuotedOfferListType)request.Request.ChangeOrderChoice.Item).SelectedPricedOffer.First().TotalOfferPriceAmount.CurCode = "USD";
            ((AcceptSelectedQuotedOfferListType)request.Request.ChangeOrderChoice.Item).SelectedPricedOffer.First().SelectedOfferItem.First().SelectedSeat.ColumnID = "UD";

            //Act
            var result = await _handler.Handle(new PostSaleOrderChangeCommand() { OrderChangeRQ = request }, new CancellationToken());

            //Assert
            var items = (ErrorType[])result.Value.Items;
            Assert.Contains(items, x => x.Code == "290");
        }

        [Fact]
        public async Task When_WrongSeat_ValidatePostSaleOrderChangeRequestAgainstSeatMap_ShouldThrowError()
        {
            //Arrange
            var request = TestDataProvider.GetXmlTestData<IATA_OrderChangeRQ>("NDCOrder\\OrderChange\\PostSaleOrderChangeReq");
            var orderSearchRS = TestDataProvider.GetTestData<OrderSearchResponse>("NDCOrder\\OrderSearchResponse_Return_3PAX");
            var seatofferDoc = TestDataProvider.GetTestData<SeatRS>("NDCOrder\\OrderChange\\SeatOfferDoc");
            var airportDetails = TestDataProvider.GetTestData<IEnumerable<AirportDetail>>("NDCOrder\\OrderChange\\AirportDetails");

            _mediator.Setup(x => x.Send(It.IsAny<AirportDetailsQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(airportDetails);
            _mediator.Setup(x => x.Send(It.IsAny<OrderDataMSQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(new Order.Infrastructure.Model.OrderMS.OrderRS() { OrderDocument = null });
            _mediator.Setup(x => x.Send(It.IsAny<Air4OrderSearchMSQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(new Air4OrderSearchMSResponse { OrderSearchResponse = orderSearchRS });
            _mediator.Setup(x => x.Send(It.IsAny<ValidationMSCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(new Order.Infrastructure.Model.ValidationMS.ValidationRS());
            _mediator.Setup(x => x.Send(It.IsAny<SeatMSQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(seatofferDoc);

            ((AcceptSelectedQuotedOfferListType)request.Request.ChangeOrderChoice.Item).SelectedPricedOffer.First().SelectedOfferItem.First().OfferItemRefID = "5d25b65b-8e8b-4a00-a02e-337a97d9f0d2_FA|1_18_35_52_69";
            request.DistributionChain.First().ParticipatingOrg.Name = "Atlas Travel Services Limited";
            request.Request.PaymentFunctions.First().PaymentProcessingDetails.Amount.Value = 69.20M;
            request.Request.PaymentFunctions.First().PaymentProcessingDetails.Amount.CurCode = "USD";
            ((AcceptSelectedQuotedOfferListType)request.Request.ChangeOrderChoice.Item).SelectedPricedOffer.First().TotalOfferPriceAmount.Value = 69.20M;
            ((AcceptSelectedQuotedOfferListType)request.Request.ChangeOrderChoice.Item).SelectedPricedOffer.First().TotalOfferPriceAmount.CurCode = "USD";
            ((AcceptSelectedQuotedOfferListType)request.Request.ChangeOrderChoice.Item).SelectedPricedOffer.First().SelectedOfferItem.First().SelectedSeat.ColumnID = "Y";

            //Act
            var result = await _handler.Handle(new PostSaleOrderChangeCommand() { OrderChangeRQ = request }, new CancellationToken());

            //Assert
            var items = (ErrorType[])result.Value.Items;
            Assert.Contains(items, x => x.Code == "256");
        }

        [Fact]
        public async Task Handler_ShouldReturnResponse_ForSuccessfulSeat()
        {
            //Arrange
            var request = TestDataProvider.GetXmlTestData<IATA_OrderChangeRQ>("NDCOrder\\OrderChange\\PostSaleSeatOrderChangeReq_FreeSeat");
            var orderSearchRS = TestDataProvider.GetTestData<OrderSearchResponse>("NDCOrder\\OrderSearchResponseGDS_Return_3ADT1CHD");
            var seatofferDoc = TestDataProvider.GetTestData<SeatRS>("NDCOrder\\OrderChange\\SeatOfferDoc_Return");
            var airportDetails = TestDataProvider.GetTestData<IEnumerable<AirportDetail>>("NDCOrder\\OrderChange\\AirportDetails");

            _mediator.Setup(x => x.Send(It.IsAny<AirportDetailsQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(airportDetails);
            _mediator.Setup(x => x.Send(It.IsAny<OrderDataMSQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(new Order.Infrastructure.Model.OrderMS.OrderRS() { OrderDocument = null });
            _mediator.Setup(x => x.Send(It.IsAny<Air4OrderSearchMSQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(new Air4OrderSearchMSResponse { OrderSearchResponse = orderSearchRS });
            _mediator.Setup(x => x.Send(It.IsAny<ValidationMSCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(new Order.Infrastructure.Model.ValidationMS.ValidationRS());
            _mediator.Setup(x => x.Send(It.IsAny<SeatMSQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(seatofferDoc);
            _mediator.Setup(x => x.Send(It.IsAny<Order.Infrastructure.Services.Air4OrderManagementMS.OrderManagementMSCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(new VAA.EAPI.NDC.Order.Infrastructure.Model.Air4OrderManagementMS.ChangeOrderRS()
            {
                orderChangeResponse = new()
                {
                    OrderDetails = new()
                    {
                        OrderItems = [new()
                        {
                            CurrentSeatAssignment ="46G",
                            SegmentRefId = "01",
                            PaymentDetails = new ()
                            {
                                PaymentReferenceId = "234567",
                                Amount = new(){MonetaryAmount = "0.00"},
                                PaymentCommitmentDateTime = "2025-06-19T05:09:56"
                            }
                        }]
                    }
                }
            });
            orderSearchRS.OrderItems = orderSearchRS.OrderItems.Append(new()
            {
                OrderItemId = "5",
                Services = [new() { CurrentSeatAssignment = "46D", ServiceId = "SV1" }]
            });
            _gdsOrderResponseMapper.Setup(x => x.AddSeatOrderItems(It.IsAny<VAA.EAPI.NDC.Order.Infrastructure.Model.Air4OrderManagementMS.OrderChangeResponse>(), It.IsAny<OrderSearchResponse>(), It.IsAny<IEnumerable<SeatDetails>>(), It.IsAny<IEnumerable<SeatTransactionId>>(), It.IsAny<List<TransactionIdStatus>>(), It.IsAny<PaymentRS>())).Returns(orderSearchRS);
            _gdsOrderResponseMapper.Setup(X => X.MapOrderResponseForGDS(It.IsAny<OrderSearchResponse>(), It.IsAny<IEnumerable<NDCWarning>>())).Returns(new IATA_OrderViewRS()
            {
                Items = new OrderViewResponseType[]
                {
                    new()
                    {
                        Order = new OrderedOrderType[]
                        {
                            new()
                            {
                                OrderID = "ESR4G3",
                                OwnerCode = "VS",
                                TotalPrice = new Domain.OrderViewRS.PriceType(){BaseAmount = new(){Value = Convert.ToDecimal(2467.76)} },
                                StatusCode = "OPENED",
                                OrderItem = [new(){OrderItemID = "1" }, new() { OrderItemID = "2"}, new() { OrderItemID = "3" }, new() { OrderItemID = "4" }, new() { OrderItemID = "5" }, new() { OrderItemID = "6"}],
                            }
                        }
                    }
                }
            });

            //Act
            var result = await _handler.Handle(new PostSaleOrderChangeCommand() { OrderChangeRQ = request }, new CancellationToken());

            //Assert
            Assert.NotNull(result.Value.Items);
            var response = result.Value.Items as OrderViewResponseType[];
            Assert.Equal(6, response?.First().Order.First().OrderItem.Length);
            Assert.Equal("ESR4G3", response?.First().Order.First().OrderID);
            Assert.Equal("VS", response?.First().Order.First().OwnerCode);
            Assert.Equal("OPENED", response?.First().Order.First().StatusCode);
            Assert.Equal(Convert.ToDecimal(2467.76), response?.First().Order.First().TotalPrice.BaseAmount.Value);
        }

        [Fact]
        public async Task Handler_ShouldReturnErrorResponse_WhenExceptionOccurs()
        {
            //Arrange
            var request = TestDataProvider.GetXmlTestData<IATA_OrderChangeRQ>("NDCOrder\\OrderChange\\PostSaleSeatOrderChangeReq_FreeSeat");
            var orderSearchRS = TestDataProvider.GetTestData<OrderSearchResponse>("NDCOrder\\OrderSearchResponseGDS_Return_3ADT1CHD");
            var seatofferDoc = TestDataProvider.GetTestData<SeatRS>("NDCOrder\\OrderChange\\SeatOfferDoc_Return");

            _mediator.Setup(x => x.Send(It.IsAny<OrderDataMSQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(new Order.Infrastructure.Model.OrderMS.OrderRS() { OrderDocument = null });
            _mediator.Setup(x => x.Send(It.IsAny<Air4OrderSearchMSQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(new Air4OrderSearchMSResponse { OrderSearchResponse = orderSearchRS });
            _mediator.Setup(x => x.Send(It.IsAny<ValidationMSCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(new Order.Infrastructure.Model.ValidationMS.ValidationRS());
            _mediator.Setup(x => x.Send(It.IsAny<SeatMSQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(seatofferDoc);
            _mediator.Setup(x => x.Send(It.IsAny<Order.Infrastructure.Services.SabreOrderManagementMS.SabreOrderManagementMSCommand>(), It.IsAny<CancellationToken>())).ThrowsAsync(new Exception());

            //Act
            var result = await _handler.Handle(new PostSaleOrderChangeCommand() { OrderChangeRQ = request }, new CancellationToken());

            //Assert
            Assert.NotNull(result.Value.Items);
            var response = (ErrorType[])result.Value.Items;
            Assert.Equal("911", response?.FirstOrDefault()?.Code);
        }

        [Fact]
        public async Task Handler_ShouldReturnError_WhenIncorrectCardNumberIsPassed()
        {
            //Arrange
            var request = TestDataProvider.GetXmlTestData<IATA_OrderChangeRQ>("NDCOrder\\OrderChange\\PostSaleOrderChangeReq");
            ((PaymentProcessingPaymentCardType)request.Request.PaymentFunctions.First().PaymentProcessingDetails.PaymentMethod.Item).CardNumber = "111122233334ad4ert";
            _hostEnvironment.Setup(x => x.ContentRootPath).Returns(Directory.GetCurrentDirectory());
            
            //Act
            var result = await _handler.Handle(new PostSaleOrderChangeCommand() { OrderChangeRQ = request }, new CancellationToken());

            //Assert
            var items = (ErrorType[])result.Value.Items;
            Assert.Contains(items, x => x.Code == "708");
        }

        [Fact]
        public async Task PaymentIFGAuthorizeHanlder_ShouldReturnSuccess()
        {
            var request = new PaymentIFGMSAuthorizeCommand()
            {
                Parties = new()
                {
                    Participants = [new() { Ordinal = "1", AgencyID = "9128103", Name = "Skyscanner" }]
                },
                PaymentDetails = new List<Payment>()
                {
                    new()
                    {
                        Amount = new()
                        {
                            MonetaryAmount = "122400",
                            BaseCurrency = "GBP2"
                        }
                    }
                },
                CustomerDetails = new List<Customer>()
                {
                    new()
                    {
                        CustId  ="ADULT_1",
                        Ptc  ="ADT"
                    }
                },
                OrderId = "VS932RVJLVA25"
            };

            ConfigReader.Set("PaymentIFGMSAuthorizeUrl", "https://Testresources.virginatlantic.com/DEV/IFGpayment/IFGAuthorizePayment");
            ConfigReader.Set("PaymentIFGMSSubscriptionKey", "Test_PaymentIFGMSSubscriptionKey");

            //Act 
            var result = await _paymentIFGMSAuthorizeCommandHandler.Handle(request, CancellationToken.None);

            //Assert
            Assert.NotNull(result);
            Assert.Equal("27be8079-9372-4b71-b80f-3064054f7a2f", result.PaymentDocument.TrackId);
        }
    }
}