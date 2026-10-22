using HotChocolate;
using VAA.EAPI.GraphQL.Common.Exceptions;
using VAA.EAPI.GraphQL.Domain.Loyalty.Interfaces;
using VAA.EAPI.GraphQL.Domain.Loyalty.Models.SFPD;
using VAA.EAPI.GraphQL.Infrastructure.Entities.CustomerMS;
using static VAA.EAPI.GraphQL.Framework.Tests.TestHelpers;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Tests
{
    public class LoyaltySyncMutationsTests
    {
        private readonly LoyaltySyncMutations _mutation;
        private readonly Mock<ILogger<LoyaltySyncMutations>> _mockLogger;
        private readonly Mock<ILoyaltySyncMutationProvider<SFPDSyncResponse>> _mockLoyaltySyncProvider;
        private readonly Mock<ICustomerProvider> _mockCustomerProvider;

        public LoyaltySyncMutationsTests()
        {
            _mockLogger = new Mock<ILogger<LoyaltySyncMutations>>();
            _mockLoyaltySyncProvider = new Mock<ILoyaltySyncMutationProvider<SFPDSyncResponse>>();
            _mockCustomerProvider = new Mock<ICustomerProvider>();
            _mutation = new LoyaltySyncMutations(_mockLoyaltySyncProvider.Object, _mockCustomerProvider.Object, _mockLogger.Object);
        }

        [Fact]
        public void ConstructorShouldThrowNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new LoyaltySyncMutations(null, _mockCustomerProvider.Object, _mockLogger.Object));
            Assert.Throws<ArgumentNullException>(() => new LoyaltySyncMutations(_mockLoyaltySyncProvider.Object, null, _mockLogger.Object));
            Assert.Throws<ArgumentNullException>(() => new LoyaltySyncMutations(_mockLoyaltySyncProvider.Object, _mockCustomerProvider.Object, null));
        }

        [Fact]
        public async Task SFPDSyncShouldSuccesswithoutMemberId()
        {
            // Arrange
            var mockRequest = TestDataProvider.GetTestData<SFPDSyncMutationRequest>("Loyalty//SFPDSyncRequest");
            mockRequest.FcNumber = null;

            var mockResponse = TestDataProvider.GetTestData<SFPDSyncResponse>("Loyalty//SFPDSyncResponse");
            mockResponse.Errors = null;

            var mockCustomerDTO = TestDataProvider.GetTestData<CustomerDTO>("Loyalty\\CustomerDTO");
            mockCustomerDTO.Customer.LoyaltyNumber = "9678234561";

            _mockCustomerProvider.Setup(x => x.GetCustomer(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(mockCustomerDTO);
            _mockLoyaltySyncProvider.Setup(x => x.Mutate(It.IsAny<SFPDSyncRequest>())).ReturnsAsync(mockResponse);

            //Act
            var result = await _mutation.SyncSFPD(mockRequest);

            //Assert
            Assert.Equal(mockResponse.PsdsSyncStatus, result.PsdsSyncStatus);
            Assert.Equal(mockResponse.CommandStatus, result.CommandStatus);
        }

        [Fact]
        public async Task SFPDSyncFailureFromMs()
        {
            // Arrange
            var mockRequest = TestDataProvider.GetTestData<SFPDSyncMutationRequest>("Loyalty//SFPDSyncRequest");
            mockRequest.FcNumber = null;

            var mockResponse = TestDataProvider.GetTestData<SFPDSyncResponse>("Loyalty//SFPDSyncResponse");

            var mockCustomerDTO = TestDataProvider.GetTestData<CustomerDTO>("Loyalty\\CustomerDTO");
            mockCustomerDTO.Customer.LoyaltyNumber = "9678234561";

            _mockCustomerProvider.Setup(x => x.GetCustomer(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(mockCustomerDTO);
            _mockLoyaltySyncProvider.Setup(x => x.Mutate(It.IsAny<SFPDSyncRequest>())).ReturnsAsync(mockResponse);

            //Act & Assert
            await Assert.ThrowsAsync<GraphQLException>(() => _mutation.SyncSFPD(mockRequest));
        }

        [Fact]
        public async Task SFPDSyncShouldSuccessWithoutCustomerId()
        {
            // Arrange
            var mockRequest = TestDataProvider.GetTestData<SFPDSyncMutationRequest>("Loyalty//SFPDSyncRequest");
            mockRequest.CustomerId = null;

            var mockResponse = TestDataProvider.GetTestData<SFPDSyncResponse>("Loyalty//SFPDSyncResponse");
            mockResponse.Errors = null;

            var mockCustomerDTO = TestDataProvider.GetTestData<CustomerDTO>("Loyalty\\CustomerDTO");
            mockCustomerDTO.Customer.CustomerId = "XXX-YYY-ZZZ";

            _mockCustomerProvider.Setup(x => x.GetCustomer(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(mockCustomerDTO);
            _mockLoyaltySyncProvider.Setup(x => x.Mutate(It.IsAny<SFPDSyncRequest>())).ReturnsAsync(mockResponse);

            //Act
            var result = await _mutation.SyncSFPD(mockRequest);

            //Assert
            Assert.Equal(mockResponse.PsdsSyncStatus, result.PsdsSyncStatus);
            Assert.Equal(mockResponse.Errors, result.Errors);
        }


        [Fact]
        public async Task SFPDSyncValidationWithoutCustomerAndMemberID()
        {
            // Arrange
            var request = TestDataProvider.GetTestData<SFPDSyncMutationRequest>("Loyalty//SFPDSyncRequest");
            request.CustomerId = null;
            request.FcNumber = null;

            //Act & Assert
            await Assert.ThrowsAsync<RequestValidationException>(() => _mutation.SyncSFPD(request));
        }


        [Fact]
        public async Task SFPDSyncWithHttpException()
        {
            // Arrange
            var mockRequest = TestDataProvider.GetTestData<SFPDSyncMutationRequest>("Loyalty//SFPDSyncRequest");
            mockRequest.FcNumber = null;

            var mockResponse = TestDataProvider.GetTestData<SFPDSyncResponse>("Loyalty//SFPDSyncResponse");

            var mockCustomerDTO = TestDataProvider.GetTestData<CustomerDTO>("Loyalty\\CustomerDTO");
            mockCustomerDTO.Customer.LoyaltyNumber = "9678234561";

            _mockCustomerProvider.Setup(x => x.GetCustomer(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(mockCustomerDTO);
            _mockLoyaltySyncProvider.Setup(x => x.Mutate(It.IsAny<SFPDSyncRequest>())).Throws(new HttpRequestException("HttpConnector failed.", new HttpResponseException(System.Net.HttpStatusCode.InternalServerError), System.Net.HttpStatusCode.InternalServerError));

            //Act & Assert
            await Assert.ThrowsAsync<HttpRequestException>(() => _mutation.SyncSFPD(mockRequest));
        }

    }
}
