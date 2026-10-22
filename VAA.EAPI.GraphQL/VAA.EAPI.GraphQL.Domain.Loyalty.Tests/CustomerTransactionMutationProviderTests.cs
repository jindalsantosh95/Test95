using VAA.EAPI.GraphQL.Common.Entities;
using VAA.EAPI.GraphQL.Common.Entities.Enums;
using VAA.EAPI.GraphQL.Domain.Loyalty.Models.MembersProfile;
using VAA.EAPI.GraphQL.Domain.Loyalty.Models.PartnerMembership;
using VAA.EAPI.GraphQL.Domain.Loyalty.Models.Transaction;
using VAA.EAPI.GraphQL.Domain.Loyalty.Services;
using static VAA.EAPI.GraphQL.Framework.Tests.TestHelpers;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Tests
{
    public class CustomerTransactionMutationProviderTests
    {
        private readonly Mock<IOAuthTokenProvider> _mocktokenProvider;
        private readonly Mock<IHttpConnector> _mockConnector;

        public CustomerTransactionMutationProviderTests()
        {
            _mocktokenProvider = new Mock<IOAuthTokenProvider>();
            _mockConnector = new Mock<IHttpConnector>();

            Setup.CreateTestEnvironmentSettings();
        }

        [Fact]
        public void ConstructorShouldThrowExceptionWithNullServices()
        {
            Assert.Throws<ArgumentNullException>(() => new CustomerTransactionMutationProvider<TransactionResponse>(null, _mockConnector.Object));
            Assert.Throws<ArgumentNullException>(() => new CustomerTransactionMutationProvider<TransactionResponse>(_mocktokenProvider.Object, null));
        }

        [Fact]
        public async Task CreateTransactionShouldSucceed()
        {
            var txnCreateRequest = TestDataProvider.GetTestData<TransactionRequest>("Loyalty\\Transaction\\TransactionRequest");
            var txnCreateResponse = TestDataProvider.GetTestData<APIResponse<TransactionResponse>>("Loyalty\\Transaction\\TransactionCreation_APIResponse");

            _mockConnector.Setup(m => m.SendAsync<TransactionResponse>(It.IsAny<string>(),
                                                         It.IsAny<Dictionary<string, string>>(),
                                                         It.IsAny<string>(),
                                                         It.IsAny<HttpMethod>(),
                                                         It.IsAny<string>(),
                                                         It.IsAny<bool>())).ReturnsAsync(txnCreateResponse);

            var provider = new CustomerTransactionMutationProvider<TransactionResponse>(_mocktokenProvider.Object, _mockConnector.Object);
            var queryResponse = await provider.CreateTransaction(txnCreateRequest);

            Assert.NotNull(queryResponse);
            Assert.Equal(Status.OK, queryResponse.CommandStatus);
            Assert.Null(queryResponse.Errors);
            Assert.Equal(txnCreateRequest.TransactionDocument.CustomerId, queryResponse.Transaction.CustomerId);
            Assert.Equal("171871054135506012", queryResponse.Transaction.TransactionId);
            Assert.Equal("Processed", queryResponse.Transaction.StatusCode);
            Assert.Equal(txnCreateRequest.TransactionDocument.FlightDetails?.ExternalReservationNumber, queryResponse.Transaction.FlightDetails?.ExternalReservationNumber);
        }

        [Fact]
        public async Task PartnerMembershipCreateShouldSuccess()
        {
            //Arrange
            var mockmutationRequest = TestDataProvider.GetTestData<PartnerMembershipMutationRequest>("Loyalty//PartnerMembershipCreateRequest");
            var mockRequest = (PartnerMembershipRequest)mockmutationRequest;
            var mockResponse = TestDataProvider.GetTestData<PartnerMembershipDocumentResponse>("Loyalty//PartnerMembershipCreateResponse");
            mockResponse.Errors = null;

            _mocktokenProvider.Setup(m => m.AcquireTokenAsync(It.IsAny<string>())).ReturnsAsync("XYZ");  // Dummy token           
            _mockConnector.Setup(m => m.SendAsync<PartnerMembershipDocumentResponse>(It.IsAny<string>(),
                                                       It.IsAny<Dictionary<string, string>>(),
                                                       It.IsAny<string>(),
                                                       It.IsAny<HttpMethod>(),
                                                       It.IsAny<string>(),
                                                       It.IsAny<bool>())).Returns(Task.FromResult(new APIResponse<PartnerMembershipDocumentResponse> { Response = mockResponse }));

            //Act
            var provider = new CustomerTransactionMutationProvider<PartnerMembershipDocumentResponse>(_mocktokenProvider.Object, _mockConnector.Object);
            var result = await provider.Mutate(mockRequest);

            //Assert
            Assert.Equal(mockResponse.PartnerMembershipDocument.PartnerStatusCode, result.PartnerMembershipDocument.PartnerStatusCode);
            Assert.Equal(mockResponse.PartnerMembershipDocument.CustomerId, result.PartnerMembershipDocument.CustomerId);
            Assert.Equal(mockResponse.PartnerMembershipDocument.PartnerId, result.PartnerMembershipDocument.PartnerId);
            Assert.Equal(mockResponse.PartnerMembershipDocument.Description, result.PartnerMembershipDocument.Description);
            Assert.Null(result.Errors);
            Assert.Equal(mockResponse.CommandStatus, result.CommandStatus);
        }

        [Fact]
        public async Task PartnerMembershipUpdateShouldSuccess()
        {
            //Arrange
            var mockmutationRequest = TestDataProvider.GetTestData<PartnerMembershipMutationRequest>("Loyalty//PartnerMembershipUpdateRequest");
            var mockRequest = (PartnerMembershipRequest)mockmutationRequest;
            var mockResponse = TestDataProvider.GetTestData<PartnerMembershipDocumentResponse>("Loyalty//PartnerMembershipUpdateResponse");
            mockResponse.Errors = null;

            _mocktokenProvider.Setup(m => m.AcquireTokenAsync(It.IsAny<string>())).ReturnsAsync("XYZ");  // Dummy token           
            _mockConnector.Setup(m => m.SendAsync<PartnerMembershipDocumentResponse>(It.IsAny<string>(),
                                                       It.IsAny<Dictionary<string, string>>(),
                                                       It.IsAny<string>(),
                                                       It.IsAny<HttpMethod>(),
                                                       It.IsAny<string>(),
                                                       It.IsAny<bool>())).Returns(Task.FromResult(new APIResponse<PartnerMembershipDocumentResponse> { Response = mockResponse }));

            //Act
            var provider = new CustomerTransactionMutationProvider<PartnerMembershipDocumentResponse>(_mocktokenProvider.Object, _mockConnector.Object);
            var result = await provider.Mutate(mockRequest);

            //Assert
            Assert.Equal(mockResponse.PartnerMembershipDocument.PartnerStatusCode, result.PartnerMembershipDocument.PartnerStatusCode);
            Assert.Equal(mockResponse.PartnerMembershipDocument.CustomerId, result.PartnerMembershipDocument.CustomerId);
            Assert.Equal(mockResponse.PartnerMembershipDocument.PartnerId, result.PartnerMembershipDocument.PartnerId);
            Assert.Equal(mockResponse.PartnerMembershipDocument.Description, result.PartnerMembershipDocument.Description);
            Assert.Null(result.Errors);
            Assert.Equal(mockResponse.CommandStatus, result.CommandStatus);
        }

        [Fact]
        public async Task VACPreferenceUpdateShouldSuccess()
        {
            //Arrange
            var mockRequest = TestDataProvider.GetTestData<MemberPreferenceUpdateRequest>("Loyalty//VACProfileUpdate//MemberPreferenceUpdateRequest");
            var mockResponse = TestDataProvider.GetTestData<MemberPreferenceUpdateResponse>("Loyalty//VACProfileUpdate//MemberPreferenceUpdateResponse");

            _mocktokenProvider.Setup(m => m.AcquireTokenAsync(It.IsAny<string>())).ReturnsAsync("XYZ");  // Dummy token           
            _mockConnector.Setup(m => m.SendAsync<MemberPreferenceUpdateResponse>(It.IsAny<string>(),
                                                       It.IsAny<Dictionary<string, string>>(),
                                                       It.IsAny<string>(),
                                                       It.IsAny<HttpMethod>(),
                                                       It.IsAny<string>(),
                                                       It.IsAny<bool>())).Returns(Task.FromResult(new APIResponse<MemberPreferenceUpdateResponse> { Response = mockResponse }));

            //Act
            var provider = new CustomerTransactionMutationProvider<MemberPreferenceUpdateResponse>(_mocktokenProvider.Object, _mockConnector.Object);
            var result = await provider.UpdateVACMemberPreference(mockRequest);

            //Assert
            Assert.Equal(mockResponse.PreferenceValue, result.PreferenceValue);
            Assert.Null(result.ErrorDetails);
        }
    }
}
