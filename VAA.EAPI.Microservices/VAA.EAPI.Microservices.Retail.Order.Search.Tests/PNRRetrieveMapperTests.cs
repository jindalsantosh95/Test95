using MediatR;
using Moq;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Virgin.Models.Customer;
using VAA.EAPI.Infrastructure.PSS.Proxy.PNRCommonV6Proxy;
using VAA.EAPI.Infrastructure.PSS.Repository;
using VAA.EAPI.Microservices.Retail.Order.Search.Application.Query;
using VAA.EAPI.Microservices.Retail.Order.Search.Domain;
using VAA.EAPI.Microservices.Retail.Order.Search.Helper.Cache.Interface;
using VAA.EAPI.Microservices.Retail.Order.Search.Mapper;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;
using TicketRetrieveResponseType = VAA.EAPI.Infrastructure.PSS.Proxy.TicketingProcessV3Proxy.RetrieveResponseType;


namespace VAA.EAPI.Microservices.Retail.Order.Search.Tests
{
    public class PNRRetrieveMapperTests
    {
        private readonly Mock<IMediator> _mediator;
        private readonly Mock<ICachedAirport> _mockCachedAirport;
        private readonly Mock<IProductName> _mockProductName;
        private readonly Mock<ICachedSegmentStatusCode> _mockCachedSegmentStatusCode;
        private readonly Mock<ICachedCabinType> _mockCachedCabinType;
        private readonly Mock<ICachedAmenities> _mockCachedAmenities;

        public PNRRetrieveMapperTests()
        {
            _mediator = new Mock<IMediator>();
            _mockCachedAirport = new Mock<ICachedAirport>();
            _mockCachedSegmentStatusCode = new Mock<ICachedSegmentStatusCode>();
            _mockCachedCabinType = new Mock<ICachedCabinType>();
            OrderSearchTestsCommon.CreateTestEnvironmentSettings();
            _mockCachedAmenities = new Mock<ICachedAmenities>();
            _mockProductName = new Mock<IProductName>();
        }

        [Fact]
        public void RequestMapper_ShouldReturnValidResponse_WithGoodRequest()
        {
            //Arrange 
            var request = TestDataProvider.GetTestData<OrderSearchResponseQuery>("OrderSearchMS\\OrderSearchValidRequest");

            //Act
            var result = PNRRetrieveMapper.RetrieveRequestMapper(request.OrderSearchRequest);

            //Assert
            Assert.Equal("GX4WOY", result.SetupContextRequest.RetrieveInfo.PassengerInfo.RecordLocator);
        }

        [Fact]
        public async Task ResponseMapper_ShouldReturnValidResponse_WithGoodRequest()
        {
            //Arrange 
            var request = TestDataProvider.GetTestData<RetrieveResponseType>("OrderSearchMS\\Air4PnrRetrieveResponse");
            request.TeardownContextResponse.Pnr.SegmentDl.FirstOrDefault().CompartmentNumber = "3";
            var AmenitiesData = TestDataProvider.GetTestData<List<AmenitiesTableEntity>>("OrderSearchMS\\AmenitiesTableData");
            var cabinTableResponse = TestDataProvider.GetTestData<List<CabinTypeDataEntity>>("OrderSearchMS\\CabinTypeData");
            var flightCustomer = TestDataProvider.GetTestData<List<Customer>>("OrderSearchMS\\FlightCustomer");
            var customer = flightCustomer.FirstOrDefault();

            _mockProductName.Setup(x => x.GetProductName(It.IsAny<string>())).ReturnsAsync("Economy Flex");
            _mockCachedAmenities.Setup(x => x.GetAmenitiesInformation(It.IsAny<string>())).ReturnsAsync(AmenitiesData);
            _mockCachedCabinType.Setup(X => X.GetCabinTypeData(It.IsAny<string>())).ReturnsAsync(cabinTableResponse);
            _mediator.Setup(x => x.Send(It.IsAny<PassengerV4ManageMultipleTravelDocsAndSecureFlightRepoQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(customer);

            //Act
            var pnrResult = await PNRRetrieveMapper.RetrieveResponseMapper(request, _mockCachedAirport.Object, new TicketRetrieveResponseType(), new List<TicketRetrieveResponseType> { new TicketRetrieveResponseType() }, _mockProductName.Object, _mockCachedSegmentStatusCode.Object, _mockCachedCabinType.Object, _mockCachedAmenities.Object, _mediator.Object, flightCustomer);

            //Assert
            Assert.Single(pnrResult.SegmentDetails);
            Assert.Equal(4, pnrResult.CustomerDetails.Count());
            Assert.Equal("1100P/03AUG", pnrResult.TimeLimits.First().Remarks);

        }
        [Fact]
        public void ResponseMapper_ShouldSucceed_WithRequestWithInfantInArmInfo()
        {
            //Arrange 
            var request = TestDataProvider.GetTestData<RetrieveResponseType>("OrderSearchMS\\PnrRetrieveResonseWithInfantData");
            var flightCustomer = TestDataProvider.GetTestData<List<Customer>>("OrderSearchMS\\FlightCustomer");

            //Act
            var pnrResult = PNRRetrieveMapper.RetrieveResponseMapper(request, _mockCachedAirport.Object, new TicketRetrieveResponseType(), new List<TicketRetrieveResponseType> { new TicketRetrieveResponseType() }, _mockProductName.Object, _mockCachedSegmentStatusCode.Object, _mockCachedCabinType.Object, _mockCachedAmenities.Object, _mediator.Object, flightCustomer);

            //Assert
            Assert.NotNull(pnrResult);
            Assert.True(pnrResult.IsCompleted);
            Assert.Equal(1, pnrResult.Id);

        }
    }
}
