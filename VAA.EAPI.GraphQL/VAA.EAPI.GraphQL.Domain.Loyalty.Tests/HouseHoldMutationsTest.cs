using VAA.EAPI.GraphQL.Common.Exceptions;
using VAA.EAPI.GraphQL.Domain.Loyalty.Interfaces;
using VAA.EAPI.GraphQL.Domain.Loyalty.Models;
using VAA.EAPI.GraphQL.Infrastructure.Entities.CustomerMS;
using static VAA.EAPI.GraphQL.Framework.Tests.TestHelpers;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Tests
{
    public class HouseHoldMutationsTest
    {
        private readonly HouseHoldMutations _mutation;
        private readonly Mock<ILogger<HouseHoldMutations>> _mockLogger;
        private readonly Mock<IHouseHoldMutationProvider> _mockHouseholdProvider;
        private readonly Mock<ICustomerProvider> _mockCustomerProvider;

        public HouseHoldMutationsTest()
        {
            _mockLogger = new Mock<ILogger<HouseHoldMutations>>();
            _mockHouseholdProvider = new Mock<IHouseHoldMutationProvider>();
            _mockCustomerProvider = new Mock<ICustomerProvider>();
            _mutation = new HouseHoldMutations(_mockCustomerProvider.Object, _mockHouseholdProvider.Object, _mockLogger.Object);
        }

        [Fact]
        public void ConstructorShouldThrowNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new HouseHoldMutations(null, _mockHouseholdProvider.Object, _mockLogger.Object));
            Assert.Throws<ArgumentNullException>(() => new HouseHoldMutations(_mockCustomerProvider.Object, null, _mockLogger.Object));
            Assert.Throws<ArgumentNullException>(() => new HouseHoldMutations(_mockCustomerProvider.Object, _mockHouseholdProvider.Object, null));
        }

        [Fact]
        public async Task HouseholdDelinkingShouldSuccess()
        {
            // Arrange
            var request = TestDataProvider.GetTestData<HouseHoldDelinkingMutationRequest>("Loyalty//HouseholdLinkDelinkRequest");
            var customerDTO = TestDataProvider.GetTestData<CustomerDTO>("Loyalty\\CustomerDTO");
            _mockCustomerProvider.Setup(x => x.GetCustomer(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(customerDTO);
            _mockHouseholdProvider.Setup(x => x.HouseHoldDelinking(It.IsAny<HouseHoldDelinkingRequest>())).ReturnsAsync("Request successfully initiated");

            //Act
            var result = await _mutation.HouseHoldDelinking(request, _mockCustomerProvider.Object);

            //Assert
            Assert.Equal("Request successfully initiated", result);
        }

        [Fact]
        public async Task HouseholdDelinkingValidationFailure()
        {
            var request = TestDataProvider.GetTestData<HouseHoldDelinkingMutationRequest>("Loyalty//HouseholdLinkDelinkRequest");
            request.InitiatedBy = null;

            await Assert.ThrowsAsync<RequestValidationException>(() => _mutation.HouseHoldDelinking(request, _mockCustomerProvider.Object));

        }

        [Fact]
        public async Task HouseholdDelinkingWithEAPIException()
        {
            var request = TestDataProvider.GetTestData<HouseHoldDelinkingMutationRequest>("Loyalty//HouseholdLinkDelinkRequest");
            var customerDTO = TestDataProvider.GetTestData<CustomerDTO>("Loyalty\\CustomerDTO");
            _mockCustomerProvider.Setup(x => x.GetCustomer(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(customerDTO);
            _mockHouseholdProvider.Setup(x => x.HouseHoldDelinking(It.IsAny<HouseHoldDelinkingRequest>())).Throws(new HttpRequestException("HttpConnector failed.", new HttpResponseException(System.Net.HttpStatusCode.InternalServerError), System.Net.HttpStatusCode.InternalServerError));

            await Assert.ThrowsAsync<HttpRequestException>(() => _mutation.HouseHoldDelinking(request, _mockCustomerProvider.Object));

        }

        [Fact]
        public async Task HouseholdLinkingShouldSuccess()
        {
            // Arrange
            var request = TestDataProvider.GetTestData<HouseHoldLinkingMutationRequest>("Loyalty//HouseholdLinkDelinkRequest");
            var apiRequest = TestDataProvider.GetTestData<HouseHoldLinkingRequest>("Loyalty//HouseholdLinkDelinkAPIRequest");
            var customerDTO = TestDataProvider.GetTestData<CustomerDTO>("Loyalty\\CustomerDTO");
            _mockCustomerProvider.Setup(x => x.GetCustomer(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(customerDTO);
            _mockHouseholdProvider.Setup(x => x.HouseHoldLinking(It.IsAny<HouseHoldLinkingRequest>())).ReturnsAsync("Request successfully initiated");

            //Act
            var result = await _mutation.HouseHoldLinking(request, _mockCustomerProvider.Object);

            //Assert
            Assert.Equal("Request successfully initiated", result);
        }

        [Fact]
        public async Task HouseholdLinkingValidationFailure()
        {
            var request = TestDataProvider.GetTestData<HouseHoldLinkingMutationRequest>("Loyalty//HouseholdLinkDelinkRequest");
            request.InitiatedBy = null;

            await Assert.ThrowsAsync<RequestValidationException>(() => _mutation.HouseHoldLinking(request, _mockCustomerProvider.Object));

        }

        [Fact]
        public async Task HouseholdLinkingWithEAPIException()
        {
            var request = TestDataProvider.GetTestData<HouseHoldLinkingMutationRequest>("Loyalty//HouseholdLinkDelinkRequest");
            var customerDTO = TestDataProvider.GetTestData<CustomerDTO>("Loyalty\\CustomerDTO");
            _mockCustomerProvider.Setup(x => x.GetCustomer(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(customerDTO);
            _mockHouseholdProvider.Setup(x => x.HouseHoldLinking(It.IsAny<HouseHoldLinkingRequest>())).Throws(new HttpRequestException("HttpConnector failed.", new HttpResponseException(System.Net.HttpStatusCode.InternalServerError), System.Net.HttpStatusCode.InternalServerError));

            await Assert.ThrowsAsync<HttpRequestException>(() => _mutation.HouseHoldLinking(request, _mockCustomerProvider.Object));

        }
    }
}
