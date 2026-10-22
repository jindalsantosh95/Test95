using MediatR;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Services.Analytics;
using VAA.EAPI.NDC.Order.Application.OrderRetrieve;
using VAA.EAPI.NDC.Order.Domain.OrderRetrieveRQ;
using VAA.EAPI.NDC.Order.Domain.OrderViewRS;
using VAA.EAPI.NDC.Order.Infrastructure.Model.Air4OrderSearchMS;
using VAA.EAPI.NDC.Order.Infrastructure.Model.BrandMS;
using VAA.EAPI.NDC.Order.Infrastructure.Model.OrderMS;
using VAA.EAPI.NDC.Order.Infrastructure.Model.ValidationMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.Air4OrderSearchMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.BrandMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.OrderDataMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.OrderSearchMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.ValidationMS;
using VAA.EAPI.NDC.Order.Mapper.Order;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.NDC.Order.Tests.OrderRetrieve
{
    public class OrderRetrieveQueryHandlerTests
    {
        private readonly Mock<IMediator> _mediator;
        private readonly Mock<ILogger<OrderRetrieveQueryHandler>> _logger;
        private readonly Mock<IHostEnvironment> _hostEnvironment;
        private readonly Mock<IGdsOrderResponseMapper> _gdsOrderResponseMapper;
        private readonly OrderRetrieveQueryHandler _handler;
        private readonly Mock<IBrandMSCommand> _command;
        private readonly Mock<IBrandMSPTCCommand> _commandPTC;
        private readonly Mock<ITrackAnalyticsEventCommandHandler> _trackAnalyticsEventCommandHandler;

        public OrderRetrieveQueryHandlerTests()
        {
            _mediator = new Mock<IMediator>();
            _logger = new Mock<ILogger<OrderRetrieveQueryHandler>>();
            _hostEnvironment = new Mock<IHostEnvironment>();
            _gdsOrderResponseMapper = new Mock<IGdsOrderResponseMapper>();
            _command = new Mock<IBrandMSCommand>();
            _commandPTC = new Mock<IBrandMSPTCCommand>();
            _trackAnalyticsEventCommandHandler = new Mock<ITrackAnalyticsEventCommandHandler>();
            _handler = new OrderRetrieveQueryHandler(_mediator.Object, _logger.Object, _hostEnvironment.Object, _gdsOrderResponseMapper.Object, _command.Object, _commandPTC.Object, _trackAnalyticsEventCommandHandler.Object);
            ConfigReader.Set("SkipGDSAgentValidation", "false");
            _hostEnvironment.Setup(x => x.ContentRootPath).Returns(Directory.GetCurrentDirectory());
        }

        [Fact]
        public void When_MediatorObjectIsNull_Constructor_ShouldThrowNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new OrderRetrieveQueryHandler(null, _logger.Object, _hostEnvironment.Object, _gdsOrderResponseMapper.Object, _command.Object, _commandPTC.Object, _trackAnalyticsEventCommandHandler.Object));
        }

        [Fact]
        public void When_LoggerObjectIsNull_Constructor_ShouldThrowNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new OrderRetrieveQueryHandler(_mediator.Object, null, _hostEnvironment.Object, _gdsOrderResponseMapper.Object, _command.Object, _commandPTC.Object, _trackAnalyticsEventCommandHandler.Object));
        }

        [Fact]
        public void When_HostEnvironmentObjectIsNull_Constructor_ShouldThrowNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new OrderRetrieveQueryHandler(_mediator.Object, _logger.Object, null, _gdsOrderResponseMapper.Object, _command.Object, _commandPTC.Object, _trackAnalyticsEventCommandHandler.Object));
        }

        [Fact]
        public async Task Handler_Exception()
        {
            //Arrange
            _hostEnvironment.Setup(x => x.ContentRootPath).Returns(Directory.GetCurrentDirectory());

            var request = TestDataProvider.GetXmlTestData<IATA_OrderRetrieveRQ>("NDCOrder\\OrderRetrieve\\OrderRetrieveRQ");

            //Act
            var result = await _handler.Handle(new OrderRetrieveQuery() { OrderRetrieveRQ = request }, new CancellationToken());

            //Assert
            var items = (ErrorType[])result.Value.Value.Items;
            Assert.Contains(items, x => x.Code == "911");
        }
        
        [Fact]
        public async Task WHen_WrongDetailsinOrderRetreiveRQ_ThrowError()
        {
            //Arrange
            _hostEnvironment.Setup(x => x.ContentRootPath).Returns(Directory.GetCurrentDirectory());
            
            var request = TestDataProvider.GetXmlTestData<IATA_OrderRetrieveRQ>("NDCOrder\\OrderRetrieve\\OrderRetrieveRQ");

            request.DistributionChain.First().Ordinal = null;
            request.DistributionChain.First().ParticipatingOrg.OrgID = null;
            request.Request.OrderValidationFilterCriteria.OrderFilterCriteria.OrderID = null;

            //Act
            var result = await _handler.Handle(new OrderRetrieveQuery() { OrderRetrieveRQ = request }, new CancellationToken());

            //Assert
            var items = (ErrorType[])result.Value.Value.Items;
            Assert.Contains(items, x => x.Code == "912");
        }
        
        [Fact]
        public async Task When_WrongAgency_ThrowError()
        {
            _hostEnvironment.Setup(x => x.ContentRootPath).Returns(Directory.GetCurrentDirectory());
           
            _mediator.Setup(x => x.Send(It.IsAny<ValidationMSCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(new Order.Infrastructure.Model.ValidationMS.ValidationRS()
            {
                Error = new List<ErrorDetails>() { new() { Code = "G009" }, new() { Code = "10144" }, new() { Code = "10146" }, new() { Code = "10147" }, new() { Code = "10140" }, new() { Code = "10152" }, new() { Code = "10153" }, new() { Code = "10145" } }
            });
            var orderDoc = TestDataProvider.GetTestData<OrderDocument>("NDCOrder\\OrderRetrieve\\OrderDocWithHistory");
            _mediator.Setup(x => x.Send(It.IsAny<OrderDataMSQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(new OrderRS() { OrderDocument = orderDoc });
            var request = TestDataProvider.GetXmlTestData<IATA_OrderRetrieveRQ>("NDCOrder\\OrderRetrieve\\OrderRetrieveRQ");

            //Act
            var result = await _handler.Handle(new OrderRetrieveQuery() { OrderRetrieveRQ = request }, new CancellationToken());

            //Assert
            var items = (ErrorType[])result.Value.Value.Items;
            Assert.Contains(items, x => x.Code == "911");
            Assert.Contains(items, x => x.Code == "368");
        }
        
        [Fact]
        public async Task When_WrongAgent_ThrowError()
        {
            _hostEnvironment.Setup(x => x.ContentRootPath).Returns(Directory.GetCurrentDirectory());
           
            var validationRS = TestDataProvider.GetTestData<ValidationRS>("NDCOrder\\OrderCreate\\ValidationMSResponse");
            _mediator.Setup(x => x.Send(It.IsAny<ValidationMSCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(validationRS);
            var orderDoc = TestDataProvider.GetTestData<OrderDocument>("NDCOrder\\OrderRetrieve\\OrderDocWithHistory");
            _mediator.Setup(x => x.Send(It.IsAny<OrderDataMSQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(new OrderRS() { OrderDocument = orderDoc });
            var request = TestDataProvider.GetXmlTestData<IATA_OrderRetrieveRQ>("NDCOrder\\OrderRetrieve\\OrderRetrieveRQ");
            request.DistributionChain.First().ParticipatingOrg.OrgID = "XXXXXXX";

            //Act
            var result = await _handler.Handle(new OrderRetrieveQuery() { OrderRetrieveRQ = request }, new CancellationToken());

            //Assert
            var items = (ErrorType[])result.Value.Value.Items;

            Assert.Contains(items, x => x.Code == "368");
        }

        [Fact]
        public async Task When_NotVSTicketed_ShouldReturnErrorForGDS()
        {
            //Arrange
            var request = TestDataProvider.GetXmlTestData<IATA_OrderRetrieveRQ>("NDCOrder\\OrderRetrieve\\OrderRetrieveRQ");
            _mediator.Setup(x => x.Send(It.IsAny<OrderDataMSQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(new OrderRS());

            var orderSearchResponse = TestDataProvider.GetTestData<OrderSearchResponse>("NDCOrder\\OrderSearchResponse_Return_3PAX");

            foreach (var order in orderSearchResponse.OrderItems)
                order.AccountableDocumentRefs.First().DocumentNumber = order.AccountableDocumentRefs.First().DocumentNumber.Replace("932", "000");

            _mediator.Setup(x => x.Send(It.IsAny<Air4OrderSearchMSQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(new Air4OrderSearchMSResponse() { OrderSearchResponse = orderSearchResponse });

            //Act
            var result = await _handler.GDSBookingOrderRetrieve(request, new EAPI.Infrastructure.Services.Analytics.TrackAnalyticsEventCommand(), new CancellationToken());

            //Assert
            Assert.Equal(Status.BusinessException, result.Value.Status);
            Assert.Equal("129", (result.Value.Value.Items as ErrorType[])?.FirstOrDefault()?.Code);
        }

        [Fact]
        public async Task When_OrderSearchResponseIsNull_ShouldReturnErrorForGDS()
        {
            //Arrange
            var request = TestDataProvider.GetXmlTestData<IATA_OrderRetrieveRQ>("NDCOrder\\OrderRetrieve\\OrderRetrieveRQ");
            _mediator.Setup(x => x.Send(It.IsAny<OrderDataMSQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(new OrderRS());
            Air4OrderSearchMSResponse? response = null;
            _mediator.Setup(x => x.Send(It.IsAny<Air4OrderSearchMSQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(response);

            //Act
            var result = await _handler.GDSBookingOrderRetrieve(request, new EAPI.Infrastructure.Services.Analytics.TrackAnalyticsEventCommand(), new CancellationToken());

            //Assert
            Assert.Equal(Status.BusinessException, result.Value.Status);
            Assert.Equal("914", (result.Value.Value.Items as ErrorType[])?.FirstOrDefault()?.Code);
        }

        [Fact]
        public async Task When_OrderSearchResponseHasErrors_ShouldReturnErrorForGDS()
        {
            //Arrange
            var request = TestDataProvider.GetXmlTestData<IATA_OrderRetrieveRQ>("NDCOrder\\OrderRetrieve\\OrderRetrieveRQ");
            _mediator.Setup(x => x.Send(It.IsAny<OrderDataMSQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(new OrderRS());
            _mediator.Setup(x => x.Send(It.IsAny<Air4OrderSearchMSQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(new Air4OrderSearchMSResponse()
            {
                ErrorDetails = new List<ErrorDetails>()
            {
                new () { Code = "1088"},
                new () { Code = "G014"},
                new () { Code = "1079"},
                new () { Code = "1100"}
            }
            });

            //Act
            var result = await _handler.GDSBookingOrderRetrieve(request, new EAPI.Infrastructure.Services.Analytics.TrackAnalyticsEventCommand(), new CancellationToken());

            //Assert
            Assert.Equal(Status.Error, result.Value.Status);
            Assert.True((result.Value.Value.Items as ErrorType[])?.Select(x => x.Code).Contains("911"));
        }

        [Fact(Skip = "To be modified as per sabre implementation")]
        public async Task When_NoError_ShouldReturnSuccessForGDS()
        {
            //Arrange
            _hostEnvironment.Setup(x => x.ContentRootPath).Returns(Directory.GetCurrentDirectory());
            var request = TestDataProvider.GetXmlTestData<IATA_OrderRetrieveRQ>("NDCOrder\\OrderRetrieve\\OrderRetrieveRQ");
            var orderDocumentFromOrderDataMS = TestDataProvider.GetTestData<OrderDocument>("NDCOrder\\OrderRetrieve\\OrderDocFromSabreOrderSearch");
            var orderDocumentFromOrderSearchMS = TestDataProvider.GetTestData<OrderDocument>("NDCOrder\\OrderRetrieve\\OrderDocFromSabreOrderSearch");
            var OrderRetrieveBrandMs = TestDataProvider.GetTestData<BrandRS>("NDCOrder\\OrderRetrieve\\OrderRetrieveBrandMs");
            var validationRS = TestDataProvider.GetTestData<ValidationRS>("NDCOrder\\OrderCreate\\ValidationMSResponse");
            var OrderViewRS = TestDataProvider.GetXmlTestData<IATA_OrderViewRS>("NDCOrder\\OrderRetrieve\\OrderViewRS");
            OrderResponseMapper.MapOrderResponse(It.IsAny<OrderDocument>(), null, false, false, false, false);
            _mediator.Setup(x => x.Send(It.IsAny<ValidationMSCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(validationRS);
            _mediator.Setup(x => x.Send(It.IsAny<BrandMSCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(OrderRetrieveBrandMs);
            _mediator.Setup(x => x.Send(It.IsAny<OrderDataMSQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(new OrderRS() { OrderDocument = orderDocumentFromOrderDataMS });
            _mediator.Setup(x => x.Send(It.IsAny<OrderSearchMSCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(orderDocumentFromOrderSearchMS);
            var response = TestDataProvider.GetXmlTestData<IATA_OrderViewRS>("NDCOrder\\OrderRetrieve\\OrderViewRS");
          _gdsOrderResponseMapper.Setup(x => x.MapOrderResponseForGDS(It.IsAny<OrderSearchResponse>(),null)).Returns(response);
            ConfigReader.Set("SkipGDSAgentValidation", "false");


            //Act
            var result = await _handler.Handle(new OrderRetrieveQuery() { OrderRetrieveRQ = request }, new CancellationToken());

            //Assert
            var orderViewRS = result.Value.Value.Items.FirstOrDefault() as OrderViewResponseType;
            Assert.True(result.IsSuccess);
            Assert.Equal("OK", result.Value.Status.ToString());
            Assert.NotNull(orderViewRS);
            Assert.Equal("VS", orderViewRS.Order.First().OwnerCode);
            Assert.Equal("VS9325C013F24", orderViewRS.Order.First().OrderID);
            Assert.Equal("TLV", orderViewRS.DataLists.OriginDestList.First().DestCode);
            Assert.Equal("LHR", orderViewRS.DataLists.OriginDestList.First().OriginCode);
            Assert.Equal("ADULT_1", orderViewRS.DataLists.PaxList.First().PaxID);
            Assert.Equal("ADT", orderViewRS.DataLists.PaxList.First().PTC);
            Assert.Equal("NG05E0LA", orderViewRS.DataLists.PriceClassList.First().FareBasisCode);
            Assert.Equal("Economy Light", orderViewRS.DataLists.PriceClassList.First().Name);
        }

        [Fact]
        public async Task When_DistribtionchainLengthexceedstwo_ThrowError()
        {
            //Arrange
            _hostEnvironment.Setup(x => x.ContentRootPath).Returns(Directory.GetCurrentDirectory());
            
            var request = TestDataProvider.GetXmlTestData<IATA_OrderRetrieveRQ>("NDCOrder\\OrderRetrieve\\OrderRetrieveRQ_Dl");

            request.DistributionChain.First().Ordinal = "2";
            request.DistributionChain.Last().Ordinal = "2";

            //Act
            var result = await _handler.Handle(new OrderRetrieveQuery() { OrderRetrieveRQ = request }, new CancellationToken());

            //Assert
            var items = (ErrorType[])result.Value.Value.Items;
            Assert.Contains(items, x => x.Code == "912");
        }

        [Fact]
        public async Task WHen_WrongContactDetailsinOrderRetreiveRQ_ThrowError()
        {
            //Arrange
            _hostEnvironment.Setup(x => x.ContentRootPath).Returns(Directory.GetCurrentDirectory());
            
            var request = TestDataProvider.GetXmlTestData<IATA_OrderRetrieveRQ>("NDCOrder\\OrderRetrieve\\OrderRetrieveRQ");

            request.DistributionChain.First().ContactInfo.ContactInfoID = null;

            //Act
            var result = await _handler.Handle(new OrderRetrieveQuery() { OrderRetrieveRQ = request }, new CancellationToken());

            //Assert
            var items = (ErrorType[])result.Value.Value.Items;

            Assert.Contains(items, x => x.Code == "912");
        }

        [Fact]
        public async Task WHen_WrongEmailDetailsinOrderRetreiveRQ_ThrowError()
        {
            //Arrange
            _hostEnvironment.Setup(x => x.ContentRootPath).Returns(Directory.GetCurrentDirectory());
            
            var request = TestDataProvider.GetXmlTestData<IATA_OrderRetrieveRQ>("NDCOrder\\OrderRetrieve\\OrderRetrieveRQ");

            request.DistributionChain.First().ContactInfo.EmailAddress.First().EmailAddressText = "EmailLengthIsGreaterThenSixtyFourThenItShoulThrowErrorToValidateThisScenario@Email.Com";
            request.DistributionChain.First().ContactInfo.EmailAddress.First().EmailAddressText = "1234";

            //Act
            var result = await _handler.Handle(new OrderRetrieveQuery() { OrderRetrieveRQ = request }, new CancellationToken());

            //Assert
            var items = (ErrorType[])result.Value.Value.Items;

            Assert.Contains(items, x => x.Code == "317");
        }

        [Fact]
        public async Task When_WrongRLOCDetails_ThrowError()
        {
            _hostEnvironment.Setup(x => x.ContentRootPath).Returns(Directory.GetCurrentDirectory());
            
            var request = TestDataProvider.GetXmlTestData<IATA_OrderRetrieveRQ>("NDCOrder\\OrderRetrieve\\OrderRetrieveRQ");
            request.Request.OrderValidationFilterCriteria.OrderFilterCriteria.OrderID = null;
            request.Request.OrderValidationFilterCriteria.BookingRefFilterCriteria.BookingID = null;

            //Act
            var result = await _handler.Handle(new OrderRetrieveQuery() { OrderRetrieveRQ = request }, new CancellationToken());

            //Assert
            var items = (ErrorType[])result.Value.Value.Items;

            Assert.Contains(items, x => x.Code == "912");
        }

        [Fact]
        public async Task When_WrongOrderIDFormatDetails_ThrowError()
        {
            _hostEnvironment.Setup(x => x.ContentRootPath).Returns(Directory.GetCurrentDirectory());
            
            var request = TestDataProvider.GetXmlTestData<IATA_OrderRetrieveRQ>("NDCOrder\\OrderRetrieve\\OrderRetrieveRQ");

            request.Request.OrderValidationFilterCriteria.OrderFilterCriteria.OrderID = "^[A-Z0-9]*$";

            //Act
            var result = await _handler.Handle(new OrderRetrieveQuery() { OrderRetrieveRQ = request }, new CancellationToken());

            //Assert
            var items = (ErrorType[])result.Value.Value.Items;

            Assert.Contains(items, x => x.Code == "914");
        }

        [Fact]
        public async Task When_WrongRLOCFormatDetails_ThrowError()
        {
            _hostEnvironment.Setup(x => x.ContentRootPath).Returns(Directory.GetCurrentDirectory());
           
            var request = TestDataProvider.GetXmlTestData<IATA_OrderRetrieveRQ>("NDCOrder\\OrderRetrieve\\OrderRetrieveRQ");

            request.Request.OrderValidationFilterCriteria.BookingRefFilterCriteria.BookingID = "^[A-Z0-9]*$";
            request.Request.OrderValidationFilterCriteria.BookingRefFilterCriteria.BookingID = "YZ";

            //Act
            var result = await _handler.Handle(new OrderRetrieveQuery() { OrderRetrieveRQ = request }, new CancellationToken());

            //Assert
            var items = (ErrorType[])result.Value.Value.Items;

            Assert.Contains(items, x => x.Code == "914");
        }

        [Fact]
        public async Task ReturnRQHandler_Exception()
        {
            //Arrange
            _hostEnvironment.Setup(x => x.ContentRootPath).Returns(Directory.GetCurrentDirectory());

            var request = TestDataProvider.GetXmlTestData<IATA_OrderRetrieveRQ>("NDCOrder\\OrderRetrieve\\OrderRetrieveRQ_return");

            //Act
            var result = await _handler.Handle(new OrderRetrieveQuery() { OrderRetrieveRQ = request }, new CancellationToken());

            //Assert
            var items = (ErrorType[])result.Value.Value.Items;
            Assert.Contains(items, x => x.Code == "911");
        }

        [Fact]
        public async Task WHen_WrongDetailsinOrderRetreive_returnRQ_ThrowError()
        {
            //Arrange
            _hostEnvironment.Setup(x => x.ContentRootPath).Returns(Directory.GetCurrentDirectory());
            
            var request = TestDataProvider.GetXmlTestData<IATA_OrderRetrieveRQ>("NDCOrder\\OrderRetrieve\\OrderRetrieveRQ_return");

            request.DistributionChain.First().Ordinal = null;
            request.DistributionChain.First().ParticipatingOrg.OrgID = null;
            request.Request.OrderValidationFilterCriteria.OrderFilterCriteria.OrderID = null;

            //Act
            var result = await _handler.Handle(new OrderRetrieveQuery() { OrderRetrieveRQ = request }, new CancellationToken());

            //Assert
            var items = (ErrorType[])result.Value.Value.Items;
            Assert.Contains(items, x => x.Code == "912");
        }

        [Fact]
        public async Task When_WrongAgency_inReturnRQ_ThrowError()
        {
            _hostEnvironment.Setup(x => x.ContentRootPath).Returns(Directory.GetCurrentDirectory());
            
            _mediator.Setup(x => x.Send(It.IsAny<ValidationMSCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(new Order.Infrastructure.Model.ValidationMS.ValidationRS()
            {
                Error = new List<ErrorDetails>() { new() { Code = "G009" }, new() { Code = "10144" }, new() { Code = "10146" }, new() { Code = "10147" }, new() { Code = "10140" }, new() { Code = "10152" }, new() { Code = "10153" }, new() { Code = "10145" } }
            });
            var orderDoc = TestDataProvider.GetTestData<OrderDocument>("NDCOrder\\OrderRetrieve\\OrderDocWithHistory");
            _mediator.Setup(x => x.Send(It.IsAny<OrderDataMSQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(new OrderRS() { OrderDocument = orderDoc });
            var request = TestDataProvider.GetXmlTestData<IATA_OrderRetrieveRQ>("NDCOrder\\OrderRetrieve\\OrderRetrieveRQ_return");

            //Act
            var result = await _handler.Handle(new OrderRetrieveQuery() { OrderRetrieveRQ = request }, new CancellationToken());

            //Assert
            var items = (ErrorType[])result.Value.Value.Items;
            Assert.Contains(items, x => x.Code == "911");
            Assert.Contains(items, x => x.Code == "368");
        }

        [Fact]
        public async Task When_WrongAgent_inReturnRQ_ThrowError()
        {
            _hostEnvironment.Setup(x => x.ContentRootPath).Returns(Directory.GetCurrentDirectory());
            
            _mediator.Setup(x => x.Send(It.IsAny<ValidationMSCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(new ValidationRS()
            {
                Error = new List<ErrorDetails>() { new() { Code = "10144" } }
            });
            var orderDoc = TestDataProvider.GetTestData<OrderDocument>("NDCOrder\\OrderRetrieve\\OrderDocWithHistory");
            _mediator.Setup(x => x.Send(It.IsAny<OrderDataMSQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(new OrderRS() { OrderDocument = orderDoc });
            var request = TestDataProvider.GetXmlTestData<IATA_OrderRetrieveRQ>("NDCOrder\\OrderRetrieve\\OrderRetrieveRQ_return");
            request.DistributionChain.First().ParticipatingOrg.OrgID = "Atlas Service Limited";

            //Act
            var result = await _handler.Handle(new OrderRetrieveQuery() { OrderRetrieveRQ = request }, new CancellationToken());

            //Assert
            var items = (ErrorType[])result.Value.Value.Items;

            Assert.Contains(items, x => x.Code == "368");
        }

        [Fact]
        public async Task When_Error_InOrderSearchMS_ShouldReturnError()
        {
            _hostEnvironment.Setup(x => x.ContentRootPath).Returns(Directory.GetCurrentDirectory());
            var request = TestDataProvider.GetXmlTestData<IATA_OrderRetrieveRQ>("NDCOrder\\OrderRetrieve\\OrderRetrieveRQ_return");
            var validationRS = TestDataProvider.GetTestData<ValidationRS>("NDCOrder\\OrderCreate\\ValidationMSResponse");
            _mediator.Setup(x => x.Send(It.IsAny<ValidationMSCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(validationRS);
            var orderDoc = TestDataProvider.GetTestData<OrderDocument>("NDCOrder\\OrderRetrieve\\OrderDoc");
            OrderRS orderRS = new() { Errors = null, OrderDocument = orderDoc };
            _mediator.Setup(x => x.Send(It.IsAny<OrderDataMSQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(orderRS);
            var brandRS = new BrandRS();
            _mediator.Setup(x => x.Send(It.IsAny<BrandMSCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(brandRS);
            var response = TestDataProvider.GetXmlTestData<IATA_OrderViewRS>("NDCOrder\\OrderRetrieve\\OrderViewRS"); 
            //Act
            var result = await _handler.Handle(new OrderRetrieveQuery() { OrderRetrieveRQ = request }, new CancellationToken());

            //Assert
            var items = (ErrorType[])result.Value.Value.Items;

            Assert.Contains(items, x => x.Code == "911");
        }

        [Fact]
        public async Task When_DistribtionchainLengthexceedstwo_inreturnRQ_ThrowError()
        {
            //Arrange
            _hostEnvironment.Setup(x => x.ContentRootPath).Returns(Directory.GetCurrentDirectory());
            
            var request = TestDataProvider.GetXmlTestData<IATA_OrderRetrieveRQ>("NDCOrder\\OrderRetrieve\\OrderRetrieveRQ_Dl_Return");

            request.DistributionChain.First().Ordinal = "2";
            request.DistributionChain.Last().Ordinal = "2";
            //Act
            var result = await _handler.Handle(new OrderRetrieveQuery() { OrderRetrieveRQ = request }, new CancellationToken());

            //Assert
            var items = (ErrorType[])result.Value.Value.Items;
            Assert.Contains(items, x => x.Code == "912");
        }

        [Fact]
        public async Task WHen_WrongContactDetailsinOrderRetreive_ReturnRQ_ThrowError()
        {
            //Arrange
            _hostEnvironment.Setup(x => x.ContentRootPath).Returns(Directory.GetCurrentDirectory());
            
            var request = TestDataProvider.GetXmlTestData<IATA_OrderRetrieveRQ>("NDCOrder\\OrderRetrieve\\OrderRetrieveRQ_Return");

            request.DistributionChain.First().ContactInfo.ContactInfoID = null;

            //Act
            var result = await _handler.Handle(new OrderRetrieveQuery() { OrderRetrieveRQ = request }, new CancellationToken());

            //Assert
            var items = (ErrorType[])result.Value.Value.Items;

            Assert.Contains(items, x => x.Code == "912");

        }

        [Fact]
        public async Task WHen_WrongEmailDetailsinOrderRetreive_ReturnRQ_ThrowError()
        {
            //Arrange
            _hostEnvironment.Setup(x => x.ContentRootPath).Returns(Directory.GetCurrentDirectory());
            
            var request = TestDataProvider.GetXmlTestData<IATA_OrderRetrieveRQ>("NDCOrder\\OrderRetrieve\\OrderRetrieveRQ_Return");

            request.DistributionChain.First().ContactInfo.EmailAddress.First().EmailAddressText = "EmailLengthIsGreaterThenSixtyFourThenItShoulThrowErrorToValidateThisScenario@Email.Com";
            request.DistributionChain.First().ContactInfo.EmailAddress.First().EmailAddressText = "1234";

            //Act
            var result = await _handler.Handle(new OrderRetrieveQuery() { OrderRetrieveRQ = request }, new CancellationToken());

            //Assert
            var items = (ErrorType[])result.Value.Value.Items;

            Assert.Contains(items, x => x.Code == "317");
        }

        [Fact]
        public async Task When_WrongRLOCDetails_inreturnRQ_ThrowError()
        {
            _hostEnvironment.Setup(x => x.ContentRootPath).Returns(Directory.GetCurrentDirectory());
            
            var request = TestDataProvider.GetXmlTestData<IATA_OrderRetrieveRQ>("NDCOrder\\OrderRetrieve\\OrderRetrieveRQ_Return");
            request.Request.OrderValidationFilterCriteria.OrderFilterCriteria.OrderID = null;
            request.Request.OrderValidationFilterCriteria.BookingRefFilterCriteria.BookingID = null;

            //Act
            var result = await _handler.Handle(new OrderRetrieveQuery() { OrderRetrieveRQ = request }, new CancellationToken());

            //Assert
            var items = (ErrorType[])result.Value.Value.Items;

            Assert.Contains(items, x => x.Code == "912");

        }

        [Fact]
        public async Task When_WrongOrderIDFormatDetails_inreturnRQ_ThrowError()
        {
            _hostEnvironment.Setup(x => x.ContentRootPath).Returns(Directory.GetCurrentDirectory());
           
            var request = TestDataProvider.GetXmlTestData<IATA_OrderRetrieveRQ>("NDCOrder\\OrderRetrieve\\OrderRetrieveRQ_Return");

            request.Request.OrderValidationFilterCriteria.OrderFilterCriteria.OrderID = "^[A-Z0-9]*$";

            //Act
            var result = await _handler.Handle(new OrderRetrieveQuery() { OrderRetrieveRQ = request }, new CancellationToken());

            //Assert
            var items = (ErrorType[])result.Value.Value.Items;

            Assert.Contains(items, x => x.Code == "914");

        }
        [Fact]
        public async Task When_WrongRLOCFormatDetails_inreturnRQ_ThrowError()
        {
            _hostEnvironment.Setup(x => x.ContentRootPath).Returns(Directory.GetCurrentDirectory());
            
            var request = TestDataProvider.GetXmlTestData<IATA_OrderRetrieveRQ>("NDCOrder\\OrderRetrieve\\OrderRetrieveRQ_Return");

            request.Request.OrderValidationFilterCriteria.BookingRefFilterCriteria.BookingID = "^[A-Z0-9]*$";
            request.Request.OrderValidationFilterCriteria.BookingRefFilterCriteria.BookingID = "YZ";

            //Act
            var result = await _handler.Handle(new OrderRetrieveQuery() { OrderRetrieveRQ = request }, new CancellationToken());

            //Assert
            var items = (ErrorType[])result.Value.Value.Items;

            Assert.Contains(items, x => x.Code == "914");

        }

        [Fact]
        public async Task WhenSeatAssignedToDisruptedSegment_ThrowWarning()
        {
            _hostEnvironment.Setup(x => x.ContentRootPath).Returns(Directory.GetCurrentDirectory());

            var request = TestDataProvider.GetXmlTestData<IATA_OrderRetrieveRQ>("NDCOrder\\OrderRetrieve\\OrderRetrieveRQ");
            var orderDoc = TestDataProvider.GetTestData<OrderDocument>("NDCOrder\\OrderRetrieve\\OrderDocWithHistory");
            var sabreOrderDoc = TestDataProvider.GetTestData<OrderDocument>("NDCOrder\\OrderRetrieve\\OrderSearchMSResponseForDisruption");
            _mediator.Setup(x => x.Send(It.IsAny<OrderDataMSQuery>() , It.IsAny<CancellationToken>())).ReturnsAsync(new OrderRS() { OrderDocument = orderDoc});
            var validationRS = TestDataProvider.GetTestData<ValidationRS>("NDCOrder\\OrderCreate\\ValidationMSResponse");
            _mediator.Setup(x => x.Send(It.IsAny<ValidationMSCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(validationRS);
            _mediator.Setup(x => x.Send(It.IsAny<OrderSearchMSCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(sabreOrderDoc);

            //Act
            var result = await _handler.Handle(new OrderRetrieveQuery() { OrderRetrieveRQ = request }, new CancellationToken());

            //Assert
            var orderViewRS = (OrderViewResponseType)result.Value.Value.Items.First();
            Assert.Contains(orderViewRS.Warning, x => x.Code == "214");
        }
    }
}
