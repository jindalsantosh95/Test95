using Microsoft.Extensions.Logging;
using Moq;
using VAA.EAPI.GraphQL.Common.Entities;
using VAA.EAPI.GraphQL.Common.Exceptions;
using VAA.EAPI.GraphQL.Domain.Account.Interface;
using VAA.EAPI.GraphQL.Domain.Account.Infrastructure.Interfaces;
using VAA.EAPI.GraphQL.Domain.Account.Infrastructure.Models.ReferenceMS;
using VAA.EAPI.GraphQL.Domain.Account.Models;
using VAA.EAPI.GraphQL.Domain.Account.Services;
using VAA.EAPI.GraphQL.Infrastructure.Entities.CustomerMS;
using VAA.EAPI.GraphQL.Infrastructure.Entities.CustomerTransactions;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;
using static VAA.EAPI.GraphQL.Framework.Tests.TestHelpers;

namespace VAA.EAPI.GraphQL.Domain.Account.Tests
{
    public class FlightQueryProviderTests
    {
        private readonly Mock<IHttpConnector> _httpConnector;
        private readonly Mock<IRefDataService<AirportInfo>> _airportRefService;
        private readonly Mock<IRefDataService<AirlineInfo>> _airlineRefService;
        private readonly Mock<IOAuthTokenProvider> _tokenProvider;
        private readonly FlightQueryProvider _queryProvider;
        private readonly Mock<ILogger<FlightQueryProvider>> _logger;
        private readonly Mock<IAccountCustomerService> _customerProvider;
        private readonly Mock<ICustomerTransactionQueryProvider<FcMemberDetails>> _customerTransFlyingClub;


        public FlightQueryProviderTests()
        {
            _airportRefService = new Mock<IRefDataService<AirportInfo>>();
            _airlineRefService = new Mock<IRefDataService<AirlineInfo>>();
            _httpConnector = new Mock<IHttpConnector>();
            _tokenProvider = new Mock<IOAuthTokenProvider>();
            _logger = new Mock<ILogger<FlightQueryProvider>>();
            _customerProvider = new Mock<IAccountCustomerService>();
            _customerTransFlyingClub = new Mock<ICustomerTransactionQueryProvider<FcMemberDetails>>();
            _queryProvider = new FlightQueryProvider(_logger.Object, _httpConnector.Object, _airportRefService.Object, _airlineRefService.Object, _tokenProvider.Object, _customerProvider.Object, _customerTransFlyingClub.Object);
            Setup.CreateTestEnvironmentSettings();
        }

        [Fact]
        public void Constructor_ShouldThrowNullException_WhenHttpConnectorObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new FlightQueryProvider(_logger.Object, null, _airportRefService.Object, _airlineRefService.Object, _tokenProvider.Object, _customerProvider.Object, _customerTransFlyingClub.Object));
        }

        [Fact]
        public void Constructor_ShouldThrowNullException_WhenAirportRefServiceObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new FlightQueryProvider(_logger.Object, _httpConnector.Object, null, _airlineRefService.Object, _tokenProvider.Object, _customerProvider.Object, _customerTransFlyingClub.Object));
        }
        [Fact]
        public void Constructor_ShouldThrowNullException_WhenAirlineRefServiceObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new FlightQueryProvider(_logger.Object, _httpConnector.Object, _airportRefService.Object, null, _tokenProvider.Object, _customerProvider.Object, _customerTransFlyingClub.Object));
        }

        [Fact]
        public void Constructor_ShouldThrowNullException_WhenTokenProviderObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new FlightQueryProvider(_logger.Object, _httpConnector.Object, _airportRefService.Object, _airlineRefService.Object, null, _customerProvider.Object, _customerTransFlyingClub.Object));
        }

        [Fact]
        public async Task Query_ShouldReturnFlightDetailswithRightPageSize_ForGoodRequest()
        {
            //Arrange
            var bookingResponse = TestDataProvider.GetTestData<APIResponse<BookingDetailsResponse.BookingResponse>>("Account//BookingDetailsResponseMultipleFlights");

            int Page = 1;
            int PageSize = 3;

            var customerResponse = TestDataProvider.GetTestData<CustomerDTO>("Account//CustomerDTO");
            var customerFCMemberResponse = TestDataProvider.GetTestData<FcMemberDetails>("Account\\ProfileInfoResponse");

            var airportResponse = TestDataProvider.GetTestData<IEnumerable<AirportInfo>>("Account//AirportRefData");
            var airlineResponse = TestDataProvider.GetTestData<IEnumerable<AirlineInfo>>("Account//AirlineRefData");

            //Act
            _customerProvider.Setup(c => c.GetCustomer(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(customerResponse);

            _customerTransFlyingClub.Setup(c => c.CustomerTransactionQuery(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(customerFCMemberResponse);

            _httpConnector.Setup(c => c.SendAsync<BookingDetailsResponse.BookingResponse>(It.IsAny<string>(), It.IsAny<Dictionary<string, string>>(), It.IsAny<string>(), It.IsAny<HttpMethod>(), It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(bookingResponse);

            _airportRefService.Setup(airport => airport.Query(It.IsAny<string[]>())).ReturnsAsync(airportResponse);

            _airlineRefService.Setup(airline => airline.Query(It.IsAny<string[]>())).ReturnsAsync(airlineResponse);

            var result = await _queryProvider.GetFlightsQueryData("9032389238", Page, PageSize);

            //Assert
            Assert.NotNull(result);
            Assert.Equal(3, result.FlightSegmentDetails.Count());
            Assert.IsType<FlightSegmentDetails>(result.FlightSegmentDetails.First());
        }

        [Fact(Skip = "Implemented test is not configured/implemented correctly")]
        public async Task Query_ShouldThrowException_ForZeroPage()
        {
            int PageSize = 2;

            await Assert.ThrowsAsync<Exception>(() => _queryProvider.GetFlightsQueryData("9032389238", 0, PageSize));
        }

        [Fact(Skip = "Implemented test is not configured/implemented correctly")]
        public async Task Query_ShouldThrowException_ForZeroPageSize()
        {
            int Page = 1;

            await Assert.ThrowsAsync<Exception>(() => _queryProvider.GetFlightsQueryData("9032389238", Page, 0));
        }

        [Fact]
        public async Task Query_ShouldThorwException_ForFailingtoConnecttoOrderSearchMS()
        {
            //Arrange
            var customerResponse = TestDataProvider.GetTestData<CustomerDTO>("Account//CustomerDTO");
            var customerFCMemberResponse = TestDataProvider.GetTestData<FcMemberDetails>("Account\\ProfileInfoResponse");

            var apiResponse = new HttpResponseMessage() { StatusCode = System.Net.HttpStatusCode.UnprocessableEntity, Content = null };

            //Act
            _customerProvider.Setup(c => c.GetCustomer(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(customerResponse);

            _customerTransFlyingClub.Setup(c => c.CustomerTransactionQuery(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(customerFCMemberResponse);

            _httpConnector.Setup(c => c.SendAsync<BookingDetailsResponse.BookingResponse>(It.IsAny<string>(), It.IsAny<Dictionary<string, string>>(), It.IsAny<string>(), It.IsAny<HttpMethod>(), It.IsAny<string>(), It.IsAny<bool>())).ThrowsAsync(new HttpRequestException("HttpConnector failed.", new HttpResponseException(apiResponse), System.Net.HttpStatusCode.InternalServerError));

            //Assert
            await Assert.ThrowsAsync<HttpRequestException>(() => _queryProvider.GetFlightsQueryData("9032389238", 1, 5));
        }

        [Fact]
        public async Task Query_ShouldThorwException_ForFailingtoConnecttoReferenceMS()
        {
            //Arrange
            var bookingResponse = TestDataProvider.GetTestData<APIResponse<BookingDetailsResponse.BookingResponse>>("Account//BookingDetailsResponseMultipleFlights");

            var apiResponse = new HttpResponseMessage() { StatusCode = System.Net.HttpStatusCode.UnprocessableEntity, Content = null };

            var customerResponse = TestDataProvider.GetTestData<CustomerDTO>("Account//CustomerDTO");
            var customerFCMemberResponse = TestDataProvider.GetTestData<FcMemberDetails>("Account\\ProfileInfoResponse");

            //Act
            _customerProvider.Setup(c => c.GetCustomer(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(customerResponse);

            _customerTransFlyingClub.Setup(c => c.CustomerTransactionQuery(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(customerFCMemberResponse);

            _httpConnector.Setup(c => c.SendAsync<BookingDetailsResponse.BookingResponse>(It.IsAny<string>(), It.IsAny<Dictionary<string, string>>(), It.IsAny<string>(), It.IsAny<HttpMethod>(), It.IsAny<string>(), It.IsAny<bool>())).ThrowsAsync(new HttpRequestException("HttpConnector failed.", new HttpResponseException(apiResponse), System.Net.HttpStatusCode.InternalServerError));

            //Assert
            await Assert.ThrowsAsync<HttpRequestException>(() => _queryProvider.GetFlightsQueryData("9032389238", 1, 5));
        }
    }
}
