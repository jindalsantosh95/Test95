using MediatR;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Hosting.Internal;
using Microsoft.Extensions.Logging;
using Moq;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Core.Xunit.Fixtures;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Infrastructure.Services;
using VAA.EAPI.Infrastructure.Services.Analytics;
using VAA.EAPI.NDC.Order.Application.OrderCreate;
using VAA.EAPI.NDC.Order.Domain.OrderCreateRQ;
using VAA.EAPI.NDC.Order.Domain.OrderViewRS;
using VAA.EAPI.NDC.Order.Infrastructure.Interface;
using VAA.EAPI.NDC.Order.Infrastructure.Model.ARCPaymentMS;
using VAA.EAPI.NDC.Order.Infrastructure.Model.BrandMS;
using VAA.EAPI.NDC.Order.Infrastructure.Model.OfferRS;
using VAA.EAPI.NDC.Order.Infrastructure.Model.OrderMS;
using VAA.EAPI.NDC.Order.Infrastructure.Model.PaymentMS;
using VAA.EAPI.NDC.Order.Infrastructure.Model.ReferenceMS;
using VAA.EAPI.NDC.Order.Infrastructure.Model.SeatMS;
using VAA.EAPI.NDC.Order.Infrastructure.Model.ValidationMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.ARCPaymentMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.BrandMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.ConfigMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.EmailMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.IFGAgentFopLookUp;
using VAA.EAPI.NDC.Order.Infrastructure.Services.OfferMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.OrderDataMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.OrderMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.OrderTicketMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.PaymentIFGMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.PaymentMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.PublishOrderDocMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.ReferenceMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.SeatMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.ValidationMS;
using VAA.EAPI.NDC.Order.Mapper.Order;
using VAA.EAPI.NDC.Order.Validations.OrderCreate;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.NDC.Order.Tests.OrderCreate
{
    public class OrderCreateCommandHandlerTests : IClassFixture<HttpClientFactoryFixture>
    {
        private readonly Mock<IMediator> _mediator;
        private readonly ILogger<HttpConnector> _loggers;
        private readonly IHostEnvironment _hostEnvironments;
        private readonly HttpConnector _httpConnector;
        private readonly Mock<ILogger<OrderCreateCommandHandler>> _logger;
        private readonly Mock<IHostEnvironment> _hostEnvironment;
        private readonly OrderCreateCommandHandler _handler;
        private readonly Mock<IBrandMSCommand> _command;
        private readonly Mock<IBrandMSPTCCommand> _commandPTC;
        private readonly Mock<IHttpConnector> _httpClientFactory = new();
        private readonly PublishOrderDocMSCommandHandler _publishOrderDocMSCommandHandler;
        private readonly IFGAgentFopLookUpCommandHandler _iFGAgentFopLookUpCommandHandler;
        private readonly SendOrderEmailCommandHandler _sendOrderEmailCommand;
        private readonly ICurrencyConversionQuery _currencyConversionQuery;
        private readonly CheckAgentStatusQueryHandler _checkAgentStatusQueryHandler;
        private readonly TransactionCommandHandler _transactionCommandHandler;
        private readonly PaymentMSRetrieveQueryHandler _paymentMSRetrieveQueryHandler;
        private readonly Mock<ITrackAnalyticsEventCommandHandler> _trackAnalyticsEventCommandHandler;
        private readonly ICityCodeQuery _cityCodeQuery;

        public OrderCreateCommandHandlerTests(HttpClientFactoryFixture httpClientFactoryFixture)
        {
            _mediator = new Mock<IMediator>();
            _logger = new Mock<ILogger<OrderCreateCommandHandler>>();
            _hostEnvironment = new Mock<IHostEnvironment>();
            _command = new Mock<IBrandMSCommand>();
            _commandPTC = new Mock<IBrandMSPTCCommand>();
            _loggers = Microsoft.Extensions.Logging.Abstractions.NullLogger<HttpConnector>.Instance;
            _hostEnvironments = new HostingEnvironment { ContentRootPath = Directory.GetCurrentDirectory() };
            _httpConnector = new HttpConnector(_loggers, httpClientFactoryFixture.Current);
            _publishOrderDocMSCommandHandler = new PublishOrderDocMSCommandHandler(_httpConnector);
            _iFGAgentFopLookUpCommandHandler = new IFGAgentFopLookUpCommandHandler(_httpConnector);
            _sendOrderEmailCommand = new SendOrderEmailCommandHandler(_httpConnector);
            _currencyConversionQuery = new CurrencyConversionQueryHandler(_httpConnector);
            _checkAgentStatusQueryHandler = new CheckAgentStatusQueryHandler(_httpConnector);
            _transactionCommandHandler = new TransactionCommandHandler(_httpConnector);
            _paymentMSRetrieveQueryHandler = new PaymentMSRetrieveQueryHandler(_httpConnector);
            _trackAnalyticsEventCommandHandler = new Mock<ITrackAnalyticsEventCommandHandler>();
            _cityCodeQuery = new CityCodeQueryHandler(_httpConnector);

            _handler = new OrderCreateCommandHandler(_mediator.Object, _logger.Object, _hostEnvironment.Object, _command.Object, _commandPTC.Object, new PaymentIFGMSAuthorizeCommandHandler(_httpClientFactory.Object), new PaymentIFGMSOrderCommandHandler(_httpClientFactory.Object), _publishOrderDocMSCommandHandler, _iFGAgentFopLookUpCommandHandler, _sendOrderEmailCommand, _currencyConversionQuery, _checkAgentStatusQueryHandler, _transactionCommandHandler, _paymentMSRetrieveQueryHandler, _trackAnalyticsEventCommandHandler.Object, _cityCodeQuery);
        }

        [Fact]
        public void When_MediatorObjectIsNull_Constructor_ShouldThrowNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new OrderCreateCommandHandler(null, _logger.Object, _hostEnvironment.Object, _command.Object, _commandPTC.Object, null, null, null,null, null,null, _checkAgentStatusQueryHandler, _transactionCommandHandler, _paymentMSRetrieveQueryHandler, _trackAnalyticsEventCommandHandler.Object, _cityCodeQuery));
        }

        [Fact]
        public void When_LoggerObjectIsNull_Constructor_ShouldThrowNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new OrderCreateCommandHandler(_mediator.Object, null, _hostEnvironment.Object, _command.Object, _commandPTC.Object, null, null, null,null, null,null, _checkAgentStatusQueryHandler, _transactionCommandHandler, _paymentMSRetrieveQueryHandler, _trackAnalyticsEventCommandHandler.Object, _cityCodeQuery));
        }

        [Fact]
        public void When_HostEnvironmentObjectIsNull_Constructor_ShouldThrowNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new OrderCreateCommandHandler(_mediator.Object, _logger.Object, null, _command.Object, _commandPTC.Object, null, null, null,null, _sendOrderEmailCommand,_currencyConversionQuery, _checkAgentStatusQueryHandler, _transactionCommandHandler, _paymentMSRetrieveQueryHandler, _trackAnalyticsEventCommandHandler.Object, _cityCodeQuery));
        }

        [Fact]
        public async Task Handler_Exception()
        {
            //Arrange
            _hostEnvironment.Setup(x => x.ContentRootPath).Returns(Directory.GetCurrentDirectory());
            _mediator.Setup(x => x.Send(It.IsAny<OfferMSQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(new Order.Infrastructure.Model.OfferRS.OfferResponse() { Parties = new Order.Infrastructure.Model.Parties() { CorporateAccount = new Order.Infrastructure.Model.CorporateAccount() { AgencyID = "" } } });
            var request = TestDataProvider.GetXmlTestData<IATA_OrderCreateRQ>("NDCOrder\\OrderCreate\\OrderCreateRQ");

            //Act
            var result = await _handler.Handle(new OrderCreateCommand() { OrderCreateRQ = request }, new CancellationToken());

            //Assert
            var items = (ErrorType[])result.Value.Value.Items;
            Assert.Contains(items, x => x.Code == "911");
        }

        [Fact]
        public async Task When_ParticipatingOrg_OrgIDIsNull_ValidateDistributionChain_ShouldThrowError()
        {
            //Arrange
            _hostEnvironment.Setup(x => x.ContentRootPath).Returns(Directory.GetCurrentDirectory());

            var request = TestDataProvider.GetXmlTestData<IATA_OrderCreateRQ>("NDCOrder\\OrderCreate\\OrderCreateRQ");
            request.DistributionChain.First().ParticipatingOrg.OrgID = null;

            //Act
            var result = await _handler.Handle(new OrderCreateCommand() { OrderCreateRQ = request }, new CancellationToken());

            //Assert
            var items = (ErrorType[])result.Value.Value.Items;
            Assert.Contains(items, x => x.Code == "912");
        }

        [Fact]
        public async Task When_DistributionChainGreaterThanTwo_OrdinalIsNull_ValidateDistributionChain_ShouldThrowError()
        {
            //Arrange
            _hostEnvironment.Setup(x => x.ContentRootPath).Returns(Directory.GetCurrentDirectory());

            var validationRS = TestDataProvider.GetTestData<ValidationRS>("NDCOrder\\OrderCreate\\ValidationMSResponse");
            _mediator.Setup(x => x.Send(It.IsAny<ValidationMSCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(validationRS);

            var request = TestDataProvider.GetXmlTestData<IATA_OrderCreateRQ>("NDCOrder\\OrderCreate\\OrderCreateRQ_DL");
            request.DistributionChain.First().Ordinal = null;
            request.DistributionChain.Last().Ordinal = null;

            //Act
            var result = await _handler.Handle(new OrderCreateCommand() { OrderCreateRQ = request }, new CancellationToken());

            //Assert
            var items = (ErrorType[])result.Value.Value.Items;
            Assert.Contains(items, x => x.Code == "912");
        }

        [Fact]
        public async Task When_DistributionChainGreaterThanTwo_OrdinalIsLetter_ValidateDistributionChain_ShouldThrowError()
        {
            //Arrange
            _hostEnvironment.Setup(x => x.ContentRootPath).Returns(Directory.GetCurrentDirectory());

            var validationRS = TestDataProvider.GetTestData<ValidationRS>("NDCOrder\\OrderCreate\\ValidationMSResponse");
            _mediator.Setup(x => x.Send(It.IsAny<ValidationMSCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(validationRS);

            var request = TestDataProvider.GetXmlTestData<IATA_OrderCreateRQ>("NDCOrder\\OrderCreate\\OrderCreateRQ_DL");
            request.DistributionChain.First().Ordinal = "A";
            request.DistributionChain.Last().Ordinal = "A";

            //Act
            var result = await _handler.Handle(new OrderCreateCommand() { OrderCreateRQ = request }, new CancellationToken());

            //Assert
            var items = (ErrorType[])result.Value.Value.Items;
            Assert.Contains(items, x => x.Code == "912");
        }

        [Fact]
        public async Task When_DistributionChainGreaterThanTwo_OrdinalNotStartWith_ValidateDistributionChain_ShouldThrowError()
        {
            //Arrange
            _hostEnvironment.Setup(x => x.ContentRootPath).Returns(Directory.GetCurrentDirectory());

            var validationRS = TestDataProvider.GetTestData<ValidationRS>("NDCOrder\\OrderCreate\\ValidationMSResponse");
            _mediator.Setup(x => x.Send(It.IsAny<ValidationMSCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(validationRS);

            var request = TestDataProvider.GetXmlTestData<IATA_OrderCreateRQ>("NDCOrder\\OrderCreate\\OrderCreateRQ_DL");
            request.DistributionChain.First().Ordinal = "2";
            request.DistributionChain.Last().Ordinal = "2";

            //Act
            var result = await _handler.Handle(new OrderCreateCommand() { OrderCreateRQ = request }, new CancellationToken());

            //Assert
            var items = (ErrorType[])result.Value.Value.Items;
            Assert.Contains(items, x => x.Code == "912");
        }

        [Fact]
        public async Task When_OrdinalIsNull_ValidateDistributionChain_ShouldThrowError()
        {
            //Arrange
            _hostEnvironment.Setup(x => x.ContentRootPath).Returns(Directory.GetCurrentDirectory());

            var validationRS = TestDataProvider.GetTestData<ValidationRS>("NDCOrder\\OrderCreate\\ValidationMSResponse");
            _mediator.Setup(x => x.Send(It.IsAny<ValidationMSCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(validationRS);

            var request = TestDataProvider.GetXmlTestData<IATA_OrderCreateRQ>("NDCOrder\\OrderCreate\\OrderCreateRQ");
            request.DistributionChain.First().Ordinal = null;

            //Act
            var result = await _handler.Handle(new OrderCreateCommand() { OrderCreateRQ = request }, new CancellationToken());

            //Assert
            var items = (ErrorType[])result.Value.Value.Items;
            Assert.Contains(items, x => x.Code == "912");
        }

        [Fact]
        public async Task When_NoOfferId_ValidateOfferID_ShouldThrowError()
        {
            //Arrange
            _hostEnvironment.Setup(x => x.ContentRootPath).Returns(Directory.GetCurrentDirectory());

            var request = TestDataProvider.GetXmlTestData<IATA_OrderCreateRQ>("NDCOrder\\OrderCreate\\OrderCreateRQ");
            ((AcceptSelectedQuotedOfferListType)request.Request.CreateOrder.Item).SelectedPricedOffer.First().OfferRefID = null;

            //Act
            var result = await _handler.Handle(new OrderCreateCommand() { OrderCreateRQ = request }, new CancellationToken());

            //Assert
            var items = (ErrorType[])result.Value.Value.Items;
            Assert.Contains(items, x => x.Code == "912");
        }

        [Fact]
        public async Task When_SelectedOfferItemIsNull_ValidateNonFlightServices_ShouldThrowError()
        {
            //Arrange
            _hostEnvironment.Setup(x => x.ContentRootPath).Returns(Directory.GetCurrentDirectory());

            var offerDoc = TestDataProvider.GetTestData<OfferResponse>("NDCOrder\\OrderChange\\OfferDoc");
            _mediator.Setup(x => x.Send(It.IsAny<OfferMSQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(offerDoc);

            var request = TestDataProvider.GetXmlTestData<IATA_OrderCreateRQ>("NDCOrder\\OrderCreate\\OrderCreateRQ");
            request.Request.PaymentFunctions = null;
            ((AcceptSelectedQuotedOfferListType)request.Request.CreateOrder.Item).SelectedPricedOffer.First().SelectedOfferItem = null;

            //Act
            var result = await _handler.Handle(new OrderCreateCommand() { OrderCreateRQ = request }, new CancellationToken());

            //Assert
            var items = (ErrorType[])result.Value.Value.Items;
            Assert.Contains(items, x => x.Code == "912");
        }

        [Fact]
        public async Task When_OfferItemRefIdIsNull_ValidateNonFlightServices_ShouldThrowError()
        {
            //Arrange
            _hostEnvironment.Setup(x => x.ContentRootPath).Returns(Directory.GetCurrentDirectory());

            var offerDoc = TestDataProvider.GetTestData<OfferResponse>("NDCOrder\\OrderChange\\OfferDoc");
            _mediator.Setup(x => x.Send(It.IsAny<OfferMSQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(offerDoc);

            var request = TestDataProvider.GetXmlTestData<IATA_OrderCreateRQ>("NDCOrder\\OrderCreate\\OrderCreateRQ");
            request.Request.PaymentFunctions = null;
            ((AcceptSelectedQuotedOfferListType)request.Request.CreateOrder.Item).SelectedPricedOffer.First().SelectedOfferItem.First().OfferItemRefID = null;

            //Act
            var result = await _handler.Handle(new OrderCreateCommand() { OrderCreateRQ = request }, new CancellationToken());

            //Assert
            var items = (ErrorType[])result.Value.Value.Items;
            Assert.Contains(items, x => x.Code == "912");
        }

        [Fact]
        public async Task When_SelectedSeatIsNotNull_ValidateNonFlightServices_ShouldThrowError()
        {
            //Arrange
            _hostEnvironment.Setup(x => x.ContentRootPath).Returns(Directory.GetCurrentDirectory());

            var request = TestDataProvider.GetXmlTestData<IATA_OrderCreateRQ>("NDCOrder\\OrderCreate\\OrderCreateRQ");
            request.Request.PaymentFunctions = null;

            //Act
            var result = await _handler.Handle(new OrderCreateCommand() { OrderCreateRQ = request }, new CancellationToken());

            //Assert
            var items = (ErrorType[])result.Value.Value.Items;
            Assert.Contains(items, x => x.Code == "495");
        }

        [Fact]
        public async Task When_GivenNameAndSurNameIsNull_ValidatePax_ShouldThrowError()
        {
            //Arrange
            _hostEnvironment.Setup(x => x.ContentRootPath).Returns(Directory.GetCurrentDirectory());

            var validationRS = TestDataProvider.GetTestData<ValidationRS>("NDCOrder\\OrderCreate\\ValidationMSResponse");
            _mediator.Setup(x => x.Send(It.IsAny<ValidationMSCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(validationRS);

            var offerDoc = TestDataProvider.GetTestData<OfferResponse>("NDCOrder\\OrderCreate\\ValidatePaxOfferDoc");
            _mediator.Setup(x => x.Send(It.IsAny<OfferMSQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(offerDoc);

            var request = TestDataProvider.GetXmlTestData<IATA_OrderCreateRQ>("NDCOrder\\OrderCreate\\OrderCreateRQ");
            request.Request.DataLists.PaxList.First().Individual.GivenName = [];
            request.Request.DataLists.PaxList.First().Individual.Surname = null;
            ((Domain.OrderCreateRQ.PayerIndividualType)request.Request.PaymentFunctions.First().PaymentProcessingDetails.Payer.PayerName.Item).GivenName = [];
            ((Domain.OrderCreateRQ.PayerIndividualType)request.Request.PaymentFunctions.First().PaymentProcessingDetails.Payer.PayerName.Item).Surname = null;

            ConfigReader.Set("IFGAgentFopLookUpUrl", "https://Testresources.virginatlantic.com/DEV/validationmicroservice/IFGAgentFopLookUp");
            ConfigReader.Set("IFGAgentFOPLookUpSubscriptionKey", "Test_IFGAgentFOPLookUpSubscriptionKey");

            //Act
            var result = await _handler.Handle(new OrderCreateCommand() { OrderCreateRQ = request }, new CancellationToken());

            //Assert
            var items = (ErrorType[])result.Value.Value.Items;
            Assert.Contains(items, x => x.Code == "152");
        }

        [Fact]
        public async Task When_GivenNameAndSurNameIsInvalid_ValidatePax_ShouldThrowError()
        {
            //Arrange
            _hostEnvironment.Setup(x => x.ContentRootPath).Returns(Directory.GetCurrentDirectory());

            var validationRS = TestDataProvider.GetTestData<ValidationRS>("NDCOrder\\OrderCreate\\ValidationMSResponse");
            _mediator.Setup(x => x.Send(It.IsAny<ValidationMSCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(validationRS);

            var offerDoc = TestDataProvider.GetTestData<OfferResponse>("NDCOrder\\OrderCreate\\ValidatePaxOfferDoc");
            _mediator.Setup(x => x.Send(It.IsAny<OfferMSQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(offerDoc);

            var request = TestDataProvider.GetXmlTestData<IATA_OrderCreateRQ>("NDCOrder\\OrderCreate\\OrderCreateRQ");
            request.Request.DataLists.PaxList.First().Individual.GivenName = new string[] { "123" };
            request.Request.DataLists.PaxList.First().Individual.Surname = "123";

            ((Domain.OrderCreateRQ.PayerIndividualType)request.Request.PaymentFunctions.First().PaymentProcessingDetails.Payer.PayerName.Item).GivenName = ["123"];
            ((Domain.OrderCreateRQ.PayerIndividualType)request.Request.PaymentFunctions.First().PaymentProcessingDetails.Payer.PayerName.Item).Surname = "123";

            //Act
            var result = await _handler.Handle(new OrderCreateCommand() { OrderCreateRQ = request }, new CancellationToken());

            //Assert
            var items = (ErrorType[])result.Value.Value.Items;
            Assert.Contains(items, x => x.Code == "314");
        }

        [Fact]
        public async Task When_PaxListSurName_ExceedsLength_ValidatePax_ShouldThrowError()
        {
            //Arrange
            _hostEnvironment.Setup(x => x.ContentRootPath).Returns(Directory.GetCurrentDirectory());

            var validationRS = TestDataProvider.GetTestData<ValidationRS>("NDCOrder\\OrderCreate\\ValidationMSResponse");
            _mediator.Setup(x => x.Send(It.IsAny<ValidationMSCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(validationRS);

            var offerDoc = TestDataProvider.GetTestData<OfferResponse>("NDCOrder\\OrderCreate\\ValidatePaxOfferDoc");
            _mediator.Setup(x => x.Send(It.IsAny<OfferMSQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(offerDoc);

            var request = TestDataProvider.GetXmlTestData<IATA_OrderCreateRQ>("NDCOrder\\OrderCreate\\OrderCreateRQ");
            request.Request.DataLists.PaxList.First().Individual.Surname = "DavidSmithDavidSmithDavidSmithDavidSmithDavid";

            //Act
            var result = await _handler.Handle(new OrderCreateCommand() { OrderCreateRQ = request }, new CancellationToken());

            //Assert
            var items = (ErrorType[])result.Value.Value.Items;
            Assert.Contains(items, x => x.Code == "149");
        }

        [Fact]
        public async Task When_PaxListGenderCode_Blank_ValidatePax_ShouldThrowError()
        {
            //Arrange
            _hostEnvironment.Setup(x => x.ContentRootPath).Returns(Directory.GetCurrentDirectory());

            var validationRS = TestDataProvider.GetTestData<ValidationRS>("NDCOrder\\OrderCreate\\ValidationMSResponse");
            _mediator.Setup(x => x.Send(It.IsAny<ValidationMSCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(validationRS);

            var offerDoc = TestDataProvider.GetTestData<OfferResponse>("NDCOrder\\OrderCreate\\ValidatePaxOfferDoc");
            _mediator.Setup(x => x.Send(It.IsAny<OfferMSQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(offerDoc);

            var request = TestDataProvider.GetXmlTestData<IATA_OrderCreateRQ>("NDCOrder\\OrderCreate\\OrderCreateRQ");
            request.Request.DataLists.PaxList.First().Individual.GenderCodeSpecified = false;

            ConfigReader.Set("IFGAgentFopLookUpUrl", "https://Testresources.virginatlantic.com/DEV/validationmicroservice/IFGAgentFopLookUp");
            ConfigReader.Set("IFGAgentFOPLookUpSubscriptionKey", "Test_IFGAgentFOPLookUpSubscriptionKey");

            //Act
            var result = await _handler.Handle(new OrderCreateCommand() { OrderCreateRQ = request }, new CancellationToken());

            //Assert
            var items = (ErrorType[])result.Value.Value.Items;
            Assert.Contains(items, x => x.Code == "914");
        }

        [Fact]
        public async Task When_PaymentProcessingDetailsIsNull_ValidatePax_ShouldThrowError()
        {
            //Arrange
            _hostEnvironment.Setup(x => x.ContentRootPath).Returns(Directory.GetCurrentDirectory());

            var validationRS = TestDataProvider.GetTestData<ValidationRS>("NDCOrder\\OrderCreate\\ValidationMSResponse");
            _mediator.Setup(x => x.Send(It.IsAny<ValidationMSCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(validationRS);

            var offerDoc = TestDataProvider.GetTestData<OfferResponse>("NDCOrder\\OrderCreate\\ValidatePaxOfferDoc");
            _mediator.Setup(x => x.Send(It.IsAny<OfferMSQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(offerDoc);

            var request = TestDataProvider.GetXmlTestData<IATA_OrderCreateRQ>("NDCOrder\\OrderCreate\\OrderCreateRQ");
            ((Domain.OrderCreateRQ.PayerIndividualType)request.Request.PaymentFunctions.First().PaymentProcessingDetails.Payer.PayerName.Item).GivenName = [];

            ConfigReader.Set("IFGAgentFopLookUpUrl", "https://Testresources.virginatlantic.com/DEV/validationmicroservice/IFGAgentFopLookUp");
            ConfigReader.Set("IFGAgentFOPLookUpSubscriptionKey", "Test_IFGAgentFOPLookUpSubscriptionKey");

            //Act
            var result = await _handler.Handle(new OrderCreateCommand() { OrderCreateRQ = request }, new CancellationToken());

            //Assert
            var items = (ErrorType[])result.Value.Value.Items;
            Assert.Contains(items, x => x.Code == "152");
        }

        [Fact]
        public async Task When_TotalOfferPriceAmountIsNull_ValidateAmountForHoldBooking_ShouldThrowError()
        {
            //Arrange
            _hostEnvironment.Setup(x => x.ContentRootPath).Returns(Directory.GetCurrentDirectory());

            var validationRS = TestDataProvider.GetTestData<ValidationRS>("NDCOrder\\OrderCreate\\ValidationMSResponse");
            _mediator.Setup(x => x.Send(It.IsAny<ValidationMSCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(validationRS);

            var offerDoc = TestDataProvider.GetTestData<OfferResponse>("NDCOrder\\OrderCreate\\ValidatePaxOfferDoc");
            _mediator.Setup(x => x.Send(It.IsAny<OfferMSQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(offerDoc);

            var request = TestDataProvider.GetXmlTestData<IATA_OrderCreateRQ>("NDCOrder\\OrderCreate\\OrderCreateRQ");
            request.Request.PaymentFunctions = null;
            SelectedPricedOfferType[] SelectedPricedOfferTypeArray = new SelectedPricedOfferType[1];
            SelectedPricedOfferTypeArray[0] = ((AcceptSelectedQuotedOfferListType)request.Request.CreateOrder.Item).SelectedPricedOffer.First();
            SelectedPricedOfferTypeArray[0].TotalOfferPriceAmount = null;
            ((AcceptSelectedQuotedOfferListType)request.Request.CreateOrder.Item).SelectedPricedOffer = SelectedPricedOfferTypeArray;

            //Act
            var result = await _handler.Handle(new OrderCreateCommand() { OrderCreateRQ = request }, new CancellationToken());

            //Assert
            var items = (ErrorType[])result.Value.Value.Items;
            Assert.Contains(items, x => x.Code == "727");
        }

        [Fact]
        public async Task When_PaymentFunctions_CurCodeIsDifferent_ValidateAmountForHoldBooking_ShouldThrowError()
        {
            //Arrange
            _hostEnvironment.Setup(x => x.ContentRootPath).Returns(Directory.GetCurrentDirectory());

            var validationRS = TestDataProvider.GetTestData<ValidationRS>("NDCOrder\\OrderCreate\\ValidationMSResponse");
            _mediator.Setup(x => x.Send(It.IsAny<ValidationMSCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(validationRS);

            var offerDoc = TestDataProvider.GetTestData<OfferResponse>("NDCOrder\\OrderCreate\\ValidatePaxOfferDoc");
            _mediator.Setup(x => x.Send(It.IsAny<OfferMSQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(offerDoc);

            var request = TestDataProvider.GetXmlTestData<IATA_OrderCreateRQ>("NDCOrder\\OrderCreate\\OrderCreateRQ");
            request.Request.PaymentFunctions = null;
            SelectedPricedOfferType[] SelectedPricedOfferTypeArray = new SelectedPricedOfferType[1];
            SelectedPricedOfferTypeArray[0] = ((AcceptSelectedQuotedOfferListType)request.Request.CreateOrder.Item).SelectedPricedOffer.First();
            SelectedPricedOfferTypeArray[0].TotalOfferPriceAmount.CurCode = "USD";
            ((AcceptSelectedQuotedOfferListType)request.Request.CreateOrder.Item).SelectedPricedOffer = SelectedPricedOfferTypeArray;

            //Act
            var result = await _handler.Handle(new OrderCreateCommand() { OrderCreateRQ = request }, new CancellationToken());

            //Assert
            var items = (ErrorType[])result.Value.Value.Items;
            Assert.Contains(items, x => x.Code == "727");
        }

        [Fact]
        public async Task When_InvalidTotalOfferPriceAmount_ValidateAmountForHoldBooking_ShouldThrowError()
        {
            //Arrange
            _hostEnvironment.Setup(x => x.ContentRootPath).Returns(Directory.GetCurrentDirectory());

            var validationRS = TestDataProvider.GetTestData<ValidationRS>("NDCOrder\\OrderCreate\\ValidationMSResponse");
            _mediator.Setup(x => x.Send(It.IsAny<ValidationMSCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(validationRS);

            var offerDoc = TestDataProvider.GetTestData<OfferResponse>("NDCOrder\\OrderCreate\\ValidatePaxOfferDoc");
            _mediator.Setup(x => x.Send(It.IsAny<OfferMSQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(offerDoc);

            var request = TestDataProvider.GetXmlTestData<IATA_OrderCreateRQ>("NDCOrder\\OrderCreate\\OrderCreateRQ");
            request.Request.PaymentFunctions.First().PaymentProcessingDetails.Amount.Value = 0M;
            SelectedPricedOfferType[] SelectedPricedOfferTypeArray = new SelectedPricedOfferType[1];
            SelectedPricedOfferTypeArray[0] = ((AcceptSelectedQuotedOfferListType)request.Request.CreateOrder.Item).SelectedPricedOffer.First();
            ((AcceptSelectedQuotedOfferListType)request.Request.CreateOrder.Item).SelectedPricedOffer = SelectedPricedOfferTypeArray;

            ConfigReader.Set("IFGAgentFopLookUpUrl", "https://Testresources.virginatlantic.com/DEV/validationmicroservice/IFGAgentFopLookUp");
            ConfigReader.Set("IFGAgentFOPLookUpSubscriptionKey", "Test_IFGAgentFOPLookUpSubscriptionKey");

            //Act
            var result = await _handler.Handle(new OrderCreateCommand() { OrderCreateRQ = request }, new CancellationToken());

            //Assert
            var items = (ErrorType[])result.Value.Value.Items;
            Assert.Contains(items, x => x.Code == "727");
        }

        [Fact]
        public async Task When_InvalidTotalOfferPriceAmount_ValidateAmount_ShouldThrowError()
        {
            //Arrange
            _hostEnvironment.Setup(x => x.ContentRootPath).Returns(Directory.GetCurrentDirectory());

            var validationRS = TestDataProvider.GetTestData<ValidationRS>("NDCOrder\\OrderCreate\\ValidationMSResponse");
            _mediator.Setup(x => x.Send(It.IsAny<ValidationMSCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(validationRS);

            var offerDoc = TestDataProvider.GetTestData<OfferResponse>("NDCOrder\\OrderCreate\\ValidatePaxOfferDoc");
            _mediator.Setup(x => x.Send(It.IsAny<OfferMSQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(offerDoc);

            var request = TestDataProvider.GetXmlTestData<IATA_OrderCreateRQ>("NDCOrder\\OrderCreate\\OrderCreateRQ");
            SelectedPricedOfferType[] SelectedPricedOfferTypeArray = new SelectedPricedOfferType[1];
            SelectedPricedOfferTypeArray[0] = ((AcceptSelectedQuotedOfferListType)request.Request.CreateOrder.Item).SelectedPricedOffer.First();
            ((AcceptSelectedQuotedOfferListType)request.Request.CreateOrder.Item).SelectedPricedOffer.First().TotalOfferPriceAmount.Value = 0M;

            //Act
            var result = await _handler.Handle(new OrderCreateCommand() { OrderCreateRQ = request }, new CancellationToken());

            //Assert
            var items = (ErrorType[])result.Value.Value.Items;
            Assert.Contains(items, x => x.Code == "727");
        }

        [Fact]
        public async Task When_CardholderAddressIsNull_Case1_ValidatePayerAddress_ShouldThrowError()
        {
            //Arrange
            _hostEnvironment.Setup(x => x.ContentRootPath).Returns(Directory.GetCurrentDirectory());

            var validationRS = TestDataProvider.GetTestData<ValidationRS>("NDCOrder\\OrderCreate\\ValidationMSResponse");
            _mediator.Setup(x => x.Send(It.IsAny<ValidationMSCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(validationRS);

            var offerDoc = TestDataProvider.GetTestData<OfferResponse>("NDCOrder\\OrderCreate\\ValidatePaxOfferDoc");
            _mediator.Setup(x => x.Send(It.IsAny<OfferMSQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(offerDoc);

            var request = TestDataProvider.GetXmlTestData<IATA_OrderCreateRQ>("NDCOrder\\OrderCreate\\OrderCreateRQ");
            ((PaymentProcessingPaymentCardType)request.Request.PaymentFunctions.First().PaymentProcessingDetails.PaymentMethod.Item).CardholderAddress.CountryCode = "";
            ((PaymentProcessingPaymentCardType)request.Request.PaymentFunctions.First().PaymentProcessingDetails.PaymentMethod.Item).CardholderAddress.CountrySubDivisionName = "";
            ((PaymentProcessingPaymentCardType)request.Request.PaymentFunctions.First().PaymentProcessingDetails.PaymentMethod.Item).CardholderAddress.PostalCode = "1234";
            ((PaymentProcessingPaymentCardType)request.Request.PaymentFunctions.First().PaymentProcessingDetails.PaymentMethod.Item).CardholderAddress.CityName = "";
            ((PaymentProcessingPaymentCardType)request.Request.PaymentFunctions.First().PaymentProcessingDetails.PaymentMethod.Item).CardholderAddress.StreetText = [];

            //Act
            var result = await _handler.Handle(new OrderCreateCommand() { OrderCreateRQ = request }, new CancellationToken());

            //Assert
            var items = (ErrorType[])result.Value.Value.Items;
            Assert.Contains(items, x => x.Code == "316");
        }

        [Fact]
        public async Task When_CardholderPostalCodeIsNull_Case1_ValidatePayerAddress_ShouldThrowError()
        {
            //Arrange
            _hostEnvironment.Setup(x => x.ContentRootPath).Returns(Directory.GetCurrentDirectory());

            var validationRS = TestDataProvider.GetTestData<ValidationRS>("NDCOrder\\OrderCreate\\ValidationMSResponse");
            _mediator.Setup(x => x.Send(It.IsAny<ValidationMSCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(validationRS);

            var offerDoc = TestDataProvider.GetTestData<OfferResponse>("NDCOrder\\OrderCreate\\ValidatePaxOfferDoc");
            _mediator.Setup(x => x.Send(It.IsAny<OfferMSQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(offerDoc);

            var request = TestDataProvider.GetXmlTestData<IATA_OrderCreateRQ>("NDCOrder\\OrderCreate\\OrderCreateRQ");
            ((PaymentProcessingPaymentCardType)request.Request.PaymentFunctions.First().PaymentProcessingDetails.PaymentMethod.Item).CardholderAddress.PostalCode = "";

            //Act
            var result = await _handler.Handle(new OrderCreateCommand() { OrderCreateRQ = request }, new CancellationToken());

            //Assert
            var items = (ErrorType[])result.Value.Value.Items;
            Assert.Contains(items, x => x.DescText == "Payment declined: Invalid Postal Code");
        }

        [Fact]
        public async Task When_CardholderStreetDetailsIsNull_Case1_ValidatePayerAddress_ShouldThrowError()
        {
            //Arrange
            _hostEnvironment.Setup(x => x.ContentRootPath).Returns(Directory.GetCurrentDirectory());

            var validationRS = TestDataProvider.GetTestData<ValidationRS>("NDCOrder\\OrderCreate\\ValidationMSResponse");
            _mediator.Setup(x => x.Send(It.IsAny<ValidationMSCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(validationRS);

            var offerDoc = TestDataProvider.GetTestData<OfferResponse>("NDCOrder\\OrderCreate\\ValidatePaxOfferDoc");
            _mediator.Setup(x => x.Send(It.IsAny<OfferMSQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(offerDoc);

            var request = TestDataProvider.GetXmlTestData<IATA_OrderCreateRQ>("NDCOrder\\OrderCreate\\OrderCreateRQ");
            ((PaymentProcessingPaymentCardType)request.Request.PaymentFunctions.First().PaymentProcessingDetails.PaymentMethod.Item).CardholderAddress.StreetText = [null];

            //Act
            var result = await _handler.Handle(new OrderCreateCommand() { OrderCreateRQ = request }, new CancellationToken());

            //Assert
            var items = (ErrorType[])result.Value.Value.Items;
            Assert.Contains(items, x => x.DescText == "Payment declined: Invalid Street details");
        }

        [Fact]
        public async Task When_CountryCodeIsNull_CardholderAddressIsNull_Case1_ValidatePayerAddress_ShouldThrowError()
        {
            //Arrange
            _hostEnvironment.Setup(x => x.ContentRootPath).Returns(Directory.GetCurrentDirectory());

            var validationRS = TestDataProvider.GetTestData<ValidationRS>("NDCOrder\\OrderCreate\\ValidationMSResponse");
            _mediator.Setup(x => x.Send(It.IsAny<ValidationMSCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(validationRS);

            var offerDoc = TestDataProvider.GetTestData<OfferResponse>("NDCOrder\\OrderCreate\\ValidatePaxOfferDoc");
            _mediator.Setup(x => x.Send(It.IsAny<OfferMSQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(offerDoc);

            var request = TestDataProvider.GetXmlTestData<IATA_OrderCreateRQ>("NDCOrder\\OrderCreate\\OrderCreateRQ");
            ((PaymentProcessingPaymentCardType)request.Request.PaymentFunctions.First().PaymentProcessingDetails.PaymentMethod.Item).CardholderAddress.CountryCode = " ";
            ((PaymentProcessingPaymentCardType)request.Request.PaymentFunctions.First().PaymentProcessingDetails.PaymentMethod.Item).CardholderAddress.CityName = null;

            ConfigReader.Set("IFGAgentFopLookUpUrl", "https://Testresources.virginatlantic.com/DEV/validationmicroservice/IFGAgentFopLookUp");
            ConfigReader.Set("IFGAgentFOPLookUpSubscriptionKey", "Test_IFGAgentFOPLookUpSubscriptionKey");

            //Act
            var result = await _handler.Handle(new OrderCreateCommand() { OrderCreateRQ = request }, new CancellationToken());

            //Assert
            var items = (ErrorType[])result.Value.Value.Items;
            Assert.Contains(items, x => x.Code == "316");
        }

        [Fact]
        public async Task When_CardholderAddressIsNull_Case2_ValidatePayerAddress_ShouldThrowError()
        {
            //Arrange
            _hostEnvironment.Setup(x => x.ContentRootPath).Returns(Directory.GetCurrentDirectory());

            var validationRS = TestDataProvider.GetTestData<ValidationRS>("NDCOrder\\OrderCreate\\ValidationMSResponse");
            _mediator.Setup(x => x.Send(It.IsAny<ValidationMSCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(validationRS);

            var offerDoc = TestDataProvider.GetTestData<OfferResponse>("NDCOrder\\OrderCreate\\ValidatePaxOfferDoc");
            _mediator.Setup(x => x.Send(It.IsAny<OfferMSQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(offerDoc);

            var request = TestDataProvider.GetXmlTestData<IATA_OrderCreateRQ>("NDCOrder\\OrderCreate\\OrderCreateRQ");
            ((PaymentProcessingPaymentCardType)request.Request.PaymentFunctions.First().PaymentProcessingDetails.PaymentMethod.Item).CardholderAddress.CityName = "";
            ((PaymentProcessingPaymentCardType)request.Request.PaymentFunctions.First().PaymentProcessingDetails.PaymentMethod.Item).CardholderAddress.StreetText = [];

            ConfigReader.Set("IFGAgentFopLookUpUrl", "https://Testresources.virginatlantic.com/DEV/validationmicroservice/IFGAgentFopLookUp");
            ConfigReader.Set("IFGAgentFOPLookUpSubscriptionKey", "Test_IFGAgentFOPLookUpSubscriptionKey");

            //Act
            var result = await _handler.Handle(new OrderCreateCommand() { OrderCreateRQ = request }, new CancellationToken());

            //Assert
            var items = (ErrorType[])result.Value.Value.Items;
            Assert.Contains(items, x => x.Code == "316");
        }

        [Fact]
        public async Task When_DuplicatePaxRefID_ValidateDuplicatePaxID_ShouldThrowError()
        {
            //Arrange
            _hostEnvironment.Setup(x => x.ContentRootPath).Returns(Directory.GetCurrentDirectory());

            var validationRS = TestDataProvider.GetTestData<ValidationRS>("NDCOrder\\OrderCreate\\ValidationMSResponse");
            _mediator.Setup(x => x.Send(It.IsAny<ValidationMSCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(validationRS);

            var offerDoc = TestDataProvider.GetTestData<OfferResponse>("NDCOrder\\OrderCreate\\ValidatePaxOfferDoc");
            _mediator.Setup(x => x.Send(It.IsAny<OfferMSQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(offerDoc);

            var request = TestDataProvider.GetXmlTestData<IATA_OrderCreateRQ>("NDCOrder\\OrderCreate\\OrderCreateRQ");
            string[] paxRefArray = new string[] { "ADULT_1", "ADULT_1" };
            ((AcceptSelectedQuotedOfferListType)request.Request.CreateOrder.Item).SelectedPricedOffer.Last().SelectedOfferItem.First().PaxRefID = paxRefArray;

            //Act
            var result = await _handler.Handle(new OrderCreateCommand() { OrderCreateRQ = request }, new CancellationToken());

            //Assert
            var items = (ErrorType[])result.Value.Value.Items;
            Assert.Contains(items, x => x.Code == "314");
        }

        [Fact]
        public async Task When_EmailIdIsNull_ValidateAgentForHoldBooking_ShouldThrowError()
        {
            //Arrange
            _hostEnvironment.Setup(x => x.ContentRootPath).Returns(Directory.GetCurrentDirectory());

            var validationRS = TestDataProvider.GetTestData<ValidationRS>("NDCOrder\\OrderCreate\\ValidationMSResponse");
            _mediator.Setup(x => x.Send(It.IsAny<ValidationMSCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(validationRS);

            var offerDoc = TestDataProvider.GetTestData<OfferResponse>("NDCOrder\\OrderChange\\OfferDoc");
            _mediator.Setup(x => x.Send(It.IsAny<OfferMSQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(offerDoc);

            var request = TestDataProvider.GetXmlTestData<IATA_OrderCreateRQ>("NDCOrder\\OrderCreate\\OrderCreateRQ");
            request.DistributionChain.First().ContactInfo.EmailAddress.First().EmailAddressText = null;

            ConfigReader.Set("IFGAgentFopLookUpUrl", "https://Testresources.virginatlantic.com/DEV/validationmicroservice/IFGAgentFopLookUp");
            ConfigReader.Set("IFGAgentFOPLookUpSubscriptionKey", "Test_IFGAgentFOPLookUpSubscriptionKey");

            //Act
            var result = await _handler.Handle(new OrderCreateCommand() { OrderCreateRQ = request }, new CancellationToken());

            //Assert
            var items = (ErrorType[])result.Value.Value.Items;
            Assert.Contains(items, x => x.Code == "368");
        }

        [Fact]
        public async Task When_EmailLengthExceeds_ValidateAgentForHoldBooking_ShouldThrowError()
        {
            //Arrange
            _hostEnvironment.Setup(x => x.ContentRootPath).Returns(Directory.GetCurrentDirectory());

            var validationRS = TestDataProvider.GetTestData<ValidationRS>("NDCOrder\\OrderCreate\\ValidationMSResponse");
            _mediator.Setup(x => x.Send(It.IsAny<ValidationMSCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(validationRS);

            var offerDoc = TestDataProvider.GetTestData<OfferResponse>("NDCOrder\\OrderChange\\OfferDoc");
            _mediator.Setup(x => x.Send(It.IsAny<OfferMSQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(offerDoc);

            var request = TestDataProvider.GetXmlTestData<IATA_OrderCreateRQ>("NDCOrder\\OrderCreate\\OrderCreateRQ");
            request.DistributionChain.First().ContactInfo.EmailAddress.First().EmailAddressText = "EmailLengthIsGreaterThenSixtyFiveItShoulThrowErrorToValidateThisScenario@Email.Com";

            //Act
            var result = await _handler.Handle(new OrderCreateCommand() { OrderCreateRQ = request }, new CancellationToken());

            //Assert
            var items = (ErrorType[])result.Value.Value.Items;
            Assert.Contains(items, x => x.Code == "317");
        }

        [Fact]
        public async Task When_EmailIsInvalid_ValidateAgentForHoldBooking_ShouldThrowError()
        {
            //Arrange
            _hostEnvironment.Setup(x => x.ContentRootPath).Returns(Directory.GetCurrentDirectory());

            var validationRS = TestDataProvider.GetTestData<ValidationRS>("NDCOrder\\OrderCreate\\ValidationMSResponse");
            _mediator.Setup(x => x.Send(It.IsAny<ValidationMSCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(validationRS);

            var offerDoc = TestDataProvider.GetTestData<OfferResponse>("NDCOrder\\OrderChange\\OfferDoc");
            _mediator.Setup(x => x.Send(It.IsAny<OfferMSQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(offerDoc);

            var request = TestDataProvider.GetXmlTestData<IATA_OrderCreateRQ>("NDCOrder\\OrderCreate\\OrderCreateRQ");
            request.DistributionChain.First().ContactInfo.EmailAddress.First().EmailAddressText = "123456";

            ConfigReader.Set("IFGAgentFopLookUpUrl", "https://Testresources.virginatlantic.com/DEV/validationmicroservice/IFGAgentFopLookUp");
            ConfigReader.Set("IFGAgentFOPLookUpSubscriptionKey", "Test_IFGAgentFOPLookUpSubscriptionKey");

            //Act
            var result = await _handler.Handle(new OrderCreateCommand() { OrderCreateRQ = request }, new CancellationToken());

            //Assert
            var items = (ErrorType[])result.Value.Value.Items;
            Assert.Contains(items, x => x.Code == "317");
        }

        [Fact]
        public async Task When_ContactInfoIDIsNull_ValidateAgentForHoldBooking_ShouldThrowError()
        {
            //Arrange
            _hostEnvironment.Setup(x => x.ContentRootPath).Returns(Directory.GetCurrentDirectory());

            var validationRS = TestDataProvider.GetTestData<ValidationRS>("NDCOrder\\OrderCreate\\ValidationMSResponse");
            _mediator.Setup(x => x.Send(It.IsAny<ValidationMSCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(validationRS);

            var offerDoc = TestDataProvider.GetTestData<OfferResponse>("NDCOrder\\OrderChange\\OfferDoc");
            _mediator.Setup(x => x.Send(It.IsAny<OfferMSQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(offerDoc);

            var request = TestDataProvider.GetXmlTestData<IATA_OrderCreateRQ>("NDCOrder\\OrderCreate\\OrderCreateRQ");
            request.DistributionChain.First().ContactInfo.ContactInfoID = null;
            ConfigReader.Set("IFGAgentFopLookUpUrl", "https://Testresources.virginatlantic.com/DEV/validationmicroservice/IFGAgentFopLookUp");
            ConfigReader.Set("IFGAgentFOPLookUpSubscriptionKey", "Test_IFGAgentFOPLookUpSubscriptionKey");

            //Act
            var result = await _handler.Handle(new OrderCreateCommand() { OrderCreateRQ = request }, new CancellationToken());

            //Assert
            var items = (ErrorType[])result.Value.Value.Items;
            Assert.Contains(items, x => x.Code == "912");
        }

        [Fact]
        public async Task When_AgentDoesNotMatch_ValidateAgentForHoldBooking_ShouldThrowError()
        {
            //Arrange
            _hostEnvironment.Setup(x => x.ContentRootPath).Returns(Directory.GetCurrentDirectory());

            var validationRS = TestDataProvider.GetTestData<ValidationRS>("NDCOrder\\OrderCreate\\ValidationMSResponse");
            _mediator.Setup(x => x.Send(It.IsAny<ValidationMSCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(validationRS);

            var offerDoc = TestDataProvider.GetTestData<OfferResponse>("NDCOrder\\OrderChange\\OfferDoc");
            _mediator.Setup(x => x.Send(It.IsAny<OfferMSQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(offerDoc);

            var request = TestDataProvider.GetXmlTestData<IATA_OrderCreateRQ>("NDCOrder\\OrderCreate\\OrderCreateRQ_DL");
            offerDoc.Parties.Participants.First().PartyType = (Order.Infrastructure.Model.Participant.PartyTypeEnum?)15;
            offerDoc.Parties.Participants.First().AgencyID = "91281030";

            ConfigReader.Set("IFGAgentFopLookUpUrl", "https://Testresources.virginatlantic.com/DEV/validationmicroservice/IFGAgentFopLookUp");
            ConfigReader.Set("IFGAgentFOPLookUpSubscriptionKey", "Test_IFGAgentFOPLookUpSubscriptionKey");

            //Act
            var result = await _handler.Handle(new OrderCreateCommand() { OrderCreateRQ = request }, new CancellationToken());

            //Assert
            var items = (ErrorType[])result.Value.Value.Items;
            Assert.Contains(items, x => x.Code == "368");
        }

        [Fact]
        public async Task When_KeyTypeIsDirectCase1_ValidateAgent_ShouldThrowError()
        {
            //Arrange
            _hostEnvironment.Setup(x => x.ContentRootPath).Returns(Directory.GetCurrentDirectory());

            var validationRS = TestDataProvider.GetTestData<ValidationRS>("NDCOrder\\OrderCreate\\ValidationMSResponse");
            _mediator.Setup(x => x.Send(It.IsAny<ValidationMSCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(validationRS);

            var offerDoc = TestDataProvider.GetTestData<OfferResponse>("NDCOrder\\OrderChange\\OfferDoc");
            _mediator.Setup(x => x.Send(It.IsAny<OfferMSQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(offerDoc);

            var request = TestDataProvider.GetXmlTestData<IATA_OrderCreateRQ>("NDCOrder\\OrderCreate\\OrderCreateRQ_DL");
            request.DistributionChain.First().ParticipatingOrg.OrgID = "123456";

            //Act
            var result = await _handler.Handle(new OrderCreateCommand() { OrderCreateRQ = request }, new CancellationToken());

            //Assert
            var items = (ErrorType[])result.Value.Value.Items;
            Assert.Contains(items, x => x.Code == "368");
        }

        [Fact]
        public async Task When_KeyTypeIsNotDirectCase2_ValidateAgent_ShouldThrowError()
        {
            //Arrange
            _hostEnvironment.Setup(x => x.ContentRootPath).Returns(Directory.GetCurrentDirectory());

            var validationRS = TestDataProvider.GetTestData<ValidationRS>("NDCOrder\\OrderCreate\\ValidationMSResponse");
            validationRS.ValidateResponse.KeyType = "INDIRECT";
            _mediator.Setup(x => x.Send(It.IsAny<ValidationMSCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(validationRS);

            var offerDoc = TestDataProvider.GetTestData<OfferResponse>("NDCOrder\\OrderChange\\OfferDoc");
            _mediator.Setup(x => x.Send(It.IsAny<OfferMSQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(offerDoc);

            var request = TestDataProvider.GetXmlTestData<IATA_OrderCreateRQ>("NDCOrder\\OrderCreate\\OrderCreateRQ");
            request.DistributionChain.First().ParticipatingOrg.OrgID = "123456";

            //Act
            var result = await _handler.Handle(new OrderCreateCommand() { OrderCreateRQ = request }, new CancellationToken());

            //Assert
            var items = (ErrorType[])result.Value.Value.Items;
            Assert.Contains(items, x => x.Code == "368");
        }

        [Fact]
        public async Task When_OfferDocNull_ValidateOffer_ShouldThrowError()
        {
            //Arrange
            _hostEnvironment.Setup(x => x.ContentRootPath).Returns(Directory.GetCurrentDirectory());

            _mediator.Setup(x => x.Send(It.IsAny<ValidationMSCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(new ValidationRS());

            var request = TestDataProvider.GetXmlTestData<IATA_OrderCreateRQ>("NDCOrder\\OrderCreate\\OrderCreateRQ");

            ConfigReader.Set("IFGAgentFopLookUpUrl", "https://Testresources.virginatlantic.com/DEV/validationmicroservice/IFGAgentFopLookUp");
            ConfigReader.Set("IFGAgentFOPLookUpSubscriptionKey", "Test_IFGAgentFOPLookUpSubscriptionKey");

            

            //Act
            var result = await _handler.Handle(new OrderCreateCommand() { OrderCreateRQ = request }, new CancellationToken());

            //Assert
            var items = (ErrorType[])result.Value.Value.Items;
            Assert.Contains(items, x => x.Code == "913");
        }

        [Fact]
        public async Task When_OwnerCodeIsNull_ValidateOffer_ShouldThrowError()
        {
            //Arrange
            _hostEnvironment.Setup(x => x.ContentRootPath).Returns(Directory.GetCurrentDirectory());

            var validationRS = TestDataProvider.GetTestData<ValidationRS>("NDCOrder\\OrderCreate\\ValidationMSResponse");
            _mediator.Setup(x => x.Send(It.IsAny<ValidationMSCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(validationRS);

            var offerDoc = TestDataProvider.GetTestData<OfferResponse>("NDCOrder\\OrderChange\\OfferDoc");
            _mediator.Setup(x => x.Send(It.IsAny<OfferMSQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(offerDoc);

            var request = TestDataProvider.GetXmlTestData<IATA_OrderCreateRQ>("NDCOrder\\OrderCreate\\OrderCreateRQ");
            ((AcceptSelectedQuotedOfferListType)request.Request.CreateOrder.Item).SelectedPricedOffer.First().OwnerCode = string.Empty;

            //Act
            var result = await _handler.Handle(new OrderCreateCommand() { OrderCreateRQ = request }, new CancellationToken());

            //Assert
            var items = (ErrorType[])result.Value.Value.Items;
            Assert.Contains(items, x => x.Code == "912");
        }

        [Fact]
        public async Task When_OwnerCodeLengthExceedsTwo_ValidateOffer_ShouldThrowError()
        {
            //Arrange
            _hostEnvironment.Setup(x => x.ContentRootPath).Returns(Directory.GetCurrentDirectory());

            var validationRS = TestDataProvider.GetTestData<ValidationRS>("NDCOrder\\OrderCreate\\ValidationMSResponse");
            _mediator.Setup(x => x.Send(It.IsAny<ValidationMSCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(validationRS);

            var offerDoc = TestDataProvider.GetTestData<OfferResponse>("NDCOrder\\OrderChange\\OfferDoc");
            _mediator.Setup(x => x.Send(It.IsAny<OfferMSQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(offerDoc);

            var request = TestDataProvider.GetXmlTestData<IATA_OrderCreateRQ>("NDCOrder\\OrderCreate\\OrderCreateRQ");
            ((AcceptSelectedQuotedOfferListType)request.Request.CreateOrder.Item).SelectedPricedOffer.First().OwnerCode = "VS1";

            ConfigReader.Set("IFGAgentFopLookUpUrl", "https://Testresources.virginatlantic.com/DEV/validationmicroservice/IFGAgentFopLookUp");
            ConfigReader.Set("IFGAgentFOPLookUpSubscriptionKey", "Test_IFGAgentFOPLookUpSubscriptionKey");

            //Act
            var result = await _handler.Handle(new OrderCreateCommand() { OrderCreateRQ = request }, new CancellationToken());

            //Assert
            var items = (ErrorType[])result.Value.Value.Items;
            Assert.Contains(items, x => x.Code == "727");
        }

        [Fact]
        public async Task When_SelectedOfferItem_PaxRefIDIsNull_OfferMismatch_ValidateOffer_ShouldThrowError()
        {
            //Arrange
            _hostEnvironment.Setup(x => x.ContentRootPath).Returns(Directory.GetCurrentDirectory());

            var validationRS = TestDataProvider.GetTestData<ValidationRS>("NDCOrder\\OrderCreate\\ValidationMSResponse");
            _mediator.Setup(x => x.Send(It.IsAny<ValidationMSCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(validationRS);

            var offerDoc = TestDataProvider.GetTestData<OfferResponse>("NDCOrder\\OrderChange\\OfferDoc");
            _mediator.Setup(x => x.Send(It.IsAny<OfferMSQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(offerDoc);

            ConfigReader.Set("IFGAgentFopLookUpUrl", "https://Testresources.virginatlantic.com/DEV/validationmicroservice/IFGAgentFopLookUp");
            ConfigReader.Set("IFGAgentFOPLookUpSubscriptionKey", "Test_IFGAgentFOPLookUpSubscriptionKey");

            var request = TestDataProvider.GetXmlTestData<IATA_OrderCreateRQ>("NDCOrder\\OrderCreate\\OrderCreateRQ");
            string[] paxRefArray = new string[] { "" };
            ((AcceptSelectedQuotedOfferListType)request.Request.CreateOrder.Item).SelectedPricedOffer.First().SelectedOfferItem.First().PaxRefID = paxRefArray;

            //Act
            var result = await _handler.Handle(new OrderCreateCommand() { OrderCreateRQ = request }, new CancellationToken());

            //Assert
            var items = (ErrorType[])result.Value.Value.Items;
            Assert.Contains(items, x => x.Code == "913");
        }

        [Fact]
        public async Task When_PaxListMismatch_ValidateOffer_ShouldThrowError()
        {
            //Arrange
            _hostEnvironment.Setup(x => x.ContentRootPath).Returns(Directory.GetCurrentDirectory());

            var validationRS = TestDataProvider.GetTestData<ValidationRS>("NDCOrder\\OrderCreate\\ValidationMSResponse");
            _mediator.Setup(x => x.Send(It.IsAny<ValidationMSCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(validationRS);

            var offerDoc = TestDataProvider.GetTestData<OfferResponse>("NDCOrder\\OrderChange\\OfferDoc");
            _mediator.Setup(x => x.Send(It.IsAny<OfferMSQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(offerDoc);

            var request = TestDataProvider.GetXmlTestData<IATA_OrderCreateRQ>("NDCOrder\\OrderCreate\\OrderCreateRQ");
            Domain.OrderCreateRQ.PaxType[] paxList = { new Domain.OrderCreateRQ.PaxType { PaxID = "ADULT_1", PTC = "ADT" }, new Domain.OrderCreateRQ.PaxType { PaxID = "ADULT_1", PTC = "ADT" } };
            request.Request.DataLists.PaxList = paxList;

            //Act
            var result = await _handler.Handle(new OrderCreateCommand() { OrderCreateRQ = request }, new CancellationToken());

            //Assert
            var items = (ErrorType[])result.Value.Value.Items;
            Assert.Contains(items, x => x.Code == "914");
        }

        [Fact]
        public async Task When_PaxListPaxIDIsNull_OfferMismatch_ValidateOffer_ShouldThrowError()
        {
            //Arrange
            _hostEnvironment.Setup(x => x.ContentRootPath).Returns(Directory.GetCurrentDirectory());

            var validationRS = TestDataProvider.GetTestData<ValidationRS>("NDCOrder\\OrderCreate\\ValidationMSResponse");
            _mediator.Setup(x => x.Send(It.IsAny<ValidationMSCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(validationRS);

            var offerDoc = TestDataProvider.GetTestData<OfferResponse>("NDCOrder\\OrderChange\\OfferDoc");
            _mediator.Setup(x => x.Send(It.IsAny<OfferMSQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(offerDoc);

            var request = TestDataProvider.GetXmlTestData<IATA_OrderCreateRQ>("NDCOrder\\OrderCreate\\OrderCreateRQ");
            request.Request.DataLists.PaxList.First().PaxID = null;
            //Act
            var result = await _handler.Handle(new OrderCreateCommand() { OrderCreateRQ = request }, new CancellationToken());

            //Assert
            var items = (ErrorType[])result.Value.Value.Items;
            Assert.Contains(items, x => x.Code == "912");
        }

        [Fact]
        public async Task When_OfferMismatch_ValidateOffer_ShouldThrowError()
        {
            //Arrange
            _hostEnvironment.Setup(x => x.ContentRootPath).Returns(Directory.GetCurrentDirectory());

            var validationRS = TestDataProvider.GetTestData<ValidationRS>("NDCOrder\\OrderCreate\\ValidationMSResponse");
            _mediator.Setup(x => x.Send(It.IsAny<ValidationMSCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(validationRS);

            var offerDoc = TestDataProvider.GetTestData<OfferResponse>("NDCOrder\\OrderChange\\OfferDoc");
            _mediator.Setup(x => x.Send(It.IsAny<OfferMSQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(offerDoc);

            ConfigReader.Set("IFGAgentFopLookUpUrl", "https://Testresources.virginatlantic.com/DEV/validationmicroservice/IFGAgentFopLookUp");
            ConfigReader.Set("IFGAgentFOPLookUpSubscriptionKey", "Test_IFGAgentFOPLookUpSubscriptionKey");

            var request = TestDataProvider.GetXmlTestData<IATA_OrderCreateRQ>("NDCOrder\\OrderCreate\\OrderCreateRQ");

            //Act
            var result = await _handler.Handle(new OrderCreateCommand() { OrderCreateRQ = request }, new CancellationToken());

            //Assert
            var items = (ErrorType[])result.Value.Value.Items;
            Assert.Contains(items, x => x.Code == "913");
        }

        [Fact]
        public async Task When_EmployerNameAndSalesTaxRegistrationIDIsNull_ValidateGSTNumberandOragName_ShouldThrowWarning()
        {
            //Arrange
            _hostEnvironment.Setup(x => x.ContentRootPath).Returns(Directory.GetCurrentDirectory());

            //Arrange
            var request = TestDataProvider.GetXmlTestData<IATA_OrderCreateRQ>("NDCOrder\\OrderCreate\\GSTValidation\\OrderCreate");
            request.Request.PaymentFunctions = null;
            var validationRS = TestDataProvider.GetTestData<ValidationRS>("NDCOrder\\OrderCreate\\GSTValidation\\ValidationRS");
            var offerDoc = TestDataProvider.GetTestData<OfferResponse>("NDCOrder\\OrderCreate\\GSTValidation\\OfferDoc");
            var brandMsRs = TestDataProvider.GetTestData<BrandRS>("NDCOrder\\OrderCreate\\PositiveFlow\\BrandMSRS");
            var orderDoc = TestDataProvider.GetTestData<OrderRS>("NDCOrder\\OrderCreate\\GSTValidation\\OrderDoc");
            var orderViewRS = TestDataProvider.GetXmlTestData<IATA_OrderViewRS>("NDCOrder\\OrderCreate\\GSTValidation\\OrderViewRS");
            var airportDetails = TestDataProvider.GetTestData<IEnumerable<AirportDetail>>("NDCOrder\\OrderCreate\\PositiveFlow\\AirportDetails");

            _mediator.Setup(x => x.Send(It.IsAny<ValidationMSCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(validationRS);
            _mediator.Setup(x => x.Send(It.IsAny<OfferMSQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(offerDoc);
            _mediator.Setup(x => x.Send(It.IsAny<BrandMSCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(brandMsRs);
            _mediator.Setup(x => x.Send(It.IsAny<OrderMSCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(orderDoc);
            _mediator.Setup(x => x.Send(It.IsAny<AirportDetailsQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(airportDetails);

            ConfigReader.Set("PublishOrderDocUrl", "https://Testresources.virginatlantic.com/dev/NdcOrderSearch/PublishOrderDoc");
            ConfigReader.Set("OrderSearchMSSubscriptionKey", "Test_OrderSearchMSSubscriptionKey");
            ConfigReader.Set("EmailMSSendOrderEmailUrl", "https://Testresources.virginatlantic.com/dev/emailmicroservice/SendOrderEmail");
            ConfigReader.Set("emailmssubscriptionkey", "Test_emailmssubscriptionkey");

            request.Request.DataLists.PaxList.First().Employer.Name = null;
            request.Request.DataLists.PaxList.First().Employer.SalesTaxRegistration.First().SalesTaxRegistrationID = null;

            //Act
            var result = await _handler.Handle(new OrderCreateCommand() { OrderCreateRQ = request }, new CancellationToken());

            //Assert
            var items = (((VAA.EAPI.NDC.Order.Domain.OrderViewRS.OrderViewResponseType)result.Value.Value.Items[0]).Warning);
            Assert.Contains(items, x => x.DescText == "GST Details Validation - Organaization Name in the request");
            Assert.Contains(items, x => x.DescText == "GST Details Validation - GST Number in the request");
        }

        [Fact]
        public async Task When_Validate_ValidateGSTPhoneEmailAddress_ShouldThrowWarning()
        {
            //Arrange
            _hostEnvironment.Setup(x => x.ContentRootPath).Returns(Directory.GetCurrentDirectory());

            //Arrange
            var request = TestDataProvider.GetXmlTestData<IATA_OrderCreateRQ>("NDCOrder\\OrderCreate\\GSTValidation\\OrderCreate");
            request.Request.PaymentFunctions = null;
            var validationRS = TestDataProvider.GetTestData<ValidationRS>("NDCOrder\\OrderCreate\\GSTValidation\\ValidationRS");
            var offerDoc = TestDataProvider.GetTestData<OfferResponse>("NDCOrder\\OrderCreate\\GSTValidation\\OfferDoc");
            var brandMsRs = TestDataProvider.GetTestData<BrandRS>("NDCOrder\\OrderCreate\\PositiveFlow\\BrandMSRS");
            var orderDoc = TestDataProvider.GetTestData<OrderRS>("NDCOrder\\OrderCreate\\GSTValidation\\OrderDoc");
            var orderViewRS = TestDataProvider.GetXmlTestData<IATA_OrderViewRS>("NDCOrder\\OrderCreate\\GSTValidation\\OrderViewRS");
            var airportDetails = TestDataProvider.GetTestData<IEnumerable<AirportDetail>>("NDCOrder\\OrderCreate\\PositiveFlow\\AirportDetails");

            _mediator.Setup(x => x.Send(It.IsAny<ValidationMSCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(validationRS);
            _mediator.Setup(x => x.Send(It.IsAny<OfferMSQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(offerDoc);
            _mediator.Setup(x => x.Send(It.IsAny<BrandMSCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(brandMsRs);
            _mediator.Setup(x => x.Send(It.IsAny<OrderMSCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(orderDoc);
            _mediator.Setup(x => x.Send(It.IsAny<AirportDetailsQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(airportDetails);

            ConfigReader.Set("PublishOrderDocUrl", "https://Testresources.virginatlantic.com/dev/NdcOrderSearch/PublishOrderDoc");
            ConfigReader.Set("OrderSearchMSSubscriptionKey", "Test_OrderSearchMSSubscriptionKey");
            ConfigReader.Set("EmailMSSendOrderEmailUrl", "https://Testresources.virginatlantic.com/dev/emailmicroservice/SendOrderEmail");
            ConfigReader.Set("emailmssubscriptionkey", "Test_emailmssubscriptionkey");


            request.Request.DataLists.PaxList.First().Employer.SalesTaxRegistration.First().EmailAddressText = null;
            request.Request.DataLists.PaxList.First().Employer.SalesTaxRegistration.First().PhoneNumber = ["asdas8e4whsdarf9", "asduiewr8ashuasda"];

            //Act
            var result = await _handler.Handle(new OrderCreateCommand() { OrderCreateRQ = request }, new CancellationToken());

            //Assert
            var items = (((VAA.EAPI.NDC.Order.Domain.OrderViewRS.OrderViewResponseType)result.Value.Value.Items[0]).Warning);
            Assert.Contains(items, x => x.DescText == "GST Details Validation - Invalid E-Mail details in the request");
            Assert.Contains(items, x => x.DescText == "GST Details Validation - Invalid Phone number in the request");

        }

        [Fact(Skip = "To be modified as per sabre implementation")]
        public async Task When_IdentityDocTypeCodeDuplicate_ValidateAPISDetails_ShouldThrowWarning()
        {
            //Arrange
            _hostEnvironment.Setup(x => x.ContentRootPath).Returns(Directory.GetCurrentDirectory());

            var offerDoc = TestDataProvider.GetTestData<OfferResponse>("NDCOrder\\OrderChange\\OfferDoc");
            _mediator.Setup(x => x.Send(It.IsAny<OfferMSQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(offerDoc);

            var response = new IATA_OrderViewRS()
            {
                Items = new List<WarningType>() { new() { Code = "914_16" } }.ToArray()
            };

            var validationRS = TestDataProvider.GetTestData<ValidationRS>("NDCOrder\\OrderCreate\\ValidationMSResponse");
            _mediator.Setup(x => x.Send(It.IsAny<ValidationMSCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(validationRS);

            var request = TestDataProvider.GetXmlTestData<IATA_OrderCreateRQ>("NDCOrder\\OrderCreate\\OrderCreateRQ");
            ((AcceptSelectedQuotedOfferListType)request.Request.CreateOrder.Item).SelectedPricedOffer.First().OfferRefID = offerDoc.Offers.First().OfferId;
            ((AcceptSelectedQuotedOfferListType)request.Request.CreateOrder.Item).SelectedPricedOffer.First().SelectedOfferItem.First().OfferItemRefID = offerDoc.Offers.First().OfferItems.First().OfferItemId;
            var offerPrice = Convert.ToDecimal(offerDoc.Offers.First().SalePrice.First().Amount.MonetaryAmount);
            ((AcceptSelectedQuotedOfferListType)request.Request.CreateOrder.Item).SelectedPricedOffer.First().TotalOfferPriceAmount.Value = offerPrice;
            request.Request.PaymentFunctions.First().PaymentProcessingDetails.Amount.Value = offerPrice;

            request.Request.DataLists.PaxList.First().IdentityDoc = request.Request.DataLists.PaxList.First().IdentityDoc.Append(new Domain.OrderCreateRQ.IdentityDocType() { IdentityDocTypeCode = Constants.Application.GC }).ToArray();

            //Act
            var result = await _handler.Handle(new OrderCreateCommand() { OrderCreateRQ = request }, new CancellationToken());

            //Assert
            var items = (WarningType[])result.Value.Value.Items;
            Assert.Contains(items, x => x.Code == "914_16");
        }

        [Fact(Skip = "To be updated as part of latest sabre implementation")]
        public async Task When_InvalidAPISDetails_2ADT1CHD1GBEReturn_ValidateAPISDetails_ShouldThrowWarning()
        {
            //Arrange
            _hostEnvironment.Setup(x => x.ContentRootPath).Returns(Directory.GetCurrentDirectory());

            _mediator.Setup(x => x.Send(It.IsAny<ValidationMSCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(new ValidationRS());

            var response = new IATA_OrderViewRS()
            {
                Items = new List<WarningType>() { new() { Code = "914_16" } }.ToArray()
            };

            var offerDoc = TestDataProvider.GetTestData<OfferResponse>("NDCOrder\\OrderCreate\\OfferDoc_Return_2ADT1CHD1GBE");
            _mediator.Setup(x => x.Send(It.IsAny<OfferMSQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(offerDoc);

            var validationRS = TestDataProvider.GetTestData<ValidationRS>("NDCOrder\\OrderCreate\\ValidationMSResponse");
            _mediator.Setup(x => x.Send(It.IsAny<ValidationMSCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(validationRS);

            var request = TestDataProvider.GetXmlTestData<IATA_OrderCreateRQ>("NDCOrder\\OrderCreate\\OrderCreate_Return_2ADT1CHD1GBE");

            foreach (var pax in request.Request.DataLists.PaxList)
            {
                pax.RedressCase.First().RedressCaseID = "ab3_12.785";
                pax.RedressCase.First().CountryCode = "1234";
                pax.IdentityDoc.First().IdentityDocID = "1234!@";
                pax.IdentityDoc.First().GivenName = new string[] { "123" };
                pax.IdentityDoc.First().Surname = "1234";
                pax.IdentityDoc.First().ResidenceCountryCode = "1country";
                pax.IdentityDoc.First().IssuingCountryCode = "1234Code";
                pax.IdentityDoc.First().Visa.First().VisaID = "abc@4590!";
                pax.IdentityDoc.First().Visa.First().HostCountryCode = "1country";
                pax.IdentityDoc.First().IssueDate = DateTime.Today.AddDays(1);
                pax.IdentityDoc.First().ExpiryDate = DateTime.Today.AddDays(-1);
            }

            //Act
            var result = await _handler.Handle(new OrderCreateCommand() { OrderCreateRQ = request }, new CancellationToken());

            //Assert
            var items = (WarningType[])result.Value.Value.Items;
            Assert.Contains(items, x => x.Code == "914_16");
        }

        [Fact(Skip = "To be updated as part of latest sabre implementation")]
        public async Task When_InvalidAPISDetails_2ADT1CHD1GBE1INFReturn_ValidateAPISDetails_ShouldThrowWarning()
        {
            //Arrange
            _hostEnvironment.Setup(x => x.ContentRootPath).Returns(Directory.GetCurrentDirectory());

            _mediator.Setup(x => x.Send(It.IsAny<ValidationMSCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(new ValidationRS());

            var response = new IATA_OrderViewRS()
            {
                Items = new List<WarningType>() { new() { Code = "914_16" } }.ToArray()
            };

            var offerDoc = TestDataProvider.GetTestData<OfferResponse>("NDCOrder\\OrderCreate\\OfferDoc_Return_2ADT1CHD1GBE1INF");
            _mediator.Setup(x => x.Send(It.IsAny<OfferMSQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(offerDoc);

            var validationRS = TestDataProvider.GetTestData<ValidationRS>("NDCOrder\\OrderCreate\\ValidationMSResponse");
            _mediator.Setup(x => x.Send(It.IsAny<ValidationMSCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(validationRS);

            var request = TestDataProvider.GetXmlTestData<IATA_OrderCreateRQ>("NDCOrder\\OrderCreate\\OrderCreateRQ_Return_2ADT1CHD1GBE1INF");

            foreach (var pax in request.Request.DataLists.PaxList)
            {
                pax.RedressCase.First().RedressCaseID = "ab3_12.785";
                pax.RedressCase.First().CountryCode = "1234";
                pax.IdentityDoc.First().IdentityDocID = "1234!@";
                pax.IdentityDoc.First().GivenName = new string[] { "123" };
                pax.IdentityDoc.First().Surname = "1234";
                pax.IdentityDoc.First().ResidenceCountryCode = "1country";
                pax.IdentityDoc.First().IssuingCountryCode = "1234Code";
                pax.IdentityDoc.First().Visa.First().VisaID = "abc@4590!";
                pax.IdentityDoc.First().Visa.First().HostCountryCode = "1country";
                pax.IdentityDoc.First().IssueDate = DateTime.Today.AddDays(1);
                pax.IdentityDoc.First().ExpiryDate = DateTime.Today.AddDays(-1);
            }

            ConfigReader.Set("IFGAgentFopLookUpUrl", "https://Testresources.virginatlantic.com/DEV/validationmicroservice/IFGAgentFopLookUp");
            ConfigReader.Set("IFGAgentFOPLookUpSubscriptionKey", "Test_IFGAgentFOPLookUpSubscriptionKey");

            //Act
            var result = await _handler.Handle(new OrderCreateCommand() { OrderCreateRQ = request }, new CancellationToken());

            //Assert
            var items = (WarningType[])result.Value.Value.Items;
            Assert.Contains(items, x => x.Code == "914_16");
        }

        [Fact]
        public async Task When_ContactInfoIdIsNull_ValidateContactInfoName_ShouldThrowError()
        {
            //Arrange
            _hostEnvironment.Setup(x => x.ContentRootPath).Returns(Directory.GetCurrentDirectory());

            ConfigReader.Set("IFGAgentFopLookUpUrl", "https://Testresources.virginatlantic.com/DEV/validationmicroservice/IFGAgentFopLookUp");
            ConfigReader.Set("IFGAgentFOPLookUpSubscriptionKey", "Test_IFGAgentFOPLookUpSubscriptionKey");

            var offerDoc = TestDataProvider.GetTestData<OfferResponse>("NDCOrder\\OrderChange\\OfferDoc");
            _mediator.Setup(x => x.Send(It.IsAny<OfferMSQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(offerDoc);

            var validationRS = TestDataProvider.GetTestData<ValidationRS>("NDCOrder\\OrderCreate\\ValidationMSResponse");
            _mediator.Setup(x => x.Send(It.IsAny<ValidationMSCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(validationRS);

            var request = TestDataProvider.GetXmlTestData<IATA_OrderCreateRQ>("NDCOrder\\OrderCreate\\OrderCreateRQ");
            ((AcceptSelectedQuotedOfferListType)request.Request.CreateOrder.Item).SelectedPricedOffer.First().OfferRefID = offerDoc.Offers.First().OfferId;
            ((AcceptSelectedQuotedOfferListType)request.Request.CreateOrder.Item).SelectedPricedOffer.First().SelectedOfferItem.First().OfferItemRefID = offerDoc.Offers.First().OfferItems.First().OfferItemId;
            var offerPrice = Convert.ToDecimal(offerDoc.Offers.First().SalePrice.First().Amount.MonetaryAmount);
            ((AcceptSelectedQuotedOfferListType)request.Request.CreateOrder.Item).SelectedPricedOffer.First().TotalOfferPriceAmount.Value = offerPrice;
            request.Request.PaymentFunctions.First().PaymentProcessingDetails.Amount.Value = offerPrice;

            request.Request.DataLists.ContactInfoList.First().ContactInfoID = null;

            //Act
            var result = await _handler.Handle(new OrderCreateCommand() { OrderCreateRQ = request }, new CancellationToken());

            //Assert
            var items = (ErrorType[])result.Value.Value.Items;
            Assert.Contains(items, x => x.Code == "912");
        }

        [Fact]
        public async Task When_ContactInfoListIsNull_ValidateEmailAndPhone_ShouldThrowError()
        {
            //Arrange
            _hostEnvironment.Setup(x => x.ContentRootPath).Returns(Directory.GetCurrentDirectory());

            var validationRS = TestDataProvider.GetTestData<ValidationRS>("NDCOrder\\OrderCreate\\ValidationMSResponse");
            _mediator.Setup(x => x.Send(It.IsAny<ValidationMSCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(validationRS);

            var offerDoc = TestDataProvider.GetTestData<OfferResponse>("NDCOrder\\OrderChange\\OfferDoc");
            _mediator.Setup(x => x.Send(It.IsAny<OfferMSQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(offerDoc);

            var request = TestDataProvider.GetXmlTestData<IATA_OrderCreateRQ>("NDCOrder\\OrderCreate\\OrderCreateRQ");
            ((AcceptSelectedQuotedOfferListType)request.Request.CreateOrder.Item).SelectedPricedOffer.First().OfferRefID = offerDoc.Offers.First().OfferId;
            ((AcceptSelectedQuotedOfferListType)request.Request.CreateOrder.Item).SelectedPricedOffer.First().SelectedOfferItem.First().OfferItemRefID = offerDoc.Offers.First().OfferItems.First().OfferItemId;
            var offerPrice = Convert.ToDecimal(offerDoc.Offers.First().SalePrice.First().Amount.MonetaryAmount);
            ((AcceptSelectedQuotedOfferListType)request.Request.CreateOrder.Item).SelectedPricedOffer.First().TotalOfferPriceAmount.Value = offerPrice;
            request.Request.PaymentFunctions.First().PaymentProcessingDetails.Amount.Value = offerPrice;

            request.Request.DataLists.ContactInfoList.First().EmailAddress.First().EmailAddressText = null;

            ConfigReader.Set("IFGAgentFopLookUpUrl", "https://Testresources.virginatlantic.com/DEV/validationmicroservice/IFGAgentFopLookUp");
            ConfigReader.Set("IFGAgentFOPLookUpSubscriptionKey", "Test_IFGAgentFOPLookUpSubscriptionKey");

            //Act
            var result = await _handler.Handle(new OrderCreateCommand() { OrderCreateRQ = request }, new CancellationToken());

            //Assert
            var items = (ErrorType[])result.Value.Value.Items;
            Assert.Contains(items, x => x.Code == "316");
        }

        [Fact]
        public async Task When_EmailAddressTextAndPhoneNumberIsNull_ValidateEmailAndPhone_ShouldThrowError()
        {
            //Arrange
            _hostEnvironment.Setup(x => x.ContentRootPath).Returns(Directory.GetCurrentDirectory());

            var validationRS = TestDataProvider.GetTestData<ValidationRS>("NDCOrder\\OrderCreate\\ValidationMSResponse");
            _mediator.Setup(x => x.Send(It.IsAny<ValidationMSCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(validationRS);

            var offerDoc = TestDataProvider.GetTestData<OfferResponse>("NDCOrder\\OrderChange\\OfferDoc");
            _mediator.Setup(x => x.Send(It.IsAny<OfferMSQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(offerDoc);

            var request = TestDataProvider.GetXmlTestData<IATA_OrderCreateRQ>("NDCOrder\\OrderCreate\\OrderCreateRQ");
            ((AcceptSelectedQuotedOfferListType)request.Request.CreateOrder.Item).SelectedPricedOffer.First().OfferRefID = offerDoc.Offers.First().OfferId;
            ((AcceptSelectedQuotedOfferListType)request.Request.CreateOrder.Item).SelectedPricedOffer.First().SelectedOfferItem.First().OfferItemRefID = offerDoc.Offers.First().OfferItems.First().OfferItemId;
            var offerPrice = Convert.ToDecimal(offerDoc.Offers.First().SalePrice.First().Amount.MonetaryAmount);
            ((AcceptSelectedQuotedOfferListType)request.Request.CreateOrder.Item).SelectedPricedOffer.First().TotalOfferPriceAmount.Value = offerPrice;
            request.Request.PaymentFunctions.First().PaymentProcessingDetails.Amount.Value = offerPrice;

            request.Request.DataLists.ContactInfoList.First().EmailAddress.First().EmailAddressText = null;
            request.Request.DataLists.ContactInfoList.First().Phone.First().PhoneNumber = "";

            //Act
            var result = await _handler.Handle(new OrderCreateCommand() { OrderCreateRQ = request }, new CancellationToken());

            //Assert
            var items = (ErrorType[])result.Value.Value.Items;
            Assert.Contains(items, x => x.Code == "316");
        }

        [Fact]
        public async Task When_EmailAddressTextAndPhoneNumberAndCountryDialingCodeIsInvalid_ValidateEmailAndPhone_ShouldThrowError()
        {
            //Arrange
            _hostEnvironment.Setup(x => x.ContentRootPath).Returns(Directory.GetCurrentDirectory());

            var validationRS = TestDataProvider.GetTestData<ValidationRS>("NDCOrder\\OrderCreate\\ValidationMSResponse");
            _mediator.Setup(x => x.Send(It.IsAny<ValidationMSCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(validationRS);

            var offerDoc = TestDataProvider.GetTestData<OfferResponse>("NDCOrder\\OrderChange\\OfferDoc");
            _mediator.Setup(x => x.Send(It.IsAny<OfferMSQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(offerDoc);

            var request = TestDataProvider.GetXmlTestData<IATA_OrderCreateRQ>("NDCOrder\\OrderCreate\\OrderCreateRQ");
            ((AcceptSelectedQuotedOfferListType)request.Request.CreateOrder.Item).SelectedPricedOffer.First().OfferRefID = offerDoc.Offers.First().OfferId;
            ((AcceptSelectedQuotedOfferListType)request.Request.CreateOrder.Item).SelectedPricedOffer.First().SelectedOfferItem.First().OfferItemRefID = offerDoc.Offers.First().OfferItems.First().OfferItemId;
            var offerPrice = Convert.ToDecimal(offerDoc.Offers.First().SalePrice.First().Amount.MonetaryAmount);
            ((AcceptSelectedQuotedOfferListType)request.Request.CreateOrder.Item).SelectedPricedOffer.First().TotalOfferPriceAmount.Value = offerPrice;
            request.Request.PaymentFunctions.First().PaymentProcessingDetails.Amount.Value = offerPrice;

            request.Request.DataLists.ContactInfoList.First().EmailAddress.First().EmailAddressText = "1234";
            request.Request.DataLists.ContactInfoList.First().Phone.First().PhoneNumber = "123456789012345678901234567890";
            request.Request.DataLists.ContactInfoList.First().Phone.First().CountryDialingCode = "";

            //Act
            var result = await _handler.Handle(new OrderCreateCommand() { OrderCreateRQ = request }, new CancellationToken());

            //Assert
            var items = (ErrorType[])result.Value.Value.Items;
            Assert.Contains(items, x => x.Code == "317");
        }

        [Fact]
        public async Task When_EmailAddressTextInvalidLength_ValidateEmailAndPhone_ShouldThrowError()
        {
            //Arrange
            _hostEnvironment.Setup(x => x.ContentRootPath).Returns(Directory.GetCurrentDirectory());

            var validationRS = TestDataProvider.GetTestData<ValidationRS>("NDCOrder\\OrderCreate\\ValidationMSResponse");
            _mediator.Setup(x => x.Send(It.IsAny<ValidationMSCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(validationRS);

            var offerDoc = TestDataProvider.GetTestData<OfferResponse>("NDCOrder\\OrderChange\\OfferDoc");
            _mediator.Setup(x => x.Send(It.IsAny<OfferMSQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(offerDoc);

            var request = TestDataProvider.GetXmlTestData<IATA_OrderCreateRQ>("NDCOrder\\OrderCreate\\OrderCreateRQ");
            ((AcceptSelectedQuotedOfferListType)request.Request.CreateOrder.Item).SelectedPricedOffer.First().OfferRefID = offerDoc.Offers.First().OfferId;
            ((AcceptSelectedQuotedOfferListType)request.Request.CreateOrder.Item).SelectedPricedOffer.First().SelectedOfferItem.First().OfferItemRefID = offerDoc.Offers.First().OfferItems.First().OfferItemId;
            var offerPrice = Convert.ToDecimal(offerDoc.Offers.First().SalePrice.First().Amount.MonetaryAmount);
            ((AcceptSelectedQuotedOfferListType)request.Request.CreateOrder.Item).SelectedPricedOffer.First().TotalOfferPriceAmount.Value = offerPrice;
            request.Request.PaymentFunctions.First().PaymentProcessingDetails.Amount.Value = offerPrice;

            request.Request.DataLists.ContactInfoList.First().EmailAddress.First().EmailAddressText = "test1234567890test1234567890test1234567890test1234567890test1234567890@test.com";

            //Act
            var result = await _handler.Handle(new OrderCreateCommand() { OrderCreateRQ = request }, new CancellationToken());

            //Assert
            var items = (ErrorType[])result.Value.Value.Items;
            Assert.Contains(items, x => x.Code == "317");
        }

        [Fact]
        public async Task When_PaxListInvalidBirthdate_ValidateBirthDate_ShouldThrowError()
        {
            //Arrange
            _hostEnvironment.Setup(x => x.ContentRootPath).Returns(Directory.GetCurrentDirectory());

            var validationRS = TestDataProvider.GetTestData<ValidationRS>("NDCOrder\\OrderCreate\\ValidationMSResponse");
            _mediator.Setup(x => x.Send(It.IsAny<ValidationMSCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(validationRS);

            var offerDoc = TestDataProvider.GetTestData<OfferResponse>("NDCOrder\\OrderCreate\\OfferDoc_Return_2ADT1CHD1GBE1INF");
            _mediator.Setup(x => x.Send(It.IsAny<OfferMSQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(offerDoc);

            var request = TestDataProvider.GetXmlTestData<IATA_OrderCreateRQ>("NDCOrder\\OrderCreate\\OrderCreateRQ_Return_2ADT1CHD1GBE1INF");
            var offerPrice = Convert.ToDecimal(offerDoc.Offers.First().SalePrice.First().Amount.MonetaryAmount);
            ((AcceptSelectedQuotedOfferListType)request.Request.CreateOrder.Item).SelectedPricedOffer.First().TotalOfferPriceAmount.Value = offerPrice;
            request.Request.PaymentFunctions.First().PaymentProcessingDetails.Amount.Value = offerPrice;
            request.Request.DataLists.PaxList[0].Individual.Birthdate = DateTime.Now.AddDays(1);
            request.Request.DataLists.PaxList[1].Individual.Birthdate = DateTime.Now.AddYears(-10);
            request.Request.DataLists.PaxList[2].Individual.Birthdate = DateTime.Now.AddYears(-20);
            request.Request.DataLists.PaxList[3].Individual.Birthdate = DateTime.Now.AddYears(-15);
            request.Request.DataLists.PaxList[4].Individual.Birthdate = DateTime.Now.AddYears(-5);

            ConfigReader.Set("IFGAgentFopLookUpUrl", "https://Testresources.virginatlantic.com/DEV/validationmicroservice/IFGAgentFopLookUp");
            ConfigReader.Set("IFGAgentFOPLookUpSubscriptionKey", "Test_IFGAgentFOPLookUpSubscriptionKey");

            //Act
            var result = await _handler.Handle(new OrderCreateCommand() { OrderCreateRQ = request }, new CancellationToken());

            //Assert
            var items = (ErrorType[])result.Value.Value.Items;
            Assert.Contains(items, x => x.Code == "73");
        }

        [Fact]
        public async Task When_PaxListAdultInvalidBirthdate_ValidateBirthDate_ShouldThrowError()
        {
            //Arrange
            _hostEnvironment.Setup(x => x.ContentRootPath).Returns(Directory.GetCurrentDirectory());

            var validationRS = TestDataProvider.GetTestData<ValidationRS>("NDCOrder\\OrderCreate\\ValidationMSResponse");
            _mediator.Setup(x => x.Send(It.IsAny<ValidationMSCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(validationRS);

            var offerDoc = TestDataProvider.GetTestData<OfferResponse>("NDCOrder\\OrderCreate\\OfferDoc_Return_2ADT1CHD1GBE1INF");
            _mediator.Setup(x => x.Send(It.IsAny<OfferMSQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(offerDoc);
           
            ConfigReader.Set("IFGAgentFopLookUpUrl", "https://Testresources.virginatlantic.com/DEV/validationmicroservice/IFGAgentFopLookUp");
            ConfigReader.Set("IFGAgentFOPLookUpSubscriptionKey", "Test_IFGAgentFOPLookUpSubscriptionKey");

            var request = TestDataProvider.GetXmlTestData<IATA_OrderCreateRQ>("NDCOrder\\OrderCreate\\OrderCreateRQ_Return_2ADT1CHD1GBE1INF");
            var offerPrice = Convert.ToDecimal(offerDoc.Offers.First().SalePrice.First().Amount.MonetaryAmount);
            ((AcceptSelectedQuotedOfferListType)request.Request.CreateOrder.Item).SelectedPricedOffer.First().TotalOfferPriceAmount.Value = offerPrice;
            request.Request.PaymentFunctions.First().PaymentProcessingDetails.Amount.Value = offerPrice;
            request.Request.DataLists.PaxList[1].Individual.Birthdate = DateTime.Now.AddYears(-150);

            //Act
            var result = await _handler.Handle(new OrderCreateCommand() { OrderCreateRQ = request }, new CancellationToken());

            //Assert
            var items = (ErrorType[])result.Value.Value.Items;
            Assert.Contains(items, x => x.Code == "73");
        }

        [Fact]
        public async Task When_WrongAgencyID_ValidationMS_ShouldThrowError()
        {
            //Arrange
            _hostEnvironment.Setup(x => x.ContentRootPath).Returns(Directory.GetCurrentDirectory());
            
            _mediator.Setup(x => x.Send(It.IsAny<ValidationMSCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(new ValidationRS()
            {
                Error = new List<ErrorDetails>() { new() { Code = "G009" }, new() { Code = "10144" }, new() { Code = "10146" }, new() { Code = "10147" }, new() { Code = "10140" }, new() { Code = "10152" }, new() { Code = "10153" }, new() { Code = "10145" } }
            });
            var request = TestDataProvider.GetXmlTestData<IATA_OrderCreateRQ>("NDCOrder\\OrderCreate\\OrderCreateRQ");
            request.DistributionChain.First().ParticipatingOrg.OrgID = "9128103";

            //Act
            var result = await _handler.Handle(new OrderCreateCommand() { OrderCreateRQ = request }, new CancellationToken());

            //Assert
            var items = (ErrorType[])result.Value.Value.Items;
            Assert.Contains(items, x => x.Code == "911");
            Assert.Contains(items, x => x.Code == "368");
        }

        [Fact]
        public async Task When_WrongseatofferItem_OrderCreate_ShouldThrowError()
        {
            //Arrange
            _hostEnvironment.Setup(x => x.ContentRootPath).Returns(Directory.GetCurrentDirectory());
            _mediator.Setup(x => x.Send(It.IsAny<ValidationMSCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(new ValidationRS());

            var offerDoc = TestDataProvider.GetTestData<OfferResponse>("NDCOrder\\OrderCreate\\PositiveFlow\\OfferDoc");
            _mediator.Setup(x => x.Send(It.IsAny<OfferMSQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(offerDoc);

            var validationRS = TestDataProvider.GetTestData<ValidationRS>("NDCOrder\\OrderCreate\\PositiveFlow\\ValidationRS");
            _mediator.Setup(x => x.Send(It.IsAny<ValidationMSCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(validationRS);

            var seatofferDoc = TestDataProvider.GetTestData<SeatRS>("NDCOrder\\OrderChange\\SeatOfferDoc");
            _mediator.Setup(x => x.Send(It.IsAny<SeatMSQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(seatofferDoc);
            
            var airportDetails = TestDataProvider.GetTestData<IEnumerable<AirportDetail>>("NDCOrder\\OrderCreate\\PositiveFlow\\AirportDetails");
            _mediator.Setup(x => x.Send(It.IsAny<AirportDetailsQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(airportDetails);

            var request = TestDataProvider.GetXmlTestData<IATA_OrderCreateRQ>("NDCOrder\\OrderCreate\\OrderCreateRQ");
            ((AcceptSelectedQuotedOfferListType)request.Request.CreateOrder.Item).SelectedPricedOffer.First().OfferRefID = "1175c744-2f9c-4b82-b175-6ec6a5d95562|SvepLZHawM6OBT8tdsPCFB008";
            ((AcceptSelectedQuotedOfferListType)request.Request.CreateOrder.Item).SelectedPricedOffer.First().SelectedOfferItem.First().OfferItemRefID = "1175c744-2f9c-4b82-b175-6ec6a5d95562|SvepLZHawM6OBT8tdsPCFB008-1";
            ((AcceptSelectedQuotedOfferListType)request.Request.CreateOrder.Item).SelectedPricedOffer.First().TotalOfferPriceAmount.Value = 1138.79M;
            request.Request.PaymentFunctions.First().PaymentProcessingDetails.Amount.Value = 1138.79M;
            request.Request.DataLists.PaxList.First().IdentityDoc = null;
            request.Request.DataLists.PaxList.First().RedressCase = null;

            //Act
            var result = await _handler.Handle(new OrderCreateCommand() { OrderCreateRQ = request }, new CancellationToken());

            //Assert
            var items = (ErrorType[])result.Value.Value.Items;
            Assert.Contains(items, x => x.Code == "913");
        }

        [Fact]
        public async Task When_WrongseatofferPrice_OrderCreate_ShouldThrowError()
        {
            //Arrange
            _hostEnvironment.Setup(x => x.ContentRootPath).Returns(Directory.GetCurrentDirectory());
            _mediator.Setup(x => x.Send(It.IsAny<ValidationMSCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(new ValidationRS());

            var offerDoc = TestDataProvider.GetTestData<OfferResponse>("NDCOrder\\OrderCreate\\PositiveFlow\\OfferDoc");
            _mediator.Setup(x => x.Send(It.IsAny<OfferMSQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(offerDoc);

            var validationRS = TestDataProvider.GetTestData<ValidationRS>("NDCOrder\\OrderCreate\\PositiveFlow\\ValidationRS");
            _mediator.Setup(x => x.Send(It.IsAny<ValidationMSCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(validationRS);

            var seatofferDoc = TestDataProvider.GetTestData<SeatRS>("NDCOrder\\OrderChange\\SeatOfferDoc");
            _mediator.Setup(x => x.Send(It.IsAny<SeatMSQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(seatofferDoc);
            
            var airportDetails = TestDataProvider.GetTestData<IEnumerable<AirportDetail>>("NDCOrder\\OrderCreate\\PositiveFlow\\AirportDetails");
            _mediator.Setup(x => x.Send(It.IsAny<AirportDetailsQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(airportDetails);

            var request = TestDataProvider.GetXmlTestData<IATA_OrderCreateRQ>("NDCOrder\\OrderCreate\\OrderCreateRQ");
            ((AcceptSelectedQuotedOfferListType)request.Request.CreateOrder.Item).SelectedPricedOffer.First().OfferRefID = "1175c744-2f9c-4b82-b175-6ec6a5d95562|SvepLZHawM6OBT8tdsPCFB008";
            ((AcceptSelectedQuotedOfferListType)request.Request.CreateOrder.Item).SelectedPricedOffer.First().SelectedOfferItem.First().OfferItemRefID = "1175c744-2f9c-4b82-b175-6ec6a5d95562|SvepLZHawM6OBT8tdsPCFB008-1";
            ((AcceptSelectedQuotedOfferListType)request.Request.CreateOrder.Item).SelectedPricedOffer.First().TotalOfferPriceAmount.Value = 1138.79M;
            ((AcceptSelectedQuotedOfferListType)request.Request.CreateOrder.Item).SelectedPricedOffer.Last().SelectedOfferItem.First().OfferItemRefID = "e0c744d7-07a6-4758-8bfc-0dfcbae93a54_FA|1_18_35_52_69";
            request.Request.PaymentFunctions.First().PaymentProcessingDetails.Amount.Value = 1138.79M;
            request.Request.DataLists.PaxList.First().IdentityDoc = null;
            request.Request.DataLists.PaxList.First().RedressCase = null;

            //Act
            var result = await _handler.Handle(new OrderCreateCommand() { OrderCreateRQ = request }, new CancellationToken());

            //Assert
            var items = (ErrorType[])result.Value.Value.Items;
            Assert.Contains(items, x => x.Code == "727");
        }

        [Fact]
        public async Task When_IncorrectCard_ShouldThrowLuhnCheckValidationError()
        {
            //Arrange
            var request = TestDataProvider.GetXmlTestData<IATA_OrderCreateRQ>("NDCOrder\\OrderCreate\\OrderCreateRQ");
            ((PaymentProcessingPaymentCardType)request.Request.PaymentFunctions.First().PaymentProcessingDetails.PaymentMethod.Item).CardNumber = "111122233334ad4ert@we";
            _hostEnvironment.Setup(x => x.ContentRootPath).Returns(Directory.GetCurrentDirectory());
            
            //Act
            var result = await _handler.Handle(new OrderCreateCommand() { OrderCreateRQ = request }, new CancellationToken());

            //Assert
            var items = (ErrorType[])result.Value.Value.Items;
            Assert.Contains(items, x => x.Code == "708");
        }

        [Fact]
        public async Task OrderCreate_ShouldSucceed_WithPayment()
        {
            //Arrange
            var request = TestDataProvider.GetXmlTestData<IATA_OrderCreateRQ>("NDCOrder\\OrderCreate\\OrderCreateWithPayment");
            var validationRS = TestDataProvider.GetTestData<ValidationRS>("NDCOrder\\OrderCreate\\PositiveFlow\\ValidationRS");
            var offerDoc = TestDataProvider.GetTestData<OfferResponse>("NDCOrder\\OrderCreate\\PositiveFlow\\OfferDoc");
            var brandMsRs = TestDataProvider.GetTestData<BrandRS>("NDCOrder\\OrderCreate\\PositiveFlow\\BrandMSRS");
            var orderDoc = TestDataProvider.GetTestData<OrderRS>("NDCOrder\\OrderCreate\\PositiveFlow\\OrderDoc");
            var orderViewRS = TestDataProvider.GetXmlTestData<IATA_OrderViewRS>("NDCOrder\\OrderViewRS");
            var currencyRS = TestDataProvider.GetTestData<string>("NDCOrder\\OrderCreate\\PositiveFlow\\CurrencyReferenceMS");
            var airportDetails = TestDataProvider.GetTestData<IEnumerable<AirportDetail>>("NDCOrder\\OrderCreate\\PositiveFlow\\AirportDetails");
            var paymentRs = new PaymentRS()
            {
                PaymentDocument = new PaymentDocument()
                {
                    TransactionId = "123",
                    Amount = 100,
                    OrderID = "VS93212345625",
                    TransactionStatus = "Success",
                    PaymentCommitedDateTime = "2026-02-05T21:02:44",
                    MaskedCardNumber = "444433XXXXXX1111",
                    CardType = "VI"
                }
            };
            _mediator.Setup(x => x.Send(It.IsAny<ValidationMSCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(validationRS);
            _mediator.Setup(x => x.Send(It.IsAny<OfferMSQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(offerDoc);
            _mediator.Setup(x => x.Send(It.IsAny<BrandMSCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(brandMsRs);
            _mediator.Setup(x => x.Send(It.IsAny<OrderMSCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(orderDoc);
            _mediator.Setup(x => x.Send(It.IsAny<PaymentMSAuthorizeCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(paymentRs);
            _mediator.Setup(x => x.Send(It.IsAny<PaymentMSCaptureCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(paymentRs);
            _mediator.Setup(x => x.Send(It.IsAny<PaymentMSRefundCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(paymentRs);
            _mediator.Setup(x => x.Send(It.IsAny<OrderTicketMSCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(new OrderTicketRS() { OrderTicketDocument = new() { AccountableDocumentRef = [new() { DocumentNumber = "1234567890" }], BookingReference = new() { ExternalRecordLocator = "ABCDEF" } } });
            _mediator.Setup(x => x.Send(It.IsAny<OrderDataMSQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(orderDoc);
            _mediator.Setup(x => x.Send(It.IsAny<ReferenceMSCurrencyQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(currencyRS);
            _mediator.Setup(x => x.Send(It.IsAny<AirportDetailsQuery>(),It.IsAny<CancellationToken>())).ReturnsAsync(airportDetails);
            _mediator.Setup(x => x.Send(It.IsAny<PublishOrderDocMSCommand>(), It.IsAny<CancellationToken>()));
            _hostEnvironment.SetupGet(x => x.ContentRootPath).Returns(".\\TestDataSamples\\NDCOrder");

            ConfigReader.Set("PublishOrderDocUrl", "https://Testresources.virginatlantic.com/dev/NdcOrderSearch/PublishOrderDoc");
            ConfigReader.Set("OrderSearchMSSubscriptionKey", "Test_OrderSearchMSSubscriptionKey");

            ConfigReader.Set("IFGAgentFopLookUpUrl", "https://Testresources.virginatlantic.com/DEV/validationmicroservice/IFGAgentFopLookUp");
            ConfigReader.Set("IFGAgentFOPLookUpSubscriptionKey", "Test_IFGAgentFOPLookUpSubscriptionKey");

            ConfigReader.Set("IsIfgIntegrated", "false");

            //OrderCreate_ShouldSucceed_WithPayment
            var result = await _handler.Handle(new OrderCreateCommand() { OrderCreateRQ = request }, new CancellationToken());

            //Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(Status.OK, result.Value.Status);
            Assert.IsType<IATA_OrderViewRS>(result.Value.Value);
            Assert.Equal("VS932131FAA24", (result.Value.Value.Items.FirstOrDefault() as OrderViewResponseType)?.Order?.FirstOrDefault()?.OrderID);

        }

        [Fact]
        public void ARC_ResponseMapper_Tests()
        {
            var orderDoc = TestDataProvider.GetTestData<OrderDocument>("NDCOrder\\OrderCreate\\ArcOrderDoc");
            var result = OrderResponseMapper.MapOrderResponse(orderDoc, null);
            var response = ((OrderViewResponseType[])result.Items).First();

            Assert.NotNull(response.Order.First().OrderItem.First());
            Assert.Equal("177.00", response.Order.First().OrderItem.First().Price.BaseAmount.Value.ToString());
            Assert.Equal("USD", response.Order.First().OrderItem.First().Price.BaseAmount.CurCode.ToString());
        }

        [Fact]
        public async Task TransactionAPIShouldReturnSuccess_ARC()
        {
            var transactionRequest = TestDataProvider.GetTestData<TransactionServiceRequest>("NDCOrder\\OrderChange\\TransactionRequest");

            ConfigReader.Set("ARCPaymentMSUrl", "https://Testresources.virginatlantic.com/dev/ARCPayment/");
            ConfigReader.Set("ARCPaymentMSSubscriptionKey", "Test_ARCPaymentMSSubscriptionKey");

            var result = await _transactionCommandHandler.Handle(transactionRequest);

            Assert.NotNull(result);
            Assert.Equal("f4da1759-68ba-49fb-bd5e-3bfc278f196b", result.RequestId);
            Assert.Equal("SUCCESS", result.Status);

        }

        [Fact]
        public void When_ContactInfoListIsNull_ValidateContactDetails_ShouldReturnMissingContactInfoError()
        {
            // Arrange
            var request = TestDataProvider.GetXmlTestData<IATA_OrderCreateRQ>("NDCOrder\\OrderCreate\\OrderCreateRQ");
            request.Request.DataLists.ContactInfoList = null;

            // Act
            var errors = OrderCreateRequestValidation.ValidateContactDetails(request).ToList();

            // Assert
            var error = Assert.Single(errors);
            Assert.Equal("911_5", error.ErrorCode);
        }

        [Fact]
        public void When_PhoneTagIsEmpty_ValidateContactDetails_ShouldReturnMissingPhoneNumberError()
        {
            // Arrange
            var request = TestDataProvider.GetXmlTestData<IATA_OrderCreateRQ>("NDCOrder\\OrderCreate\\OrderCreateRQ");
            request.Request.DataLists.ContactInfoList.First().Phone = [new VAA.EAPI.NDC.Order.Domain.OrderCreateRQ.PhoneType()];

            // Act
            var errors = OrderCreateRequestValidation.ValidateContactDetails(request).ToList();

            // Assert
            var error = Assert.Single(errors);
            Assert.Equal("316_2", error.ErrorCode);
        }
        
    }
}
