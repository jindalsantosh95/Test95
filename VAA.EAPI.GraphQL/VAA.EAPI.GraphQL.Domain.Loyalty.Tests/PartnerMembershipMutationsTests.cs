using HotChocolate;
using VAA.EAPI.GraphQL.Common.Entities;
using VAA.EAPI.GraphQL.Common.Exceptions;
using VAA.EAPI.GraphQL.Domain.Loyalty.Interfaces;
using VAA.EAPI.GraphQL.Domain.Loyalty.Models.PartnerMembership;
using VAA.EAPI.GraphQL.Domain.Loyalty.Models.Reference;
using VAA.EAPI.GraphQL.Infrastructure.Entities.CustomerMS;
using static VAA.EAPI.GraphQL.Framework.Tests.TestHelpers;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Tests
{
    public class PartnerMembershipMutationsTests
    {
        private readonly PartnerMembershipMutations _mutation;
        private readonly Mock<ILogger<PartnerMembershipMutations>> _mockLogger;
        private readonly Mock<ICustomerTransactionMutationProvider<PartnerMembershipDocumentResponse>> _mockCustomerTransactionProvider;
        private readonly Mock<ICustomerProvider> _mockCustomerProvider;
        private readonly Mock<IPartnersProfilesQueryProvider> _mockPartnersProfilesQueryProvider;

        public PartnerMembershipMutationsTests()
        {
            _mockLogger = new Mock<ILogger<PartnerMembershipMutations>>();
            _mockCustomerTransactionProvider = new Mock<ICustomerTransactionMutationProvider<PartnerMembershipDocumentResponse>>();
            _mockCustomerProvider = new Mock<ICustomerProvider>();
            _mockPartnersProfilesQueryProvider = new Mock<IPartnersProfilesQueryProvider>();
            _mutation = new PartnerMembershipMutations(_mockCustomerTransactionProvider.Object, _mockCustomerProvider.Object, _mockPartnersProfilesQueryProvider.Object, _mockLogger.Object);
        }

        [Fact]
        public void ConstructorShouldThrowNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new PartnerMembershipMutations(null, _mockCustomerProvider.Object, _mockPartnersProfilesQueryProvider.Object, _mockLogger.Object));
            Assert.Throws<ArgumentNullException>(() => new PartnerMembershipMutations(_mockCustomerTransactionProvider.Object, null, _mockPartnersProfilesQueryProvider.Object, _mockLogger.Object));
            Assert.Throws<ArgumentNullException>(() => new PartnerMembershipMutations(_mockCustomerTransactionProvider.Object, _mockCustomerProvider.Object, null, _mockLogger.Object));
            Assert.Throws<ArgumentNullException>(() => new PartnerMembershipMutations(_mockCustomerTransactionProvider.Object, _mockCustomerProvider.Object, _mockPartnersProfilesQueryProvider.Object, null));
        }

        [Fact]
        public async Task PartnerMembershipCreateShouldSuccesswithoutMemberId()
        {
            // Arrange
            var mockRequest = TestDataProvider.GetTestData<PartnerMembershipMutationRequest>("Loyalty//PartnerMembershipCreateRequest");
            mockRequest.MemberNumber = null;

            var mockResponse = TestDataProvider.GetTestData<PartnerMembershipDocumentResponse>("Loyalty//PartnerMembershipCreateResponse");
            mockResponse.Errors = null;

            var mockPartnerProfilesResponse = TestDataProvider.GetTestData<APIResponse<IEnumerable<PartnerProfileDetails>>>("MemberCardLink\\PartnersProfiles_APIResponse");
            var mockCustomerDTO = TestDataProvider.GetTestData<CustomerDTO>("Loyalty\\CustomerDTO");
            mockCustomerDTO.Customer.LoyaltyNumber = "9678234561";

            _mockPartnersProfilesQueryProvider.Setup(x => x.GetPartnerProfileDetails()).ReturnsAsync(mockPartnerProfilesResponse.Response);
            _mockCustomerProvider.Setup(x => x.GetCustomer(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(mockCustomerDTO);
            _mockCustomerTransactionProvider.Setup(x => x.Mutate(It.IsAny<PartnerMembershipRequest>())).ReturnsAsync(mockResponse);

            //Act
            var result = await _mutation.CreatePartnerMembership(mockRequest);

            //Assert
            Assert.Equal(mockResponse.PartnerMembershipDocument, result);
            Assert.Equal(mockResponse.PartnerMembershipDocument.CustomerId, result.CustomerId);
            Assert.Equal(mockResponse.PartnerMembershipDocument.PartnerStatusCode, result.PartnerStatusCode);
            Assert.Equal(mockResponse.PartnerMembershipDocument.Description, result.Description);
            Assert.Equal(mockResponse.PartnerMembershipDocument.MemberNumber, result.MemberNumber);
        }

        [Fact]
        public async Task PartnerMembershipCreateFailureFromMs()
        {
            // Arrange
            var mockRequest = TestDataProvider.GetTestData<PartnerMembershipMutationRequest>("Loyalty//PartnerMembershipCreateRequest");
            mockRequest.MemberNumber = null;

            var mockResponse = TestDataProvider.GetTestData<PartnerMembershipDocumentResponse>("Loyalty//PartnerMembershipCreateResponse");

            var mockPartnerProfilesResponse = TestDataProvider.GetTestData<APIResponse<IEnumerable<PartnerProfileDetails>>>("MemberCardLink\\PartnersProfiles_APIResponse");
            var mockCustomerDTO = TestDataProvider.GetTestData<CustomerDTO>("Loyalty\\CustomerDTO");
            mockCustomerDTO.Customer.LoyaltyNumber = "9678234561";

            _mockPartnersProfilesQueryProvider.Setup(x => x.GetPartnerProfileDetails()).ReturnsAsync(mockPartnerProfilesResponse.Response);
            _mockCustomerProvider.Setup(x => x.GetCustomer(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(mockCustomerDTO);
            _mockCustomerTransactionProvider.Setup(x => x.Mutate(It.IsAny<PartnerMembershipRequest>())).ReturnsAsync(mockResponse);

            //Act & Assert
            await Assert.ThrowsAsync<GraphQLException>(() => _mutation.CreatePartnerMembership(mockRequest));
        }

        [Fact]
        public async Task PartnerMembershipCreateShouldSuccessWithoutCustomerId()
        {
            // Arrange
            var mockRequest = TestDataProvider.GetTestData<PartnerMembershipMutationRequest>("Loyalty//PartnerMembershipCreateRequest");
            mockRequest.CustomerId = null;

            var mockResponse = TestDataProvider.GetTestData<PartnerMembershipDocumentResponse>("Loyalty//PartnerMembershipCreateResponse");
            mockResponse.Errors = null;

            var mockPartnerProfilesResponse = TestDataProvider.GetTestData<APIResponse<IEnumerable<PartnerProfileDetails>>>("MemberCardLink\\PartnersProfiles_APIResponse");
            var mockCustomerDTO = TestDataProvider.GetTestData<CustomerDTO>("Loyalty\\CustomerDTO");
            mockCustomerDTO.Customer.CustomerId = "D92252A8-5198-6F04-E053-7D41D70A1575";

            _mockPartnersProfilesQueryProvider.Setup(x => x.GetPartnerProfileDetails()).ReturnsAsync(mockPartnerProfilesResponse.Response);
            _mockCustomerProvider.Setup(x => x.GetCustomer(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(mockCustomerDTO);
            _mockCustomerTransactionProvider.Setup(x => x.Mutate(It.IsAny<PartnerMembershipRequest>())).ReturnsAsync(mockResponse);

            //Act
            var result = await _mutation.CreatePartnerMembership(mockRequest);

            //Assert
            Assert.Equal(mockResponse.PartnerMembershipDocument, result);
            Assert.Equal(mockResponse.PartnerMembershipDocument.MemberNumber, result.MemberNumber);
            Assert.Equal(mockResponse.PartnerMembershipDocument.CreatedBy, result.CreatedBy);
            Assert.Equal(mockResponse.PartnerMembershipDocument.PartnerId, result.PartnerId);
        }

        [Fact]
        public async Task PartnerMembershipCreateValidationFailure()
        {
            await Assert.ThrowsAsync<RequestValidationException>(() => _mutation.CreatePartnerMembership(null));
        }

        [Fact]
        public async Task PartnerMembershipCreateValidationWithoutPartnercode()
        {
            // Arrange
            var request = TestDataProvider.GetTestData<PartnerMembershipMutationRequest>("Loyalty//PartnerMembershipCreateRequest");
            request.PartnerCode = null;

            //Act & Assert
            await Assert.ThrowsAsync<RequestValidationException>(() => _mutation.CreatePartnerMembership(request));
        }


        [Fact]
        public async Task PartnerMembershipCreateValidationWithoutCustomerAndMemberID()
        {
            // Arrange
            var request = TestDataProvider.GetTestData<PartnerMembershipMutationRequest>("Loyalty//PartnerMembershipCreateRequest");
            request.CustomerId = null;
            request.MemberNumber = null;

            //Act & Assert
            await Assert.ThrowsAsync<RequestValidationException>(() => _mutation.CreatePartnerMembership(request));
        }

        [Fact]
        public async Task PartnerMembershipCreateValidationWithCardType()
        {
            // Arrange
            var request = TestDataProvider.GetTestData<PartnerMembershipMutationRequest>("Loyalty//PartnerMembershipCreateRequest");
            request.CardType = "TestCard";

            //Act & Assert
            await Assert.ThrowsAsync<RequestValidationException>(() => _mutation.CreatePartnerMembership(request));
        }

        [Fact]
        public async Task CreatePartnerMembershipWithHttpException()
        {
            // Arrange
            var mockRequest = TestDataProvider.GetTestData<PartnerMembershipMutationRequest>("Loyalty//PartnerMembershipCreateRequest");
            mockRequest.MemberNumber = null;

            var mockResponse = TestDataProvider.GetTestData<PartnerMembership>("Loyalty//PartnerMembershipCreateResponse");

            var mockPartnerProfilesResponse = TestDataProvider.GetTestData<APIResponse<IEnumerable<PartnerProfileDetails>>>("MemberCardLink\\PartnersProfiles_APIResponse");
            var mockCustomerDTO = TestDataProvider.GetTestData<CustomerDTO>("Loyalty\\CustomerDTO");
            mockCustomerDTO.Customer.LoyaltyNumber = "9678234561";

            _mockPartnersProfilesQueryProvider.Setup(x => x.GetPartnerProfileDetails()).ReturnsAsync(mockPartnerProfilesResponse.Response);
            _mockCustomerProvider.Setup(x => x.GetCustomer(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(mockCustomerDTO);
            _mockCustomerTransactionProvider.Setup(x => x.Mutate(It.IsAny<PartnerMembershipRequest>())).Throws(new HttpRequestException("HttpConnector failed.", new HttpResponseException(System.Net.HttpStatusCode.InternalServerError), System.Net.HttpStatusCode.InternalServerError));

            //Act & Assert
            await Assert.ThrowsAsync<HttpRequestException>(() => _mutation.CreatePartnerMembership(mockRequest));
        }

        [Fact]
        public async Task PartnerMembershipUpdateValidationFailure()
        {
            await Assert.ThrowsAsync<RequestValidationException>(() => _mutation.UpdatePartnerMembership(null));
        }

        [Fact]
        public async Task PartnerMembershipUpdateShouldSuccesswithoutMemberId()
        {
            // Arrange
            var mockRequest = TestDataProvider.GetTestData<PartnerMembershipMutationRequest>("Loyalty//PartnerMembershipUpdateRequest");
            mockRequest.MemberNumber = null;

            var mockResponse = TestDataProvider.GetTestData<PartnerMembershipDocumentResponse>("Loyalty//PartnerMembershipUpdateResponse");
            mockResponse.Errors = null;

            var mockPartnerProfilesResponse = TestDataProvider.GetTestData<APIResponse<IEnumerable<PartnerProfileDetails>>>("MemberCardLink\\PartnersProfiles_APIResponse");
            var mockCustomerDTO = TestDataProvider.GetTestData<CustomerDTO>("Loyalty\\CustomerDTO");
            mockCustomerDTO.Customer.LoyaltyNumber = "9107070394";

            _mockPartnersProfilesQueryProvider.Setup(x => x.GetPartnerProfileDetails()).ReturnsAsync(mockPartnerProfilesResponse.Response);
            _mockCustomerProvider.Setup(x => x.GetCustomer(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(mockCustomerDTO);
            _mockCustomerTransactionProvider.Setup(x => x.Mutate(It.IsAny<PartnerMembershipRequest>())).ReturnsAsync(mockResponse);

            //Act
            var result = await _mutation.UpdatePartnerMembership(mockRequest);

            //Assert
            Assert.Equal(mockResponse.PartnerMembershipDocument, result);
            Assert.Equal(mockResponse.PartnerMembershipDocument.CustomerId, result.CustomerId);
            Assert.Equal(mockResponse.PartnerMembershipDocument.PartnerStatusCode, result.PartnerStatusCode);
            Assert.Equal(mockResponse.PartnerMembershipDocument.Description, result.Description);
            Assert.Equal(mockResponse.PartnerMembershipDocument.MemberNumber, result.MemberNumber);
        }

        [Fact]
        public async Task PartnerMembershipUpdateFailureFromMs()
        {
            // Arrange
            var mockRequest = TestDataProvider.GetTestData<PartnerMembershipMutationRequest>("Loyalty//PartnerMembershipUpdateRequest");
            mockRequest.MemberNumber = null;

            var mockResponse = TestDataProvider.GetTestData<PartnerMembershipDocumentResponse>("Loyalty//PartnerMembershipUpdateResponse");

            var mockPartnerProfilesResponse = TestDataProvider.GetTestData<APIResponse<IEnumerable<PartnerProfileDetails>>>("MemberCardLink\\PartnersProfiles_APIResponse");
            var mockCustomerDTO = TestDataProvider.GetTestData<CustomerDTO>("Loyalty\\CustomerDTO");
            mockCustomerDTO.Customer.LoyaltyNumber = "9107070394";

            _mockPartnersProfilesQueryProvider.Setup(x => x.GetPartnerProfileDetails()).ReturnsAsync(mockPartnerProfilesResponse.Response);
            _mockCustomerProvider.Setup(x => x.GetCustomer(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(mockCustomerDTO);
            _mockCustomerTransactionProvider.Setup(x => x.Mutate(It.IsAny<PartnerMembershipRequest>())).ReturnsAsync(mockResponse);

            //Act & Assert
            await Assert.ThrowsAsync<GraphQLException>(() => _mutation.UpdatePartnerMembership(mockRequest));
        }

        [Fact]
        public async Task PartnerMembershipUpdateShouldSuccessWithoutCustomerId()
        {
            // Arrange
            var mockRequest = TestDataProvider.GetTestData<PartnerMembershipMutationRequest>("Loyalty//PartnerMembershipUpdateRequest");
            mockRequest.CustomerId = null;
            mockRequest.MemberNumber = "9107070394";

            var mockResponse = TestDataProvider.GetTestData<PartnerMembershipDocumentResponse>("Loyalty//PartnerMembershipUpdateResponse");
            mockResponse.Errors = null;

            var mockPartnerProfilesResponse = TestDataProvider.GetTestData<APIResponse<IEnumerable<PartnerProfileDetails>>>("MemberCardLink\\PartnersProfiles_APIResponse");
            var mockCustomerDTO = TestDataProvider.GetTestData<CustomerDTO>("Loyalty\\CustomerDTO");
            mockCustomerDTO.Customer.CustomerId = "b535cf88-a41d-4747-b352-494106ecec1f";

            _mockPartnersProfilesQueryProvider.Setup(x => x.GetPartnerProfileDetails()).ReturnsAsync(mockPartnerProfilesResponse.Response);
            _mockCustomerProvider.Setup(x => x.GetCustomer(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(mockCustomerDTO);
            _mockCustomerTransactionProvider.Setup(x => x.Mutate(It.IsAny<PartnerMembershipRequest>())).ReturnsAsync(mockResponse);

            //Act
            var result = await _mutation.UpdatePartnerMembership(mockRequest);

            //Assert
            Assert.Equal(mockResponse.PartnerMembershipDocument, result);
            Assert.Equal(mockResponse.PartnerMembershipDocument.MemberNumber, result.MemberNumber);
            Assert.Equal(mockResponse.PartnerMembershipDocument.CreatedBy, result.CreatedBy);
            Assert.Equal(mockResponse.PartnerMembershipDocument.PartnerId, result.PartnerId);
        }

        [Fact]
        public async Task PartnerMembershipUpdateValidationWithoutPartnercode()
        {
            // Arrange
            var request = TestDataProvider.GetTestData<PartnerMembershipMutationRequest>("Loyalty//PartnerMembershipUpdateRequest");
            request.PartnerCode = null;
            request.MemberNumber = "9107070394";

            //Act & Assert
            await Assert.ThrowsAsync<RequestValidationException>(() => _mutation.UpdatePartnerMembership(request));
        }


        [Fact]
        public async Task PartnerMembershipUpdateValidationWithoutCustomerAndMemberID()
        {
            // Arrange
            var request = TestDataProvider.GetTestData<PartnerMembershipMutationRequest>("Loyalty//PartnerMembershipUpdateRequest");
            request.CustomerId = null;
            request.MemberNumber = null;

            //Act & Assert
            await Assert.ThrowsAsync<RequestValidationException>(() => _mutation.UpdatePartnerMembership(request));
        }

        [Fact]
        public async Task UpdatePartnerMembershipWithHttpException()
        {
            // Arrange
            var mockRequest = TestDataProvider.GetTestData<PartnerMembershipMutationRequest>("Loyalty//PartnerMembershipUpdateRequest");
            mockRequest.MemberNumber = null;

            var mockResponse = TestDataProvider.GetTestData<PartnerMembership>("Loyalty//PartnerMembershipUpdateResponse");

            var mockPartnerProfilesResponse = TestDataProvider.GetTestData<APIResponse<IEnumerable<PartnerProfileDetails>>>("MemberCardLink\\PartnersProfiles_APIResponse");
            var mockCustomerDTO = TestDataProvider.GetTestData<CustomerDTO>("Loyalty\\CustomerDTO");
            mockCustomerDTO.Customer.LoyaltyNumber = "9107070394";

            _mockPartnersProfilesQueryProvider.Setup(x => x.GetPartnerProfileDetails()).ReturnsAsync(mockPartnerProfilesResponse.Response);
            _mockCustomerProvider.Setup(x => x.GetCustomer(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(mockCustomerDTO);
            _mockCustomerTransactionProvider.Setup(x => x.Mutate(It.IsAny<PartnerMembershipRequest>())).Throws(new HttpRequestException("HttpConnector failed.", new HttpResponseException(System.Net.HttpStatusCode.InternalServerError), System.Net.HttpStatusCode.InternalServerError));

            //Act & Assert
            await Assert.ThrowsAsync<HttpRequestException>(() => _mutation.UpdatePartnerMembership(mockRequest));
        }
    }
}
