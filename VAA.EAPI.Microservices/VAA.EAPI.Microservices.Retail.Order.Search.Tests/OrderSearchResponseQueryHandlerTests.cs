using MediatR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Domain.Exceptions;
using VAA.EAPI.Domain.Virgin.Models.Customer;
using VAA.EAPI.Domain.Virgin.Models.Products.Brand;
using VAA.EAPI.Infrastructure.PSS.Proxy.PNRCommonV6Proxy;
using VAA.EAPI.Infrastructure.PSS.Repository;
using VAA.EAPI.Infrastructure.PSS.Repository.Query;
using VAA.EAPI.Microservices.Retail.Order.Search.Domain;
using VAA.EAPI.Microservices.Retail.Order.Search.Application.Query;
using VAA.EAPI.Microservices.Retail.Order.Search.Domain;
using VAA.EAPI.Microservices.Retail.Order.Search.Helper.Cache.Interface;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Retail.Order.Search.Tests
{
    public class OrderSearchResponseQueryHandlerTests
    {
        private readonly Mock<IMediator> _mediator;
        private readonly Mock<ILogger<OrderSearchResponseQueryHandler>> _logger;
        private readonly OrderSearchResponseQueryHandler _handler;
        private readonly Mock<ICachedAirport> _mockCachedAirport;
        private readonly Mock<FunctionContext> _context;
        private readonly Mock<HttpRequestData> _httpRequestData;
        private readonly Mock<ICachedRFICData> _mockCachedRFICData;
        private readonly Mock<IProductName> _mockproductName;
        private readonly Mock<ICachedSegmentStatusCode> _mockCachedSegmentStatusCode;
        private readonly Mock<ICachedCabinType> _mockCachedCabinType;
        private readonly Mock<ICachedAmenities> _mockCachedAmenities;

        public OrderSearchResponseQueryHandlerTests()
        {
            _mediator = new Mock<IMediator>();
            _logger = new Mock<ILogger<OrderSearchResponseQueryHandler>>();
            _mockCachedAirport = new Mock<ICachedAirport>();
            _mockCachedRFICData = new Mock<ICachedRFICData>();
            _mockproductName = new Mock<IProductName>();
            _mockCachedSegmentStatusCode = new Mock<ICachedSegmentStatusCode>();
            _mockCachedCabinType = new Mock<ICachedCabinType>();
            _mockCachedAmenities = new Mock<ICachedAmenities>();

            _handler = new OrderSearchResponseQueryHandler(_mediator.Object, _logger.Object, _mockCachedAirport.Object, _mockCachedRFICData.Object, _mockproductName.Object, _mockCachedSegmentStatusCode.Object, _mockCachedCabinType.Object, _mockCachedAmenities.Object);
            _context = new Mock<FunctionContext>();
            _httpRequestData = new Mock<HttpRequestData>(_context.Object);
            OrderSearchTestsCommon.CreateTestEnvironmentSettings();
        }

        [Fact]

        public void Constructor_ShouldThrowNullExceptionWhenMediatorObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new OrderSearchResponseQueryHandler(null, _logger.Object, _mockCachedAirport.Object, _mockCachedRFICData.Object, _mockproductName.Object, null, _mockCachedCabinType.Object, _mockCachedAmenities.Object));
        }

        [Fact]

        public void Constructor_ShouldThrowNullExceptionWhenLoggerObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new OrderSearchResponseQueryHandler(_mediator.Object, null, _mockCachedAirport.Object, _mockCachedRFICData.Object, _mockproductName.Object, null, _mockCachedCabinType.Object, _mockCachedAmenities.Object));
        }

        [Fact]

        public void Constructor_ShouldThrowNullExceptionWhenCachedRFICDataObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new OrderSearchResponseQueryHandler(_mediator.Object, _logger.Object, _mockCachedAirport.Object, null, _mockproductName.Object, null, _mockCachedCabinType.Object, _mockCachedAmenities.Object));
        }
        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenCachedProductNameObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new OrderSearchResponseQueryHandler(_mediator.Object, _logger.Object, _mockCachedAirport.Object, _mockCachedRFICData.Object, null, null, _mockCachedCabinType.Object, _mockCachedAmenities.Object));
        }
        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenCachedSegmentStatusCodeObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new OrderSearchResponseQueryHandler(_mediator.Object, _logger.Object, _mockCachedAirport.Object, _mockCachedRFICData.Object, _mockproductName.Object, null, _mockCachedCabinType.Object, _mockCachedAmenities.Object));
        }
        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenCachedCabinTypeObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new OrderSearchResponseQueryHandler(_mediator.Object, _logger.Object, _mockCachedAirport.Object, _mockCachedRFICData.Object, _mockproductName.Object, null, null, _mockCachedAmenities.Object));
        }
        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenCachedAmenitiesObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new OrderSearchResponseQueryHandler(_mediator.Object, _logger.Object, _mockCachedAirport.Object, _mockCachedRFICData.Object, _mockproductName.Object, null, null, null));
        }

        [Fact]
        public async Task Handler_SuccessResponseForPNR()
        {
            //Arrange
            var request = TestDataProvider.GetTestData<OrderSearchResponseQuery>("OrderSearchMS\\OrderSearchValidRequest");
            var response = TestDataProvider.GetTestData<RetrieveResponseType>("OrderSearchMS\\Air4PnrRetrieveResponse");
            var ticketingResponse = TestDataProvider.GetTestData<EAPI.Infrastructure.PSS.Proxy.TicketingProcessV3Proxy.RetrieveResponseType>("OrderSearchMS\\TicketingRetreiveResponse");
            response.TeardownContextResponse.Pnr.SegmentDl.FirstOrDefault().CompartmentNumber = "3";
            var cabinTableResponse = TestDataProvider.GetTestData<List<CabinTypeDataEntity>>("OrderSearchMS\\CabinTypeData");
            var AmenitiesData = TestDataProvider.GetTestData<List<AmenitiesTableEntity>>("OrderSearchMS\\AmenitiesTableData");
            var headers = new HttpHeadersCollection
            {
                { "ChannelId", "NDC" }
            };
            _httpRequestData.Setup(x => x.Headers).Returns(headers);
            request.HttpRequest = _httpRequestData.Object;

            _mockCachedAirport.Setup(x => x.GetCachedAirport("LHR")).ReturnsAsync(new AirportLocation()
            {
                AirportCode = "LHR",
                AirportName = "Heathrow",
                CityCode = "LON"
            });
            _mockCachedAirport.Setup(x => x.GetCachedAirport("JFK")).ReturnsAsync(new AirportLocation()
            {
                AirportCode = "JFK",
                AirportName = "John F Kennedy Intl",
                CityCode = "NYC"
            });
            var flightCustomer = TestDataProvider.GetTestData<List<Customer>>("OrderSearchMS\\FlightCustomer");
            var customer = flightCustomer.FirstOrDefault();

            _mediator.Setup(x => x.Send(It.IsAny<PassengerV4ManageMultipleTravelDocsAndSecureFlightRepoQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(customer);
            _mediator.Setup(x => x.Send(It.IsAny<PNRRetrieveRepoQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(response);
            _mediator.Setup(x => x.Send(It.IsAny<TicketingRetrieveRepoQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(ticketingResponse);
            _mockproductName.Setup(x => x.GetProductName(It.IsAny<string>())).ReturnsAsync("Economy Flex");
            _mockCachedCabinType.Setup(x => x.GetCabinTypeData(It.IsAny<string>())).ReturnsAsync(cabinTableResponse);
            _mockCachedAmenities.Setup(x => x.GetAmenitiesInformation(It.IsAny<string>())).ReturnsAsync(AmenitiesData);

            //Act
            var result = await _handler.Handle(request, CancellationToken.None);

            //Assert
            Assert.Equal(Status.OK, result.Value.Status);
        }
        [Fact]
        public async Task Handler_SuccessResponseForeTicket()
        {
            //Arrange
            var request = TestDataProvider.GetTestData<OrderSearchResponseQuery>("OrderSearchMS\\OrderSearcheTicketValidRequest");
            var ticketResponse = TestDataProvider.GetTestData<EAPI.Infrastructure.PSS.Proxy.TicketingProcessV3Proxy.RetrieveResponseType>("OrderSearchMS\\TicketingRetreiveResponse");
            var cabinTableResponse = TestDataProvider.GetTestData<List<CabinTypeDataEntity>>("OrderSearchMS\\CabinTypeData");
            var AmenitiesData = TestDataProvider.GetTestData<List<AmenitiesTableEntity>>("OrderSearchMS\\AmenitiesTableData");
            var headers = new HttpHeadersCollection
            {
                { "ChannelId", "NDC" }
            };
            _httpRequestData.Setup(x => x.Headers).Returns(headers);
            request.HttpRequest = _httpRequestData.Object;
            var response = TestDataProvider.GetTestData<RetrieveResponseType>("OrderSearchMS\\Air4PnrRetrieveResponse");
            response.TeardownContextResponse.Pnr.SegmentDl.FirstOrDefault().CompartmentNumber = "3";

            _mockCachedAirport.Setup(x => x.GetCachedAirport("LHR")).ReturnsAsync(new AirportLocation()
            {
                AirportCode = "LHR",
                AirportName = "Heathrow",
                CityCode = "LON"
            });
            _mockCachedAirport.Setup(x => x.GetCachedAirport("JFK")).ReturnsAsync(new AirportLocation()
            {
                AirportCode = "JFK",
                AirportName = "John F Kennedy Intl",
                CityCode = "NYC"
            });
            var flightCustomer = TestDataProvider.GetTestData<List<Customer>>("OrderSearchMS\\FlightCustomer");
            var customer = flightCustomer.FirstOrDefault();

            _mediator.Setup(x => x.Send(It.IsAny<PassengerV4ManageMultipleTravelDocsAndSecureFlightRepoQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(customer);
            _mediator.Setup(x => x.Send(It.IsAny<TicketingRetrieveRepoQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(ticketResponse);
            _mediator.Setup(x => x.Send(It.IsAny<PNRRetrieveRepoQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(response);
            _mockproductName.Setup(x => x.GetProductName(It.IsAny<string>())).ReturnsAsync("Economy Flex");
            _mockCachedCabinType.Setup(x => x.GetCabinTypeData(It.IsAny<string>())).ReturnsAsync(cabinTableResponse);
            _mockCachedAmenities.Setup(x => x.GetAmenitiesInformation(It.IsAny<string>())).ReturnsAsync(AmenitiesData);

            //Act
            var result = await _handler.Handle(request, CancellationToken.None);

            //Assert
            Assert.Equal(Status.OK, result.Value.Status);
        }
        [Fact]

        public async Task Handler_FailureWhenInputIsNull()
        {
            //Arrange
            _mediator.Setup(x => x.Send(It.IsAny<PNRRetrieveRepoQuery>(), It.IsAny<CancellationToken>())).Throws<ArgumentNullException>();

            //Act
            var result = await _handler.Handle(null, CancellationToken.None);

            //Assert
            Assert.True(result.IsFailure);
        }

        [Fact]
        public async Task Handler_FailureWhenPNRIsNull()
        {
            //Arrange
            var request = TestDataProvider.GetTestData<OrderSearchResponseQuery>("OrderSearchMS\\OrderSearchRequestWithOutPNR");
            var headers = new HttpHeadersCollection
            {
                { "ChannelId", "NDC" }
            };
            _httpRequestData.Setup(x => x.Headers).Returns(headers);
            request.HttpRequest = _httpRequestData.Object;

            //Act
            var result = await _handler.Handle(request, CancellationToken.None);

            //Assert
            Assert.Equal(Status.BusinessException, result.Value.Status);
        }
        [Fact]
        public async Task Handler_FailureWheneTicketIsNull()
        {
            //Arrange
            var request = TestDataProvider.GetTestData<OrderSearchResponseQuery>("OrderSearchMS\\OrderSearchWithInvalidETicketNumber");
            var headers = new HttpHeadersCollection
            {
                { "ChannelId", "NDC" }
            };
            _httpRequestData.Setup(x => x.Headers).Returns(headers);
            request.HttpRequest = _httpRequestData.Object;

            //Act
            var result = await _handler.Handle(request, CancellationToken.None);

            //Assert
            Assert.Equal(Status.BusinessException, result.Value.Status);
        }
        [Fact]
        public async Task Handler_FailureWhenThrowsException()
        {
            //Arrange
            var request = TestDataProvider.GetTestData<OrderSearchResponseQuery>("OrderSearchMS\\OrderSearcheTicketValidRequest");
            var response = TestDataProvider.GetTestData<RetrieveResponseType>("OrderSearchMS\\Air4PnrRetrieveResponse");

            _mockCachedAirport.Setup(x => x.GetCachedAirport("LHR")).ReturnsAsync(new AirportLocation()
            {
                AirportCode = "LHR",
                AirportName = "Heathrow",
                CityCode = "LON"
            });
            _mockCachedAirport.Setup(x => x.GetCachedAirport("JFK")).ReturnsAsync(new AirportLocation()
            {
                AirportCode = "JFK",
                AirportName = "John F Kennedy Intl",
                CityCode = "NYC"
            });
            _mediator.Setup(x => x.Send(It.IsAny<TicketingRetrieveRepoQuery>(), It.IsAny<CancellationToken>())).Throws<Exception>();
            _mediator.Setup(x => x.Send(It.IsAny<PNRRetrieveRepoQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(response);

            //Act
            var result = await _handler.Handle(request, CancellationToken.None);

            //Assert
            Assert.True(result.IsFailure);
        }

        [Fact]
        public async Task Handler_FailureWhenThrowsServiceExceptionForPNRNotFound()
        {
            //Arrange
            var request = TestDataProvider.GetTestData<OrderSearchResponseQuery>("OrderSearchMS\\OrderSearcheTicketValidRequest");
            var response = TestDataProvider.GetTestData<RetrieveResponseType>("OrderSearchMS\\Air4PnrRetrieveResponse");

            _mockCachedAirport.Setup(x => x.GetCachedAirport("LHR")).ReturnsAsync(new AirportLocation()
            {
                AirportCode = "LHR",
                AirportName = "Heathrow",
                CityCode = "LON"
            });
            _mockCachedAirport.Setup(x => x.GetCachedAirport("JFK")).ReturnsAsync(new AirportLocation()
            {
                AirportCode = "JFK",
                AirportName = "John F Kennedy Intl",
                CityCode = "NYC"
            });
            _mediator.Setup(x => x.Send(It.IsAny<TicketingRetrieveRepoQuery>(), It.IsAny<CancellationToken>())).ThrowsAsync(new ExternalServiceIntegrationException("1079", new Exception("NO MATCH FOUND")));
            _mediator.Setup(x => x.Send(It.IsAny<PNRRetrieveRepoQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(response);

            //Act
            var result = await _handler.Handle(request, CancellationToken.None);

            //Assert
            Assert.True(result.IsFailure);
        }
        [Fact]
        public async Task Handler_FailureWhenThrowsServiceException()
        {
            //Arrange
            var request = TestDataProvider.GetTestData<OrderSearchResponseQuery>("OrderSearchMS\\OrderSearcheTicketValidRequest");
            var response = TestDataProvider.GetTestData<RetrieveResponseType>("OrderSearchMS\\Air4PnrRetrieveResponse");

            _mockCachedAirport.Setup(x => x.GetCachedAirport("LHR")).ReturnsAsync(new AirportLocation()
            {
                AirportCode = "LHR",
                AirportName = "Heathrow",
                CityCode = "LON"
            });
            _mockCachedAirport.Setup(x => x.GetCachedAirport("JFK")).ReturnsAsync(new AirportLocation()
            {
                AirportCode = "JFK",
                AirportName = "John F Kennedy Intl",
                CityCode = "NYC"
            });
            _mediator.Setup(x => x.Send(It.IsAny<TicketingRetrieveRepoQuery>(), It.IsAny<CancellationToken>())).ThrowsAsync(new ExternalServiceIntegrationException("G035", new Exception("Exception in service")));
            _mediator.Setup(x => x.Send(It.IsAny<PNRRetrieveRepoQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(response);

            //Act
            var result = await _handler.Handle(request, CancellationToken.None);

            //Assert
            Assert.True(result.IsFailure);
        }

        [Fact]
        public async Task Handler_FailureResponseWithWrongPassengerNamesInPNR()
        {
            //Arrange
            var request = TestDataProvider.GetTestData<OrderSearchResponseQuery>("OrderSearchMS\\OrderSearchValidRequestInvalidPtc");
            var response = TestDataProvider.GetTestData<RetrieveResponseType>("OrderSearchMS\\Air4PnrRetrieveResponse");
            _mediator.Setup(x => x.Send(It.IsAny<PNRRetrieveRepoQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(response);

            //Act
            var result = await _handler.Handle(request, CancellationToken.None);

            //Assert
            Assert.True(result.IsFailure);
        }

        [Fact]
        public async Task Handler_ShouldThrowBusinessException_WhenPassengerNamesMisMatch_ForSELFSERVICE()
        {
            //Arrange
            var request = TestDataProvider.GetTestData<OrderSearchResponseQuery>("OrderSearchMS\\OrderSearchValidRequestInvalidPtc");
            var headers = new HttpHeadersCollection
            {
                { "ChannelId", "SELFSERVICE" },
                { "HashKey" , "dummy" }
            };
            _httpRequestData.Setup(x => x.Headers).Returns(headers);
            request.HttpRequest = _httpRequestData.Object;
            var response = TestDataProvider.GetTestData<RetrieveResponseType>("OrderSearchMS\\Air4PnrRetrieveResponse");
            _mediator.Setup(x => x.Send(It.IsAny<PNRRetrieveRepoQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(response);

            //Act
            var result = await _handler.Handle(request, CancellationToken.None);

            //Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(Status.BusinessException, result.Value.Status);
            Assert.Contains(result.Value.Errors, a => a.Code == "1080");
        }

        [Fact]
        public async Task Handler_ShouldThrowBusinessException_WhenChannelIDIsMissing()
        {
            var request = TestDataProvider.GetTestData<OrderSearchResponseQuery>("OrderSearchMS\\OrderSearchValidRequestInvalidPtc");
            _httpRequestData.Setup(x => x.Headers).Returns(new HttpHeadersCollection());
            request.HttpRequest = _httpRequestData.Object;

            //Act
            var result = await _handler.Handle(request, CancellationToken.None);

            //Assert
            Assert.Equal(Status.BusinessException, result.Value.Status);
            Assert.Contains(result.Value.Errors, a => a.Code == "1088");
        }

        [Fact]
        public async Task Handler_ShouldThrowBusinessException_WhenChannelIDIsInvalid()
        {
            var request = TestDataProvider.GetTestData<OrderSearchResponseQuery>("OrderSearchMS\\OrderSearchValidRequestInvalidPtc");
            var headers = new HttpHeadersCollection
            {
                { "ChannelId", "Invalid" }
            };
            _httpRequestData.Setup(x => x.Headers).Returns(headers);
            request.HttpRequest = _httpRequestData.Object;

            //Act
            var result = await _handler.Handle(request, CancellationToken.None);

            //Assert
            Assert.Equal(Status.BusinessException, result.Value.Status);
            Assert.Contains(result.Value.Errors, a => a.Code == "1088");
        }

        [Fact]
        public async Task Handler_ShouldReturnRFICData_WhenPaidSeatIsInPnr()
        {
            //Arrange
            var request = TestDataProvider.GetTestData<OrderSearchResponseQuery>("OrderSearchMS\\OrderSearcheTicketValidRequest");
            var headers = new HttpHeadersCollection
            {
                { "ChannelId", "NDC" }
            };
            _httpRequestData.Setup(x => x.Headers).Returns(headers);
            request.HttpRequest = _httpRequestData.Object;

            var pnrResponse = TestDataProvider.GetTestData<RetrieveResponseType>("OrderSearchMS\\Air4PnrRetrieveResponse");
            var ticketingResponse = TestDataProvider.GetTestData<EAPI.Infrastructure.PSS.Proxy.TicketingProcessV3Proxy.RetrieveResponseType>("OrderSearchMS\\TicketingRetreiveResponse");
            pnrResponse.TeardownContextResponse.Pnr.SegmentDl.FirstOrDefault().CompartmentNumber = "3";
            var cabinTableResponse = TestDataProvider.GetTestData<List<CabinTypeDataEntity>>("OrderSearchMS\\CabinTypeData");
            var AmenitiesData = TestDataProvider.GetTestData<List<AmenitiesTableEntity>>("OrderSearchMS\\AmenitiesTableData");
            var flightCustomer = TestDataProvider.GetTestData<List<Customer>>("OrderSearchMS\\FlightCustomer");
            var customer = flightCustomer.FirstOrDefault();

            _mediator.Setup(x => x.Send(It.IsAny<PassengerV4ManageMultipleTravelDocsAndSecureFlightRepoQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(customer);
            _mediator.Setup(x => x.Send(It.IsAny<PNRRetrieveRepoQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(pnrResponse);
            _mediator.Setup(x => x.Send(It.IsAny<TicketingRetrieveRepoQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(ticketingResponse);
            _mockproductName.Setup(x => x.GetProductName(It.IsAny<string>())).ReturnsAsync("Economy Flex");
            _mockCachedCabinType.Setup(x => x.GetCabinTypeData(It.IsAny<string>())).ReturnsAsync(cabinTableResponse);
            _mockCachedAmenities.Setup(x => x.GetAmenitiesInformation(It.IsAny<string>())).ReturnsAsync(AmenitiesData);

            _mockCachedRFICData.Setup(x => x.GetCachedRFICData("Seat")).ReturnsAsync(new RFICTableEntity() { Code = "A", SubCode = "0B5" });

            //Act
            var result = await _handler.Handle(request, CancellationToken.None);

            //Assert
            Assert.Contains(result.Value.Value.OrderItems, x => x.AccountableDocumentRefs.Any(x => x.ReasonForIssuance != null && x.ReasonForIssuance.RFIC == "A"));
            Assert.Contains(result.Value.Value.OrderItems, x => x.AccountableDocumentRefs.Any(x => x.ReasonForIssuance != null && x.ReasonForIssuance.RFISC == "0B5"));
        }


        [Fact]
        public async Task Handler_ShouldReturnCabinTypeData_ForBusniess()
        {
            //Arrange
            var request = TestDataProvider.GetTestData<OrderSearchResponseQuery>("OrderSearchMS\\OrderSearcheTicketValidRequest");
            var headers = new HttpHeadersCollection
            {
                { "ChannelId", "NDC" }
            };
            _httpRequestData.Setup(x => x.Headers).Returns(headers);
            request.HttpRequest = _httpRequestData.Object;

            var pnrResponse = TestDataProvider.GetTestData<RetrieveResponseType>("OrderSearchMS\\Air4PnrRetrieveResponse");
            pnrResponse.TeardownContextResponse.Pnr.SegmentDl.FirstOrDefault().CompartmentNumber = "2";
            var cabinCode = pnrResponse.TeardownContextResponse.Pnr.SegmentDl.FirstOrDefault().CompartmentNumber;
            var ticketingResponse = TestDataProvider.GetTestData<EAPI.Infrastructure.PSS.Proxy.TicketingProcessV3Proxy.RetrieveResponseType>("OrderSearchMS\\TicketingRetreiveResponse");
            var cabinTableResponse = TestDataProvider.GetTestData<List<CabinTypeDataEntity>>("OrderSearchMS\\CabinTypeData");
            var AmenitiesData = TestDataProvider.GetTestData<List<AmenitiesTableEntity>>("OrderSearchMS\\AmenitiesTableData");
            var flightCustomer = TestDataProvider.GetTestData<List<Customer>>("OrderSearchMS\\FlightCustomer");
            var customer = flightCustomer.FirstOrDefault();

            _mediator.Setup(x => x.Send(It.IsAny<PassengerV4ManageMultipleTravelDocsAndSecureFlightRepoQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(customer);
            _mediator.Setup(x => x.Send(It.IsAny<PNRRetrieveRepoQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(pnrResponse);
            _mediator.Setup(x => x.Send(It.IsAny<TicketingRetrieveRepoQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(ticketingResponse);

            _mockCachedRFICData.Setup(x => x.GetCachedRFICData("Seat")).ReturnsAsync(new RFICTableEntity() { Code = "A", SubCode = "0B5" });
            _mockCachedCabinType.Setup(X => X.GetCabinTypeData(It.IsAny<string>())).ReturnsAsync(cabinTableResponse);
            _mockproductName.Setup(x => x.GetProductName(It.IsAny<string>())).ReturnsAsync("Economy Flex");
            _mockCachedAmenities.Setup(x => x.GetAmenitiesInformation(It.IsAny<string>())).ReturnsAsync(AmenitiesData);

            //Act
            var result = await _handler.Handle(request, CancellationToken.None);

            //Assert
            Assert.NotNull(result.Value.Value.SegmentDetails.FirstOrDefault().CabinType);
            Assert.Equal("2", result.Value.Value.SegmentDetails.FirstOrDefault().CabinType.CabinCode);
            Assert.Equal("Business", result.Value.Value.SegmentDetails.FirstOrDefault().CabinType.CabinName);
        }
        [Fact]
        public async Task Handler_ShouldReturnCabinTypeData_ForPremiumEconomy()
        {
            //Arrange
            var request = TestDataProvider.GetTestData<OrderSearchResponseQuery>("OrderSearchMS\\OrderSearcheTicketValidRequest");
            var headers = new HttpHeadersCollection
            {
                { "ChannelId", "NDC" }
            };
            _httpRequestData.Setup(x => x.Headers).Returns(headers);
            request.HttpRequest = _httpRequestData.Object;

            var pnrResponse = TestDataProvider.GetTestData<RetrieveResponseType>("OrderSearchMS\\Air4PnrRetrieveResponse");
            pnrResponse.TeardownContextResponse.Pnr.SegmentDl.FirstOrDefault().CompartmentNumber = "5";
            var cabinCode = pnrResponse.TeardownContextResponse.Pnr.SegmentDl.FirstOrDefault().CompartmentNumber;
            var ticketingResponse = TestDataProvider.GetTestData<EAPI.Infrastructure.PSS.Proxy.TicketingProcessV3Proxy.RetrieveResponseType>("OrderSearchMS\\TicketingRetreiveResponse");
            var cabinTableResponse = TestDataProvider.GetTestData<List<CabinTypeDataEntity>>("OrderSearchMS\\CabinTypeData");
            var AmenitiesData = TestDataProvider.GetTestData<List<AmenitiesTableEntity>>("OrderSearchMS\\AmenitiesTableData");
            var flightCustomer = TestDataProvider.GetTestData<List<Customer>>("OrderSearchMS\\FlightCustomer");
            var customer = flightCustomer.FirstOrDefault();

            _mediator.Setup(x => x.Send(It.IsAny<PassengerV4ManageMultipleTravelDocsAndSecureFlightRepoQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(customer);
            _mediator.Setup(x => x.Send(It.IsAny<PNRRetrieveRepoQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(pnrResponse);
            _mediator.Setup(x => x.Send(It.IsAny<TicketingRetrieveRepoQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(ticketingResponse);

            _mockCachedRFICData.Setup(x => x.GetCachedRFICData("Seat")).ReturnsAsync(new RFICTableEntity() { Code = "A", SubCode = "0B5" });
            _mockCachedCabinType.Setup(X => X.GetCabinTypeData(It.IsAny<string>())).ReturnsAsync(cabinTableResponse);
            _mockproductName.Setup(x => x.GetProductName(It.IsAny<string>())).ReturnsAsync("Economy Flex");
            _mockCachedAmenities.Setup(x => x.GetAmenitiesInformation(It.IsAny<string>())).ReturnsAsync(AmenitiesData);


            //Act
            var result = await _handler.Handle(request, CancellationToken.None);

            //Assert
            Assert.NotNull(result.Value.Value.SegmentDetails.FirstOrDefault().CabinType);
            Assert.Equal("4", result.Value.Value.SegmentDetails.FirstOrDefault().CabinType.CabinCode);
            Assert.Equal("Premium Economy", result.Value.Value.SegmentDetails.FirstOrDefault().CabinType.CabinName);
        }
        [Fact]
        public async Task Handler_ShouldReturnCabinTypeData_ForEconomy()
        {
            //Arrange
            var request = TestDataProvider.GetTestData<OrderSearchResponseQuery>("OrderSearchMS\\OrderSearcheTicketValidRequest");
            var headers = new HttpHeadersCollection
            {
                { "ChannelId", "NDC" }
            };
            _httpRequestData.Setup(x => x.Headers).Returns(headers);
            request.HttpRequest = _httpRequestData.Object;

            var pnrResponse = TestDataProvider.GetTestData<RetrieveResponseType>("OrderSearchMS\\Air4PnrRetrieveResponse");
            pnrResponse.TeardownContextResponse.Pnr.SegmentDl.FirstOrDefault().CompartmentNumber = "3";
            var cabinCode = pnrResponse.TeardownContextResponse.Pnr.SegmentDl.FirstOrDefault().CompartmentNumber;
            var ticketingResponse = TestDataProvider.GetTestData<EAPI.Infrastructure.PSS.Proxy.TicketingProcessV3Proxy.RetrieveResponseType>("OrderSearchMS\\TicketingRetreiveResponse");
            var cabinTableResponse = TestDataProvider.GetTestData<List<CabinTypeDataEntity>>("OrderSearchMS\\CabinTypeData");
            var AmenitiesData = TestDataProvider.GetTestData<List<AmenitiesTableEntity>>("OrderSearchMS\\AmenitiesTableData");
            var flightCustomer = TestDataProvider.GetTestData<List<Customer>>("OrderSearchMS\\FlightCustomer");
            var customer = flightCustomer.FirstOrDefault();

            _mediator.Setup(x => x.Send(It.IsAny<PassengerV4ManageMultipleTravelDocsAndSecureFlightRepoQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(customer);
            _mediator.Setup(x => x.Send(It.IsAny<PNRRetrieveRepoQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(pnrResponse);
            _mediator.Setup(x => x.Send(It.IsAny<TicketingRetrieveRepoQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(ticketingResponse);

            _mockCachedRFICData.Setup(x => x.GetCachedRFICData("Seat")).ReturnsAsync(new RFICTableEntity() { Code = "A", SubCode = "0B5" });
            _mockCachedCabinType.Setup(X => X.GetCabinTypeData(It.IsAny<string>())).ReturnsAsync(cabinTableResponse);
            _mockproductName.Setup(x => x.GetProductName(It.IsAny<string>())).ReturnsAsync("Economy Flex");
            _mockCachedAmenities.Setup(x => x.GetAmenitiesInformation(It.IsAny<string>())).ReturnsAsync(AmenitiesData);

            //Act
            var result = await _handler.Handle(request, CancellationToken.None);

            //Assert
            Assert.NotNull(result.Value.Value.SegmentDetails.FirstOrDefault().CabinType);
            Assert.Equal("5", result.Value.Value.SegmentDetails.FirstOrDefault().CabinType.CabinCode);
            Assert.Equal("Economy", result.Value.Value.SegmentDetails.FirstOrDefault().CabinType.CabinName);
        }
        [Fact]
        public async Task Handler_ShouldReturnAmenities_ForInterLineFlights()
        {
            //Arrange
            var request = TestDataProvider.GetTestData<OrderSearchResponseQuery>("OrderSearchMS\\OrderSearcheTicketValidRequest");
            var headers = new HttpHeadersCollection
            {
                { "ChannelId", "NDC" }
            };
            _httpRequestData.Setup(x => x.Headers).Returns(headers);
            request.HttpRequest = _httpRequestData.Object;

            var pnrResponse = TestDataProvider.GetTestData<RetrieveResponseType>("OrderSearchMS\\Air4PnrRetrieveResponse");
            pnrResponse.TeardownContextResponse.Pnr.SegmentDl.FirstOrDefault().CompartmentNumber = "3";
            var cabinCode = pnrResponse.TeardownContextResponse.Pnr.SegmentDl.FirstOrDefault().CompartmentNumber;
            var ticketingResponse = TestDataProvider.GetTestData<EAPI.Infrastructure.PSS.Proxy.TicketingProcessV3Proxy.RetrieveResponseType>("OrderSearchMS\\TicketingRetreiveResponse");
            var cabinTableResponse = TestDataProvider.GetTestData<List<CabinTypeDataEntity>>("OrderSearchMS\\CabinTypeData");
            var AmenitiesData = TestDataProvider.GetTestData<List<AmenitiesTableEntity>>("OrderSearchMS\\AmenitiesTableData");
            var flightCustomer = TestDataProvider.GetTestData<List<Customer>>("OrderSearchMS\\FlightCustomer");
            var customer = flightCustomer.FirstOrDefault();

            _mediator.Setup(x => x.Send(It.IsAny<PassengerV4ManageMultipleTravelDocsAndSecureFlightRepoQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(customer);
            _mediator.Setup(x => x.Send(It.IsAny<PNRRetrieveRepoQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(pnrResponse);
            _mediator.Setup(x => x.Send(It.IsAny<TicketingRetrieveRepoQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(ticketingResponse);

            _mockCachedRFICData.Setup(x => x.GetCachedRFICData("Seat")).ReturnsAsync(new RFICTableEntity() { Code = "A", SubCode = "0B5" });
            _mockCachedCabinType.Setup(X => X.GetCabinTypeData(It.IsAny<string>())).ReturnsAsync(cabinTableResponse);
            _mockproductName.Setup(x => x.GetProductName(It.IsAny<string>())).ReturnsAsync("Economy Flex");
            _mockCachedAmenities.Setup(x => x.GetAmenitiesInformation(It.IsAny<string>())).ReturnsAsync(AmenitiesData);

            //Act
            var result = await _handler.Handle(request, CancellationToken.None);

            //Assert
            Assert.NotNull(result.Value.Value.ProductComponents);
            Assert.NotNull(result.Value.Value.OrderItems.First().Services.First().ProductComponentRefIds);
            Assert.Equal("OAE03", result.Value.Value.ProductComponents.First().Code);
            Assert.Equal("Operated by a partner airline", result.Value.Value.ProductComponents.First().Description);
            Assert.Equal("PRDC1", result.Value.Value.ProductComponents.First().Id);
        }
        [Fact]
        public async Task Handler_ShouldReturnAmenities_ForCodeShareFlights()
        {
            //Arrange
            var request = TestDataProvider.GetTestData<OrderSearchResponseQuery>("OrderSearchMS\\OrderSearcheTicketValidRequest");
            var headers = new HttpHeadersCollection
            {
                { "ChannelId", "NDC" }
            };
            _httpRequestData.Setup(x => x.Headers).Returns(headers);
            request.HttpRequest = _httpRequestData.Object;

            var pnrResponse = TestDataProvider.GetTestData<RetrieveResponseType>("OrderSearchMS\\Air4PnrRetrieveResponse");
            pnrResponse.TeardownContextResponse.Pnr.SegmentAdditionalInfo.FirstOrDefault().OperatingCarrierCode = "DL";
            pnrResponse.TeardownContextResponse.Pnr.SegmentDl.FirstOrDefault().CompartmentNumber = "2";
            var cabinCode = pnrResponse.TeardownContextResponse.Pnr.SegmentDl.FirstOrDefault().CompartmentNumber;
            var ticketingResponse = TestDataProvider.GetTestData<EAPI.Infrastructure.PSS.Proxy.TicketingProcessV3Proxy.RetrieveResponseType>("OrderSearchMS\\TicketingRetreiveResponse");
            var cabinTableResponse = TestDataProvider.GetTestData<List<CabinTypeDataEntity>>("OrderSearchMS\\CabinTypeData");
            var AmenitiesData = TestDataProvider.GetTestData<List<AmenitiesTableEntity>>("OrderSearchMS\\AmenitiesTableData");
            var flightCustomer = TestDataProvider.GetTestData<List<Customer>>("OrderSearchMS\\FlightCustomer");
            var customer = flightCustomer.FirstOrDefault();

            _mediator.Setup(x => x.Send(It.IsAny<PassengerV4ManageMultipleTravelDocsAndSecureFlightRepoQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(customer);
            _mediator.Setup(x => x.Send(It.IsAny<PNRRetrieveRepoQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(pnrResponse);
            _mediator.Setup(x => x.Send(It.IsAny<TicketingRetrieveRepoQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(ticketingResponse);

            _mockCachedRFICData.Setup(x => x.GetCachedRFICData("Seat")).ReturnsAsync(new RFICTableEntity() { Code = "A", SubCode = "0B5" });
            _mockCachedCabinType.Setup(X => X.GetCabinTypeData(It.IsAny<string>())).ReturnsAsync(cabinTableResponse);
            _mockproductName.Setup(x => x.GetProductName(It.IsAny<string>())).ReturnsAsync("Upper Flex");
            _mockCachedAmenities.Setup(x => x.GetAmenitiesInformation(It.IsAny<string>())).ReturnsAsync(AmenitiesData);

            //Act
            var result = await _handler.Handle(request, CancellationToken.None);

            //Assert
            Assert.NotNull(result.Value.Value.ProductComponents);
            Assert.NotNull(result.Value.Value.OrderItems.First().Services.First().ProductComponentRefIds);
            Assert.Equal("DLDO02", result.Value.Value.ProductComponents.First().Code);
            Assert.Equal("Cabin features may differ", result.Value.Value.ProductComponents.First().Description);
            Assert.Equal("PRDC1", result.Value.Value.ProductComponents.First().Id);
        }
        [Fact]
        public async Task Handler_ShouldReturnAmenities_ForVSFlights()
        {
            //Arrang;
            var request = TestDataProvider.GetTestData<OrderSearchResponseQuery>("OrderSearchMS\\OrderSearchValidRequest");
            request.OrderSearchRequest.SearchFilterCriteria.bookingReferenceCriteria.customerName.GivenName = "David";
            request.OrderSearchRequest.SearchFilterCriteria.bookingReferenceCriteria.customerName.SurName = "Smith";
            var headers = new HttpHeadersCollection
            {
                { "ChannelId", "NDC" }
            };
            _httpRequestData.Setup(x => x.Headers).Returns(headers);
            request.HttpRequest = _httpRequestData.Object;

            var pnrResponse = TestDataProvider.GetTestData<RetrieveResponseType>("OrderSearchMS\\Air4PnrRetrieveResponseVSFlight");
            pnrResponse.TeardownContextResponse.Pnr.SegmentAdditionalInfo.FirstOrDefault().CassEquipment = null;
            var cabinCode = pnrResponse.TeardownContextResponse.Pnr.SegmentDl.FirstOrDefault().CompartmentNumber;
            var ticketingResponse = TestDataProvider.GetTestData<EAPI.Infrastructure.PSS.Proxy.TicketingProcessV3Proxy.RetrieveResponseType>("OrderSearchMS\\TicketingRetreiveResponse");
            var cabinTableResponse = TestDataProvider.GetTestData<List<CabinTypeDataEntity>>("OrderSearchMS\\CabinTypeData");
            var AmenitiesData = TestDataProvider.GetTestData<List<AmenitiesTableEntity>>("OrderSearchMS\\AmenitiesTableData");
            var brandApiResponse = TestDataProvider.GetTestData<List<FlightSegment>>("OrderSearchMS\\BrandApiResponse");
            var flightCustomer = TestDataProvider.GetTestData<List<Customer>>("OrderSearchMS\\FlightCustomer");
            var customer = flightCustomer.FirstOrDefault();

            _mediator.Setup(x => x.Send(It.IsAny<PassengerV4ManageMultipleTravelDocsAndSecureFlightRepoQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(customer);
            _mediator.Setup(x => x.Send(It.IsAny<PNRRetrieveRepoQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(pnrResponse);
            _mediator.Setup(x => x.Send(It.IsAny<TicketingRetrieveRepoQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(ticketingResponse);

            _mockCachedRFICData.Setup(x => x.GetCachedRFICData("Seat")).ReturnsAsync(new RFICTableEntity() { Code = "A", SubCode = "0B5" });
            _mediator.Setup(x => x.Send(It.IsAny<RetrieveAmenitiesQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(brandApiResponse);

            _mockCachedCabinType.Setup(X => X.GetCabinTypeData(It.IsAny<string>())).ReturnsAsync(cabinTableResponse);
            _mockproductName.Setup(x => x.GetProductName(It.IsAny<string>())).ReturnsAsync("Upper Flex");
            _mockCachedAmenities.Setup(x => x.GetAmenitiesInformation(It.IsAny<string>())).ReturnsAsync(AmenitiesData);

            //Act
            var result = await _handler.Handle(request, CancellationToken.None);

            //Assert
            Assert.NotNull(result.Value.Value.ProductComponents);
            Assert.NotNull(result.Value.Value.OrderItems.First().Services.First().ProductComponentRefIds);
            Assert.Equal("0AX3VS", result.Value.Value.ProductComponents.First().Code);
            Assert.Equal("Meal & Drinks - Economy Meal", result.Value.Value.ProductComponents.First().Description);
            Assert.Equal("PRDC1", result.Value.Value.ProductComponents.First().Id);
        }
        [Fact]
        public async Task Handler_ShouldReturnCountryCode_ForGoodRequest()
        {
            //Arrang;
            var request = TestDataProvider.GetTestData<OrderSearchResponseQuery>("OrderSearchMS\\OrderSearchValidRequest");
            request.OrderSearchRequest.SearchFilterCriteria.bookingReferenceCriteria.customerName.GivenName = "David";
            request.OrderSearchRequest.SearchFilterCriteria.bookingReferenceCriteria.customerName.SurName = "Smith";
            var headers = new HttpHeadersCollection
            {
                { "ChannelId", "NDC" }
            };
            _httpRequestData.Setup(x => x.Headers).Returns(headers);
            request.HttpRequest = _httpRequestData.Object;

            var pnrResponse = TestDataProvider.GetTestData<RetrieveResponseType>("OrderSearchMS\\Air4PnrRetrieveResponseVSFlight");
            pnrResponse.TeardownContextResponse.Pnr.SegmentAdditionalInfo.FirstOrDefault().CassEquipment = null;
            var cabinCode = pnrResponse.TeardownContextResponse.Pnr.SegmentDl.FirstOrDefault().CompartmentNumber;
            var ticketingResponse = TestDataProvider.GetTestData<EAPI.Infrastructure.PSS.Proxy.TicketingProcessV3Proxy.RetrieveResponseType>("OrderSearchMS\\TicketingRetreiveResponse");
            var cabinTableResponse = TestDataProvider.GetTestData<List<CabinTypeDataEntity>>("OrderSearchMS\\CabinTypeData");
            var AmenitiesData = TestDataProvider.GetTestData<List<AmenitiesTableEntity>>("OrderSearchMS\\AmenitiesTableData");
            var brandApiResponse = TestDataProvider.GetTestData<List<FlightSegment>>("OrderSearchMS\\BrandApiResponse");
            var flightCustomer = TestDataProvider.GetTestData<List<Customer>>("OrderSearchMS\\FlightCustomer");
            var customer = flightCustomer.FirstOrDefault();

            _mediator.Setup(x => x.Send(It.IsAny<PassengerV4ManageMultipleTravelDocsAndSecureFlightRepoQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(customer);
            _mediator.Setup(x => x.Send(It.IsAny<PNRRetrieveRepoQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(pnrResponse);
            _mediator.Setup(x => x.Send(It.IsAny<TicketingRetrieveRepoQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(ticketingResponse);

            _mockCachedRFICData.Setup(x => x.GetCachedRFICData("Seat")).ReturnsAsync(new RFICTableEntity() { Code = "A", SubCode = "0B5" });
            _mediator.Setup(x => x.Send(It.IsAny<RetrieveAmenitiesQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(brandApiResponse);

            _mockCachedCabinType.Setup(X => X.GetCabinTypeData(It.IsAny<string>())).ReturnsAsync(cabinTableResponse);
            _mockproductName.Setup(x => x.GetProductName(It.IsAny<string>())).ReturnsAsync("Upper Flex");
            _mockCachedAmenities.Setup(x => x.GetAmenitiesInformation(It.IsAny<string>())).ReturnsAsync(AmenitiesData);

            //Act
            var result = await _handler.Handle(request, CancellationToken.None);

            //Assert
            Assert.NotNull(result.Value.Value.CustomerDetails.FirstOrDefault().Contacts.Countrycode);
            Assert.Equal("GB", result.Value.Value.CustomerDetails.FirstOrDefault().Contacts.Countrycode);
        }
        [Fact]
        public async Task Handler_ShouldReturnEmptyString_ForMissingContactInfo()
        {
            //Arrang;
            var request = TestDataProvider.GetTestData<OrderSearchResponseQuery>("OrderSearchMS\\OrderSearchValidRequest");
            request.OrderSearchRequest.SearchFilterCriteria.bookingReferenceCriteria.customerName.GivenName = "David";
            request.OrderSearchRequest.SearchFilterCriteria.bookingReferenceCriteria.customerName.SurName = "Smith";
            var headers = new HttpHeadersCollection
            {
                { "ChannelId", "NDC" }
            };
            _httpRequestData.Setup(x => x.Headers).Returns(headers);
            request.HttpRequest = _httpRequestData.Object;

            var pnrResponse = TestDataProvider.GetTestData<RetrieveResponseType>("OrderSearchMS\\Air4PnrRetrieveResponseVSFlight");
            pnrResponse.TeardownContextResponse.Pnr.SegmentAdditionalInfo.FirstOrDefault().CassEquipment = null;
            pnrResponse.TeardownContextResponse.Pnr.Phone.Where(x => x.PhoneFieldNumber == "01").FirstOrDefault().FreeformText = "072324343434";
            pnrResponse.TeardownContextResponse.Pnr.Phone.Where(x => x.PhoneFieldNumber == "02").FirstOrDefault().FreeformText = null;
            var cabinCode = pnrResponse.TeardownContextResponse.Pnr.SegmentDl.FirstOrDefault().CompartmentNumber;
            var ticketingResponse = TestDataProvider.GetTestData<EAPI.Infrastructure.PSS.Proxy.TicketingProcessV3Proxy.RetrieveResponseType>("OrderSearchMS\\TicketingRetreiveResponse");
            var cabinTableResponse = TestDataProvider.GetTestData<List<CabinTypeDataEntity>>("OrderSearchMS\\CabinTypeData");
            var AmenitiesData = TestDataProvider.GetTestData<List<AmenitiesTableEntity>>("OrderSearchMS\\AmenitiesTableData");
            var brandApiResponse = TestDataProvider.GetTestData<List<FlightSegment>>("OrderSearchMS\\BrandApiResponse");
            var flightCustomer = TestDataProvider.GetTestData<List<Customer>>("OrderSearchMS\\FlightCustomer");
            var customer = flightCustomer.FirstOrDefault();

            _mediator.Setup(x => x.Send(It.IsAny<PassengerV4ManageMultipleTravelDocsAndSecureFlightRepoQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(customer);
            _mediator.Setup(x => x.Send(It.IsAny<PNRRetrieveRepoQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(pnrResponse);
            _mediator.Setup(x => x.Send(It.IsAny<TicketingRetrieveRepoQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(ticketingResponse);

            _mockCachedRFICData.Setup(x => x.GetCachedRFICData("Seat")).ReturnsAsync(new RFICTableEntity() { Code = "A", SubCode = "0B5" });
            _mediator.Setup(x => x.Send(It.IsAny<RetrieveAmenitiesQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(brandApiResponse);

            _mockCachedCabinType.Setup(X => X.GetCabinTypeData(It.IsAny<string>())).ReturnsAsync(cabinTableResponse);
            _mockproductName.Setup(x => x.GetProductName(It.IsAny<string>())).ReturnsAsync("Upper Flex");
            _mockCachedAmenities.Setup(x => x.GetAmenitiesInformation(It.IsAny<string>())).ReturnsAsync(AmenitiesData);

            //Act
            var result = await _handler.Handle(request, CancellationToken.None);

            //Assert
            Assert.NotNull(result.Value.Value.CustomerDetails.FirstOrDefault().Contacts.Countrycode);
            Assert.Equal("", result.Value.Value.CustomerDetails.FirstOrDefault().Contacts.Countrycode);
        }
    }
}
